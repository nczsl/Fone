using Fone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Netlibs.Test {
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class emmet {
        Task Work1() {
            return Task.Run(() => {
                Thread.Sleep(10*1000);
            });
        }
        async Task Test() {
            Console.WriteLine(DateTime.Now);
            Func<Task> w1=()=>Work1();
            Func<Task> w2=()=>Work1();
            Func<Task> w3=()=>Work1();
            await w1.Invoke();
            await w2.Invoke();
            await w3.Invoke();
            Console.WriteLine(DateTime.Now);
        }

        async Task Test2() {
            Console.WriteLine(DateTime.Now);
            Func<Task> w1 =async () => await Work1();
            Func<Task> w2 =async () => await Work1();
            Func<Task> w3 = async () => await Work1();
            Task.WaitAll(new[] { w1.Invoke(),w2.Invoke(), w3.Invoke() });
            Console.WriteLine(DateTime.Now);
        }
        [TestMethod]
        public void m12() {
            Test2().Wait();
        }
        [TestMethod]
        public void m11() {
            Test().Wait();
        }
    }
}
