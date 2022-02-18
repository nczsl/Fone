using System;
using System.Collections.Generic;
using System.Linq;
using Util.Mathematics.Discrete;
using Util.Mathematics.LinearAlgebra2;

namespace Util.Mathematics.Basic {
    /// <summary>
    /// 数学基本算式
    /// 数学是形势逻辑，形势在数学里既是实质也是表象，形势衡等变形是数表述的最核心本质之一。
    /// 这个m,是一个数，也是一个算式，数和数的结构形势是等价的用这个m来表示
    /// </summary>
    public class M : Tree<M> {
        public double value;
        //如果value表达不了的情况使用 底数+指数的表达式给出值
        public double exp;
        public double floor;
        public string name;
        public string Value {
            get {
                return $"{value}";
            }
        }
        static public implicit operator M(double o) {
            var num = new M();
            num.value = o;
            return num;
        }
        static public implicit operator double(M m) => m.value;

        static public implicit operator M((double, string) e) {
            return new M { value = e.Item1, name = e.Item2 };
        }
    }
    /// <summary>
    /// 算子
    /// </summary>
    public class Oper2 : M {
        public Action func;
        public int unit;
        public Oper2(Func<M, M, M> func) {
            unit = 2;
            for (var i = 0; i < unit; i++) {
                M temp = 0d;
                temp.Parent = this;
                Childen.Add(temp);
            }
            this.func = () => {
                var temp = func.Invoke(first, seconds);
                value = temp;
            };
        }
        public Oper2(Func<M, M> func) {
            unit = 1;
            for (var i = 0; i < unit; i++) {
                M temp = 0;
                temp.Parent = this;
                Childen.Add(temp);
            }
            this.func = () => {
                var temp = func.Invoke(first);
                value = temp;
            };
        }
        protected Oper2() {
        }
        public M first {
            get => Childen[0] as M;
            protected set => Childen[0] = value;
        }
        public M seconds {
            get => Childen[1] as M;
            protected set => Childen[1] = value;
        }
        protected Oper2 LoadFirst(string name) => Load(null, name: name);
        public Oper2 LoadFirst(M m) => Load(m);
        protected Oper2 LoadSconds(string name) => Load(null, false, name: name);
        public Oper2 LoadSconds(M m) => Load(m, false);
        protected Oper2 Load(M m, bool isf = true, string name = null) {
            if (this == m) throw new Exception("不能重复添加已经存在的算子");
            m = m ?? 0;
            if (name != null)
                m.name = name;
            if (isf) {
                first = m;
                first.Parent = this;
            } else {
                seconds = m;
                seconds.Parent = this;
            }
            if (m is Oper2) {
                return m as Oper2;
            } else {
                return this;
            }
        }
        new public Oper2 Upon(int i = 1) => base.Upon(i) as Oper2;
        #region build

