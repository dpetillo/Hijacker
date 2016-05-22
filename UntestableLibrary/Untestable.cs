using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UntestableLibrary
{
    public class Untestable
    {

        public string StaticCallOnly()
        {
            //'Untestable' because we're using a static call to File
            using (var filestream = File.Open(@"C:\something", FileMode.Open))
            using (var sw = new StreamReader(filestream))
            {
                return sw.ReadToEnd();
            }
        }
        public int InstanceCall()
        {
            var rand = new Random();
            return rand.Next() + 10;
        }

    }
}
