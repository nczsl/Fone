using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Util.Mathematics.Basic {
    /// <summary>
    /// 数据表达式，其实就是一个函数，数据函数最基础的
    /// 是一个输入的函数，二个输入的函数是建立表达式链的最小单位
    /// 所以表达式是一个自嵌套的树结构,并且因为数据表达式总是一行
    /// 书写的一个链结构 ，是一维的，只有左连接和右连接所以按这种
    /// 方式进行构建
    /// </summary>
    abstract public class Expr {
        protected Expr left;
        protected Expr right;
        protected Expr parent;
        /// <summary>
        /// 遍历
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<Expr> Travel(TravelType tt = TravelType.Foreword) {
            switch (tt) {
                case TravelType.Foreword:
                    yield return this;
                    if (left != null) foreach (var item in left.Travel(tt)) yield return item;
                    if (right != null) foreach (var item in right.Travel(tt)) yield return item;
                    break;
                case TravelType.Middle:
                    if (left != null) foreach (var item in left.Travel(tt)) yield return item;
                    yield return this;
                    if (right != null) foreach (var item in right.Travel(tt)) yield return item;
                    break;
                case TravelType.Back:
                    if (left != null) foreach (var item in left.Travel(tt)) yield return item;
                    if (right != null) foreach (var item in right.Travel(tt)) yield return item;
                    yield return this;
                    break;
            }
        }
    }
    public enum TravelType {
        Foreword, Middle, Back
    }
    /// <summary>
    /// 常量的最简表达是一个值 ，默认情况下是一个double类型的值 
    /// </summary>
    public class Constant : Expr {
        static public Constant PI = Math.PI;
        static public Constant E = Math.E;
        protected double value;
        public double Value { get { return value; } set { this.value = value; } }
        public Oper Parent { get => parent as Oper; set => parent = value; }
        static public implicit operator double(Constant node) => node.value;
        static public implicit operator Constant(double value) => new Constant(value);
        public Constant(double value) {
            this.value = value;
        }
        public override string ToString() {
            return $"{this.value}";
        }
    }
    public struct OperPattern {
        public const string line = "x?x";
        public const string func1 = "f(x)";
        public const string func2 = "f(x,x)";
        public const string mod = "|x|";
        public const string not = "-(x)";
    }
    public struct FoundationConnect {
        internal string value;
        public FoundationConnect(string value) {
            this.value = value;
        }
        static public FoundationConnect Add = "+";
        static public FoundationConnect Subtract = "-";
        static public FoundationConnect Multiply = "*";
        static public FoundationConnect Divide = "/";
        static public FoundationConnect Pow = "^";
        static public FoundationConnect Log = "log";
        static public FoundationConnect Lg = "lg";
        static public FoundationConnect Sin = "sin";
        static public FoundationConnect Cos = "cos";
        static public FoundationConnect Tan = "tan";
        static public FoundationConnect Ctan = "ctan";
        static public FoundationConnect Abs = "abs";
        static public FoundationConnect Exp = "exp";
        static public FoundationConnect Ln = "ln";
        static public FoundationConnect Round = "round";
        static public FoundationConnect Floor = "floor";
        static public FoundationConnect Max = "max";
        static public FoundationConnect Min = "min";
        static public FoundationConnect Ceiling = "ceiling";
        static public FoundationConnect Asin = "asin";
        static public FoundationConnect Acos = "acos";
        static public FoundationConnect Atan = "atan";
        static public implicit operator FoundationConnect(string value) => new FoundationConnect(value);
        static public implicit operator string(FoundationConnect con) => con.value;
        public int Priority => this switch
        {
            FoundationConnect x when x == Add => 0,
            FoundationConnect x when x == Subtract => 0,
            FoundationConnect x when x == Multiply => 1,
            FoundationConnect x when x == Divide => 1,
            FoundationConnect x when x == Pow => 2,
            FoundationConnect x when x == Log => 2,
            FoundationConnect x when x == Lg => 2,
            FoundationConnect x when x == Ln => 2,
            _ => 3
        };
        public override string ToString() => this.value;
    }
    /// <summary>
    /// 变量的最简表达就是一个关系，
    /// </summary>
    public class Oper : Expr {
        private FoundationConnect name;
        public FoundationConnect Name { get { return name; } set { name = value; } }
        public Oper Parent { get => parent as Oper; set => parent = value; }
        public Expr Left { get => left; set => left = value; }
        public Expr Right { get => right; set => right = value; }
        public Expr Child {
            get {
                switch ((Left, Right)) {
                    case (var l, null) when l != null: return l;
                    case (null, var r) when r != null: return r;
                    default: throw new Exception("要么Left为空，要么Right为空，二择其一");
                }
            }
            set {
                switch ((Left, Right)) {
                    case (var l, null) when l != null: l = value; break;
                    case (null, var r) when r != null: r = value; break;
                    default: throw new Exception("要么Left为空，要么Right为空，二择其一");
                }
            }
        }
        public Constant GetConstantChild(Constant left, Constant right) {
            switch (left, right) {
                case (var l, null) when l != null: return l;
                case (null, var r) when r != null: return r;
                default: throw new Exception("要么Left为空，要么Right为空，二择其一");
            }
        }
        public Oper(FoundationConnect name) {
            this.name = name;
            func = name.value switch
            {
                "+" => (l, r) => l + r,
                "-" => (l, r) => l - r,
                "*" => (l, r) => l * r,
                "/" => (l, r) => l / r,
                "^" => (l, r) => Math.Pow(l, r),
                "&" => (l, r) => Math.Log(l, r),
                "sin" => (l, r) => Math.Sin(GetConstantChild(l, r)),
                "cos" => (l, r) => Math.Cos(GetConstantChild(l, r)),
                "tan" => (l, r) => Math.Tan(GetConstantChild(l, r)),
                "ctan" => (l, r) => 1 / Math.Tan(GetConstantChild(l, r)),
                "exp" => (l, r) => Math.Exp(GetConstantChild(l, r)),
                "ln" => (l, r) => Math.Log(GetConstantChild(l, r)),
                "abs" => (l, r) => Math.Abs(GetConstantChild(l, r)),
                "max" => (l, r) => Math.Max(l, r),
                "min" => (l, r) => Math.Min(l, r),
                "floor" => (l, r) => Math.Floor(GetConstantChild(l, r)),
                "ceiling" => (l, r) => Math.Ceiling(GetConstantChild(l, r)),
                "round" => (l, r) => Math.Round(GetConstantChild(l, r)),
                "asin" => (l, r) => Math.Asin(GetConstantChild(l, r)),
                "acos" => (l, r) => Math.Acos(GetConstantChild(l, r)),
                "atan" => (l, r) => Math.Atan(GetConstantChild(l, r)),
                _ => null,
            };
        }
        public override string ToString() {
            /*
             * 管理子类Oper是否需要大括号，默认不需要
             * 基本连接的带刺仅有两种，一种是左连右线性的
             * 一种是函数名加括号左右的，left op right 和
             * op(left,right),这两种模式中又分仅有一个值 的情况，
             * 特殊的绝对值 为|left or right|
             */
            switch ((Left, Right)) {
                case (Constant l, Constant r): break;
            }
            return "";
        }
        Func<Constant, Constant, Constant> func;
        public Constant Calculate() {
            switch ((Left, Right)) {
                case (Constant l, Constant r): return func(l, r);
                case (Oper l, Constant r): return func(l.Calculate(), r);
                case (Constant l, Oper r): return func(l, r.Calculate());
                case (Oper l, Oper r): return func(l.Calculate(), r.Calculate());
                case (Oper l, null): return func(l.Calculate(), null);
                case (Constant l, null): return func(l, null);
                case (null, Oper r): return func(null, r.Calculate());
                case (null, Constant r): return func(null, r);
                default: throw new Exception("意外计算节点");
            }
        }
    }
}
