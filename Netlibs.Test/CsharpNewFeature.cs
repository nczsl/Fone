using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Netlibs.Test {
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class CsharpNewFeature {
        [TestMethod]
        public void m15() {
            //continue m12 modify that insteed Span<T>
            //indexor
            var x = new[] { 1, 2, 3, 4, 5, 6, 7 };
            var x2 = x[2..5];
            var x3=new Span<int>(x);
            x3=x3.Slice(2,3);
            foreach (var item in x2) {
                Console.WriteLine(item);
            }
            Console.WriteLine("update");
            //update value
            x2[0] += 1;
            x2[^1] += 2;
            foreach (var item in x) {
                Console.WriteLine(item);
            }
            Console.WriteLine("-----");
            foreach (var item in x2) {
                Console.WriteLine(item);
            }
            Console.WriteLine("after x2[?..?] update then orginal array is:-----");
            foreach (var item in x) {
                Console.WriteLine(item);
            }
            Console.WriteLine("Span<T> is under begin");
            //update at Span<T>
            x3[0] += 1;
            x3[^1] += 2;
            foreach (var item in x) {
                Console.WriteLine(item);
            }
            Console.WriteLine("update at Span<T>-----");
            foreach (var item in x3) {
                Console.WriteLine(item);
            }
            Console.WriteLine("after Span<T> update then orginal array is:-----");
            foreach (var item in x) {
                Console.WriteLine(item);
            }
        }
        [TestMethod]
        public void m14() {
            //span<T>
            var x1=new Employee { Id=1,Name="Simth"};
            var x3=new Employee { Id=2,Name="Tomas"};
            var x4=new Employee { Id=3,Name= "Lucy" };
            var x2=new Span<Employee>(new[]{x1,x3,x4});
            foreach (var item in x2) {
                Console.WriteLine("{0},{1}",item.Id,item.Name);
            }
        }
        [TestMethod]
        public void m13() {
            //span
            var x = new[] {1,2,3,4,5,6,7};
            Span<int> x2 = stackalloc int[5];
            for (int i = 0; i < 5; i++) {
                x2[i]=x[i*3%7];
            }
            foreach (var item in x2) {
                Console.WriteLine(item);
            }
        }
        [TestMethod]
        public void m12() {
            //indexor
            var x = new[] {1,2,3,4,5,6,7};
            var x2=x[2..5];
            foreach (var item in x2) {
                Console.WriteLine(item);
            }
            Console.WriteLine("update");
            //update value
            x2[0]+=1;
            x2[^1]+=2;
            foreach (var item in x) {
                Console.WriteLine(item);
            }
            Console.WriteLine("-----");
            foreach (var item in x2) {
                Console.WriteLine(item);
            }
        }
        [TestMethod]
        public void m11() {
            //ref vairable
            ref int m11_1(int[] x) => ref x[x.Length / 2];
            var v1 = new[] {1,2,3};
            var x=m11_1(v1);
            x=5;
            foreach (var item in v1) {
                Console.WriteLine(item);
            }
            Console.WriteLine("----------");
            ref var x2=ref m11_1(v1);
            x2=5;
            foreach (var item in v1) {
                Console.WriteLine(item);
            }
        }
    }
}
