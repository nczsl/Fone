using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Fone;
using LYF.Math;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Util.Ex;
using Util.Generator;

namespace Netlibs.Test {
    [TestClass]
    public class UnitTest1 {
        [TestMethod]
        public void m61() {
            var root="../../../../../apps/codev/server/";
            var entitydir = $"{root}biz/Manage";
            var ctxdir = $"{root}biz/Manage";
            var sqlpath = $"{root}codetogether.Gateway/wwwroot/design/manage.sql";
            Configh.Deputy.G_BizEntityFramework(entitydir, ctxdir, sqlpath);
        }
        record modelx(int Id, string Name, int Grade, int OrderId);
        [TestMethod]
        public void m60() {
            var ran = new Random();
            var mlist = new[]{
                new modelx(1,"A",ran.Next(10)*10,0),
                new modelx(2,"B",ran.Next(10)*10,0),
                new modelx(3,"C",ran.Next(10)*10,1),
                new modelx(4,"A",ran.Next(10)*10,2),
                new modelx(5,"A",ran.Next(10)*10,1),
                new modelx(6,"A",ran.Next(10)*10,3),
                new modelx(7,"A",ran.Next(10)*10,3),
            };
            var test = mlist.ToList();
            var test1 = from i in test
                        let no = test.IndexOf(i)
                        group new { i.Id, i.Name, i.Grade, i.OrderId, No = no }
                        by i.OrderId;
            foreach (var i in test1) {
                System.Console.WriteLine();
                foreach (var j in i) {
                    System.Console.WriteLine(j);
                }
            }
        }
        [TestMethod]
        public void m59() {
            var ran = new Random();
            var mlist = new[]{
                new modelx(1,"A",ran.Next(10)*10,0),
                new modelx(2,"B",ran.Next(10)*10,0),
                new modelx(3,"C",ran.Next(10)*10,1),
                new modelx(4,"A",ran.Next(10)*10,2),
                new modelx(5,"A",ran.Next(10)*10,1),
                new modelx(6,"A",ran.Next(10)*10,3),
                new modelx(7,"A",ran.Next(10)*10,3),
            };
            foreach (var i in mlist) {
                System.Console.WriteLine(i);
            }
            var test1 = from i in mlist
                        group i by i.OrderId into x
                        from j in x
                        select new { x.Key, order = j.OrderId };
            foreach (var i in test1) {
                System.Console.WriteLine($"key:{i.Key},value:{i.order}");
            }
            // var test2=test1.ToDictionary(i=>new{i.Key,i.order});
            // System.Console.WriteLine("--------");
            // foreach(var i in test2){
            //     System.Console.WriteLine($"key:{i.Key},orderid:{i.Value}");
            // }
        }
        [TestMethod]
        public void m58() {
            Console.WriteLine("Starting on threadId:{0}", Thread.CurrentThread.ManagedThreadId);
            var source = Observable.Create<int>(
            o => {
                Console.WriteLine("Invoked on threadId:{0}", Thread.CurrentThread.ManagedThreadId);
                o.OnNext(1);
                o.OnNext(2);
                o.OnNext(3);
                o.OnCompleted();
                Console.WriteLine("Finished on threadId:{0}",
                Thread.CurrentThread.ManagedThreadId);
                return Disposable.Empty;
            });
            source
            //.SubscribeOn(Scheduler.ThreadPool)
            .Subscribe(
            o => Console.WriteLine("Received {1} on threadId:{0}",
            Thread.CurrentThread.ManagedThreadId,
            o),
            () => Console.WriteLine("OnCompleted on threadId:{0}",
            Thread.CurrentThread.ManagedThreadId));
            Console.WriteLine("Subscribed on threadId:{0}", Thread.CurrentThread.ManagedThreadId);

        }
        [TestMethod]
        public void m57() {
            Console.WriteLine("Starting on threadId:{0}", Thread.CurrentThread.ManagedThreadId);
            var sub = new Subject<Object>();

            sub.Subscribe(o => Console.WriteLine("Received {1} on threadId:{0}",    //???Observable??????????????????handler????????????handler thread id
                Thread.CurrentThread.ManagedThreadId,
                o));
            ParameterizedThreadStart notify = obj =>    //?????????????????????????????????????????????thread id
            {
                Console.WriteLine("OnNext({1}) on threadId:{0}",
                Thread.CurrentThread.ManagedThreadId,
                obj);
                sub.OnNext(obj);
            };
            notify(1);
            new Thread(notify).Start(2);
            new Thread(notify).Start(3);
        }
        [TestMethod]
        public void m56() {
            var path = @"F:\netenv\tools\jupyter\data\maoyan.csv";
            var datastr = System.IO.File.ReadAllText(path);
            System.Console.WriteLine(datastr);
            System.Console.WriteLine(
                string.Join(",", from i in datastr.Split(Environment.NewLine) select $"'{i.Split(',')[0]}'")
            );
            System.Console.WriteLine(
                string.Join(",", from i in datastr.Split(Environment.NewLine) select i.Split(',')[1].Replace("%", ""))
            );
        }
        [TestMethod]
        public void m55() {
            int[,] aaa = new int[4, 4]{{1,2,3,6},
                            {4,5,7,8},
                            {7,8,9,10},
                            {3,8,4,3}};

            //LYF.Math.Determinant<int> d = new Determinant<int>(4);
            LYF.Math.Determinant<int> d = new Determinant<int>(aaa);
            d.SetItem(aaa);
            Console.WriteLine("??????????????????");
            Console.WriteLine(d.ToString());
            Console.WriteLine("?????????M11???");
            Console.WriteLine(d.A(1, 1).ToString());
            Console.WriteLine("?????????M12???");
            Console.WriteLine(d.A(1, 2).ToString());
            Console.WriteLine("?????????M22???");
            Console.WriteLine(d.A(2, 2).ToString());
            Console.WriteLine("N=" + d.N);
            Console.WriteLine("??????????????????:" + d.Value.ToString());
        }
        [TestMethod]
        public void m54() {
            void m_1(object senderkk, EventArgs e) {
                System.Console.WriteLine("hello event handler");
            }
            var x = m_1;//.net6 new feature
            EventHandler x2 = m_1;//ok
            EventHandler x3 = new EventHandler(m_1);
            x3(this, EventArgs.Empty);
        }
        [TestMethod]
        public void m52() {
            Nrow_culmn.Program.Test(null);
        }
        [TestMethod]
        public void m51() {
            var x = 1_0;
            System.Console.WriteLine(x);
            System.Console.WriteLine(0b1001_0011);
            System.Console.WriteLine(0xfb_772_eac);
            System.Console.WriteLine(string.Empty.Trim());
            System.Console.WriteLine(System.Numerics.Vector2.Zero);
        }
        [TestMethod]
        public void m50() {
            var x = 10;
            var y = 5;
            var z = x < y ? x : y;
            System.Console.WriteLine(z);
            var data = new[] { 1, 3, 8, 7, 5, 22, 5, 6, 4, 9, 82 };
            System.Console.WriteLine(data.Count());
        }
        [TestMethod]
        public void m49() {
            var data = new[] { 1, 3, 8, 7, 5, 22, 5, 6, 4, 9, 82 };
            var x = data.SplitGroup(
                i => data[i.idx]
                , i => i.item % 2 != 0
            );
            System.Console.WriteLine(string.Join(",", data));
            foreach (var item in x) {
                foreach (var it in item) {
                    System.Console.Write("{0},", it);
                }
                System.Console.Write(Environment.NewLine);
            }
        }
        [TestMethod]
        public void m48() {
            var path = System.IO.Path.Combine("f:\\work\\netcorework\\tools", "template", "other", "appsettings.json");
            System.Console.WriteLine(path);
        }
        [TestMethod]
        public void m47() {
            var source = new[] { 1, 2, 3, 5, 8 };
            var querysource = source.AsQueryable();
            var temp = querysource.Where("i=>i%2!=0");
            for (var i = 0; i < temp.Count(); i++) {
                i += 1;
            }
            foreach (var i in querysource) {
                System.Console.WriteLine(i);
            }
            foreach (var i in temp) {
                System.Console.WriteLine(i);
            }
        }
        [TestMethod]
        public void m46() {
            var source = new[] { 1, 2, 3, 5, 8 };
            var querysource = source.AsQueryable();
            var result = querysource.Where("i=>i%2=0");
            foreach (var i in result) {
                System.Console.WriteLine(i);
            }
        }
        [TestMethod]
        public void m45() {
            var source1 = new[] { 1, 2, 3, 5, 8, 6, 4 };
            var source2 = new[] { 5, 7, 6, 9, 4, 2 };
            var querysource1 = source1.AsQueryable();
            var querysource2 = source2.AsQueryable();
            var tempsource = from i in querysource1
                             join j in querysource2
                             on i equals j
                             select i * j;
            var result = tempsource.Where("i=>i%2==0");
            foreach (var i in result) {
                System.Console.WriteLine(i);
            }
        }
        [TestMethod]
        public void m44() {
            var sqlpath = @"F:\work\netcorework\bofu\Bizallview.Test\db\Biz.Whtb.sql";
            var result = Util.Generator.FactoryDbCode.GenerateProtoBySql(System.IO.File.ReadAllText(sqlpath), "Whtb", "Whtb.Grpc");
            System.Console.WriteLine(result);
        }
        [TestMethod]
        public void m43() {
            var be = new BoolExpression();
            be.Entity += id => {
                return id switch {
                    2 => typeof(Employee),
                };
            };
            be.RegistryNode("i", Util.Generator.ValueType.Int);
            be.RegistryOperate(FunctionType.Equal);
            be.RegistryNode("employee", Util.Generator.ValueType.Entity, nameof(Employee.Id));
            var f = be.Compile();
            System.Console.WriteLine(f(1, new Employee { Id = 1, Name = "John" }));
            System.Console.WriteLine(f(1, new Employee { Id = 2, Name = "Smith" }));
        }
        [TestMethodAttribute]
        public void m42() {
            //???????????????????????????
            var be = new BoolExpression();
            be.RegistryNode("i", Util.Generator.ValueType.Int);
            be.RegistryOperate(FunctionType.Equal);
            be.RegistryNode(
                new Employee { Id = 1, Name = "XiaoLi" }
                , entityField: nameof(Employee.Id)
            );
            var f = be.Compile();
            System.Console.WriteLine(f(1));
            System.Console.WriteLine(f(2));
        }
        [TestMethod]
        public void m41() {
            var source = new[] { 1, 2, 3 };
            System.Console.WriteLine(
                typeof(Enumerable)
                .GetMethod(
                    nameof(Enumerable.Contains)
                    , 1
                    , new[]{
                        typeof(IEnumerable<>).MakeGenericType(Type.MakeGenericMethodParameter(0))
                        ,Type.MakeGenericMethodParameter(0)
                    }
                )
                .MakeGenericMethod(typeof(int))
                .Invoke(null, new object[] { source, 2 })
            );
        }
        [TestMethod]
        public void m40() {
            System.Console.WriteLine(
                new[] { 1, 2, 3 }.Contains(2)
            );
        }
        [TestMethod]
        public void m39() {
            var source = new[] { 1, 2, 3 };
            System.Console.WriteLine(
                typeof(Enumerable)
                // .GetMethod("Contains",new Type[]{typeof(int)})
                .GetMethods().FirstOrDefault(i => i.Name == "Contains" && i.GetParameters().Length == 2)
                .MakeGenericMethod(new[] { typeof(int) })
                .Invoke(null, new object[] { source, 2 })
            );

        }
        [TestMethod]
        public void m38() {
            var pleft = Expression.Parameter(typeof(int));
            var pright = Expression.Parameter(typeof(IEnumerable<int>));
            var lam = Expression.Lambda<Func<int, IEnumerable<int>, bool>>(
                Expression.Block(
                    Expression.Call(pright, typeof(IEnumerable<int>).GetMethod("Contains", new[] { typeof(int) })
                    , pleft)
                ), pleft, pright
            );
            var f = lam.Compile();
            System.Console.WriteLine(f(2, new[] { 1, 2, 3 }));
            System.Console.WriteLine(f(2, new[] { 1, 4, 3 }));
        }
        [TestMethod]
        public void m37() {
            System.Console.WriteLine(typeof(char).IsCollection());
            System.Console.WriteLine(typeof(Guid).IsCollection());
            System.Console.WriteLine(typeof(double).IsCollection());
            System.Console.WriteLine(typeof(string).IsCollection());
            System.Console.WriteLine(typeof(int[]).IsCollection());
            System.Console.WriteLine(typeof(Dictionary<,>).IsCollection());
        }
        [TestMethod]
        public void m36() {
            var x = 2.5f;
            var x2 = new[] { "aa" };
            var target = new BoolExpression();
            var rx = target.MapptingTo(x);
            var rx2 = target.MapptingTo(x2);
            System.Console.WriteLine(target.MapptingTo(new[] { 2.3f, 3f }));
            System.Console.WriteLine(target.MapptingTo(new List<int> { 1, 2, 3 }));
            System.Console.WriteLine(target.MapptingTo(3));
            System.Console.WriteLine(target.MapptingTo("aa"));
            System.Console.WriteLine($"{rx},{rx2}");
            System.Console.WriteLine(Activator.CreateInstance(typeof(double)));
            System.Console.WriteLine(Activator.CreateInstance(typeof(int)));
            System.Console.WriteLine(Activator.CreateInstance(typeof(decimal)));
            // System.Console.WriteLine(Activator.CreateInstance(typeof(string)));
            System.Console.WriteLine(Activator.CreateInstance(typeof(bool)));
            System.Console.WriteLine(Activator.CreateInstance(typeof(DateTime)));
            System.Console.WriteLine(Activator.CreateInstance(typeof(TimeSpan)));
        }
        public delegate string D1(int i);
        [TestMethod]
        public void m35() {
            var be = new BoolExpression();
            // be.Compile<int>();
            be.RegistryNode(3);
            be.RegistryOperate(FunctionType.LessThan);
            be.RegistryNode("i", Util.Generator.ValueType.Int);
            // be.RegistryOperate(FunctionType.Not);
            // be.Add();
            var result = be.Serialization();
            System.Console.WriteLine(result);
            var be2 = BoolExpression.Deserialization(result);
            System.Console.WriteLine(be2);
            System.Console.WriteLine(be2.Serialization());
            var x = be2.Compile();
            System.Console.WriteLine(x(2));
            System.Console.WriteLine(x(3));
            System.Console.WriteLine(x(4));
            var x2 = be2.Build();
            var x3 = x2.Compile();
            System.Console.WriteLine(x3(2));
            System.Console.WriteLine(x3(3));
            System.Console.WriteLine(x3(4));
            var x4 = be2.Build1();
            var x5 = x4.Compile();
            System.Console.WriteLine(x5(2));
            System.Console.WriteLine(x5(3));
            System.Console.WriteLine(x5(4));
        }
        [TestMethod]
        public void m34() {
            //?????????????????????
            Expression<Func<int, bool>> exp =
            i => new Random().Next(i - 5, i + 5) > i;
            var result = System.Text.Json.JsonSerializer.Serialize(exp);
            System.Console.WriteLine(result);
        }
        [TestMethod]
        public void m33() {
            var source = new List<Dictionary<string, int>>();
            source.Add(new Dictionary<string, int> {
                ["col1"] = 1,
                ["col2"] = 2,
                ["col3"] = 3,
            });
            source.Add(new Dictionary<string, int> {
                ["col1"] = 2,
                ["col2"] = 4,
                ["col3"] = 8,
            });
            var result = (from i in source select i).Sum(item => item.Sum(it => it.Value));
            System.Console.WriteLine("the result is {0}", result);
        }