        static public Oper2 BuildAdd() => new Oper2((i, j) => { return i + j; }) { name = "add" };
        static public Oper2 BuildSubtraction() => new Oper2((i, j) => i - j) { name = "subtraction" };
        static public Oper2 BuildMultiply() => new Oper2((i, j) => i * j) { name = "multiply" };
        static public Oper2 BuildDivision() => new Oper2((i, j) => i / j) { name = "division" };
        static public Oper2 BuildLog() => new Oper2((i, j) => Math.Log(i, j)) { name = "log" };
        static public Oper2 BuildLn() => new Oper2((i) => Math.Log(i)) { name = "ln" };
        static public Oper2 BuildLg() => new Oper2((i) => Math.Log10(i)) { name = "lg" };
        static public Oper2 BuildPow() => new Oper2((i, j) => Math.Pow(i, j)) { name = "pow" };
        static public Oper2 BuildExp() => new Oper2((i) => Math.Exp(i)) { name = "exp" };
        static public Oper2 BuildAbs() => new Oper2((i) => Math.Abs(i)) { name = "abs" };
        static public Oper2 BuildMax() => new Oper2((i, j) => Math.Max(i, j)) { name = "max" };
        static public Oper2 BuildMin() => new Oper2((i, j) => Math.Min(i, j)) { name = "min" };
        static public Oper2 BuildSin() => new Oper2(angle => Math.Sin(angle)) { name = "sin" };
        static public Oper2 BuildCos() => new Oper2((i) => Math.Cos(i)) { name = "cos" };
        static public Oper2 BuildTan() => new Oper2((i) => Math.Tan(i)) { name = "tan" };
        static public Oper2 BuildAsin() => new Oper2((i) => Math.Asin(i)) { name = "asin" };
        static public Oper2 BuildAcos() => new Oper2((i) => Math.Acos(i)) { name = "acos" };
        static public Oper2 BuildTruncate() => new Oper2((i) => Math.Truncate(i)) { name = "truncate" };
        #endregion
        static public implicit operator Oper2(double o) {
            var num = new Oper2();
            num.value = o;
            return num;
        }
        static public implicit operator double(Oper2 m) => m.value;
        public double Calculate(double? first = null, double? seconds = null) {
            if (this.first is Oper2) {
                ((Oper2)this.first).Calculate();
            } else if (first != null) {
                this.first.value = first.Value;
            }
            if (unit == 2 && this.seconds is Oper2) {
                ((Oper2)this.first).Calculate();
            } else if (seconds != null) {
                this.seconds = seconds.Value;
            }
            func.Invoke();
            return this;
        }
    }
    /// <summary>
    /// 按书写习惯，函数中，加减乘除，包括指数是一前一后的参数连接，而
    /// log,sin,等其它是一个或两个参数均写在其右侧，最后构造的函数
    /// 是根据算子优先级，和括号综合计算的优先级数值进行的。
    /// </summary>
    public enum equ {
        c, x, p, q, add, subtraction, multiply, division
            , pow, exp, sin, cos, tg, ctg, asin, acos, log, ln, lg
    }
    public struct equp {
        public equ e;
        public string name;
        static public implicit operator equp(equ e) {
            return new equp { e = e, name = e.ToString() };
        }
        static public implicit operator equp((equ, string) e) {
            return new equp { e = e.Item1, name = e.Item2 };
        }
    }
    /// <summary>
    /// 方程
    /// </summary>
    public class Equation {
        /// <summary>
        /// 函数输出集
        /// </summary>
        public Vector output;
        /// <summary>
        /// 自变量输入集
        /// </summary>
        public Vector input;
        public Equation() {
            parameters = new List<M>();
            independents = new List<M>();
        }
        public void Build(string expression) {

        }
        Oper2 function;
        readonly List<M> independents;
        readonly List<M> parameters;
        public void UpdateParameter(string name, double value) => parameters.Find(i => i.name == name).value = value;
        equp[] buildp;
        public void Build(params equp[] es) {
            buildp = es;
            //1，正确的找出算子
            //2, 确定各算子的优先级
            var list = new List<(int i, int fp, M v)>();//第一个int为下标，第二个为，函数优先级,
            var list2 = new List<(int i, int p)>();
            var list3 = new List<(int i, M m)>();
            var priority = 0;
            for (var i = 0; i < es.Length; i++) {
                var temp = default(M);
                switch (es[i].e) {
                    case equ.c:
                        temp = (0, es[i].name);
                        parameters.Add(temp);
                        break;
                    case equ.x:
                        temp = (0, es[i].name);
                        independents.Add(temp);
                        break;
                    case equ.p:
                        list2.Add((i, priority++));
                        break;
                    case equ.q:
                        list2.Add((i, priority--));
                        break;
                    case equ.add:
                        temp = Oper2.BuildAdd();
                        list.Add((i, 0, temp));
                        break;
                    case equ.subtraction:
                        temp = Oper2.BuildSubtraction();
                        list.Add((i, 0, temp));
                        break;
                    case equ.multiply:
                        temp = Oper2.BuildMultiply();
                        list.Add((i, 1, temp));
                        //to be continue
                        //note:每个区域里面从优先级最高的函数开始收纳叶节点，然后从list3中移除被收纳的节点
                        //层层递进，达到函数算式树构造的目的，后面的算子的收纳必然是list3中剩下的节点，如果
                        //叶节点被移除后那么剩下的更多的就是子级算子节点了
                        break;
                    case equ.division:
                        temp = Oper2.BuildDivision();
                        list.Add((i, 1, temp));
                        break;
                    case equ.pow:
                        temp = Oper2.BuildPow();
                        list.Add((i, 2, temp));
                        break;
                    case equ.exp:
                        temp = Oper2.BuildExp();
                        list.Add((i, 2, temp));
                        break;
                    case equ.sin:
                        temp = Oper2.BuildSin();
                        list.Add((i, 3, temp));
                        break;
                    case equ.cos:
                        temp = Oper2.BuildCos();
                        list.Add((i, 3, temp));
                        break;
                    case equ.tg:
                        temp = Oper2.BuildTan();
                        list.Add((i, 3, temp));
                        break;
                    case equ.ctg:
                        temp = new Oper2((a) => 1 / Math.Tan(i));
                        list.Add((i, 3, temp));
                        break;
                    case equ.asin:
                        temp = Oper2.BuildAsin();
                        list.Add((i, 3, temp));
                        break;
                    case equ.acos:
                        temp = Oper2.BuildAcos();
                        list.Add((i, 3, temp));
                        break;
                    case equ.log:
                        temp = Oper2.BuildLog();
                        list.Add((i, 2, temp));
                        break;
                    case equ.ln:
                        temp = Oper2.BuildLn();
                        list.Add((i, 2, temp));
                        break;
                    case equ.lg:
                        temp = Oper2.BuildLg();
                        list.Add((i, 2, temp));
                        break;
                    default:
                        break;
                }
                list3.Add((i, temp));
            }
            var area = new (int s, int e, int p)[list2.Count + 1];
            area[0] = (s: 0, e: list2.Count > 0 ? list2[0].i : list3.Count, p: 0);
            for (var i = 0; i < list2.Count; i++) {
                area[i + 1] = (area[i].e, list2[i].i, list2[i].p);
            }
            var trace = default(Oper2);
            foreach (var item in from it in area orderby it.p descending select it) {
                foreach (var it in from iu in list where iu.i > item.s && iu.i < item.e orderby iu.fp descending select iu) {
                    var temp = default((int i, M m));
                    var target = list3.Find(k => k.m == it.v);
                    switch (it.v.name) {
                        case "add":
                        case "subtraction":
                        case "multiply":
                        case "division":
                        case "pow":
                            temp = list3[list3.IndexOf(target) - 1];
                            ((Oper2)it.v).LoadFirst(temp.m);
                            list3.Remove(temp);
                            temp = list3[list3.IndexOf(target) + 1];
                            ((Oper2)it.v).LoadSconds(temp.m);
                            list3.Remove(temp);
                            break;
                        case "sin":
                        case "asin":
                        case "cos":
                        case "acos":
                        case "tg":
                        case "ctg":
                        case "ln":
                        case "lg":
                        case "exp":
                            temp = list3[list3.IndexOf(target) + 1];
                            ((Oper2)it.v).LoadFirst(temp.m);
                            list3.Remove(temp);
                            break;
                        case "log":
                            temp = list3[list3.IndexOf(target) + 1];
                            ((Oper2)it.v).LoadFirst(temp.m);
                            list3.Remove(temp);
                            temp = list3[list3.IndexOf(target) + 2];
                            ((Oper2)it.v).LoadSconds(temp.m);
                            list3.Remove(temp);
                            break;
                    }
                    trace = it.v as Oper2;
                }
            }
            function = trace;
        }
        public void Solve() {

        }
        public bool isLoaded;
        public void Load(params double[] ps) {
            if (ps.Length < parameters.Count) throw new Exception("载入参数不足");
            var idx = 0;
            foreach (var item in parameters) {
                item.value = ps[idx];
                idx++;
            }
            isLoaded = true;
        }
        public void Calculate(params double[] xs) {
            input = new Vector(xs);
            output = new Vector(xs);
            if (!isLoaded) throw new Exception("还没有载入参数模型");
            var idx = 0;
            foreach (var item in xs) {
                foreach (var it in independents) {
                    it.value = item;
                }
                output[idx] = function.Calculate();
                idx++;
            }
        }
        public override string ToString() {
            var temp = "y = ";
            var idx = 1;
            foreach (var item in buildp) {
                switch (item.e) {
                    case equ.c:
                        temp += $"{parameters.ElementAt(idx - 1).name ?? "c" + idx}";
                        idx++;
                        break;
                    case equ.x:
                        temp += "x";
                        break;
                    case equ.p:
                        temp += " ( ";
                        break;
                    case equ.q:
                        temp += " ) ";
                        break;
                    case equ.add:
                        temp += " + ";
                        break;
                    case equ.subtraction:
                        temp += " - ";
                        break;
                    case equ.multiply:
                        temp += ".";
                        break;
                    case equ.division:
                        temp += "/";
                        break;
                    case equ.pow:
                        temp += "^";
                        break;
                    case equ.exp:
                        temp += $"e(p ";
                        break;
                    case equ.sin:
                        temp += "sin(p)";
                        break;
                    case equ.cos:
                        temp += "cos(p)";
                        break;
                    case equ.tg:
                        temp += "tg(p)";
                        break;
                    case equ.ctg:
                        temp += "ctg(p)";
                        break;
                    case equ.asin:
                        temp += "asin(p)";
                        break;
                    case equ.acos:
                        temp += "acos(p)";
                        break;
                    case equ.log:
                        temp += "log(p)";
                        break;
                    case equ.ln:
                        temp += "ln(p)";
                        break;
                    case equ.lg:
                        temp += "lg(p)";
                        break;
                    default:
                        break;
                }
            }
            return temp;
        }
    }

