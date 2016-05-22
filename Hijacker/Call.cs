using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hijacker
{
    class Call : IEquatable<Call>
    {
        public MethodInfo Method { get; private set; }

        public static Call FromExpression(Expression<Action> setupExpression)
        {
            var methodCallExp = setupExpression.Body as MethodCallExpression;
            if (methodCallExp == null)
            {
                throw new ArgumentException("Expression not a method call.");
            }

            var call = new Call();

            call.Method = methodCallExp.Method;

            return call;
        }

        public static Call FromExpression<T>(Expression<Action<T>> setupExpression)
        {
            var methodCallExp = setupExpression.Body as MethodCallExpression;
            if (methodCallExp == null)
            {
                throw new ArgumentException("Expression not a method call.");
            }
            var call = new Call();

            call.Method = methodCallExp.Method;

            return call;

        }

        public bool Equals(Call other)
        {
            return Method == other.Method;
        }
    }
}