        [TestMethod]
        public void m32() {
            var ins = Activator.CreateInstance(this.GetType().Assembly.GetType("Netlibs.Test.UnitTest1"));
            this.GetType().GetMethod("m31").Invoke(ins, null);
        }

        [TestMethod]
        public void m31() {
            var x = new HashSet<int>();
            x.Add(1);
            x.Add(1);
            foreach (var item in x) {
                Console.WriteLine(item);
            }
            Console.WriteLine("------------------------");
            x.Remove(1);
            foreach (var item in x) {
                Console.WriteLine(item);
            }
            var x2 = x.ToArray();
        }

        [TestMethod]
        public void m30() { }

        [TestMethod]
        public void m29() {
            var x = new Memory<int>();
            //string input = ...;
            //ReadOnlySpan<char> inputSpan = input;
            //int commaPos = input.IndexOf(',');
            //int first = int.Parse(inputSpan.Slice(0, commaPos));
            //int second = int.Parse(inputSpan.Slice(commaPos + 1));
        }

        [TestMethod]
        public void m28() {
            var x = new[] { 1, 2, 3, 4, 5 };
            var x2 = new Span<int>(x);
            x2[2] = 8;
            foreach (var item in x) {
                Console.WriteLine(item);
            }
            var x3 = x2.Slice(1, 3);
            foreach (var item in x3) {
                Console.WriteLine(item);
            }
            x3[0] = 55;
            foreach (var item in x) {
                Console.Write("{0} ", item);
            }
            foreach (var item in x3) {
                Console.Write("{0} ", item);
            }
        }

