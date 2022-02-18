using System;
using System.Collections.Generic;
using System.Linq;
namespace Util.Mathematics.Basic.Colculate {
    public delegate double Fun(params double[] ps);
    /// <summary>
    /// 计算类
    /// </summary>
    public class Cu {
        List<Cu> parameters;
        Dictionary<string, Fun> funTemplates;
        /// <summary>
        /// 基础计算优先级
        /// </summary>
        int Priority {
            get {
                var c = 0;
                switch (name) {
                    case "add": c = 0; break;
                    case "subtract": c = 0; break;
                    case "multiply": c = 1; break;
                    case "divide": c = 1; break;
                    case "log": c = 2; break;
                    case "pow": c = 2; break;
                }
                return c;
            }
        }
        /// <summary>括号层次</summary>
        int level;
        public double? value;
        double ToValue() {
            if (value != null) {
                return value.Value;
            } else if (string.IsNullOrWhiteSpace(name)) {
                throw new Exception("没有函数可供计算");
            } else {
                var ps = new List<double>();
                foreach (var item in parameters) {
                    if (item.value != null) {
                        ps.Add(item.value.Value);
                    } else {
                        ps.Add(item.ToValue());
                    }
                }
                return funTemplates[name].Invoke(ps.ToArray());
            }
        }
        void ToStruct() {

        }
        string expression;
        public bool hasValue;
        public Cu Calculate() {
            ExpressionBiuld();
            if (hasValue) {
                value = ToValue();
            } else {
                ToStruct();
            }
            return this;
        }
        public override string ToString() {
            return base.ToString();
        }
        public string name;
        List<Cu> chainTable;
        public Cu() {
            Initial();
        }
        void Initial() {
            chainTable = new List<Cu>();
            funTemplates = new Dictionary<string, Fun>();
            funTemplates.Add("add", add);
            funTemplates.Add("subtract", subtract);
            funTemplates.Add("multiply", multiply);
            funTemplates.Add("divide", divide);
            funTemplates.Add("log", log);
            funTemplates.Add("pow", pow);
            funTemplates.Add("negation", negation);
            //
            baseExprPattern = @"\d+|\+|-|\*|/|^|~";
            funcPattern = @"\w+\s?\(\s?()+\s?\)";
        }
#pragma warning disable CS0414 // 字段“Cu.baseExprPattern”已被赋值，但从未使用过它的值
        string baseExprPattern;
#pragma warning restore CS0414 // 字段“Cu.baseExprPattern”已被赋值，但从未使用过它的值
#pragma warning disable CS0414 // 字段“Cu.funcPattern”已被赋值，但从未使用过它的值
        string funcPattern;
#pragma warning restore CS0414 // 字段“Cu.funcPattern”已被赋值，但从未使用过它的值
        /// <summary>
        /// 解析一个字符串表达式
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <param name="expression">例如:25*(2+8)-sin(30)*100</param>
        public void ExpressionBiuld(string expression) {
            this.expression = expression.Replace(" ", "");
            foreach (var item in this.expression) {

            }
        }
        void ExpressionBiuld() {
            //检查表达式是不是正确
            //检查表达式是值还是结构
            /*采用左收敛推进方式解析chainTable
            从左到右依次找到所有是函数的节点，判断这些函数是不是与数字节点 相间的
            如果不相间说明表达式不正确。
            从最左边的函数开始，判断该函数是不是基本二元函数，如果是，那么把它的前继数字节点加入参数，然后判断
            它的后继节点的level值，如果level值相同，则比对priority值，如果priority小于等于，那么，把该后继数字节点，加入该函数的
            参数节点，依次判断一个函数，
             */
            //计算差异序号
            foreach (var item in from i in chainTable where !string.IsNullOrWhiteSpace(i.name) select i.name) {
                hasValue = funTemplates.Keys.Contains(item);
                if (!hasValue) {
                    break;
                }
            }
            while (chainTable.Count > 1) {
                var cus = new List<Cu>();
                var idx = 0;
                for (var i = 0; i < chainTable.Count; i++) {
                    if (chainTable[idx].level < chainTable[i].level) {
                        idx = i;
                    }
                }
                var level = chainTable[idx].level;
                while (chainTable.Count > 0 && chainTable[idx].level == level) {
                    var temp = chainTable[idx];
                    cus.Add(temp);
                    chainTable.Remove(temp);
                }
                var cu = BuildFromBaseExpre(cus);
                chainTable.Insert(idx, cu);
            }
            var result = chainTable[0];
            name = result.name;
            parameters = result.parameters;
        }
        Cu BuildFromBaseExpre(List<Cu> cus) {
            /*
            每次迭代消耗一个节点，以数字节点为依据
             */
            while (cus.Count > 1) {
                for (var i = 0; i < cus.Count; i++) {
                    var temp = cus[i];
                    if (CheckNode(temp)) {
                        //采用左收敛
                        /*
                        三种情况:
                        1,左边为空节点，右边为可参函数
                        2，左边右边皆为可参函数
                        3，右边为空，左边为可参函数
                         */
                        if (i == 0) {
                            //左为空
                            var right = cus[i + 1];
                            right.parameters.Add(temp);
                        } else if (i + 1 == cus.Count) {
                            //右边为空
                            var left = cus[i - 1];
                            left.parameters.Add(temp);
                        } else {
                            var left = cus[i - 1];
                            var right = cus[i + 1];
                            if (left.Priority >= right.Priority) {
                                left.parameters.Add(temp);
                            } else {
                                right.parameters.Add(temp);
                            }
                        }
                        cus.Remove(temp);
                        break;
                    }
                }
            }
            var one = cus[0];
            if (one.level > 0) {
                one.level--;
            }
            return one;
        }
        int BaseFunCode(string funName) {
            var c = 0;
            switch (funName) {
                case "add": c = 2; break;
                case "subtract": c = 2; break;
                case "multiply": c = 2; break;
                case "divide": c = 2; break;
                case "log": c = 2; break;
                case "pow": c = 2; break;
            }
            return c;
        }
        bool CheckNode(Cu cu) {
            var r = false;
            r = (string.IsNullOrWhiteSpace(cu.name) && cu.value != null) || BaseFunCode(cu.name) == cu.parameters.Count;
            return r;
        }
        public void RegistryFun(string name, Fun fun) {
            funTemplates.Add(name, fun);
        }
        public void Chain(char baseFunChar) {
            var x = new Cu { parameters = new List<Cu>() };
            switch (baseFunChar) {
                case '+': x.name = "add"; break;
                case '-': x.name = "subtract"; break;
                case '*': x.name = "multiply"; break;
                case '/': x.name = "divide"; break;
                case '^': x.name = "log"; break;
                case '~': x.name = "pow"; break;
            }
            chainTable.Add(x);
            if (chainTable.Count > 1) {
                var temp = chainTable[chainTable.Count - 2];
                x.level = temp.bracket;
                x.bracket = temp.bracket;
            }
        }
        public void Chain(string funName, params double[] ps) {
            var x = new Cu { name = funName };
            foreach (var item in ps) {
                x.parameters.Add(item);
            }
            chainTable.Add(x);
            if (chainTable.Count > 1) {
                var temp = chainTable[chainTable.Count - 2];
                x.level = temp.bracket;
                x.bracket = temp.bracket;
            }
        }
        public void Chain(double v) {
            var x = new Cu { value = v, parameters = new List<Cu>() };
            chainTable.Add(x);
            if (chainTable.Count > 1) {
                var temp = chainTable[chainTable.Count - 2];
                x.level = temp.bracket;
                x.bracket = temp.bracket;
            }
        }
        int bracket;
        public void Chain(double x, bool bracketMark, int lev = 1) {
            var x2 = new Cu { value = x, parameters = new List<Cu>() };
            chainTable.Add(x2);
            var temp = default(Cu);
            if (chainTable.Count > 1)
                temp = chainTable[chainTable.Count - 2];
            else
                temp = chainTable[0];
            if (bracketMark) {
                x2.level = temp.level + lev;
                x2.bracket = temp.bracket + lev;
            } else {
                x2.level = temp.level;
                x2.bracket = temp.bracket - lev;
            }
        }
        public void Chain(BaseFunctionType bft) {
            var x = new Cu { name = Enum.GetName(typeof(BaseFunctionType), bft), parameters = new List<Cu>() };
            chainTable.Add(x);
            if (chainTable.Count > 1) {
                var temp = chainTable[chainTable.Count - 2];
                x.level = temp.bracket;
                x.bracket = temp.bracket;
            }
        }
        static public implicit operator Cu(double v) {
            return new Cu { value = v };
        }
        static public implicit operator Cu(char bf) {
            var r = new Cu();
            switch (bf) {
                case '+': r.name = "add"; break;
                case '-': r.name = "subtract"; break;
                case '*': r.name = "multiply"; break;
                case '/': r.name = "divide"; break;
                case '^': r.name = "log"; break;
                case '~': r.name = "pow"; break;
            }
            return r;
        }
        static public implicit operator double?(Cu cu) {
            return cu.value;
        }
        static public implicit operator char(Cu cu) {
            var c = default(char);
            switch (cu.name) {
                case "add": c = '+'; break;
                case "subtract": c = '-'; break;
                case "multiply": c = '*'; break;
                case "divide": c = '/'; break;
                case "log": c = '~'; break;
                case "pow": c = '^'; break;
            }
            return c;
        }
        #region 内置基础函数
        static double add(double[] ps) => ps[0] + ps[1];
        static double subtract(double[] ps) => ps[0] - ps[1];
        static double multiply(double[] ps) => ps[0] * ps[1];
        static double divide(double[] ps) => ps[0] / ps[1];
        static double log(double[] ps) => System.Math.Log(ps[0], ps[1]);
        static double pow(double[] ps) => System.Math.Pow(ps[0], ps[1]);
        static double negation(double[] ps) => -ps[0];
        #endregion
    }
    public enum BaseFunctionType {
        add, subtract, multiply, divide, pow, log
    }
}
