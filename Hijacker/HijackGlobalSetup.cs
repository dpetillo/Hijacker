using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hijacker
{
    public class HijackGlobalSetup
    {
        internal class CallingAssembly
        {
            public string Name { get; set; }
            public string Location { get; set; }
        }

        private List<CallingAssembly> _callingAssemblies = new List<CallingAssembly>();
        private List<Call> _expressions = new List<Call>();

        internal List<CallingAssembly> CallingAssemblies { get { return _callingAssemblies; } }

        internal List<Call> Expressions { get { return _expressions; } }

        public HijackGlobalSetup AddCallingAssembly(string assemblyName, string assemblyLocation)
        {
            _callingAssemblies.Add(new CallingAssembly() { Name = assemblyName, Location = assemblyLocation });

            return this;
        }

        public HijackGlobalSetup Setup(Expression<Action> expression)
        {

            _expressions.Add(Call.FromExpression(expression));
            return this;
        }

        public HijackGlobalSetup Setup<T>(Expression<Action<T>> expression)
        {
            _expressions.Add(Call.FromExpression(expression));

            return this;
        }


        public void Start()
        {
            Interceptor.Start(this);
        }

    }
}