        [TestMethod]
        public void m27() {
            var x = new List<int>();
            x.AddRange(new[] { 1, 2, 3, 6, 5, 4 }); foreach (var item in x) {
                Console.WriteLine(item);
            }
            var x2 = new Span<int>(x.ToArray());
            x2[2] = 8;
            foreach (var item in x) {
                Console.WriteLine(item);
            }
        }

        [TestMethod]
        public void m26() {
            int[] sequence = Sequence(1000);

            for (int start = 0; start < sequence.Length; start += 100) {
                Range r = start..(start + 10);
                var (min, max, average) = MovingAverage(sequence, r);
                Console.WriteLine($"From {r.Start} to {r.End}:  \tMin: {min},\tMax: {max},\tAverage: {average}");
            }

            for (int start = 0; start < sequence.Length; start += 100) {
                Range r = ^(start + 10)..^start;
                var (min, max, average) = MovingAverage(sequence, r);
                Console.WriteLine($"From {r.Start} to {r.End}: \tMin: {min},\tMax: {max},\tAverage: {average}");
            }

            (int min, int max, double average) MovingAverage(int[] subSequence, Range range) =>
                (
                    subSequence[range].Min(),
                    subSequence[range].Max(),
                    subSequence[range].Average()
                );

            int[] Sequence(int count) =>
                Enumerable.Range(0, count).Select(x => (int)(Math.Sqrt(x) * 100)).ToArray();
        }

