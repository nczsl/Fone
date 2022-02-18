using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Util.Ex;

namespace Netlibs.Test {
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class AcexTest {
        [TestMethod]
        public void m11() {
            var x=typeof(Tuple<int,List<string>,Dictionary<int,double>,Dictionary<string,Tuple<int,string,Dictionary<string,int>>>>);
            Console.WriteLine(x.GetStandardTypeName());
        }
    }
}
