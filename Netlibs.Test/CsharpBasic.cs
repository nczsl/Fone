using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Netlibs.Test {
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class CsharpBasic {
        [TestMethod]
        public void m12() {
            var x=new TestIEnumerable();
            foreach (var item in x) {
                Console.WriteLine(item);
            }
        }
        [TestMethod]
        public void m11() {
            //自制迭代器
            var x=new RandomLuncher(10);
            foreach (var item in x) {
                Console.WriteLine(item);
            }
        }
    }
    /// <summary>
    /// 这个类因没有继承有泛型那个接口所以只有迭代能力而没有，linq能力
    /// </summary>
    class TestIEnumerable : IEnumerable {
        public IEnumerator GetEnumerator() {
            yield return "aa";
            yield return "aa";
            yield return "bb";
        }
    }
    class RandomLuncher : IEnumerable<double>, IEnumerator<double> {
        Random ran;
        public RandomLuncher(int count) {
            if(count<0)throw new ArgumentException("迭代次数必须为正数");
            Length=count;
            ran=new Random();
        }
        public double Current => ran.NextDouble();

        public int Length { get; private set; }

        object IEnumerator.Current => this.Current;

        public void Dispose() {
            Length=default(int);
            ran=null;
            Reset();
        }

        public IEnumerator<double> GetEnumerator() {
            return this;
        }

        public bool MoveNext() => Length-- > 0;

        public void Reset() {
            Length=0;
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
