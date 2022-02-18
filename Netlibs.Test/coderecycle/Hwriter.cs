using System;
using System.Collections.Generic;
using System.Text;

namespace Netlibs.Test.coderecycle {
    public class Hwriter {
        static public string getclass = @"(?<=\.)\w+";
        static public string gettime = @"(?<=\*)\d+";
        static public string gettag = @"(?<!\.|\w|\*)\w+";
        static public string gethold = @"_\d+";
        public void Template(string exp) {

        }
        public string Parse(string exp) {
            var emmet = string.Empty;
            /*
             * 分析emmet表达式发现，考虑优先处理圆括号是能够解析的一种方案
             * 圆括号内部应该由多元素收敛为一个元素然后将圆括号消除，如果不能消解为一个元素
             * 比如通过+号连接的情况，那么考虑使用一个虚拟占位元素替代，使之暂时成为一个元素
             */
            //按> 进行第一波切取
            /*
             * 算法设计思路，
             * 使用lambda不动点，递归迭代的方式，将emmet表达式
             * 通过第一个>号与后的串的关系构成递归单元，即lambda不动点
             */
            var split = exp.IndexOf(">");
            if (split == -1) return GetTag(exp);
            var first = exp.Substring(0, split);
            var gettime = Regex.Match(first, Hwriter.gettime).Value;
            var last = exp.Substring(split + 1);
            var tag = GetTag(first);
            var tagclose = tag.Split(' ').First();
            if (!string.IsNullOrWhiteSpace(gettime)) {
                var _gettime = int.Parse(gettime);
                var sb = new StringBuilder();
                for (int i = 0; i < _gettime; i++) {
                    sb.AppendLine($"<{tag}>{Parse(last)}</{tagclose}>");
                }
                emmet = sb.ToString();
            } else {
                emmet = $"<{tag}>{Parse(last)}</{tagclose}>";
            }
            return emmet;
        }
        string GetTag(string fragment) {
            var gethold = Regex.Match(fragment, Hwriter.gethold).Value;
            var gettag = Regex.Match(fragment, Hwriter.gettag).Value;
            var getclass = Regex.Match(fragment, Hwriter.getclass).Value;
            if (!string.IsNullOrWhiteSpace(gethold)) {
                return fragment;
            }
            /*
             * 返回空标签，或带有非标签内容的（文本）的标签
             * 解析，{},^,+ .#[] 属性
             */
            var tags = fragment.Split('+');
            var r = string.Empty;
            if (!string.IsNullOrWhiteSpace(getclass)) {
                if (string.IsNullOrWhiteSpace(gettag)) {
                    gettag = "div";
                }
                getclass = $" class='{getclass}'";
            }
            r = $"{gettag}{getclass}";
            return r;
        }
        struct TagInfo {

        }
        /// <summary>
        /// 存储括号替换
        /// </summary>
        Dictionary<string, string> bhs;
        public Hwriter() {
            this.bhs = new Dictionary<string, string>();
        }
        public string Handle(string exp) {
            var r = string.Empty;
            r = BracketsHandle(exp);
            r = Parse(r);
            foreach (var item in bhs.Reverse()) {
                r = r.Replace(item.Key, item.Value);
            }
            return r;
        }
        public string BracketsHandle(string exp) {
            var x = exp.IndexOf(")");
            if (x == -1) return exp;
            var i = exp.Substring(0, x + 1);
            var cars = new List<char>();
            for (int j = i.Length - 1; j >= 0; j--) {
                cars.Add(i[j]);
                if (i[j] == '(') {
                    break;
                }
            }
            cars.Reverse();
            var current = new string(cars.ToArray());
            var key = $"_{bhs.Count}";
            exp = exp.Replace(current, key);
            bhs.Add(key, Parse(current.Substring(1, current.Length - 1)));
            exp = BracketsHandle(exp);
            return exp;
        }
    }
}
