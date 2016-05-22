using System;
using System.Linq.Expressions;

namespace Hijacker
{
    public class Hijack
    {
        public static CallSetup Setup(Expression<Action> setupExpr)
        {
            return new CallSetup( Call.FromExpression(setupExpr));
        }
        public static CallSetup Setup<T>(Expression<Action<T>> setupExpr)
        {
            return new CallSetup(Call.FromExpression<T>(setupExpr));
        }

    }

    public class CallSetup
    {
        private Call _setupExpr;
        private object _retVal;

        internal CallSetup(Call setupExpr)
        {
            _setupExpr = setupExpr;
        }

        public void Returns(object retVal)
        {
            _retVal = retVal;

            Interceptor.SetIntercept(_setupExpr, @params => retVal);
        }
    
    }
}
