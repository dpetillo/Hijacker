using System.IO;
using NUnit.Framework;
using System;
using Hijacker;

namespace UntestableTests
{
    [TestFixture]
    class Tests
    {
        [OneTimeSetUp]
        public static void TestSetup()
        {
        }

        [Test]
        public void StaticCallOnly()
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            {
                sw.Write("hello world");
                sw.Flush();
            }
            ms.Position = 0;

            Hijack.Setup(() => File.Open(It.Any<string>(), It.Any<FileMode>())).Returns(ms);

            var untestable = new UntestableLibrary.Untestable();

            string s = untestable.StaticCallOnly();

            Assert.AreEqual("hello world", s);
        }


        [Test]
        public void InstanceCall()
        {
            Hijack.Setup<Random>(r => r.Next()).Returns(10);

            var untestable = new UntestableLibrary.Untestable();

            var result = untestable.InstanceCall();

            Assert.AreEqual(20, result);
        }

        
    }



}