    public class ExprVisitor : ExpressionVisitor {
        public Expr exp;
        protected override Expression VisitBinary(BinaryExpression node) {
            Console.WriteLine("{0},{1}", node.Left, node.Left.NodeType);
            Console.WriteLine("{0},{1}", node.Right, node.Right.NodeType);
            Console.WriteLine(node.NodeType);
            /*
             * 叶结点三种类型：
             * Constant,Parameter,Call
             */

            return base.VisitBinary(node);
        }
        protected override Expression VisitConstant(ConstantExpression node) {
            Console.WriteLine("VisitConstant:{0}", node);
            return node;
        }
        protected override Expression VisitMethodCall(MethodCallExpression node) {
            Console.WriteLine("VisitMethodCall:{0}", node);
            return node;
        }
        protected override Expression VisitParameter(ParameterExpression node) {
            Console.WriteLine("VisitParameter:{0},{1}", node, node.Type);
            if (node.Type.IsSubclassOf(typeof(Expr))) {
                //(node as Expr).ParameterName=
            }
            return node;
        }
        protected override Expression VisitBlock(BlockExpression node) {
            Console.WriteLine("VisitBlock:{0}", node);
            return node;
        }
        protected override Expression VisitConditional(ConditionalExpression node) {
            Console.WriteLine("VisitConditional:{0}", node);
            return node;
        }
        protected override Expression VisitDebugInfo(DebugInfoExpression node) {
            Console.WriteLine("VisitDebugInfo:{0}", node);
            return node;
        }
        protected override Expression VisitDefault(DefaultExpression node) {
            Console.WriteLine("VisitDefault:{0}", node);
            return node;
        }
        protected override Expression VisitExtension(Expression node) {
            Console.WriteLine("VisitExtension:{0}", node);
            return node;
        }
        protected override Expression VisitGoto(GotoExpression node) {
            Console.WriteLine("VisitGoto:{0}", node);
            return node;
        }
        protected override Expression VisitInvocation(InvocationExpression node) {
            Console.WriteLine("VisitInvocation:{0}", node);
            return node;
        }
        protected override LabelTarget VisitLabelTarget(LabelTarget node) {
            Console.WriteLine("VisitLabelTarget:{0}", node);
            return node;
        }
        protected override Expression VisitLabel(LabelExpression node) {
            Console.WriteLine("VisitLabel:{0}", node);
            return node;
        }
        protected override Expression VisitLambda<T>(Expression<T> node) {
            Console.WriteLine("VisitLambda:{0}", node);
            return node;
        }
        protected override Expression VisitLoop(LoopExpression node) {
            Console.WriteLine("VisitLoop:{0}", node);
            return node;
        }
        protected override Expression VisitMember(MemberExpression node) {
            Console.WriteLine("VisitMember:{0}", node);
            return node;
        }
        protected override Expression VisitIndex(IndexExpression node) {
            Console.WriteLine("VisitIndex:{0}", node);
            return node;
        }
        protected override Expression VisitNewArray(NewArrayExpression node) {
            Console.WriteLine("VisitNewArray:{0}", node);
            return node;
        }
        protected override Expression VisitNew(NewExpression node) {
            Console.WriteLine("VisitNew:{0}", node);
            return node;
        }
        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node) {
            Console.WriteLine("VisitRuntimeVariables:{0}", node);
            return node;
        }
        protected override SwitchCase VisitSwitchCase(SwitchCase node) {
            Console.WriteLine("VisitSwitchCase:{0}", node);
            return node;
        }
        protected override Expression VisitSwitch(SwitchExpression node) {
            Console.WriteLine("VisitSwitch:{0}", node);
            return node;
        }
        protected override CatchBlock VisitCatchBlock(CatchBlock node) {
            Console.WriteLine("VisitCatchBlock:{0}", node);
            return node;
        }
        protected override Expression VisitTry(TryExpression node) {
            Console.WriteLine("VisitTry:{0}", node);
            return node;
        }
        protected override Expression VisitTypeBinary(TypeBinaryExpression node) {
            Console.WriteLine("VisitTypeBinary:{0}", node);
            return node;
        }
        protected override Expression VisitUnary(UnaryExpression node) {
            Console.WriteLine("VisitUnary:{0}", node);
            return node;
        }
        protected override Expression VisitMemberInit(MemberInitExpression node) {
            Console.WriteLine("VisitMemberInit:{0}", node);
            return node;
        }
        protected override Expression VisitListInit(ListInitExpression node) {
            Console.WriteLine("VisitListInit:{0}", node);
            return node;
        }
        protected override ElementInit VisitElementInit(ElementInit node) {
            Console.WriteLine("VisitElementInit:{0}", node);
            return node;
        }
        protected override MemberBinding VisitMemberBinding(MemberBinding node) {
            Console.WriteLine("VisitMemberBinding:{0}", node);
            return node;
        }
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node) {
            Console.WriteLine("VisitMemberAssignment:{0}", node);
            return node;
        }
        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node) {
            Console.WriteLine("VisitMemberMemberBinding:{0}", node);
            return node;
        }
        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node) {
            Console.WriteLine("VisitMemberListBinding:{0}", node);
            return node;
        }
        protected override Expression VisitDynamic(DynamicExpression node) {
            Console.WriteLine("VisitDynamic:{0}", node);
            return node;
        }
    }
}