        [TestMethod]
        public void m25() {
            var jagged = new int[10][] {
                new int[10] {                0,                1,                2,                3,                4,                5,                6,                7,                8,                9                }, new int[10] {                10,                11,                12,                13,                14,                15,                16,                17,                18,                19                },                new int[10] {                20,                21,                22,                23,                24,                25,                26,                27,                28,                29                },                new int[10] {                30,                31,                32,                33,                34,                35,                36,                37,                38,                39                },                new int[10] {                40,                41,                42,                43,                44,                45,                46,                47,                48,                49                },                new int[10] {                50,                51,                52,                53,                54,                55,                56,                57,                58,                59                },                new int[10] {                60,                61,                62,                63,                64,                65,                66,                607,                68,                69                },                new int[10] {                70,                71,                72,                73,                74,                75,                76,                77,                78,                79                },                new int[10] {                80,                81,                82,                83,                84,                85,                86,                87,                88,                89                },                new int[10] {                90,                91,                92,                93,                94,                95,                96,                97,                98,                99                },            }; var selectedRows = jagged[3..^3];

            foreach (var row in selectedRows) {
                var selectedColumns = row[2..^2];
                foreach (var cell in selectedColumns) {
                    Console.Write($"{cell}, ");
                }
                Console.WriteLine();
            }
        }

