namespace Netlibs.Test {
    using System;
    using Util;
    using Util.Ex;
    using Fone;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Util.Mathematics.Function;
    using System.Runtime.CompilerServices;
    using System.Linq.Expressions;
    using System.Drawing;
    using Util.Generator;
    using System.Diagnostics;
    using System.Text.RegularExpressions;

    [TestClass]
    public class Gm {
        [TestMethod]
        public void m42() {
            var frh = new FuncRadicalHandler();
            frh.Build(
                (new Radical(2), 0),
                (new Radical(), 0),
                (new Radical(3, PatternType.Subtract), 1),
                (new Radical(2, PatternType.Subtract), 1),
                (new Radical(5, PatternType.Multiply), 0),
                (new Radical(2, PatternType.Divide), 0),
                (new Radical(4, PatternType.Add), 0),
                (new Radical(3, PatternType.Pow), 0),
                (new Radical(2, PatternType.Multiply), 0)
            );
            Trace.Listeners.Add(new TextWriterTraceListener(@"D:\debug info\netenv\temp.txt"));
            Trace.AutoFlush=true;
            Debug.WriteLine(frh.chain.Query().Print());
            Debug.WriteLine("---------------");
            frh.Procedrue();
            Debug.WriteLine(frh.Target.Query().Print(),nameof(frh.Target));
        }
        [TestMethod]
        public void m41() {
            var s = string.Concat(new[] { 5, 3, 4, 9, 1 });
            System.Console.WriteLine(string.Concat(",", new[] { 5, 3, 4, 9, 1 }.OrderBy(i => i)));
            System.Console.WriteLine(string.Concat(new[] { 5, 3, 4, 9, 1 }.OrderBy(i => i)));
            var x = Regex.Replace(s, "\\d", "1");
            var s2 = string.Concat(new[] { 1, 1, 1, 1, 1 });
            var x2 = Regex.Replace(s2, "\\d", "1");
            System.Console.WriteLine(x);
            System.Console.WriteLine(2);
            System.Console.WriteLine($"{s}=={x},{s == x}");
            System.Console.WriteLine($"{s2}=={x2},{s2 == x2}");
        }
        [TestMethod]
        public void m40() {
            var x = "aaa";
            var x2 = x?.Length;
            System.Console.WriteLine(x2);
            double d;
            // var x3=d?.Equals(3.0);
            System.ArgIterator ai = default;
            //ai?.End();
            ai.End();
            List<int> list = new();
            var t = list?.Capacity.GetType();
            System.Console.WriteLine(t);
            var csps = new CsNamespace();
            System.Console.WriteLine(csps.enums?.Count);
            System.Console.WriteLine(csps.typs?[0]?.name);
        }
        [TestMethod]
        public void m39() {
            var sfo = Fone.Sqlitefo.Attach("f:\\dbstore\\simple_log.db");
            // sfo.CreateTable<SimpleLog>()
            // sfo.Insert(new SimpleLog{Message="test info",Note="/",OnTime=DateTime.Now})
            // sfo.Update(new SimpleLog{Id=2,Message="update message",Note="ok2",OnTime=DateTime.Now})
            // sfo.RemoveTable<SimpleLog>()
            // .Detach();
            sfo.Query<SimpleLog>("Note !='/'");
            foreach (SimpleLog i in sfo.resultset[nameof(SimpleLog)]) {
                System.Console.WriteLine(i);
                System.Console.WriteLine(i.Message);
            }
            sfo.QueryTable();
            foreach (var i in sfo.tableNames) {
                System.Console.WriteLine(i);
            }
        }
        [TestMethod]
        public void m38() {
            var frh = new FuncRadicalHandler();
            frh.Build(
                (new Radical("a"), 0)
                , (new Radical(), 0)
                , (new Radical("a", PatternType.Subtract), 1)
                , (new Radical("c", PatternType.Subtract), 1)
                , (new Radical("b", PatternType.Multiply), 0)
                , (new Radical(2, PatternType.Divide), 0)
                , (new Radical("b", PatternType.Add), 0)
                , (new Radical(3, PatternType.Pow), 0)
                , (new Radical("d", PatternType.Multiply), 0)
            );
            Trace.Listeners.Add(new TextWriterTraceListener(@"D:\debug info\netenv\temp.txt"));
            Debug.AutoFlush = true;
            System.Console.WriteLine(frh.chain.Query().Print());
            var coding = frh.chain.Encoding();
            System.Console.WriteLine(string.Join(",", frh.chain.signhash));
            // System.Console.WriteLine(string.Join(",",from i in frh.chain.QueryGroup() select i.Radical));
            frh.Procedrue();
            Trace.Flush();
        }
        [TestMethod]
        public void m37() {
            double x1 = 0d;
            int x2 = 1;
            var convertint = Map<int>.NumberConvert<int>();
            var convertdouble = Map<double>.NumberConvert<double>();
            System.Console.WriteLine(convertdouble(x1));
            System.Console.WriteLine(convertdouble(x2));
            System.Console.WriteLine(convertint(x2));
        }
        [TestMethod]
        public void m36() {
            int i1 = 3;
            double d2 = i1;
            System.Console.WriteLine(d2);
            short i2 = 3;
            d2 = i2;
            System.Console.WriteLine(d2);
            long i3 = 3;
            d2 = i3;
            System.Console.WriteLine(d2);
            byte i4 = 3;
            d2 = i4;
            System.Console.WriteLine(d2);
            float i5 = 3f;
            d2 = i5;
            System.Console.WriteLine(d2);
            i5 = i1;
            System.Console.WriteLine(i5);
            i1 = i2;
            System.Console.WriteLine(i2);
            System.Console.WriteLine(sizeof(int));
            System.Console.WriteLine(sizeof(uint));
            System.Console.WriteLine(sizeof(ulong));
            System.Console.WriteLine(sizeof(short));
            System.Console.WriteLine(sizeof(sbyte));
            System.Console.WriteLine(sizeof(byte));
            System.Console.WriteLine(sizeof(float));
            System.Console.WriteLine(sizeof(double));
            System.Console.WriteLine(sizeof(decimal));
            System.Console.WriteLine(sizeof(long));
        }
        [TestMethod]
        public void m35() {
            var x = new Map<int>();
            x.Load(new[,] { { 1, 2, 3, 4 }, { 5, 6, 7, 8 } });
            System.Console.WriteLine(x.Print());
            System.Console.WriteLine(x.Transpose().Print());
        }
        //构造单位反对称矩阵
        [TestMethod]
        public void m34() {
            var x = 5.AntisymmetricIdentityOperator<int>();
            System.Console.WriteLine(x.Print());
        }
        //反对称矩阵
        [TestMethod]
        public void m33() {
            var left = new Map<double>();
            var right = new Map<double>();
            left.ReserveSpace(3, 3);
            right.ReserveSpace(3, 3);
            left.AppendRow(3, 7, 9);
            left.AppendRow(3, 4, 9);
            left.AppendRow(1, 4, 9);
            right.AppendRow(2, 5, 4);
            right.AppendRow(6, 1, 8);
            right.AppendRow(3, 1, 5);
            System.Console.WriteLine(left.Print());
            System.Console.WriteLine(left.Antisymmetric().Print());
            System.Console.WriteLine();
            System.Console.WriteLine(right.Print());
            System.Console.WriteLine(right.Antisymmetric().Print());
        }
        //对称矩阵
        [TestMethod]
        public void m32() {
            var left = new Map<int>();
            var right = new Map<int>();
            left.ReserveSpace(3, 3);
            right.ReserveSpace(3, 3);
            left.AppendRow(3, 7, 9);
            left.AppendRow(3, 4, 9);
            left.AppendRow(1, 4, 9);
            right.AppendRow(2, 5, 4);
            right.AppendRow(6, 1, 8);
            right.AppendRow(3, 1, 5);
            System.Console.WriteLine(left.Print());
            System.Console.WriteLine(left.Symmetric().Print());
            System.Console.WriteLine();
            System.Console.WriteLine(right.Print());
            System.Console.WriteLine(right.Symmetric().Print());
        }
        //转置
        [TestMethod]
        public void m31() {
            var left = new Map<int>();
            var right = new Map<int>();
            left.ReserveSpace(3, 3);
            right.ReserveSpace(3, 3);
            left.AppendRow(3, 7, 9);
            left.AppendRow(3, 4, 9);
            left.AppendRow(1, 4, 9);
            right.AppendRow(2, 5, 4);
            right.AppendRow(6, 1, 8);
            right.AppendRow(3, 1, 5);
            System.Console.WriteLine(left.Print());
            System.Console.WriteLine(left.Transpose().Print());
            System.Console.WriteLine();
            System.Console.WriteLine(right.Print());
            System.Console.WriteLine(right.Transpose().Print());
        }
        //test getrow getcolumn
        [TestMethod]
        public void m30() {
            var left = new Map<int>();
            var right = new Map<int>();
            left.ReserveSpace(3, 3);
            right.ReserveSpace(3, 3);
            left.AppendRow(3, 7, 9);
            left.AppendRow(3, 4, 9);
            left.AppendRow(1, 4, 9);
            right.AppendRow(2, 5, 4);
            right.AppendRow(6, 1, 8);
            right.AppendRow(3, 1, 5);

            System.Console.WriteLine(string.Join(",", left.GetRow(1)));
            System.Console.WriteLine(string.Join(",", left.GetRow(0)));
            System.Console.WriteLine(string.Join(",", left.GetRow(2)));
            System.Console.WriteLine(string.Join(",", right.GetColumn(1)));
            System.Console.WriteLine(string.Join(",", right.GetColumn(2)));
            System.Console.WriteLine(string.Join(",", right.GetColumn(0)));
            System.Console.WriteLine(string.Join(",", right.GetColumn(2)));
        }
        ///矩阵的加减，和数乘
        [TestMethod]
        public void m29() {
            var left = new Map<int>();
            var right = new Map<int>();
            left.ReserveSpace(3, 3);
            right.ReserveSpace(3, 3);
            left.AppendRow(3, 7, 9);
            left.AppendRow(3, 4, 9);
            left.AppendRow(1, 4, 9);
            right.AppendRow(2, 5, 4);
            right.AppendRow(6, 1, 8);
            right.AppendRow(3, 1, 5);
            var x = left + right;
            var x2 = left - right;
            var x3 = 2 * right;
            System.Console.WriteLine(left.Print());
            System.Console.WriteLine(x.Print());
            System.Console.WriteLine(x2.Print());
            System.Console.WriteLine(x3.Print());
        }
        [TestMethod]
        public void m28() {
            var x = new Map<int>();
            x.Load(Enumerable.Range(0, 10 * 10).ToArray(), 10, 10);
            System.Console.WriteLine(x.Print());
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(0, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(1, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(2, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(3, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(4, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(5, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(6, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(7, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(8, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(9, true)));
            // System.Console.WriteLine(string.Join(",",x.GetDiagonal(0,false)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(1, false)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(2, false)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(3, false)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(4, false)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(5, false)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(6, false)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(7, false)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(8, false)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(9, false)));
            System.Console.WriteLine();

            System.Console.WriteLine(string.Join(",", x.GetDiagonal(0, true, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(1, true, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(2, true, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(3, true, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(4, true, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(5, true, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(6, true, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(7, true, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(8, true, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(9, true, true)));
            // System.Console.WriteLine(string.Join(",",x.GetDiagonal(0,false)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(1, false, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(2, false, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(3, false, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(4, false, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(5, false, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(6, false, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(7, false, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(8, false, true)));
            System.Console.WriteLine(string.Join(",", x.GetDiagonal(9, false, true)));
        }
        [TestMethod]
        public void m27() {
            var left = new[] { 1, 2, 3 };
            var right = new[] { 2, 1, 4 };
            var x = Map<int>.Cross(left, right);
            var x2 = Map<int>.Cross(right, left);
            System.Console.WriteLine(x.Print());
            System.Console.WriteLine(x2.Print());
            left = new[] { 3, 7, 9 };
            right = new[] { 2, 5, 4 };
            x = Map<int>.Cross(left, right);
            System.Console.WriteLine(x.Print());
            left = new[] { 1, 2, 3, 8 };
            right = new[] { 2, 1, 4, 7 };
            x = Map<int>.Cross(left, right);
            x2 = Map<int>.Cross(right, left);
            System.Console.WriteLine(x.Print());
            System.Console.WriteLine(x2.Print());
        }
        [TestMethod]
        public void m26() {
            var map = new Map<float>();
            //map.Delta();
            // double.IsInfinity()

            var x = new Random();
            for (var i = 0; i < 10; i++) {
                var num = x.Next(10 * 1000000);
                System.Console.WriteLine("{0}:{1}", num, Math.Floor(Math.Log10(num)));

            }
            System.Console.WriteLine("{0}:{1}", 9999, Math.Floor(Math.Log10(9999)));
            System.Console.WriteLine("{0}:{1}", 10000, Math.Floor(Math.Log10(10000)));

        }
        [TestMethod]
        public void m25() {
            var left = new Map<int>();
            left.ReserveSpace(2, 3);
            var right = new Map<int>();
            right.ReserveSpace(3, 2);
            left.AppendRow(new[] { 1, 2, 3 });
            left.AppendRow(4, 5, 6);
            right.AppendColumn(7, 8, 9);
            right.AppendColumn(4, 3, 1);
            var x = left * right;
            System.Console.WriteLine(left.Print());
            System.Console.WriteLine(right.Print());
            System.Console.WriteLine(x.Print());
            // var y=Map<double>.Multiple(left,right);
            // System.Console.WriteLine(y.Print());
        }
        [TestMethod]
        public void m24() {
            var map = new Map<double>();
            var mul = Map<double>.BuildBinaryExpression(PatternType.Multiply);
            System.Console.WriteLine(mul(3, 8));
            var pow = Map<double>.BuildBinaryExpression(PatternType.Pow);
            System.Console.WriteLine(pow(2, 8));
            var log = Map<double>.BuildBinaryExpression(PatternType.Log);
            System.Console.WriteLine(log(2, 1024));
            System.Console.WriteLine(Math.Log(1024, 2));

            var root = Map<double>.BuildBinaryExpression(PatternType.Root);
            System.Console.WriteLine(root(27, 3));
            System.Console.WriteLine(root(2, 2));
        }
        [TestMethod]
        public void m23() {
            T Add<T>(T left, T right) {
                var t = default(T);
                var pleft = Expression.Parameter(typeof(T));
                var pright = Expression.Parameter(typeof(T));
                var add = Expression.Add(pleft, pright);
                var lambda = Expression.Lambda<Func<T, T, T>>(
                    add, new[] { pleft, pright }
                );
                var madd = lambda.Compile();
                t = madd(left, right);
                return t;
            }
            var x = Add<double>(3.0d, 5.1d);
            System.Console.WriteLine(x);
        }

        [TestMethod]
        public void m22() {
            var eye5 = Map<float>.Identity(5);
            System.Console.WriteLine(eye5.Print());
        }
        [TestMethod]
        public void m21() {
            var left = new Map<double>();
            left.ReserveSpace(3, 3);
            var right = new Map<double>();
            right.ReserveSpace(3, 3);
            left.AppendRow(new double[] { 1, 2, 3 });
            left.AppendRow(4, 5, 6);
            left.AppendRow(7, 8, 9);
            right.AppendColumn(2, 9, 1);
            right.AppendColumn(5, 6, 4);
            right.AppendColumn(8, 3, 7);
            var x = left * right;
            System.Console.WriteLine(left.Print());
            System.Console.WriteLine(right.Print());
            System.Console.WriteLine(x.Print());
        }
        [TestMethod]
        public void m20() {
            var map = new Map<byte>();
            map.Load(new byte[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } });
            System.Console.WriteLine(string.Join(" ", map.GetDiagonal(0, true, true)));
            System.Console.WriteLine(string.Join(" ", map.GetDiagonal(0, false, false)));
        }
        [TestMethod]
        public void m19() {
            var map = new Map<byte>();
            map.ReserveSpace(10, 4);
            map.AppendRow(new byte[] { 1, 2, 3, 4 });
            map.AppendRow(new byte[] { 5, 6, 7, 8 });
            System.Console.WriteLine(map.Print());
            System.Console.WriteLine(string.Join(" ", from i in map.Query(0, 1, map.Length) select i));
        }
        [TestMethod]
        public void m18() {
            var map = new Map<byte>();
            map.ReserveSpace(4, 10);
            map.AppendColumn(new byte[] { 1, 2, 3, 4 });
            map.AppendColumn(new byte[] { 5, 6, 7, 8 });
            System.Console.WriteLine(map.Print());
            System.Console.WriteLine(string.Join(" ", from i in map.Query(0, 1, map.Length) select i));
            System.Console.WriteLine(string.Join(" ", map.GetRow(1)));
            System.Console.WriteLine(string.Join(" ", map.GetRow(2)));
        }
        [TestMethod]
        public void m17() {
            var map = new Map<byte>();
            map.Load(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 3, 3);
            map[1, 1] += 1;
            System.Console.WriteLine(map.Print());
            System.Console.WriteLine(string.Join(" ", map.GetRow(1)));
            System.Console.WriteLine(string.Join(" ", map.GetColumn(1)));
        }
        [TestMethod]
        public void m16() {
            var v = new Map<byte>();
            v.Load(new byte[,] { { 2, 18, 55, 32 }, { 3, 19, 21, 7 } });
            System.Console.WriteLine(v.Row);
            System.Console.WriteLine(v.Column);
            var data = v.Query(1, 2, 3);
            foreach (var i in data) {
                //ref var temp=ref i;
                System.Console.WriteLine(i);
            }
            foreach (var i in v) {
                System.Console.WriteLine(i);
            }
            //ref int[] bb;
        }
        ///<summary>
        ///C# 10新特性
        ///</summary>
        [TestMethod]
        public void m15() {
            IEnumerable<int[]> chunks = Enumerable.Range(0, 10).Chunk(size: 3);
            var r2 = Enumerable.Range(1, 20).DistinctBy(x => x % 3); // {1, 2, 3}
            System.Console.WriteLine(string.Join(",", r2));

            var first = new (string Name, int Age)[] { ("Francis", 20), ("Lindsey", 30), ("Ashley", 40) };
            var second = new (string Name, int Age)[] { ("Claire", 30), ("Pat", 30), ("Drew", 33) };
            var r1 = first.UnionBy(second, person => person.Age);
            System.Console.WriteLine(string.Join(",", r1));

            var source = Enumerable.Range(1, 15);
            System.Console.WriteLine(string.Join(",", source));
            System.Console.WriteLine(string.Join(",", source.Take(..3)));
            System.Console.WriteLine(string.Join(",", source.Take(3..)));
            System.Console.WriteLine(string.Join(",", source.Take(2..7)));
            System.Console.WriteLine(string.Join(",", source.Take(^3..)));
            System.Console.WriteLine(string.Join(",", source.Take(..^3)));
            System.Console.WriteLine(string.Join(",", source.Take(^7..^3)));
            System.Console.WriteLine(string.Join(",", source.Chunk(3)));

            var x = new { A = 1, B = 2 };
            var y = x with { A = 3 };
            System.Console.WriteLine(y.A);
            const string x2 = "hello";
            const string y2 = $"{x2}, world!";
            System.Console.WriteLine(y2);
            var f = int () => 4;
            System.Console.WriteLine(f());
            var f2 = ref int (ref int x) => ref x; // 返回一个参数的引用
            var i = 8;
            System.Console.WriteLine(f2(ref i));
            void Foo() { Console.WriteLine("hello"); }
            var x3 = Foo;
            x3(); // hello
            var f3 = () => 1; // Func<int>
            var g = string (int x, string y) => $"{y}{x}"; // Func<int, string, string>
            //var h = "test".GetHashCode; // Func<int>
            System.Console.WriteLine(f3());
            System.Console.WriteLine(g(3, "haha"));
            // System.Console.WriteLine(h());
            void Foo2(int value, [CallerArgumentExpression("value")] string? expression = null) {
                Console.WriteLine(expression + " = " + value);
            }
            Foo2(5 + 8 * 2 / (6 - 4));
        }
        [TestMethod]
        public void m14() {
            var x3 = (int i) => i > 5;

            System.Console.WriteLine(x3(2));
            System.Console.WriteLine(x3(8));
            System.Console.WriteLine(x3(4));
            System.Console.WriteLine(x3(5));
        }
        [TestMethod]
        public void m13() {
            string TestCsharp10Pattern(FuncRadical node) {
                var result = string.Empty;
                switch (node) {
                    case FuncRadical { Group: 0 } and { Id: > 1 }: result = "xxx"; break;
                    case FuncRadical { Group: 0 } and { Radical.Scalar: > 5.0d }: result = "yyy"; break;
                }
                return result;
            }
            var fr = new FuncRadical(new Radical { Scalar = 6.2d }) { Group = 0 };
            var mark = TestCsharp10Pattern(fr);
            System.Console.WriteLine(mark);
        }
        [TestMethod]
        public void m12() {
            var frh = new FuncRadicalHandler();
            frh.Build(
                (new Radical("a"), 0)
                , (new Radical(), 0)
                , (new Radical("a", PatternType.Subtract), 1)
                , (new Radical("c", PatternType.Subtract), 1)
                , (new Radical("b", PatternType.Multiply), 0)
                , (new Radical(2, PatternType.Divide), 0)
                , (new Radical("b", PatternType.Add), 0)
                , (new Radical(3, PatternType.Pow), 0)
                , (new Radical("d", PatternType.Multiply), 0)
            );
            System.Console.WriteLine(frh.chain.Query().Print());
            foreach (var i in frh.chain.Query().GetItems()) {
                System.Console.WriteLine(i.Print());
                foreach (var j in i.GetFactors()) {
                    System.Console.WriteLine(j.Print());
                }
            }
        }

        [TestMethod]
        public void m11() {
            System.Console.WriteLine("welcome to this test case for mathmetics by library of Util.General Model");
        }
    }
}