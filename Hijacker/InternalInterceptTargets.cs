using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hijacker
{
    public class InternalInterceptTargets
    {
        public static object Intercept<T>(object arg1, short arg2)
        {
            return Interceptor.GetDelegate(typeof(T).Name).Invoke(new object[] { arg1, arg2 });
        }
        /*
        public static object Intercept<T>()
        {
            return Interceptor.GetDelegate(typeof(T).Name).Invoke(new object[] {});
        }*/

        public static int Intercept<T>()
        {
            return (int)Interceptor.GetDelegate(typeof(T).Name).Invoke(new object[] { });
        }

        public static int Intercept<T>(object instance)
        {
            return (int)Interceptor.GetDelegate(typeof(T).Name).Invoke(new object[] { });
        }


    }

}