        [TestMethod]
        public void m24() {
            var x = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var x2 = x[1..3];
            Console.WriteLine(x2);
            foreach (var item in x2) {
                Console.WriteLine(item);
            }
            foreach (var item in x) {
                Console.WriteLine(item);
            }
            x2[0] = x2[0] + 100;
            foreach (var item in x2) {
                Console.WriteLine(item);
            }
            foreach (var item in x) {
                Console.WriteLine(item);
            }
        }
        [TestMethod]
        public void m23() {
            //????????????????????ExpressionVisitor?????? override 
            var nsps = new CsNamespace();
            var c = nsps.StartClass("ExprVisitor", inhlist: "ExpressionVisitor");
            var t = typeof(ExpressionVisitor);
            foreach (var item in
                    from i in t.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                    where i.IsVirtual
                    select i) {
                var m = c.StartMethod(item.Name,
                    string.Join(",",
                        from i in item.GetParameters() select $"{i.ParameterType.Name} {i.Name}"), item.ReturnType.Name, visit: "protected override");
                m.Sentence($"Console.WriteLine(\"{item.Name}:{{0}}\", node)");
                m.Sentence("return node");
            }
            Configh.GenerateCodeSnippet(nsps, @"F:\work\netcorework\core\Netlibs.Test\temp.txt", "test");
        }

