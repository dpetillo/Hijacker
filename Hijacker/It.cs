using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hijacker
{
    public class It
    {
        public static T Any<T>() {
            return default(T);
        }
    }
}
