using DescriptionModel.oa;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Netlibs.Test {
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class ExpressionParse {
        [TestMethod]
        public void m12() {

            var emps = new[] {
                new Employee{Id=1,Name="aaa",},
                new Employee{Id=2,Name="bbb",},
                new Employee{Id=3,Name="ccc",},
                new Employee{Id=4,Name="ddd",},
                new Employee{Id=5,Name="eee",},
            };
        }
        [TestMethod]
        public void m11() {
            //expression表达式树的自动转换尝试
            Expression<Action> x1 = ()=>Console.WriteLine(222);
            x1.Compile()();
            var _x2=0;
            //不能是赋值语句
            //Expression<Action> x2=()=>_x2=3;
            //不能有大括号
            //Expression<Action> x3 = () => {Console.WriteLine(233)};

        }
    }
}