        [TestMethod]
        public void m22() {
            Console.WriteLine(typeof(IEnumerable<>).IsAssignableFrom(typeof(List<string>)));
            Console.WriteLine(typeof(IEnumerable<string>).IsAssignableFrom(typeof(List<string>)));
            Console.WriteLine(typeof(IEnumerable).IsAssignableFrom(typeof(List<string>)));
            Console.WriteLine(typeof(IEnumerable<string>).GetGenericTypeDefinition().Name);
            Console.WriteLine(typeof(List<string>).GetGenericTypeDefinition().Name);
            Console.WriteLine(typeof(IEnumerable<string>).GetGenericTypeDefinition() == typeof(IEnumerable<>));
            Console.WriteLine(typeof(IDictionary<string, int>).GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }

        [TestMethod]
        public void m21() {
            var a = 1;
            var b = 2;
            var x = (a, b) switch {
                (1, 2) => "aaa",
                (2, 2) => "bbb",
                _ => null,
            };
            Console.WriteLine(x);

        }

        [TestMethod]
        public void m20() {
            //switch??????????????
            var x = 2;
        }

        [TestMethod]
        public void m19() {
            var t1 = new Test1.TestFor();
            var t2 = new Test1.TestForeach();
            Console.WriteLine("TestFor");
            t1.test();
            Console.WriteLine("TestForeach");
            t2.test();
        }

        [TestMethod]
        public void m18() {
            // var hw = new Hwriter();
            // var x = hw.ParenthesisHandle(".cuba>div>(a>b+c)^(a+d>(b+e))");
            // Console.WriteLine(x);
        }

        [TestMethod]
        public void m17() {
            Console.WriteLine(Regex.IsMatch("38920s2", @"\w+"));
            Console.WriteLine(Regex.IsMatch("s38920s2", @"\w+"));
            Console.WriteLine(Regex.IsMatch("asdfkjdasf", @"\w+"));
            Console.WriteLine(Regex.IsMatch("hasdflk2", @"\w+"));
            Console.WriteLine(Regex.IsMatch("2lasdkfj", @"\w+"));
            Console.WriteLine(Regex.IsMatch("38920s2", @"\w+"));
            Console.WriteLine(Regex.IsMatch("ewlk899", @"\w+"));
            Console.WriteLine(Regex.IsMatch("109238", @"\w+"));
        }

        [TestMethod]
        public void m16() {
            var hc = new HttpClient {
                BaseAddress = new Uri("http://localhost:5000/home/")
            };
            var dic = new Dictionary<string, string> {
                ["content"] = "hello",
                ["_38"] = "388"
            };
            var fuec = new FormUrlEncodedContent(dic);
            //fuec.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var res = hc.PostAsync("./test3", fuec).Result;
        }

        [TestMethod]
        public void m15() {
            var hc = new HttpClient {
                BaseAddress = new Uri("http://localhost:5000/home/")
            };
            //var dic = new Dictionary<string, string> {
            //    ["content"] = "hello"
            //};
            //var fuec = new FormUrlEncodedContent(dic);
            var sc = new StringContent("{\"content\":\"aab\"}");
            sc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var res = hc.PostAsync("./test2", sc).Result;
        }

        [TestMethod]
        public void m14() {
            var hc = new HttpClient {
                BaseAddress = new Uri("http://localhost:5000/home/")
            };
            var dic = new Dictionary<string, string> {
                ["content"] = "hello"
            };

            var fuec = new FormUrlEncodedContent(dic);
            var sc = new StringContent("aaa");
            sc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/json");
            var res = hc.PostAsync("./test", sc).Result;
            Console.WriteLine(res.Content.ReadAsStringAsync().Result);
        }

        [TestMethod]
        public void m13() {
            var url = "http://s14.sinaimg.cn/bmiddle/4d367ff5xa6e552e844ad&amp;690";
            var hc = new HttpClient();
            var asyncer1Completed = false;
            Action asyncer1 = async () => {
                await Task.Delay(0);
                var res = await hc.GetAsync(url);
                //hc.PostAsync()
                if (!res.IsSuccessStatusCode) return;
                //var imgstream = await res.Content.ReadAsStreamAsync();
                var content = await res.Content.ReadAsByteArrayAsync();
                //var img=Image.FromStream(imgstream);
                //using (var fs=File.Open("d:/temp/xx.png",FileMode.OpenOrCreate,FileAccess.Write)) {
                //    img.Save(fs,ImageFormat.Png);
                //}
                var bytecontent = new ByteArrayContent(content);
                bytecontent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(Mime.bytes);
                var res2 = await hc.PostAsync("http://localhost:5000/home/localimage", bytecontent);

                asyncer1Completed = true;
            };
            asyncer1.Invoke();
            while (!asyncer1Completed) {
                Thread.Sleep(100);
            }
            Console.WriteLine("complate");
        }

        [TestMethod]
        public void m12() {
            var path = @"D:\work\netcorework\netcore\Netlibs.Test\temp.txt";
            var txt = File.ReadAllText(path);
            var xdoc = XDocument.Parse(txt);
            var sb = new StringBuilder();
            sb.AppendLine("banks:[");
            foreach (var item in xdoc.Root.XPathSelectElements("//tbody/tr")) {
                //System.Console.WriteLine(item.Value);
                var name = item.XPathSelectElement("./td[@style='width:132px;']")?.Value;
                if (string.IsNullOrEmpty(name)) {
                    continue;
                }
                var abbreviation = item.XPathSelectElement("./td[@style='width:108px;']")?.Value;
                //System.Console.WriteLine($"{name},{abbreviation}");
                sb.AppendLine($"{{name:{name},abbreviation:{abbreviation ?? "none"}}},");
                var imgurl = item.XPathSelectElement("./td[@style='width:176px;']").Element("img").Attribute("src").Value;
                //Tool.getimg(imgurl, @"D:\work\git\w29072019proj\server\w29072019.Gateway\wwwroot\res\imgs\bankicon", name);
            }
            sb.AppendLine("]");
            Console.WriteLine(sb.ToString());
        }

        [TestMethod]
        public void m11() {
            var _deviceno = $"0x";
            var no1 = 0x_38_fe;
            var no2 = 133_5_33_234;
            System.Console.WriteLine(no1);
            System.Console.WriteLine(no2);
        }

        [TestMethod]
        public void build_dll_w29072019_ref() {
            Tool.UpdateFileTo(@"D:\work\git\w29072019proj\ref");
        }
    }
}
namespace Test1 {
    delegate void Func();
    public class TestFor {
        public void test() {
            var l = new List<Func>();
            for (var i = 0; i < 5; i++) {
                var j = i;
                l.Add(() => {
                    Console.WriteLine(j);
                });
            }

            for (var i = 0; i < 5; i++) {
                l[i]();
            }
        }
    }

