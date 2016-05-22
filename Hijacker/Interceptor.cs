using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hijacker
{

    public static class Interceptor
    {
        private static HijackGlobalSetup _setup;
        private static Dictionary<string, HijackGlobalSetup.CallingAssembly> _callingAssemblyCache = new Dictionary<string, HijackGlobalSetup.CallingAssembly>();
        private static AssemblyDefinition _interceptAssemblyDef;
        private static Dictionary<string, Call> _callDictionary = new Dictionary<string, Call>();
        private static Dictionary<Call, string> _callKeyDictionary = new Dictionary<Call, string>();
        private static Dictionary<string, Delegate> _delegateDictionary = new Dictionary<string, Delegate>();


        internal static void Start(HijackGlobalSetup setup)
        {
            _setup = setup;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            _interceptAssemblyDef = AssemblyDefinition.ReadAssembly(typeof(Interceptor).Assembly.Location);

            PreAssemblyResolveCaching();

            foreach (var exp in _setup.Expressions)
            {
                var interceptKey = Guid.NewGuid().ToString().Replace("-", string.Empty);

                _callDictionary.Add(interceptKey, exp);
                _callKeyDictionary.Add(exp, interceptKey);
            }
        }

        private static void PreAssemblyResolveCaching()
        {
            foreach (var callingAssy in _setup.CallingAssemblies)
            {
                _callingAssemblyCache.Add(callingAssy.Name, callingAssy);
            }
        }

        internal static void SetIntercept(Call callSetup, Func<object[], object> @delegate)
        {
            Call matchCall = null;
            foreach (var exp in _setup.Expressions)
            {
                if (exp.Equals(callSetup))
                {
                    matchCall = exp;
                    break;
                }
            }
            if (matchCall == null)
            {
                throw new ArgumentException("Setup expression not registered during Intercept setup (see InterceptSetup.AddIntercept).");
            }

            string interceptKey =_callKeyDictionary[matchCall];


            _delegateDictionary.Remove(interceptKey);
            _delegateDictionary.Add(interceptKey, @delegate);

        }

        public static Func<object[], object> GetDelegate(string interceptKey)
        {
            return (Func<object[], object>)_delegateDictionary[interceptKey];
        }


        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string simpleAssyName = args.Name.Split(',')[0];

            if (!_callingAssemblyCache.ContainsKey(simpleAssyName))
            {
                return null;
            }

            var callingAssy = _callingAssemblyCache[simpleAssyName];
            var exp1 =  _setup.Expressions.First();

            try
            {
                var assyDef = AssemblyDefinition.ReadAssembly(callingAssy.Location);

                foreach (var setupExpression in _setup.Expressions)
                {
                    ReplaceMethodCalls(assyDef, setupExpression);
                }

                using (var ms = new MemoryStream())
                using (var symbolMs = new MemoryStream())
                {
                    var writerParameters = new WriterParameters();
                    writerParameters.WriteSymbols = true;
                    writerParameters.SymbolStream = symbolMs;
                    assyDef.Write(ms, writerParameters);

                    assyDef.Write(@"c:\users\derek\documents\visual studio 2015\Projects\Interceptor\DemoLibrary\bin\Debug\intercepted.dll");

                    var newAssy = Assembly.Load(ms.ToArray(), symbolMs.ToArray());

                    return newAssy;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }


        private static void ReplaceMethodCalls(AssemblyDefinition callingAssyDef, Call call)
        {

            var interceptorTypeDef = _interceptAssemblyDef.Modules[0].Types.Where(td => td.Name == "InternalInterceptTargets").FirstOrDefault();

            //create a new type with the name of the key associated with the setup expression, add it to the calling assembly
            //this Type will be used as the generic parameter when calling back into InternalInterceptTargets.  This is all the context
            //the call into InternalInterceptTargets will have.  It will be used to find the active delegate associated to that interception and invoke it
            var systemAssemblyDef = AssemblyDefinition.ReadAssembly(typeof(object).Assembly.Location);
            var objectTypeDef = systemAssemblyDef.MainModule.Types.Where(t => t.Name == "Object").First();
            var objectTypeRef = callingAssyDef.MainModule.Import(objectTypeDef);

            var interceptKeyTypeDef = new TypeDefinition("Interception", _callKeyDictionary[call], Mono.Cecil.TypeAttributes.Class, objectTypeRef);
            callingAssyDef.MainModule.Types.Add(interceptKeyTypeDef);

            foreach (var module in callingAssyDef.Modules)
            {
                foreach (var type in module.Types)
                {
                    foreach (var method in type.Methods)
                    {
                        foreach (var instr in method.Body.Instructions)
                        {
                            if (instr.OpCode.Code == Code.Callvirt || instr.OpCode.Code == Code.Call || instr.OpCode.Code == Code.Calli)
                            {
                                MethodReference mr = (MethodReference)instr.Operand;
                                if (mr.DeclaringType.FullName == call.Method.DeclaringType.FullName
                                    && mr.Name == call.Method.Name
                                    && mr.Parameters.Count == call.Method.GetParameters().Count()
                                    )
                                {

                                    MethodDefinition interceptMethodDef;
                                    if (instr.OpCode.Code == Code.Call)
                                    {
                                        interceptMethodDef = interceptorTypeDef.Resolve().Methods.Where(md => md.Name == "Intercept" && md.Parameters.Count == call.Method.GetParameters().Count()).First();
                                    }
                                    else
                                    {
                                        //instance method
                                        interceptMethodDef = interceptorTypeDef.Resolve().Methods.Where(md => md.Name == "Intercept" && md.Parameters.Count == call.Method.GetParameters().Count() + 1).First();
                                    }

                                    var importedInterceptType = module.Import(interceptorTypeDef);
                                    var importedInterceptMethod = module.Import(interceptMethodDef);

                                    var genericMethod = new GenericInstanceMethod(importedInterceptMethod);

                                    genericMethod.GenericArguments.Add(interceptKeyTypeDef);

                                    instr.Operand = genericMethod;
                                    //force it to be a static call
                                    instr.OpCode = OpCodes.Call;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
