using Fone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Util.Ex;
namespace Netlibs.Test
{
    /// <summary>
    /// 测试类
    /// </summary>
    public class SearchInfo
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Id { get; set; }
        public string Addr { get; set; }
        public string Res { get; set; }
        public DateTime? CreateOn { get; set; }
    }
    public class Employee
    {
        [System.ComponentModel.Description("唯一标识")]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public double Salary { get; set; }
        public DateTime CreateOn { get; set; }
        public int mark;
    }
    [System.ComponentModel.Description("唯一标识")]
    class PersonModel
    {
        [System.ComponentModel.Description("唯一标识")]
        public string ID { get; set; }
        [System.ComponentModel.Description("名称")]
        public string Name { get; set; }
        [System.ComponentModel.Description("值")]
        public double Value { get; set; }
        [System.ComponentModel.Description("年齡")]
        public double Age { get; set; }
        [System.ComponentModel.Description("收入")]
        public double InCome { get; set; }
        [System.ComponentModel.Description("支出")]
        public double Pay { get; set; }
    }
    // Add the following directive to your file:
    // using System.Linq.Expressions;  
    public class SampleClass
    {
        public int AddIntegers(int arg1, int arg2)
        {
            return arg1 + arg2;
        }
    }
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class ExpressionTest
    {
        [TestMethod]
        public void m93()
        {
            //成员访问
            var entityExp = Expression.Variable(typeof(Employee));
            //输入一个值，判断是否大于测试员工的薪资
            var testSalary = Expression.Parameter(typeof(double));
            var lam = Expression.Lambda<Func<double, bool>>(
                Expression.Block(
                    new[] { entityExp },
                    Expression.Assign(
                        entityExp,

                    Expression.MemberInit(
                        Expression.New(typeof(Employee))
                        , new[]{
                            Expression.Bind(
                                typeof(Employee).GetProperty(nameof(Employee.Name))
                                ,Expression.Constant("Smith")
                            ),

                            Expression.Bind(
                                typeof(Employee).GetProperty(nameof(Employee.Code))
                                ,Expression.Constant(Guid.NewGuid().ToString())
                            )
                            ,
                            Expression.Bind(
                                typeof(Employee).GetProperty(nameof(Employee.CreateOn))
                                ,Expression.Constant(DateTime.Now)
                            )
                            ,
                            Expression.Bind(
                                typeof(Employee).GetProperty(nameof(Employee.Id))
                                ,Expression.Constant(125)
                            )
                            ,
                            Expression.Bind(
                                typeof(Employee).GetProperty(nameof(Employee.Salary))
                                ,Expression.Constant(new Random().Next(8_000,12_000)+0d)
                            )
                        }
                    )),
                    Expression.LessThan(
                        Expression.PropertyOrField(entityExp, nameof(Employee.Salary))
                        , testSalary
                    )
                )
                , testSalary
            );
            var f = lam.Compile();
            System.Console.WriteLine("about 11350? {0}", f(11_350));
            System.Console.WriteLine("about 8_792? {0}", f(8_792));
            System.Console.WriteLine("about 8_103? {0}", f(8_103));
            System.Console.WriteLine("about 10_857? {0}", f(10_857));
        }
        [TestMethod]
        public void m92()
        {
            var x1 = Expression.Parameter(typeof(IDictionary<string, double>), "px");
            var idxer1 = Expression.MakeIndex(x1,
                typeof(IDictionary<string, double>).GetProperty("Item"),
                new[] { Expression.Constant("col1", typeof(string)) });
            var idxer2 = Expression.MakeIndex(x1,
                typeof(IDictionary<string, double>).GetProperty("Item"),
                new[] { Expression.Constant("col3", typeof(string)) });

            var caller2 = Expression.Call(typeof(Console).GetMethod("WriteLine",
                new[] { typeof(double) }),
                idxer1
                );
            var x2 = Expression.Lambda<Action<IDictionary<string, double>>>(Expression.Block(idxer1, idxer2, caller2), x1).Compile();
            x2.Invoke(
                new Dictionary<string, double>
                {
                    ["col1"] = 0.01d,
                    ["col2"] = 0.22d,
                    ["col3"] = 0.33d,
                }
                );
        }
        [TestMethod]
        public void m91()
        {
            var p1 = Expression.Parameter(typeof(double), "p1");
            var p2 = Expression.Parameter(typeof(double), "p2");
            var x = Expression.Power(p1, p2);
            var x2 = Expression.Lambda<Func<double, double, double>>(x, p1, p2);
            var x3 = x2.Compile();
            var x4 = x3.Invoke(1024, 1d / 10);
            Console.WriteLine(x4);
            //Math.
        }
        [TestMethod]
        public void m90()
        {
            var p1 = Expression.Parameter(typeof(double), "p1");
            var p2 = Expression.Parameter(typeof(double), "p2");
            //Math.Log()
            var logexp = Expression.Call(typeof(Math).GetMethod("Log", new[] { typeof(double), typeof(double) }), p1, p2);
            var x = Expression.Lambda<Func<double, double, double>>(logexp, p1, p2);
            var x2 = x.Compile();
            var x3 = x2.Invoke(100, 10);
            Console.WriteLine(x3);
        }
        [TestMethod]
        public void m89()
        {
            //阶乘
            // Creating a parameter expression.  
            var value = Expression.Parameter(typeof(int), "value");
            // Creating an expression to hold a local variable.   
            var result = Expression.Parameter(typeof(int), "result");
            // Creating a label to jump to from a loop.  
            var label = Expression.Label(typeof(int));
            // Creating a method body.  
            var block = Expression.Block(
                // Adding a local variable.  
                new[] { result },
                // Assigning a constant to a local variable: result = 1  
                Expression.Assign(result, Expression.Constant(1)),
                // Adding a loop.  
                Expression.Loop(
                    // Adding a conditional block into the loop.  
                    Expression.IfThenElse(
                        // Condition: value > 1  
                        Expression.GreaterThan(value, Expression.Constant(1)),
                        // If true: result *= value --  
                        Expression.MultiplyAssign(result,
                            Expression.PostDecrementAssign(value)),
                        // If false, exit the loop and go to the label.  
                        Expression.Break(label, result)
                    ),
                // Label to jump to.  
                label
                )
            );
            // Compile and execute an expression tree.  
            var factorial = Expression.Lambda<Func<int, int>>(block, value).Compile()(5);
            Console.WriteLine(factorial);
            // Prints 120.
        }
        [TestMethod]
        public void m88()
        {
            //Fone.Tool.GetTable<T> test
            var emps = new[] {
                new Employee{Id=1,Name="aaa",Code="sls",CreateOn=DateTime.Now,Salary=250d},
                new Employee{Id=2,Name="bbb",Code="skslls",CreateOn=DateTime.Now,Salary=3250d},
                new Employee{Id=3,Name="ccc",Code="2215j",CreateOn=DateTime.Now,Salary=2150d},
                new Employee{Id=4,Name="ddd",Code="jlq4ol1",CreateOn=DateTime.Now,Salary=2000d},
                new Employee{Id=5,Name="eee",Code="138134ls",CreateOn=DateTime.Now,Salary=1250d},
            };
            var dataTableExpr = Fone.Tool.GetTable<Employee>();
            var dataTableFunc = dataTableExpr.Compile();
            var table = dataTableFunc.Invoke(emps);
            foreach (DataRow item in table.Rows)
            {
                Console.WriteLine(item["Name"]);
            }
        }
        [TestMethod]
        public void m87()
        {
            //foreach
            var list = new List<int>() { 10, 20, 30 };
            var collection = Expression.Variable(typeof(List<int>), "collection");
            var loop = collection.Foreach<int>(i =>
                Expression.Call(typeof(Console).GetMethod("WriteLine", new[] { typeof(int) }), i)
            );
            var results = Expression.Lambda<Action<List<int>>>(loop, collection).Compile();
            results(list);
        }
        [TestMethod]
        public void m86()
        {
            //注意和ArrayIndex的区别
            var s = Expression.Parameter(typeof(IList<string>));
            var indexer = Expression.MakeIndex(s,
                            typeof(IList<string>).GetProperty("Item"),
                            new[] { Expression.Constant(0) }
                        );
            var indexer2 = Expression.MakeIndex(s,
                         typeof(IList<string>).GetProperty("Item"),
                         new[] { Expression.Constant(1) }
                     );
            var f = Expression.Lambda<Func<IList<string>, string>>(
                Expression.Block(
                    Expression.Assign(
                        indexer,
                        Expression.Constant("you just fadded away")
                        ),
                    indexer2
                    ),
                    s
                ).Compile();
            var m = new string[] { "", "A Place Nearby" };
            Console.WriteLine(f(m));
            /*对象索引器则并没有什么变动 原先是怎么访问数组的就怎么去设置数组 相对于Array它相对的比较统一 
             * 如何要访问或设置对象索引器的元素 必须通过“MakeIndex”*/
        }
        [TestMethod]
        public void m85()
        {
            //数组访问
            /*从上面中的代码中看到设置数组成员代码与上面的代码基本都大同小易 
             * 但也有一些变动设置数组元素是通过“Assign & ArrayAccess”完成的 为什么不通过“ArrayIndex”呢？
             * 这是因为ArrayIndex只允许访问索引位置的成员 而不允许设置其索引位置成员的内容但ArrayAccess
             * 则不存在这些限制 ArrayAccess它可以设置也可以访问、
            */
            var s = Expression.Parameter(typeof(string[]));
            var f = Expression.Lambda<Action<string[]>>(
                    Expression.Assign(
                        Expression.ArrayAccess(s, Expression.Constant(0)),
                        Expression.Constant("you just fadded away")
                    ),
                    s
                ).Compile();
            var m = new string[] { "A Place Nearby" };
            f(m);
            Console.WriteLine(m[0]);
        }
        [TestMethod]
        public void m84()
        {
            //condition
            // Add the following directive to your file:
            // using System.Linq.Expressions; 
            var num = 100;
            // This expression represents a conditional operation. 
            // It evaluates the test (first expression) and
            // executes the iftrue block (second argument) if the test evaluates to true, 
            // or the iffalse block (third argument) if the test evaluates to false.
            Expression conditionExpr = Expression.Condition(
                                       Expression.Constant(num > 10),
                                       Expression.Constant("num is greater than 10"),
                                       Expression.Constant("num is smaller than 10")
                                     );
            // Print out the expression.
            Console.WriteLine(conditionExpr.ToString());
            // The following statement first creates an expression tree,
            // then compiles it, and then executes it.       
            Console.WriteLine(
                Expression.Lambda<Func<string>>(conditionExpr).Compile()());
            // This code example produces the following output:
            //
            // IIF("True", "num is greater than 10", "num is smaller than 10")
            // num is greater than 10
        }
        [TestMethod]
        public void m83()
        {
            //convert
            // Add the following directive to your file:
            // using System.Linq.Expressions;  
            // This expression represents a type conversion operation. 
            Expression convertExpr = Expression.Convert(
                                        Expression.Constant(5.5),
                                        typeof(Int16)
                                    );
            // Print out the expression.
            Console.WriteLine(convertExpr.ToString());
            // The following statement first creates an expression tree,
            // then compiles it, and then executes it.
            Console.WriteLine(Expression.Lambda<Func<Int16>>(convertExpr).Compile()());
            // This code example produces the following output:
            //
            // Convert(5.5)
            // 5
        }
        [TestMethod]
        public void m82()
        {
            //equal
            // Add the following directive to your file:
            // using System.Linq.Expressions;  
            // This expression compares the values of its two arguments.
            // Both arguments need to be of the same type.
            Expression equalExpr = Expression.Equal(
                Expression.Constant(42),
                Expression.Constant(45)
            );
            // Print out the expression.
            Console.WriteLine(equalExpr.ToString());
            // The following statement first creates an expression tree,
            // then compiles it, and then executes it.
            Console.WriteLine(
                Expression.Lambda<Func<bool>>(equalExpr).Compile()());
            // This code example produces the following output:
            //
            // (42 == 45)
            // False
        }
        [TestMethod]
        public void m81()
        {
            //field
            var obj = new Employee { mark = 22 };
            // This expression represents accessing a field.
            // For static fields, the first parameter must be null.
            Expression fieldExpr = Expression.Field(
                Expression.Constant(obj),
                "mark"
            );
            // The following statement first creates an expression tree,
            // then compiles it, and then runs it.
            Console.WriteLine(Expression.Lambda<Func<int>>(fieldExpr).Compile()());
        }
        [TestMethod]
        public void m80()
        {
            //lessthan
            // Add the following directive to your file:
            // using System.Linq.Expressions;  
            // This expression compares the values of its two arguments.
            // Both arguments must be of the same type.
            Expression lessThanExpr = Expression.LessThan(
                Expression.Constant(42),
                Expression.Constant(45)
            );
            // Print out the expression.
            Console.WriteLine(lessThanExpr.ToString());
            // The following statement first creates an expression tree,
            // then compiles it, and then executes it.    
            Console.WriteLine(
                Expression.Lambda<Func<bool>>(lessThanExpr).Compile()());
            // This code example produces the following output:
            //
            // (42 < 45)
            // True
        }
        [TestMethod]
        public void m79()
        {
            // and assigns 10 to its sample property.
            Expression testExpr = Expression.MemberInit(
                Expression.New(typeof(Employee)),
                new List<MemberBinding>() {
            Expression.Bind(typeof(Employee).GetProperty("Name"), Expression.Constant("Simth"))
                }
            );
            // The following statement first creates an expression tree,
            // then compiles it, and then runs it.
            System.Console.WriteLine(testExpr.ToString());
            var test = Expression.Lambda<Func<Employee>>(testExpr).Compile()();
            Console.WriteLine(test.Name);
        }
        [TestMethod]
        public void m78()
        {
            //orelse
            // Add the following directive to your file:
            // using System.Linq.Expressions;  
            // This expression perfroms a logical OR operation
            // on its two arguments, but if the first argument is true,
            // then the second arument is not evaluated.
            // Both arguments must be of the boolean type.
            Expression orElseExpr = Expression.OrElse(
                Expression.Constant(false),
                Expression.Constant(true)
            );
            // Print out the expression.
            Console.WriteLine(orElseExpr.ToString());
            // The following statement first creates an expression tree,
            // then compiles it, and then executes it. 
            Console.WriteLine(Expression.Lambda<Func<bool>>(orElseExpr).Compile().Invoke());
            // This code example produces the following output:
            //
            // (False OrElse True)
            // True
            System.Console.WriteLine(false ^ true);
        }
        [TestMethod]
        public void m77()
        {
            // Create a TypeBinaryExpression that represents a
            // type test of the string "spruce" against the 'int' type.
            var typeBinaryExpression =
                System.Linq.Expressions.Expression.TypeIs(
                    System.Linq.Expressions.Expression.Constant("spruce"),
                    typeof(int));
            Console.WriteLine(typeBinaryExpression.ToString());
            // This code produces the following output:
            //
            // ("spruce" Is Int32)
            var lam = Expression.Lambda<Func<bool>>(
                typeBinaryExpression
            );
            System.Console.WriteLine(lam.Compile()());//flase
        }
        [TestMethod]
        public void m76()
        {
            //box unbox
            // Create a UnaryExpression that represents a
            // conversion of an int to an int?.
            var typeAsExpression =
                System.Linq.Expressions.Expression.TypeAs(
                    System.Linq.Expressions.Expression.Constant(34, typeof(int)),
                    typeof(int?));
            Console.WriteLine(typeAsExpression.ToString());
            // This code produces the following output:
            //
            // (34 As Nullable`1)
        }
        [TestMethod]
        public void m75()
        {
            //switch again
            // Add the following directive to the file:
            // using System.Linq.Expressions;  
            // An expression that represents the switch value.
            var switchValue = Expression.Constant(3);
            // This expression represents a switch statement 
            // that has a default case.
            var switchExpr =
                Expression.Switch(
                    switchValue,
                    Expression.Call(
                                null,
                                typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                                Expression.Constant("Default")
                            ),
                    new SwitchCase[] {
            Expression.SwitchCase(
                Expression.Call(
                    null,
                    typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                    Expression.Constant("First")
                ),
                Expression.Constant(1)
            ),
            Expression.SwitchCase(
                Expression.Call(
                    null,
                    typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                    Expression.Constant("Second")
                ),
                Expression.Constant(2)
            )
                    }
                );
            // The following statement first creates an expression tree,
            // then compiles it, and then runs it.
            Expression.Lambda<Action>(switchExpr).Compile()();
            // This code example produces the following output:
            //
            // Default
        }
        [TestMethod]
        public void m74()
        {
            //switch 练习
            var p = Expression.Parameter(typeof(bool));
            var p2 = Expression.Constant("Locy");
            var ss = Expression.Switch(p,
                Expression.SwitchCase(
                    Expression.Call(null, typeof(Console).GetMethod("WriteLine", new[] { typeof(string), typeof(string) }), Expression.Constant("Mis:{0}"), p2)
                , Expression.Constant(true))
                ,
                Expression.SwitchCase(
                    Expression.Call(null, typeof(Console).GetMethod("WriteLine", new[] { typeof(string), typeof(string) }), Expression.Constant("Mr:{0}"), p2)
                , Expression.Constant(false))
                );
            var lam = Expression.Lambda<Action<bool>>(ss, p);
            var func = lam.Compile();
            func(true); func(false);
        }
        [TestMethod]
        public void m73()
        {
            // Add the following directive to the file:
            // using System.Linq.Expressions;  
            // An expression that represents the switch value.
            var switchValue = Expression.Constant(2);
            // This expression represents a switch statement 
            // without a default case.
            var switchExpr =
                Expression.Switch(
                    switchValue,
                    new SwitchCase[] {
            Expression.SwitchCase(
                Expression.Call(
                    null,
                    typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                    Expression.Constant("First")
                ),
                Expression.Constant(1)
            ),
            Expression.SwitchCase(
                Expression.Call(
                    null,
                    typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                    Expression.Constant("Second")
                ),
                Expression.Constant(2)
            )
                    }
                );
            // The following statement first creates an expression tree,
            // then compiles it, and then runs it.
            Expression.Lambda<Action>(switchExpr).Compile()();
            // This code example produces the following output:
            //
            // Second
        }
        [TestMethod]
        public void m72()
        {
            //return 语句
            // Add the following directive to the file:
            // using System.Linq.Expressions;  
            // A label expression of the void type that is the target for Expression.Return().
            var returnTarget = Expression.Label();
            // This block contains a GotoExpression that represents a return statement with no value.
            // It transfers execution to a label expression that is initialized with the same LabelTarget as the GotoExpression.
            // The types of the GotoExpression, label expression, and LabelTarget must match.
            var blockExpr =
                Expression.Block(
                    Expression.Call(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expression.Constant("Return")),
                    Expression.Return(returnTarget),
                    Expression.Call(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expression.Constant("Other Work")),
                    Expression.Label(returnTarget)
                );
            // The following statement first creates an expression tree,
            // then compiles it, and then runs it.
            Expression.Lambda<Action>(blockExpr).Compile()();
            // This code example produces the following output:
            //
            // Return
            // "Other Work" is not printed because 
            // the Return expression transfers execution from Expression.Return(returnTarget)
            // to Expression.Label(returnTarget).
        }
        [TestMethod]
        public void m71()
        {
            //补数表达式
            var mark = Expression.Constant(15);
            var test = Expression.OnesComplement(mark);
            var lam = Expression.Lambda<Func<int>>(test);
            var func = lam.Compile();
            Console.WriteLine(func());
            System.Console.WriteLine(~15);
        }
        [TestMethod]
        public void m70()
        {
            //下面的示例演示如何创建表示逻辑 "非" 运算的表达式。
            // Add the following directive to your file:
            // using System.Linq.Expressions; 
            // This expression represents a NOT operation.
            Expression notExpr = Expression.Not(Expression.Constant(true));
            Console.WriteLine(notExpr);
            // The following statement first creates an expression tree,
            // then compiles it, and then runs it.
            Console.WriteLine(Expression.Lambda<Func<bool>>(notExpr).Compile()());
            // This code example produces the following output:
            //
            // Not(True)
            // False
        }
        [TestMethod]
        public void m69()
        {
            //下面的示例演示如何使用 NewArrayInit 方法创建表示创建一维字符串数组的表达式树，该数组是使用字符串表达式列表进行初始化的。
            var trees =
    new List<System.Linq.Expressions.Expression>()
        { System.Linq.Expressions.Expression.Constant("oak"),
          System.Linq.Expressions.Expression.Constant("fir"),
          System.Linq.Expressions.Expression.Constant("spruce"),
          System.Linq.Expressions.Expression.Constant("alder") };
            // Create an expression tree that represents creating and  
            // initializing a one-dimensional array of type string.
            var newArrayExpression =
                System.Linq.Expressions.Expression.NewArrayInit(typeof(string), trees);
            // Output the string representation of the Expression.
            Console.WriteLine(newArrayExpression.ToString());
            // This code produces the following output:
            //
            // new [] {"oak", "fir", "spruce", "alder"}
            var lam = Expression.Lambda<Func<string[]>>(newArrayExpression);
            var func = lam.Compile();
            foreach (var item in func())
            {
                Console.WriteLine(item);
            }
        }
        [TestMethod]
        public void m68()
        {
            //创建指定秩的数组
            // Create an expression tree that represents creating a 
            // two-dimensional array of type string with bounds [3,2].
            var ilen = System.Linq.Expressions.Expression.Constant(3, typeof(int));
            var jlen = System.Linq.Expressions.Expression.Constant(2, typeof(int));
            var newArrayExpression =
                System.Linq.Expressions.Expression.NewArrayBounds(
                        typeof(string),
                        ilen,
                        jlen);
            // Output the string representation of the Expression.
            Console.WriteLine(newArrayExpression.ToString());
            // This code produces the following output:
            var i = Expression.Variable(typeof(int));
            var j = Expression.Variable(typeof(int));
            var x = Expression.Variable(typeof(string[,]));
            var label1 = Expression.Label();
            var label2 = Expression.Label();
            var lam1 = Expression.Lambda<Func<string[,]>>(
                Expression.Block(
                    new[] { i, j, x },
                    Expression.Assign(x, newArrayExpression),
                    Expression.Assign(i, Expression.Constant(0)),
                    Expression.Assign(j, Expression.Constant(0)),
                    Expression.Loop(
                        Expression.IfThenElse(
                            Expression.LessThan(i, ilen),
                            Expression.Block(
                                Expression.Loop(
                                    Expression.IfThenElse(
                                        Expression.LessThan(j, jlen),
                                        Expression.Block(
                                            Expression.Assign(
                                                Expression.ArrayAccess(x, i, j)
                                                , Expression.Call(
                                                    Expression.Multiply(i, j),
                                                    typeof(int).GetMethod("ToString", new Type[] { }))
                                            ),
                                            Expression.PostIncrementAssign(j)
                                        ),
                                        Expression.Break(label2)
                                    ),
                                    label2
                                ),
                                Expression.Assign(j, Expression.Constant(0)),
                                Expression.PostIncrementAssign(i)
                            ),
                            Expression.Break(label1)
                        ),
                        label1
                    )
                    , x
                )
            );
            // System.Console.WriteLine(lam1);
            var x2 = lam1.Compile()();
            for (int k1 = 0; k1 < x2.GetLength(0); k1++)
            {
                for (int k2 = 0; k2 < x2.GetLength(1); k2++)
                {
                    System.Console.WriteLine(x2[k1, k2]);
                }
                System.Console.WriteLine();
            }
        }
        [TestMethod]
        public void m67()
        {
            //算术求反
            // Add the following directive to your file:
            // using System.Linq.Expressions; 
            // This expression represents a negation operation.
            Expression negateExpr = Expression.Negate(Expression.Constant(5));
            // The following statement first creates an expression tree,
            // then compiles it, and then runs it.
            Console.WriteLine(Expression.Lambda<Func<int>>(
                Expression.Add(negateExpr, Expression.Constant(5))
            ).Compile()());
            // This code example produces the following output:
            //
            // -5
        }
        [TestMethod]
        public void m66()
        {
            //取模运算
            var x1 = Expression.Variable(typeof(int));
            var x2 = Expression.Modulo(x1, Expression.Constant(3));
            var lam = Expression.Lambda<Func<int, int>>(x2, x1);
            var func = lam.Compile();
            for (var i = 0; i < 10; i++)
            {
                Console.WriteLine(func(i));
            }
        }
        [TestMethod]
        public void m65()
        {
            // Create a BinaryExpression that represents subtracting 14 from 53.
            var binaryExpression =
                System.Linq.Expressions.Expression.MakeBinary(
                    System.Linq.Expressions.ExpressionType.Subtract,
                    System.Linq.Expressions.Expression.Constant(53),
                    System.Linq.Expressions.Expression.Constant(14));
            Console.WriteLine(binaryExpression.ToString());
            // This code produces the following output:
            //
            // (53 - 14)
        }
        /// <summary>
        /// aaa
        /// </summary>
        [TestMethod]
        public void m64()
        {
            var tree1 = "maple";
            var tree2 = "oak";
            var addMethod = typeof(Dictionary<int, string>).GetMethod("Add");
            // Create two ElementInit objects that represent the
            // two key-value pairs to add to the Dictionary.
            var elementInit1 =
                System.Linq.Expressions.Expression.ElementInit(
                    addMethod,
                    System.Linq.Expressions.Expression.Constant(tree1.Length),
                    System.Linq.Expressions.Expression.Constant(tree1));
            var elementInit2 =
                System.Linq.Expressions.Expression.ElementInit(
                    addMethod,
                    System.Linq.Expressions.Expression.Constant(tree2.Length),
                    System.Linq.Expressions.Expression.Constant(tree2));
            // Create a NewExpression that represents constructing
            // a new instance of Dictionary<int, string>.
            var newDictionaryExpression =
                System.Linq.Expressions.Expression.New(typeof(Dictionary<int, string>));
            // Create a ListInitExpression that represents initializing
            // a new Dictionary<> instance with two key-value pairs.
            var listInitExpression =
                System.Linq.Expressions.Expression.ListInit(
                    newDictionaryExpression,
                    elementInit1,
                    elementInit2);
            Console.WriteLine(listInitExpression.ToString());
            // This code produces the following output:
            //
            // new Dictionary`2() {Void Add(Int32, System.String)(5,"maple"),
            // Void Add(Int32, System.String)(3,"oak")}
            var lam = Expression.Lambda<Func<Dictionary<int, string>>>(listInitExpression);
            var dic = lam.Compile().Invoke();
            foreach (var item in dic)
            {
                Console.WriteLine("key:{0}=>value:{1}", item.Key, item.Value);
            }
        }
        [TestMethod]
        public void m63()
        {
            //表示一个将委托或 Lambda 表达式应用到一个自变量表达式列表的表达式。
            System.Linq.Expressions.Expression<Func<int, int, bool>> largeSumTest =
    (num1, num2) => (num1 + num2) > 1000;
            // Create an InvocationExpression that represents applying
            // the arguments '539' and '281' to the lambda expression 'largeSumTest'.
            var invocationExpression =
                System.Linq.Expressions.Expression.Invoke(
                    largeSumTest,
                    System.Linq.Expressions.Expression.Constant(539),
                    System.Linq.Expressions.Expression.Constant(481));
            Console.WriteLine(invocationExpression.ToString());
            var lam = Expression.Lambda<Func<bool>>(invocationExpression);
            Console.WriteLine(lam.Compile().Invoke());
            // This code produces the following output:
            //
            // Invoke((num1, num2) => ((num1 + num2) > 1000),539,281)
        }
        [TestMethod]
        public void m62()
        {
            System.Linq.Expressions.Expression<Func<int, int, bool>> largeSumTest =
    (num1, num2) => (num1 + num2) > 1000;
            // Create an InvocationExpression that represents applying
            // the arguments '539' and '281' to the lambda expression 'largeSumTest'.
            var invocationExpression =
                System.Linq.Expressions.Expression.Invoke(
                    largeSumTest,
                    System.Linq.Expressions.Expression.Constant(539),
                    System.Linq.Expressions.Expression.Constant(281));
            Console.WriteLine(invocationExpression.ToString());
            // This code produces the following output:
            //
            // Invoke((num1, num2) => ((num1 + num2) > 1000),539,281)
        }
        [TestMethod]
        public void m61()
        {
            // Add the following directive to your file:
            // using System.Linq.Expressions;  
            // This statement creates an empty expression.
            var emptyExpr = Expression.Empty();
            //空表达式可以在需要表达式的地方使用，但不需要任何操作。
            //例如，可以将空表达式用作块表达式中的最后一个表达式。
            //在这种情况下，块表达式的返回值为void。
            // The empty expression can be used where an expression is expected, but no action is desired.
            // For example, you can use the empty expression as the last expression in the block expression.
            // In this case the block expression's return value is void.
            var emptyBlock = Expression.Block(emptyExpr);
            System.Console.WriteLine(emptyBlock);
        }
        [TestMethod]
        public void m60()
        {
            //写一个没有label的loop
            var x1 = Expression.Variable(typeof(int), "x");
            var breaklabel = Expression.Label();
            var loop = Expression.Loop(
                Expression.Block(
                        Expression.IfThen(
                                Expression.GreaterThan(x1, Expression.Constant(10)),
                                Expression.Break(breaklabel)
                            ),
                        Expression.Call(
                            typeof(Console).GetMethod("WriteLine",
                             new[] { typeof(string) }),
                            Expression.Call(x1, typeof(int).GetMethod("ToString", new Type[] { }))
                        )
                        , Expression.PostIncrementAssign(x1)
                    )
                );
            var labelx = Expression.Label(breaklabel);
            var test = Expression.Block(loop, labelx);
            var lam = Expression.Lambda<Action<int>>(test, x1);
            lam.Compile().Invoke(2);
        }
        [TestMethod]
        public void m59()
        {
            //goto
            // Add the following directive to your file:
            // using System.Linq.Expressions;  
            // A label expression of the void type that is the target for the GotoExpression.
            var returnTarget = Expression.Label();
            // This block contains a GotoExpression.
            // It transfers execution to a label expression that is initialized with the same LabelTarget as the GotoExpression.
            // The types of the GotoExpression, label expression, and LabelTarget must match.
            var blockExpr =
                Expression.Block(
                    Expression.Call(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expression.Constant("GoTo")),
                    Expression.Goto(returnTarget),
                    Expression.Call(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expression.Constant("Other Work")),
                    Expression.Label(returnTarget)
                );
            // The following statement first creates an expression tree,
            // then compiles it, and then runs it.
            Expression.Lambda<Action>(blockExpr).Compile()();
            // This code example produces the following output:
            //
            // GoTo
            // "Other Work" is not printed because 
            // the GoTo expression transfers execution from Expression.GoTo(returnTarget)
            // to Expression.Label(returnTarget).
        }
        [TestMethod]
        public void m58()
        {
            // Add the following directive to your file:
            // using System.Linq.Expressions;  
            // This expression represents an exclusive OR operation for its two arguments.
            // Both arguments must be of the same type, 
            // which can be either integer or boolean.
            Expression exclusiveOrExpr = Expression.ExclusiveOr(
                Expression.Constant(5),
                Expression.Constant(3)
            );
            // Print out the expression.
            Console.WriteLine(exclusiveOrExpr.ToString());
            // The following statement first creates an expression tree,
            // then compiles it, and then executes it.           
            Console.WriteLine(
                Expression.Lambda<Func<int>>(exclusiveOrExpr).Compile()());
            // The XOR operation is performed as follows:
            // 101 xor 011 = 110
            // This code example produces the following output:
            //
            // (5 ^ 3)
            // 6
        }
        [TestMethod]
        public void m57()
        {
            //ElementInit 表示 IEnumerable 集合的单个元素的初始值设定项。
            var tree = "maple";
            var addMethod = typeof(Dictionary<int, string>).GetMethod("Add");
            // Create an ElementInit that represents calling
            // Dictionary<int, string>.Add(tree.Length, tree).
            var elementInit =
                System.Linq.Expressions.Expression.ElementInit(
                    addMethod,
                    System.Linq.Expressions.Expression.Constant(tree.Length),
                    System.Linq.Expressions.Expression.Constant(tree));
            Console.WriteLine(elementInit.ToString());
            // This code produces the following output:
            //
            // Void Add(Int32, System.String)(5,"maple")
        }
        [TestMethod]
        public void m56()
        {
            //创建一个 DefaultExpression，Type 属性设置为指定类型。
            // Add the following directive to your file:
            // using System.Linq.Expressions;  
            // This expression represents the default value of a type
            // (0 for integer, null for a string, etc.)
            Expression defaultExpr = Expression.Default(
                                        typeof(byte)
                                    );
            // Print out the expression.
            Console.WriteLine(defaultExpr.ToString());
            // The following statement first creates an expression tree,
            // then compiles it, and then executes it.
            Console.WriteLine(
                Expression.Lambda<Func<byte>>(defaultExpr).Compile()());
            // This code example produces the following output:
            //
            // default(Byte)
            // 0
        }
        [TestMethod]
        public void m55()
        {
            //创建一个表示类型转换运算的 UnaryExpression。
            // Add the following directive to your file:
            // using System.Linq.Expressions;  
            // This expression represents a type conversion operation. 
            Expression convertExpr = Expression.Convert(
                                        Expression.Constant(5.5),
                                        typeof(Int16)
                                    );
            // Print out the expression.
            Console.WriteLine(convertExpr.ToString());
            // The following statement first creates an expression tree,
            // then compiles it, and then executes it.
            Console.WriteLine(Expression.Lambda<Func<Int16>>(convertExpr).Compile()());
            // This code example produces the following output:
            //
            // Convert(5.5)
            // 5
        }
        [TestMethod]
        public void m54()
        {
            //new sb
            var vSb = Expression.Variable(typeof(StringBuilder));
            var expSb = Expression.New(typeof(StringBuilder));
            var lam1 = Expression.Lambda<Func<string>>(
                Expression.Block(
                    new[] { vSb },
                Expression.Assign(vSb, expSb),
                Expression.Call(vSb, typeof(StringBuilder).GetMethod("AppendLine", new[] { typeof(string) })
                    , Expression.Constant("aaa")
                ), Expression.Call(vSb, typeof(StringBuilder).GetMethod("AppendLine", new[] { typeof(string) })
                    , Expression.Constant("bbb")
                ),
                Expression.Call(vSb, typeof(StringBuilder).GetMethod("ToString", new Type[] { })

                )
            )
            ).Compile();
            var lam2 = Expression.Lambda<Func<string>>(
                Expression.Block(
                Expression.Call(expSb, typeof(StringBuilder).GetMethod("AppendLine", new[] { typeof(string) })
                    , Expression.Constant("ccc")
                ), Expression.Call(expSb, typeof(StringBuilder).GetMethod("AppendLine", new[] { typeof(string) })
                    , Expression.Constant("dde")
                ),
                Expression.Call(expSb, typeof(StringBuilder).GetMethod("ToString", new Type[] { })

                )
            )
            ).Compile();
            System.Console.WriteLine(lam1());
            System.Console.WriteLine(lam2());
        }
        [TestMethod]
        public void m53()
        {
            // Add the following directive to your file:
            // using System.Linq.Expressions;  
            // A label that is used by a break statement and a loop. 
            var breakLabel = Expression.Label();
            // A label that is used by the Continue statement and the loop it refers to.
            var continueLabel = Expression.Label();
            // This expression represents a Continue statement.
            Expression continueExpr = Expression.Continue(continueLabel);
            // A variable that triggers the exit from the loop.
            var count = Expression.Parameter(typeof(int));
            // A loop statement.
            Expression loopExpr = Expression.Loop(
                Expression.Block(
                    Expression.IfThen(
                        Expression.GreaterThan(count, Expression.Constant(3)),
                        Expression.Break(breakLabel)
                    ),
                    Expression.PreIncrementAssign(count),
                    Expression.Call(
                                null,
                                typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                                Expression.Constant("Loop")
                            )
                    , continueExpr
                    , Expression.PreDecrementAssign(count)
                ),
                breakLabel
                , continueLabel
            );
            // The following statement first creates an expression tree,
            // then compiles it, and then runs it.
            // Without the Continue statement, the loop would go on forever.
            Expression.Lambda<Action<int>>(loopExpr, count).Compile()(1);
            // This code example produces the following output:
            //
            // Loop
            // Loop
            // Loop
        }
        [TestMethod]
        public void m52()
        {
            // Add the following directive to your file:
            // using System.Linq.Expressions; 
            var num = 100;
            // This expression represents a conditional operation. 
            // It evaluates the test (first expression) and
            // executes the iftrue block (second argument) if the test evaluates to true, 
            // or the iffalse block (third argument) if the test evaluates to false.
            Expression conditionExpr = Expression.Condition(
                                       Expression.Constant(num > 10),
                                       Expression.Constant("num is greater than 10"),
                                       Expression.Constant("num is smaller than 10")
                                     );
            // Print out the expression.
            Console.WriteLine(conditionExpr.ToString());
            // The following statement first creates an expression tree,
            // then compiles it, and then executes it.       
            Console.WriteLine(
                Expression.Lambda<Func<string>>(conditionExpr).Compile()());
            // This code example produces the following output:
            //
            // IIF("True", "num is greater than 10", "num is smaller than 10")
            // num is greater than 10
        }
        [TestMethod]
        public void m51()
        {
            //?? operator
            var x1 = Expression.Variable(typeof(Employee), "emp");
            var emp = new Employee { Name = "Simth" };
            //Coalesce 表示 ?? 计算符
            var x2 = Expression.Coalesce(x1, Expression.Constant(emp));
            var x6 = Expression.Assign(x1, x2);
            var x3 = Expression.Parameter(typeof(Employee));
            //var x4 = Expression.Assign(x3, Expression.Constant(new Employee { Name = "Tomas" }));
            var x5 = Expression.Assign(x1, x3);
            var show = Expression.Call(null, typeof(Console).GetMethod("WriteLine", new[] { typeof(string), typeof(string) }), Expression.Constant(":{0}"), Expression.Property(x1, "Name"));
            var lam1 = Expression.Lambda(Expression.Block(new[] { x1 }, x5, x2, x6, show), x3);
            var p = new Employee { Name = "Tomas" }; ;
            Console.WriteLine(lam1.Compile().DynamicInvoke(default(Employee)));
            Console.WriteLine(lam1.Compile().DynamicInvoke(p));
        }
        [TestMethod]
        public void m50()
        {
            // Add the following directive to your file:
            // using System.Linq.Expressions;  
            // This expression represents a call to an instance method without arguments.
            Expression callExpr = Expression.Call(
                Expression.Constant("sample string"), typeof(String).GetMethod("ToUpper", new Type[] { }));
            // Print out the expression.
            Console.WriteLine(callExpr.ToString());
            // The following statement first creates an expression tree,
            // then compiles it, and then executes it.  
            Console.WriteLine(Expression.Lambda<Func<String>>(callExpr).Compile()());
            // This code example produces the following output:
            //
            // "sample string".ToUpper
            // SAMPLE STRING
        }
        [TestMethod]
        public void m49()
        {
            //callmethod
            // This expression represents a call to an instance method that has two arguments.
            // The first argument is an expression that creates a new object of the specified type.
            Expression callExpr = Expression.Call(
                Expression.New(typeof(SampleClass)),
                typeof(SampleClass).GetMethod("AddIntegers", new Type[] { typeof(int), typeof(int) }),
                Expression.Constant(1),
                Expression.Constant(2)
                );
            // Print out the expression.
            Console.WriteLine(callExpr.ToString());
            // The following statement first creates an expression tree,
            // then compiles it, and then executes it.
            Console.WriteLine(Expression.Lambda<Func<int>>(callExpr).Compile()());
            // This code example produces the following output:
            //
            // new SampleClass().AddIntegers(1, 2)
            // 3
        }
        [TestMethod]
        public void m48()
        {
            //break
            // Add the following directive to the file:
            // using System.Linq.Expressions;  
            // Creating a parameter expression.
            var value = Expression.Parameter(typeof(int), "value");
            // Creating an expression to hold a local variable. 
            var result = Expression.Parameter(typeof(int), "result");
            // Creating a label to jump to from a loop.
            var label = Expression.Label(typeof(int));
            // Creating a method body.
            var block = Expression.Block(
                new[] { result },
                Expression.Assign(result, Expression.Constant(1)),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.GreaterThan(value, Expression.Constant(1)),
                        Expression.MultiplyAssign(result,
                            Expression.PostDecrementAssign(value)),
                        Expression.Break(label, result)//循环体结束后传递出来的一个返回值
                    ),
                   label//这个label处有一个返回值是循环结束跳转时带出来的值
                )
            );
            // Compile and run an expression tree.
            var factorial = Expression.Lambda<Func<int, int>>(block, value).Compile()(5);
            Console.WriteLine(factorial);
            // This code example produces the following output:
            //
            // 120
        }
        [TestMethod]
        public void m47()
        {
            //array length
            // string[,] gradeArray = { { "chemistry", "history", "mathematics" }, { "78", "61", "82" } };
            var arr2 = new[] { 1, 2, 3, 4, 5 };
            var array1 = Expression.Constant(arr2);
            //注意ArrayLength只能用于1维数组
            var x1 = Expression.ArrayLength(array1);
            Console.WriteLine(x1);
            var lambda = Expression.Lambda<Func<int>>(x1);
            Console.WriteLine(lambda.Compile().Invoke());
        }
        [TestMethod]
        public void m46()
        {
            string[,] gradeArray = { { "chemistry", "history", "mathematics" }, { "78", "61", "82" } };
            var arrayExpression = Expression.Constant(gradeArray);
            // Create a MethodCallExpression that represents indexing
            // into the two-dimensional array 'gradeArray' at (0, 2).
            // Executing the expression would return "mathematics".
            var methodCallExpression = Expression.ArrayIndex(
                    arrayExpression, Expression.Constant(0), Expression.Constant(2));
            Console.WriteLine(methodCallExpression.ToString());
            // This code produces the following output:
            //
            // value(System.String[,]).Get(0, 2)
            var lambda = Expression.Lambda<Func<string>>(methodCallExpression);
            Console.WriteLine(lambda.Compile().Invoke());
            var ac = Expression.ArrayAccess(arrayExpression, Expression.Constant(0), Expression.Constant(2));
            var lambda2 = Expression.Lambda<Func<string>>(ac);
            System.Console.WriteLine(lambda2.Compile()());
            Console.WriteLine(ac);
        }
        [TestMethod]
        public void m45()
        {
            // Add the following directive to your file:
            // using System.Linq.Expressions;  
            // This parameter expression represents a variable that will hold the array.
            var arrayExpr = Expression.Parameter(typeof(int[]), "Array");
            // This parameter expression represents an array index.            
            var indexExpr = Expression.Parameter(typeof(int), "Index");
            // This parameter represents the value that will be added to a corresponding array element.
            var valueExpr = Expression.Parameter(typeof(int), "Value");
            // This expression represents an array access operation.
            // It can be used for assigning to, or reading from, an array element.
            Expression arrayAccessExpr = Expression.ArrayAccess(
                arrayExpr,
                indexExpr
            );
            // This lambda expression assigns a value provided to it to a specified array element.
            // The array, the index of the array element, and the value to be added to the element
            // are parameters of the lambda expression.
            var lambdaExpr = Expression.Lambda<Func<int[], int, int, int>>(
                Expression.Assign(arrayAccessExpr, Expression.Add(arrayAccessExpr, valueExpr)),
                arrayExpr,
                indexExpr,
                valueExpr
            );
            // Print out expressions.
            Console.WriteLine("Array Access Expression:");
            Console.WriteLine(arrayAccessExpr.ToString());
            Console.WriteLine("Lambda Expression:");
            Console.WriteLine(lambdaExpr.ToString());
            Console.WriteLine("The result of executing the lambda expression:");
            // The following statement first creates an expression tree,
            // then compiles it, and then executes it.
            // Parameters passed to the Invoke method are passed to the lambda expression.
            Console.WriteLine(lambdaExpr.Compile().Invoke(new int[] { 10, 20, 30 }, 0, 5));
            // This code example produces the following output:
            //
            // Array Access Expression:
            // Array[Index]
            // Lambda Expression:
            // (Array, Index, Value) => (Array[Index] = (Array[Index] + Value))
            // The result of executing the lambda expression:
            // 15
        }
        [TestMethod]
        public void m44()
        {
            var x = 3 & 5;
            Console.WriteLine(x);
            Console.WriteLine(3 | 5);
            Console.WriteLine(52 & 29);
            Console.WriteLine("{0}&{1}={2}", Convert.ToString(52, 2), Convert.ToString(29, 2), Convert.ToString(20, 2));
            var x2 = 32;
            x2 &= 17;
            Console.WriteLine(x2);
            var v1 = Expression.Variable(typeof(int), "a");
            var exp = Expression.Assign(v1
                , Expression.Constant(32));
            var exp2 = Expression.AndAssign(v1, Expression.Constant(129));
            var exp3 = Expression.Block(new[] { v1 }, exp2, v1);
            var fun = Expression.Lambda<Func<int>>(exp3).Compile();
            Console.WriteLine(fun.Invoke());
        }
        [TestMethod]
        public void m43()
        {
            // Add the following directive to your file:
            // using System.Linq.Expressions;  
            // This expression perfroms a logical AND operation
            // on its two arguments. Both arguments must be of the same type,
            // which can be boolean or integer.             
            Expression andExpr = Expression.And(
                Expression.Constant(true),
                Expression.Constant(false)
            );
            // Print out the expression.
            Console.WriteLine(andExpr.ToString());
            // The following statement first creates an expression tree,
            // then compiles it, and then executes it.       
            Console.WriteLine(Expression.Lambda<Func<bool>>(andExpr).Compile()());
            // This code example produces the following output:
            //
            // (True And False)
            // False
        }
        [TestMethod]
        public void m42()
        {
            var si = new SearchInfo();
            var ss = new Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo[] { Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo.Create(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags.None, null) };
            var exp = Expression.MakeDynamic(typeof(Func<System.Runtime.CompilerServices.CallSite, SearchInfo, SearchInfo>)
                , Microsoft.CSharp.RuntimeBinder.Binder.InvokeConstructor(Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags.None
                , typeof(ExpressionTest), ss), Expression.Constant(si));
            var r = Expression.Lambda<Func<System.Runtime.CompilerServices.CallSite, SearchInfo, SearchInfo>>(exp);
            var r2 = r.Compile();
            //Console.WriteLine(r2(si));
        }
        [TestMethod]
        public void m41()
        {
            var eo = new System.Dynamic.ExpandoObject();
            dynamic o = eo;
            o.hello = "world";
            var exp = Expression.MakeDynamic(
                    typeof(Func<System.Runtime.CompilerServices.CallSite, object, string>),
                    Microsoft.CSharp.RuntimeBinder.Binder.Convert(0, typeof(string), typeof(ExpressionTest)),
                    Expression.MakeDynamic(typeof(Func<System.Runtime.CompilerServices.CallSite, object, object>),
                        Microsoft.CSharp.RuntimeBinder.Binder.GetMember(
                            Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags.None,
                            "hello",
                            typeof(ExpressionTest),
                            new Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo[] { Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo.Create(0, null) }),
                        Expression.Constant(eo)
                    )
                );
            var oDynamic = Expression.Lambda<Func<string>>(exp);
            Console.WriteLine(oDynamic.Compile()());
        }
        [TestMethod]
        public void m40()
        {
            //解析lambda
            Expression<Func<int, int>> x = i => 2 * i;
            System.Console.WriteLine(x.ToString());
            System.Console.WriteLine(x.Body);
            System.Console.WriteLine(x.Body.NodeType);
            System.Console.WriteLine(x.Name);
            System.Console.WriteLine(x.Parameters);
            foreach (var i in x.Parameters)
            {
                System.Console.WriteLine(i.Name);
                System.Console.WriteLine(i.NodeType);
                System.Console.WriteLine(i.Type);
            }
        }
        [TestMethod]
        public void m39()
        {
            //  Message：根据表达式获取对应属性的值  
            var models = new List<PersonModel>();
            var r = new Random();
            string[] names = { "张学友", "王杰", "刘德华", "张曼玉", "李连杰", "孙悟空" };
            //  Message：构造测试数据
            for (var i = 0; i < 80; i++)
            {
                var model = new PersonModel();
                model.ID = i.ToString();
                model.Name = names[r.Next(6)];
                model.Value = r.Next(20, 100);
                model.InCome = r.Next(20, 100);
                model.Pay = r.Next(20, 100);
                model.Age = r.Next(20, 100);
                models.Add(model);
            }
            //  Message：生成自定义报表
            var dt = GetReport(models.AsQueryable(), l => l.Name, l => l.Max(k => k.InCome), l => l.Min(k => k.InCome), l => l.Sum(k => k.InCome));
            WriteTable(dt);
        }
        /// <summary>
        /// 获取汇总求和数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="groupby"></param>
        /// <param name="expressions"></param>
        /// <returns></returns>
        DataTable GetReport<T>(IQueryable<T> collection, Expression<Func<T, String>> groupby, params Expression<Func<IQueryable<T>, double>>[] expressions)
        {
            var table = new DataTable();
            //  Message：利用表达式设置列名称 
            var memberExpression = groupby.Body as MemberExpression;
            var displayName = (memberExpression.Member.GetCustomAttributes(false)[0] as System.ComponentModel.DescriptionAttribute).Description;
            table.Columns.Add(new DataColumn(displayName));
            foreach (var expression in expressions)
            {
                var dynamicExpression = expression.Body as MethodCallExpression;
                var groupName = dynamicExpression.Method.Name;
                var unaryexpression = dynamicExpression.Arguments[1] as UnaryExpression;
                var LambdaExpression = unaryexpression.Operand as LambdaExpression;
                memberExpression = LambdaExpression.Body as MemberExpression;
                displayName = (memberExpression.Member.GetCustomAttributes(false)[0] as System.ComponentModel.DescriptionAttribute).Description;
                table.Columns.Add(new DataColumn(displayName + $"({groupName})"));
            }
            //  Message：通过表达式设置数据体 
            var groups = collection.GroupBy(groupby);
            foreach (var group in groups)
            {
                //  Message：设置分组列头
                var dataRow = table.NewRow();
                dataRow[0] = group.Key;
                //  Message：设置分组汇总数据
                for (var i = 0; i < expressions.Length; i++)
                {
                    var expression = expressions[i];
                    var fun = expression.Compile();
                    dataRow[i + 1] = fun(group.AsQueryable());
                }
                table.Rows.Add(dataRow);
            }
            return table;
        }
        [TestMethod]
        public void m38()
        {
            //  Message：根据表达式获取对应属性的值  
            var models = new List<PersonModel>();
            var r = new Random();
            string[] names = { "张学友", "王杰", "刘德华", "张曼玉", "李连杰", "孙悟空" };
            //  Message：构造测试数据
            for (var i = 0; i < 80; i++)
            {
                var model = new PersonModel();
                model.ID = i.ToString();
                model.Name = names[r.Next(6)];
                model.Value = r.Next(20, 100);
                model.InCome = r.Next(20, 100);
                model.Pay = r.Next(20, 100);
                model.Age = r.Next(20, 100);
                models.Add(model);
            }
            //  Message：生成自定义报表
            var dt = GetSum(models.AsQueryable(), l => l.Name, l => l.Value, l => l.Age);
            WriteTable(dt);
        }
        public void WriteTable(DataTable dt)
        {
            var colums = string.Empty;
            foreach (DataColumn item in dt.Columns)
            {
                colums += item.ColumnName.PadRight(5, ' ') + " ";
            }
            Console.WriteLine(colums);
            foreach (DataRow item in dt.Rows)
            {
                var rows = string.Empty;
                for (var i = 0; i < dt.Columns.Count; i++)
                {
                    rows += item[i].ToString().PadRight(5, ' ') + " ";
                }
                Console.WriteLine(rows);
            }
        }
        /// <summary>
        /// 获取汇总求和数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="groupby"></param>
        /// <param name="expressions"></param>
        /// <returns></returns>
        public DataTable GetSum<T>(IQueryable<T> collection, Expression<Func<T, String>> groupby, params Expression<Func<T, double>>[] expressions)
        {
            var table = new DataTable();
            //  Message：利用表达式设置列名称
            var memberExpression = groupby.Body as MemberExpression;
            var displayName = (memberExpression.Member.GetCustomAttributes(false)[0] as System.ComponentModel.DescriptionAttribute).Description;
            table.Columns.Add(new DataColumn(displayName));
            foreach (var expression in expressions)
            {
                memberExpression = expression.Body as MemberExpression;
                displayName = (memberExpression.Member.GetCustomAttributes(false)[0] as System.ComponentModel.DescriptionAttribute).Description;
                table.Columns.Add(new DataColumn(displayName));
            }
            //  Message：通过表达式设置数据体 
            var groups = collection.GroupBy(groupby);
            foreach (var group in groups)
            {
                //  Message：设置分组列头
                var dataRow = table.NewRow();
                dataRow[0] = group.Key;
                //  Message：设置分组汇总数据
                for (var i = 0; i < expressions.Length; i++)
                {
                    var expression = expressions[i];
                    var fun = expression.Compile();
                    dataRow[i + 1] = group.Sum(fun);
                }
                table.Rows.Add(dataRow);
            }
            return table;
        }
        /// <summary>
        /// 通过Linq表达式获取成员属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Tuple<string, string> GetPropertyValue<T>(T instance, Expression<Func<T, string>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            var propertyName = memberExpression.Member.Name;
            var attributeName = "_";
            if (memberExpression.Member.CustomAttributes.Count() > 0)
                attributeName = (memberExpression.Member.GetCustomAttributes(false)[0] as DescriptionAttribute).Description;
            var property = typeof(T).GetProperties().Where(l => l.Name == propertyName).First();
            return new Tuple<string, string>(attributeName, property.GetValue(instance).ToString());
        }
        [TestMethod]
        public void m37()
        {
            //  Message：根据表达式获取对应属性的值  
            var model = new Employee();
            model.Id = 1;
            model.Name = "王杰";
            model.Salary = 90;
            var result = GetPropertyValue(model, l => l.Name);
            Console.WriteLine($"显示名称：{result.Item1}-值:{result.Item2}");
        }
        [TestMethod]
        public void m36()
        {
            //测试 poco convert
            var x = new Employee { Id = 1, Name = "wang xiao shuai", Code = "12345", CreateOn = DateTime.Now };
            var pocoConverter = Fone.Tool.GetPocoConverter<Employee, SearchInfo>().Compile();
            var x3 = pocoConverter.Invoke(x);
            Console.WriteLine(x3.Addr);
            Console.WriteLine(x3.Code);
            Console.WriteLine(x3.Id);
            Console.WriteLine(x3.Name);
            Console.WriteLine(x3.Res);
            Console.WriteLine(x3.CreateOn);
        }
        [TestMethod]
        public void m35()
        {
            var p = Expression.Parameter(typeof(String), "name");
            var newExp = Expression.New(typeof(SearchInfo));
            var bindsExp = new[]{
                Expression.Bind(typeof(SearchInfo).GetProperty("Name"),p)
　　　　　　　//这个地方还可以写 Person 类的其它属性，本示例只写了一个
            };
            var body = Expression.MemberInit(newExp, bindsExp);
            var func = Expression.Lambda<Func<String, SearchInfo>>(body, new[] { p }).Compile();
            var person = func("JRoger22");
            Console.WriteLine(person.GetType() == typeof(SearchInfo));    // True
            Console.WriteLine(person.Name);
        }
        [TestMethod]
        public void m34()
        {
            //9*9 乘法表
            var outerBreak = Expression.Label();
            var innerBreak = Expression.Label();
            var x = Expression.Variable(typeof(int), "x");
            var y = Expression.Variable(typeof(int), "y");
            var result = Expression.Variable(typeof(int), "result");
            var block = Expression.Block(
                new[] { x },
                Expression.Assign(x, Expression.Constant(1)),
                //循环
                Expression.Loop(
                    //条件判断
                    Expression.IfThenElse(
                        //如果表达式为真
                        Expression.LessThan(x, Expression.Constant(10)), // if x<10
                                                                         //为真时执行
                        Expression.Block(
                            new[] { y },
                            Expression.Assign(y, Expression.Constant(1)),
                            //内层循环
                            Expression.Loop(
                                Expression.IfThenElse(
                                    Expression.LessThanOrEqual(y, x), // if y <= x
                                                                      //为真时执行
                                    Expression.Block(
                                        new[] { result },
                                        Expression.Assign(result, Expression.Multiply(x, y)),
                                        Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(int) }), y),
                                        Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(string) }), Expression.Constant("×")),
                                        Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(int) }), x),
                                        Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(string) }), Expression.Constant("=")),
                                        Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(int) }), result),
                                        Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(string) }), Expression.Constant("\t")),
                                        Expression.PostIncrementAssign(y) // y++
                                    ),
                                    //为假时退出内层循环
                                    Expression.Break(innerBreak)
                                ),
                                innerBreak
                            ),//内层循环end
                            Expression.Call(null, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expression.Constant("")),
                            Expression.PostIncrementAssign(x) // x++
                        ),
                        //为假时执行
                        Expression.Break(outerBreak)
                        )
                , outerBreak)
            );
            Expression.Lambda<Action>(block).Compile()();
        }
        [TestMethod]
        public void m33()
        {
            //自己写个loop，从1累加到10
            var x1 = Expression.Variable(typeof(int), "i");
            var x2 = Expression.Assign(x1, Expression.Constant(1));
            //var looplabel = Expression.Label(typeof(int));
            var looplabel = Expression.Label();
            var time = Expression.Variable(typeof(int), "idx");
            var _time = Expression.Assign(time, Expression.Constant(1));
            var iftest = Expression.LessThan(time, Expression.Constant(10));
            var x3 = Expression.AddAssign(x1, Expression.Constant(1));
            var time2 = Expression.PostIncrementAssign(time);
            //var _break = Expression.Break(looplabel, time);
            var _break = Expression.Break(looplabel);
            var iftrue = Expression.Block(x3, time2);
            var iffalse = Expression.Block(_break);
            var ifthen = Expression.IfThenElse(iftest, iftrue, iffalse);
            // var loopbady = Expression.Block(ifthen);
            //var loop = Expression.Loop(loopbady, looplabel);
            var loop = Expression.Loop(ifthen, looplabel);
            //此时循环值的跳转label没带返回值 ，但下面这名包装的块语句最后一句做为返回值 ，使结果保持正确
            var exe = Expression.Block(new[] { x1, time }, loop, x1);
            var lab = Expression.Lambda<Func<int>>(exe).Compile();
            Console.WriteLine(lab.Invoke());
        }
        ///<summary>
        ///表达式返回值不一定要刻意的使用 ruturn语句，只要最后一个表达式
        ///是一个可计算出值的表达式即可
        ///</summary>
        [TestMethod]
        public void m32()
        {
            //对象成员相关
            var si = new SearchInfo();
            si.Name = "hello expression";
            var x0 = Expression.Variable(si.GetType(), "si");
            var x3 = Expression.Assign(x0, Expression.Constant(si));
            var x1 = Expression.PropertyOrField(x0, "Name");
            var x4 = Expression.Assign(x1, Expression.Constant("i'm not curently working on anything"));
            Console.WriteLine(x1.ToString());
            var x2 = Expression.Lambda<Func<string>>(
                Expression.Block(new[] { x0 }, x3, x4)
                );
            Console.WriteLine(x2.Compile().Invoke());
        }
        [TestMethod]
        public void m31()
        {
            //Loop()
            //创建一个LoopExpression表示循环语句表达式
            //示例：
            var label = Expression.Label(typeof(int));
            var x = Expression.Variable(typeof(int), "x");
            var block = Expression.Block(
                new[] { x },
                Expression.Assign(x, Expression.Constant(0)),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(
                            x,
                            Expression.Constant(10)
                        ),
                        Expression.PostIncrementAssign(x),// x++
                        Expression.Break(label, x) //将x作为标签的值
                    ),
                label
                )
            );
            var r = Expression.Lambda<Func<int>>(block).Compile()();
            Console.Write(r); // print 10
        }
        [TestMethod]
        public void m30()
        {
            //Label(Type type)
            //创建一个LabelTarget表示标签，此标签常用于退出语句块的标志，将标签作为Loop ( )方法的最后一个参数，然后在某个条件表达式中使用Expression.Break(LabelTarget )退出循环
            //此方法可接收一个可选的Type类型的参数，在循环中假设循环退出时可以将某个值输出到标签中，这样你可以在外部拿到这个值
            //参看下面的Loop语句表达式的示例，示例使用了Label ( )方法创建标签
            //Label(LabelTarget label, ParameterExpression defaultValue)
            //创建一个LabelExpression表示与LabelTarget关联的终止执行表达式，常用语在块中结合Expression.Return()方法使用
            //参看下面的Return语句表达式的示例，示例使用了Label ( )方法创建标签
            //Break()
            //创建一个GotoExpression表示退出循环，如果有嵌套循环，嵌套的循环内也得使用此方法来退出嵌套循环
            //此方法可接收一个LabelTarget和一个可选的值参，退出循环时，值参会赋给标签，以便拿到这个值
            //Return()
            //创建一个GotoExpression表示退出循环、退出方法体、退出块
            //示例：
            var x = Expression.Parameter(typeof(int));
            var label = Expression.Label(typeof(int));
            var block = Expression.Block(
                 //如果x==1
                 Expression.IfThen(
                     Expression.Equal(x, Expression.Constant(1)),
                     Expression.Block(
                         Expression.Assign(x, Expression.Constant(100)),
                         Expression.Return(label, x) //直接跳转到与label标签关联的LabelExpression表达式
                     )
                 ),
                 //如果x==2
                 Expression.IfThen(
                     Expression.Equal(x, Expression.Constant(2)),
                     Expression.Block(
                         Expression.Assign(x, Expression.Constant(200)),
                         Expression.Return(label, x) //直接跳转到与label标签关联的LabelExpression表达式
                     )
                 ),
                 //与label标签关联的LabelExpression表达式，
                 //无论两个IfThen表达式执行与否，此LabelExpressio始终会执行，
                 //如果是这样，那么label就没有默认值，所以需要为label提供一个默认值Expression.Constant(300) 
                 Expression.Label(label, Expression.Constant(300))
            );
            var r = Expression.Lambda<Func<int, int>>(block, x).Compile();

            Console.WriteLine(r(1)); //print 200
            Console.WriteLine(r(2)); //print 200
            Console.WriteLine(r(3)); //print 200
            Console.WriteLine(r(34)); //print 200
        }
        [TestMethod]
        public void m29()
        {
            //创建一个ConditionalExpression表示条件语句表达式
            //示例：
            var x = Expression.Parameter(typeof(int), "x");
            var y = Expression.Parameter(typeof(int), "y");
            var block = Expression.Block(
                new ParameterExpression[] { x, y },
                Expression.Assign(x, Expression.Constant(100000)),
                Expression.Assign(y, Expression.Constant(200)),
                Expression.IfThenElse(
                    Expression.GreaterThanOrEqual(x, y), // if ( x >= y )
                    Expression.Call(null, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expression.Constant("x>y==true")), //条件为真时执行
                    Expression.Call(null, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expression.Constant("x>y==false")) //条件为假时执行
                )
            );
            Expression.Lambda<Action>(block).Compile()(); // print x>y==true
        }
        [TestMethod]
        public void m28()
        {
            //创建一个BinaryExpression表示>=的表达式，类似的有GreaterThan ( )
            //示例：
            var x = Expression.Parameter(typeof(int), "x");
            var y = Expression.Parameter(typeof(int), "y");
            var block = Expression.Block(
                new ParameterExpression[] { x, y },
                Expression.Assign(x, Expression.Constant(100000)),
                Expression.Assign(y, Expression.Constant(200)),
                Expression.Call(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expression.Constant(Expression.LessThanOrEqual(x, y).ToString())),
                Expression.LessThanOrEqual(x, y)  // x >= y
            );
            var IsTrue = Expression.Lambda<Func<bool>>(block).Compile()();
            Console.WriteLine(IsTrue); // print true
        }
        [TestMethod]
        public void m27()
        {
            //BlockExpression是一个块语句，相当于函数，
            //如果块要接收参数，比如外部调用时传递实参，则不要在块中使用new ParameterExpression[ ] { } 声明同名的变量表达式，否则会覆盖掉参数
            //示例：
            var name = Expression.Parameter(typeof(string), "namek");
            var x = Expression.Variable(typeof(string), "x");
            var function = Expression.Block(
                new ParameterExpression[] { x }, //覆盖掉了块的参数name
                                                 // Expression.Call(null,"WriteLine",new Type[]{typeof(string)},name),
                                                 //  Expression.Call(
                                                 //  null,
                                                 //  typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }),
                                                 //  x
                                                 //  ),
                 Expression.Assign(x, name),
                //  Expression.Assign(x,name),
                // name //返回的是块内部定义的name表达式
                x
            );
            var s = Expression.Lambda<Func<string, string>>(function, name).Compile()("sam");
            Console.WriteLine(s); // print ""
            var name2 = Expression.Parameter(typeof(string), "name");
            var function2 = Expression.Block(
                name2
            );
            var s2 = Expression.Lambda<Func<string, string>>(function2, name2).Compile()("sam");
            Console.WriteLine(s2); // print sam
        }
        [TestMethod]
        public void m26()
        {
            //创建一个BlockExpression表示块表达式，此方法的最后一个参数表达式的值将会自动作为返回的结果
            //示例：块中的表达式都是一步一步的定义出来的，创建块表达式时你可以想象一下在块中写C#代码块的流程，这样你就知道下面Block ( )方法的参数（表达式）是如何创建的了
            var x = Expression.Parameter(typeof(int), "x"); // int x
            var y = Expression.Parameter(typeof(int), "y"); // int y
            var block = Expression.Block(
                new ParameterExpression[] { x, y }, // int x,int y 定义块作用域中的变量表达式
                Expression.Assign(x, Expression.Constant(100)), //x=100 定义块作用域中的赋值表达式
                Expression.Assign(y, Expression.Constant(200)), //y =200 定义块作用域中的赋值表达式
                Expression.AddAssign(x, y) // var r = x + y ，r将自动作为块的返回结果
            );
            var func = Expression.Lambda<Func<int>>(block).Compile();
            Console.WriteLine(func()); // print 300
        }
        [TestMethod]
        public void m25()
        {
            var x = Expression.Parameter(typeof(int), "x"); //表示定义参数的Expression表达式
            var y = Expression.Parameter(typeof(int), "y"); //表示定义参数的Expression表达式
            var add = Expression.Add(x, y); //表示加法运算的Expression表达式
            //将表达式封装到表示Lambda的表达式中，因为add是表示计算x+y的表达式，所以Lambda<TDelagate>中的委托应定义为Lambda<Func<int,int,int>>
            var lambdaInfo = Expression.Lambda<Func<int, int, int>>(add, new[] { x, y });
            var r = lambdaInfo.Compile()(1, 2); //执行
            Console.WriteLine(r); // print 3
        }
        /// <summary>
        ///————————————————
        ///版权声明：本文为CSDN博主「mango_love」的原创文章，遵循 CC 4.0 BY - SA 版权协议，转载请附上原文出处链接及本声明。
        ///原文链接：https://blog.csdn.net/mango_love/article/details/97616120
        /// </summary>
        [TestMethod]
        public void m24()
        {
            Expression<Func<int, int, int>> expression = (a, b) => a + b;
            var parmeter1 = expression.Parameters[0];
            var parmeter2 = expression.Parameters[1];
            var body = (BinaryExpression)expression.Body;
            var left = (ParameterExpression)body.Left;
            var right = (ParameterExpression)body.Right;
            var symbols = body.NodeType;
            var result = expression.Compile()(2, 3);
            Console.WriteLine("表达式参数1：{0}", parmeter1);
            Console.WriteLine("表达式参数2：{0}", parmeter2);
            Console.WriteLine("表达式体：{0}", body);
            Console.WriteLine("表达式左边节点：{0}", left);
            Console.WriteLine("表达式右边节点：{0}", right);
            Console.WriteLine("表达式符号：{0}", symbols);
            Console.WriteLine("表达式执行结果：{0}", result);
        }
        [TestMethod]
        public void m23()
        {
            var func = GenerateExpression(new SearchInfo { });
            var List = new List<SearchInfo>();
            List.Add(new SearchInfo() { Code = "1", Id = "1", Name = "3", Addr = "5", Res = "6" });
            List.Add(new SearchInfo() { Code = "2", Id = "2", Name = "4", Addr = "5", Res = "6" });
            List.Add(new SearchInfo() { Code = "3", Id = "3", Name = "5", Addr = "5", Res = "6" });
            List.Add(new SearchInfo() { Code = "2", Id = "4", Name = "6", Addr = "5", Res = "6" });
            List.Add(new SearchInfo() { Code = "5", Id = "5", Name = "7", Addr = "5", Res = "6" });
            List.Add(new SearchInfo() { Code = "6", Id = "6", Name = "8", Addr = "5", Res = "6" });
            List.Add(new SearchInfo() { Code = "7", Id = "7", Name = "9", Addr = "5", Res = "6" });
            List.Add(new SearchInfo() { Code = "8", Id = "8", Name = "3", Addr = "5", Res = "6" });
            var li = List.Where(func).ToList(); //8个结果
            foreach (var item in li)
            {
                Console.WriteLine(item.Name);
            }
        }
        public static Func<T, bool> GenerateExpression<T>(T searchModel) where T : class, new()
        {
            var mcList = new List<MethodCallExpression>();
            var type = searchModel.GetType();
            var parameterExpression = Expression.Parameter(type, "x");
            var pros = type.GetProperties();
            foreach (var t in pros)
            {
                var objValue = t.GetValue(searchModel, null);
                if (objValue != null)
                {
                    Expression proerty = Expression.Property(parameterExpression, t);
                    var constantExpression = Expression.Constant(objValue, t.PropertyType);
                    mcList.Add(Expression.Call(proerty, typeof(string).GetMethod("Contains"), new Expression[] { constantExpression }));
                }
            }
            if (mcList.Count == 0)
                return Expression.Lambda<Func<T, bool>>(Expression.Constant(true, typeof(bool)), new ParameterExpression[] { parameterExpression }).Compile();
            else
                return Expression.Lambda<Func<T, bool>>(MethodCall(mcList), new ParameterExpression[] { parameterExpression }).Compile();
        }
        public static Expression MethodCall<T>(List<T> mcList) where T : MethodCallExpression
        {
            if (mcList.Count == 1) return mcList[0];
            BinaryExpression binaryExpression = null;
            for (var i = 0; i < mcList.Count; i += 2)
            {
                if (i < mcList.Count - 1)
                {
                    var binary = Expression.OrElse(mcList[i], mcList[i + 1]);
                    if (binaryExpression != null)
                        binaryExpression = Expression.OrElse(binaryExpression, binary);
                    else
                        binaryExpression = binary;
                }
            }
            if (mcList.Count % 2 != 0)
                return Expression.OrElse(binaryExpression, mcList[mcList.Count - 1]);
            else
                return binaryExpression;
        }
        [TestMethod]
        public void m22()
        {
            var value = Expression.Parameter(typeof(int), "value");
            //创建一个表达式，用来存在本地变量
            var result = Expression.Parameter(typeof(int));
            //跳转标识表达式
            var label = Expression.Label(typeof(int));
            //语句块表达式
            var be = Expression.Block(
                //添加一个局部变量
                new[] { result },
                //赋值操作
                Expression.Assign(result, Expression.Constant(1)),
                //循环结构
                Expression.Loop(
                    Expression.IfThenElse(
                        //判断条件 如果value > 1
                        Expression.GreaterThan(value, Expression.Constant(1)),
                            //执行乘法操作 result *= value--;
                            Expression.MultiplyAssign(result, Expression.PostDecrementAssign(value)),
                        //退出循环跳转到Label
                        Expression.Break(label, result)),
                label));
            var r = Expression.Lambda<Func<int, int>>(be, value).Compile()(10);
            Console.WriteLine(r.ToString());
            Console.WriteLine(be.ToString());
            Console.WriteLine(r);
        }
        [TestMethod]
        public void m21()
        {
            //创建代表x的变量表达式
            var peParameter = Expression.Parameter(typeof(int), "x");
            //创建代表10 的常量表达式
            var peRight = Expression.Constant(10);
            //创建大于运算的表达式
            var be = Expression.GreaterThan(peParameter, peRight);
            //构建为表达式树
            var expression = Expression.Lambda<Func<int, bool>>(be, new ParameterExpression[] { peParameter });
            Console.WriteLine(expression.ToString());
            Console.WriteLine(expression.Compile()(20));
            Console.WriteLine(expression.Compile()(5));
        }
        [TestMethod]
        public void m20()
        {
            var i = Expression.Parameter(typeof(int), "i");
            var block = Expression.Block(
                new[] { i },
                //赋初值 i=5
                // i,
                Expression.Assign(i, Expression.Constant(5, typeof(int))),
                //i+=5 10
                Expression.AddAssign(i, Expression.Constant(5, typeof(int))),
                //i-=5 5
                Expression.SubtractAssign(i, Expression.Constant(5, typeof(int))),
               //i*=5 25
               Expression.MultiplyAssign(i, Expression.Constant(5, typeof(int))),
               //i/=5 5
               Expression.DivideAssign(i, Expression.Constant(5, typeof(int)))
               );
            Console.WriteLine(Expression.Lambda<Func<int>>(block).Compile()());
        }
        [TestMethod]
        public void m19()
        {
            var test = true;
            var codition = Expression.IfThenElse(
                //条件
                Expression.Constant(test),
                 //如果条件为true，调用WriteLine方法输出“条件为true”
                 Expression.Call(
                 null,
                 typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }),
                 Expression.Constant("条件为true")
                 ),
                  //如果条件false
                  Expression.Call(
                 null,
                 typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }),
                 Expression.Constant("条件为false")
                 )
                 );
            //编译表达式树，输出结果
            Expression.Lambda<Action>(codition).Compile()();
        }
        [TestMethod]
        public void m18()
        {
            var x1 = Expression.Variable(typeof(string), "x");
            var x6 = Expression.Constant("hello expression", typeof(string));
            var x2 = Expression.Assign(x1, x6);
            var mi = typeof(System.Console).GetMethod("WriteLine", new[] { typeof(string) });
            var x3 = Expression.Call(mi, x1);
            var x4 = Expression.Block(new[] { x1 }, x2, x3);
            var x5 = Expression.Lambda<Action>(x4);
            x5.Compile().Invoke();
        }
        [TestMethod]
        public void m17()
        {
            var op = ">=";
            var integerPredicate = Generate<int>(op).Compile();
            var floatPredicate = Generate<float>(op).Compile();
            int iA = 12, iB = 4;
            Console.WriteLine("{0} {1} {2} : {3}",
                      iA, op, iB, integerPredicate(iA, iB));
            float fA = 867.0f, fB = 867.0f;
            Console.WriteLine("{0} {1} {2} : {3}",
                      fA, op, fB, floatPredicate(fA, fB));
            Console.WriteLine("{0} {1} {2} : {3}",
                      fA, ">", fB, Generate<float>(">").Compile()(fA, fB));
            // Console.ReadLine();
        }
        public static Expression<Func<T, T, bool>> Generate<T>(string op)
        {
            var x = Expression.Parameter(typeof(T), "x");
            var y = Expression.Parameter(typeof(T), "y");
            return Expression.Lambda<Func<T, T, bool>>
            (
              (op.Equals(">")) ? Expression.GreaterThan(x, y) :
                (op.Equals("<")) ? Expression.LessThan(x, y) :
                  (op.Equals(">=")) ? Expression.GreaterThanOrEqual(x, y) :
                    (op.Equals("<=")) ? Expression.LessThanOrEqual(x, y) :
                      (op.Equals("!=")) ? Expression.NotEqual(x, y) :
                        Expression.Equal(x, y),
              x,
              y
            );
        }
        [TestMethod]
        public void m16()
        {
            //变量i
            var i = Expression.Parameter(typeof(int), "i");
            //跳出循环
            var label = Expression.Label();
            var block = Expression.Block(
                new[] { i },
                //为i赋初值
                Expression.Assign(i, Expression.Constant(1, typeof(int))),
                Expression.Loop(
                    Expression.IfThenElse(
                      //如果i<=100
                      Expression.LessThanOrEqual(i, Expression.Constant(100, typeof(int))),
                        //如果为true.进入循环体
                        Expression.Block(
                             Expression.IfThen(
                                    //条件i%2==0;
                                    Expression.Equal(Expression.Modulo(i, Expression.Constant(2, typeof(int))),
                                    Expression.Constant(0, typeof(int))),
                                    Expression.Call(typeof(Console).GetMethod("WriteLine",
                                    new Type[] { typeof(int) }), new[] { i })),
                             //i++
                             Expression.PostIncrementAssign(i)
                             ),
                //如果i>100
                Expression.Break(label)),
                label
                ));
            Expression.Lambda<Action>(block).Compile()();
            //Console.Read();
        }
        [TestMethod]
        public void m15()
        {
            //+ using System.Reflection;
            //+ using System.Linq.Expressions;
            //参数
            var pa = Expression.Parameter(typeof(int), "i");
            //本地变量
            var loc = Expression.Variable(typeof(string), "str");
            //创建LabelTarget用来返回值
            var labelTarget = Expression.Label(typeof(string));
            //调用i.ToString()
            var med = Expression.Call(pa, typeof(object).GetMethod("ToString", new Type[] { }));
            //将结果赋值给本地字符串变量
            var asn = Expression.Assign(loc, med);
            //创建返回表达式（实际上就是Goto表达式）
            var ret = Expression.Return(labelTarget, loc);
            //创建返回表达式的目标Label
            var lbl = Expression.Label(labelTarget, Expression.Constant(String.Empty));
            //生成BlockExpression
            var blocks = Expression.Block(
                new ParameterExpression[] { loc },
                asn,
                ret,
                lbl);
            //生成Lambda表达式
            var lam = Expression.Lambda<Func<int, string>>(blocks,
                new ParameterExpression[] { pa });
            //运行并输出结果
            var del = lam.Compile();
            Console.WriteLine(del(17));
        }
        [TestMethod]
        public void m14()
        {
            //做一个 加法运算
            var x1 = Expression.Variable(typeof(double), "x");
            var x2 = Expression.Variable(typeof(double), "y");
            var add = Expression.Add(x1, x2);
            var x3 = Expression.Assign(x1, Expression.Constant(5.0d));
            var x4 = Expression.Assign(x2, Expression.Constant(5.23d));
            var x5 = Expression.Label(typeof(double));
            var x6 = Expression.Return(x5, add);
            var x7 = Expression.Label(x5, Expression.Constant(0d));
            var block = Expression.Block(typeof(double), new[] { x1, x2 }, new Expression[] { x3, x4, add, x6, x7 });
            var lambda = Expression.Lambda<Func<double>>(block);
            var func = lambda.Compile();
            Console.WriteLine(func.Invoke());
        }
        /// <summary>
        /// SELECT * FROM [T_GROUP] g LEFT JOIN [T_USER] u 
        /// ON g.MemberId=u.MemberId 
        /// ORDER BY g.GroupId,g.IsLeader 
        /// DESC,u.RealName
        /// </summary>
        [TestMethod]
        public void m13()
        {
            //var p = @"F:\work\netcorework\apps\w171219\server\w171219.Support\temp.json";
            //var content = File.ReadAllText(p);
            //var source = JsonConvert.DeserializeObject<Rootobject>(content, new JsonSerializerSettings { });//as Rootobject;
            //Console.WriteLine(source);
            //var query = from i in source.source1.pd
            //            join j in source.source2.pd
            //            on i.MemberId equals j.MemberId
            //            orderby j.RealName descending
            //            select new { i.GroupId, i.MemberId, i.IsLeader, j.RealName };
            //foreach (var item in query) {
            //    Console.WriteLine("GroupId:{0},MemberId:{1},IsLeader:{2},RealName:{3}", item.GroupId, item.MemberId, item.IsLeader, item.RealName);
            //}
            //var result = JsonConvert.SerializeObject(query);
            //Console.WriteLine(result);
        }
        [TestMethod]
        public void m12()
        {
            // The expression tree to execute.  
            var be = Expression.Power(Expression.Constant(2D), Expression.Constant(3D));
            // Create a lambda expression.  
            var le = Expression.Lambda<Func<double>>(be);
            // Compile the lambda expression.  
            var compiledExpression = le.Compile();
            // Execute the lambda expression.  
            var result = compiledExpression();
            // Display the result.  
            Console.WriteLine(result);
            // This code produces the following output:  
            // 8
        }
        [TestMethod]
        public void m11()
        {
            /*rx*/
        }
    }
}