    public class TestForeach {
        public void test() {
            var l = new List<Func>();
            int[] a = { 0, 1, 2, 3, 4 };
            foreach (var i in a) {
                l.Add(() => {
                    Console.WriteLine(i);
                });
            }

            for (var i = 0; i < 5; i++) {
                l[i]();
            }
        }
    }

}

namespace Nrow_culmn {
    public class Program {
        //??????????????? ??????????????????O(n???3??????)
        public static double jlength = 0;
        static public void Test(string[] args) {
            //double[,] row_culmn = { { 3, 1, -1, 1 }, { 1, -1, 1, 2 }, { 2, 1, 2, -1 }, { 1, 0, 2, 1, } };
            //?????????????????????
            double[,] row_culmn = { { 1, 4, 9, 16, 8 }, { 4, 9, 16, 25, 4 }, { 9, 16, 25, 36, 0 }, { 16, 25, 36, 49, 10 }, { 3, 15, 3, 69, 11 } };

            //????????????????????????
            jlength = Math.Sqrt(row_culmn.Length);

            Console.WriteLine("?????????????????????");
            for (int i = 0; i < jlength; i++) {
                for (int j = 0; j < jlength; j++) {
                    Console.Write(row_culmn[i, j].ToString() + "  ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            int row = 0;//??? ?????????????????????0??????

            int rowup = 1;//???????????????????????????????????????

            for (row = 0; row < jlength - 1; row++) {
                //???????????????????????????????????????????????????0
                ValueRow_Culmn(ref row_culmn, ref row, rowup);
                rowup++;
            }
            //?????????????????????double?????????????????????0 ???????????????????????????
            double a = 1;
            for (int i = 0; i < jlength; i++) {
                Console.WriteLine("???" + (i + 1) + "??? ???" + (i + 1) + "???" + row_culmn[i, i]);
                a *= row_culmn[i, i];
            }
            //???????????????
            Console.WriteLine("????????????");
            Console.WriteLine(string.Format("{0:F}", a));
        }

        public static void ValueRow_Culmn(ref double[,] rc, ref int row, int rowup) {
            //double jlength = Math.Sqrt(rc.Length);
            double k;//?????????????????????
            if (rowup < jlength) {
                //???????????????????????????i?????????i-1??????
                k = -rc[rowup, row] / rc[row, row];
                //?????????????????? ?????????i?????????
                for (int j = 0; j < jlength; j++) {
                    rc[rowup, j] += rc[row, j] * k;
                }

                Console.WriteLine();
                Console.WriteLine();
                //??????????????????????????????

                for (int m = 0; m < jlength; m++) {
                    for (int j = 0; j < jlength; j++) {
                        Console.Write(rc[m, j].ToString() + "  ");
                    }
                    Console.WriteLine();
                }

                Console.WriteLine();
                //???????????????
                rowup++;
                //????????????????????????
                ValueRow_Culmn(ref rc, ref row, rowup);
            } else { return; }
        }



    }
}
namespace LYF.Math {
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    /// <summary>
    /// ????????? Determinant
    /// </summary>
    [SerializableAttribute]
    [ComVisibleAttribute(true)]
    public class Determinant<T> where T : IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T> {
        T[,] tarr = null;
        public Determinant(int n) {
            tarr = new T[n, n];
        }

        public Determinant(T[,] arrT) {
            if (arrT == null || arrT.GetLength(0) != arrT.GetLength(1) || arrT.GetLength(0) < 1) {
                throw new MathException("?????????????????????????????????????????????????????????1???");
            } else {
                tarr = new T[arrT.GetLength(0), arrT.GetLength(0)];
                SetItem(arrT);
            }
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public T this[int i, int j] {
            //??????????????????get??????
            get {
                return GetItem(i, j);
            }

            //??????????????????set??????
            set {
                SetItem(i, j, value);
            }
        }

        /// <summary>
        /// ????????????????????????
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public Determinant<T> A(int i, int j) {
            if (N == 1) {
                return null;
            } else if (i > N || j > N) {
                return null;
            } else {
                Determinant<T> a = new Determinant<T>(N - 1);
                for (int m = 1; m <= N - 1; m++) {
                    for (int n = 1; n <= N - 1; n++) {
                        int p = m, q = n;
                        if (p >= i) {
                            p = m + 1;
                        }
                        if (q >= j) {
                            q = n + 1;
                        }
                        a[m, n] = this[p, q];
                    }
                }
                return a;
            }
        }


        /// <summary>
        /// ?????????????????????
        /// </summary>
        /// <param name="i">????????????1?????????</param>
        /// <param name="j">????????????1?????????</param>
        /// <param name="value">???</param>
        public void SetItem(int i, int j, T value) {
            if (tarr == null) {
                throw new MathException("???????????????????????????");
            } else if (i > N || j > N) {
                throw new MathException("???????????????????????????");
            } else {
                tarr[i - 1, j - 1] = value;
            }
        }

        public void SetItem(T[,] arrT) {
            if (arrT == null || tarr == null) {
                throw new MathException("????????????");
            } else if (arrT.GetLength(0) != N || arrT.GetLength(1) != N) {
                throw new MathException("??????????????????");
            } else {
                for (int m = 0; m <= N - 1; m++) {
                    for (int n = 0; n <= N - 1; n++) {
                        this[m + 1, n + 1] = arrT[m, n];
                    }
                }
            }
        }

        /// <summary>
        /// ?????????????????????
        /// </summary>
        /// <param name="i">????????????1?????????</param>
        /// <param name="j">????????????1?????????</param>
        /// <param name="value">???</param>
        public T GetItem(int i, int j) {
            if (tarr == null) {
                throw new MathException("???????????????????????????");
            } else if (i > N || j > N) {
                throw new MathException("???????????????????????????");
            } else {
                return tarr[i - 1, j - 1];
            }
        }

        /// <summary>
        /// ?????????????????????
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            StringBuilder sbRs = new StringBuilder();
            if (tarr != null) {
                for (int m = 0; m <= N - 1; m++) {
                    for (int n = 0; n <= N - 1; n++) {
                        sbRs.Append(string.Format("{0}\t", tarr[m, n]));
                    }
                    sbRs.Append("\n");
                }

            }
            return sbRs.ToString();
        }

        /// <summary>
        /// ????????????????????????
        /// </summary>
        public int N {
            get {
                if (tarr != null) {
                    return tarr.GetLength(0);
                } else {
                    return 0;
                }
            }

        }


        private string typeName = string.Empty;
        // private string GetType() {
        //     if (string.IsNullOrEmpty(typeName)) {
        //         typeName = typeof(T).Name;
        //         File.AppendAllText("E:\\op.txt", typeName);
        //     }
        //     return typeName;

        // }

        /// <summary>
        /// ?????????????????????
        /// </summary>
        public T Value {
            get {
                if (N == 1) {
                    return tarr[0, 0];
                } else if (N == 2) {
                    return Minus(MUL(tarr[0, 0], tarr[1, 1]), MUL(tarr[0, 1], tarr[1, 0]));
                } else {
                    T sum = default(T);
                    for (int i = 1; i <= N; i++) {
                        if ((1 + i) % 2 == 0) {
                            //???????????????
                            sum = Add(sum, MUL(this[1, i], this.A(1, i).Value));
                        } else {
                            //???????????????
                            sum = Minus(sum, MUL(this[1, i], this.A(1, i).Value));
                        }
                    }
                    return sum;
                }

            }
        }

        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private T Add(T left, T right) {
            switch (Type.GetTypeCode(typeof(T))) {
                case TypeCode.Int16:
                return ((T)(object)((short)(object)left + (short)(object)right));
                case TypeCode.Int32:
                return ((T)(object)((int)(object)left + (int)(object)right));
                case TypeCode.Int64:
                return ((T)(object)((long)(object)left + (long)(object)right));
                case TypeCode.Single:
                return ((T)(object)((float)(object)left + (float)(object)right));
                case TypeCode.Double:
                return ((T)(object)((double)(object)left + (double)(object)right));
                case TypeCode.Decimal:
                return ((T)(object)((decimal)(object)left + (decimal)(object)right));
            }
            throw new MathException("????????????????????????");
        }

        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private T Minus(T left, T right) {
            switch (Type.GetTypeCode(typeof(T))) {
                case TypeCode.Int16:
                return ((T)(object)((short)(object)left - (short)(object)right));
                case TypeCode.Int32:
                return ((T)(object)((int)(object)left - (int)(object)right));
                case TypeCode.Int64:
                return ((T)(object)((long)(object)left - (long)(object)right));
                case TypeCode.Single:
                return ((T)(object)((float)(object)left - (float)(object)right));
                case TypeCode.Double:
                return ((T)(object)((double)(object)left - (double)(object)right));
                case TypeCode.Decimal:
                return ((T)(object)((decimal)(object)left - (decimal)(object)right));
            }
            throw new MathException("????????????????????????");
        }

        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private T MUL(T left, T right) {
            switch (Type.GetTypeCode(typeof(T))) {
                case TypeCode.Int16:
                return ((T)(object)((short)(object)left * (short)(object)right));
                case TypeCode.Int32:
                return ((T)(object)((int)(object)left * (int)(object)right));
                case TypeCode.Int64:
                return ((T)(object)((long)(object)left * (long)(object)right));
                case TypeCode.Single:
                return ((T)(object)((float)(object)left * (float)(object)right));
                case TypeCode.Double:
                return ((T)(object)((double)(object)left * (double)(object)right));
                case TypeCode.Decimal:
                return ((T)(object)((decimal)(object)left * (decimal)(object)right));
            }
            throw new MathException("????????????????????????");
        }
    }

    [Serializable]
    internal class MathException : Exception {
        public MathException() {
        }

        public MathException(string message) : base(message) {
        }

        public MathException(string message, Exception innerException) : base(message, innerException) {
        }

        protected MathException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }

}

