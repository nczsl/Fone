using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Util.Mathematics.Discrete;
/*
* 设计书:
* 后端emmet ，emmet是一种简写html树结构的表达式
* 高效可靠，本工具，将数据与emmet进行接合，是一种创新
* 让后端在高表达效益的基础上尽量做到兼顾运行效率，使用
* 正则表达式，分块处理，将一个完整的emmet表达式，进行解析
* 如果发现括号部分则进入子解析，最后合并解析结果
* 对于传统的emmet中的 *数字的表达更改为，*{绑定集合}
* 算法设计关键，
* 要提升或将运行效率稳定在一定水平，必须对表达式所产生的树型数据结构
* 进行粗颗粒处理，然每一个子树直接翻译成一串html片断
* 
* 使用本工具的另一个好处是，临时数据自身解决，直接写在Razor视图文档即可
* 减少了ViewModel的占比，送货了开发
*/
namespace Fone {
    /// <summary>
    /// 后端emmet包装器
    /// </summary>
    [HtmlTargetElement("emmet")]
    public class HwrapTagHelper : TagHelper {
        [HtmlAttributeName("config")]
        public Emmet config { get; set; }
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output) {
            output.TagName = string.Empty;
            output.TagMode = TagMode.SelfClosing;
            var hdoc = await config.Build();
            output.Content.SetHtmlContent(hdoc);
        }
    }
    public class Hwriter {
        public List<string> parenthesis;
        public Hwriter() {
            parenthesis = new List<string>();
            bindingnote = new Dictionary<string, Hsource>();
        }
        public void Binding(Tree<EmmetNode> root) {
            foreach (var item in from i in root.Traversal() where i.data.isSourceBinding select i) {
                var source = bindingnote[item.data.sourceStr];
                var htmlfragment = new StringBuilder();//运行效率的关键
                var treenodestr = GetTreeStr(item); var parentopen = $"{item.data.open}";
                var parentclose = $"{item.data.close}"; var chtemp = GetTreeStr(item.Childen[0]);
                var keys = GetKeys(treenodestr);
                if (source.unGeneric) {
                    UnGenericBinding(source, htmlfragment, treenodestr, parentopen, parentclose, chtemp, keys);
                } else {
                    GenericBinding(source, htmlfragment, treenodestr, parentopen, parentclose, chtemp);
                }
                item.data.htmlfragment = htmlfragment.ToString();
            }
        }
        static string[] GetKeys(string treestr) {
            var matches = Regex.Matches(treestr, itemTemplatepattern);
            var keys = new List<string>();
            foreach (Match item in matches) {
                var temp = item.Value;
                keys.Add(temp.Substring(1, temp.Length - 2));
            }
            return keys.ToArray();
        }
        private static void GenericBinding(Hsource source, StringBuilder htmlfragment, string temp, string parentopen, string parentclose, string chtemp) {
            switch (source.stype) {
                case Hsource.SourceType.dic:
                case Hsource.SourceType.entity:
                    foreach (var it in source.Dic) {
                        htmlfragment.AppendLine(temp.Replace("{item}", it.Value?.ToString()));
                    }
                    break;
                case Hsource.SourceType.entities:
                    foreach (var it in source.Entities) {
                        htmlfragment.AppendLine(parentopen);
                        var dic = it.GetEoo() as IDictionary<string, object>;
                        foreach (var iu in dic) {
                            htmlfragment.Append(chtemp.Replace("{item}", iu.Value?.ToString()));
                        }
                        htmlfragment.AppendLine(parentclose);
                    }
                    break;
                case Hsource.SourceType.dics:
                    foreach (var it in source.Dics) {
                        htmlfragment.AppendLine(parentopen);
                        foreach (var iu in it) {
                            htmlfragment.Append(chtemp.Replace("{item}", iu.Value?.ToString()));
                        }
                        htmlfragment.AppendLine(parentclose);
                    }
                    break;
                case Hsource.SourceType.table:
                    foreach (DataRow row in source.Table.Rows) {
                        htmlfragment.AppendLine(parentopen);
                        foreach (DataColumn column in source.Table.Columns) {
                            htmlfragment.AppendLine(chtemp.Replace("{item}", row[column].ToString()));
                        }
                        htmlfragment.AppendLine(parentclose);
                    }
                    break;
                case Hsource.SourceType.row:
                    foreach (DataColumn column in source.Row.Table.Columns) {
                        htmlfragment.AppendLine(temp.Replace("{item}", source.Row[column].ToString()));
                    }
                    break;
                case Hsource.SourceType.meta:
                    foreach (var it in source.MetaData) {
                        htmlfragment.AppendLine(temp.Replace("{item}", it.Name));
                    }
                    break;
            }
        }

        private static void UnGenericBinding(Hsource source, StringBuilder htmlfragment, string temp, string parentopen, string parentclose, string chtemp, string[] keys) {
            switch (source.stype) {
                case Hsource.SourceType.dic:
                case Hsource.SourceType.entity:
                    foreach (var it in keys) {
                        temp = temp.Replace($"{it}", source.Dic[it].ToString());
                    }
                    htmlfragment.AppendLine(temp);
                    break;
                case Hsource.SourceType.entities:
                    foreach (var it in source.Entities) {
                        htmlfragment.AppendLine(parentopen);
                        var dic = it.GetEoo() as IDictionary<string, object>;
                        foreach (var iu in keys) {
                            chtemp = chtemp.Replace($"{it}", dic[iu].ToString());
                        }
                        htmlfragment.AppendLine(chtemp);
                        htmlfragment.AppendLine(parentclose);
                    }
                    break;
                case Hsource.SourceType.dics:
                    foreach (var it in source.Dics) {
                        htmlfragment.AppendLine(parentopen);
                        foreach (var iu in keys) {
                            chtemp = chtemp.Replace($"{it}", it[iu].ToString());
                        }
                        htmlfragment.AppendLine(chtemp);
                        htmlfragment.AppendLine(parentclose);
                    }
                    break;
                case Hsource.SourceType.table:
                    foreach (DataRow row in source.Table.Rows) {
                        htmlfragment.AppendLine(parentopen);
                        foreach (var key in keys) {
                            chtemp = chtemp.Replace($"{key}", row[key].ToString());
                        }
                        htmlfragment.AppendLine(chtemp);
                        htmlfragment.AppendLine(parentclose);
                    }
                    break;
                case Hsource.SourceType.row:
                    foreach (var key in keys) {
                        chtemp = chtemp.Replace($"{key}", source.Row[key].ToString());
                    }
                    htmlfragment.AppendLine(chtemp);
                    break;
                case Hsource.SourceType.meta:
                    foreach (var it in keys) {
                        //htmlfragment.AppendLine(temp.Replace("{item}", it.Name));
                        var mt = source.MetaData.Single(i => i.Name == it);
                        temp = temp.Replace($"{{{it}}}", mt.Name);
                    }
                    htmlfragment.AppendLine(temp);
                    break;
            }
        }
        public Tree<EmmetNode> Merge() {
            var tree = default(Tree<EmmetNode>);
            var trees = new Dictionary<int, Tree<EmmetNode>>();
            var idx = 0;
            foreach (var item in parenthesis) {
                trees.Add(idx++, Parse(item));
            }
            tree = trees[idx - 1];//通过最后一个查找
            while (trees.Count > 1) {
                var xx = from i in tree.Traversal()
                         where !string.IsNullOrWhiteSpace(i.data.placeholdid)
                         select i;
                for (var i = 0; i < xx.Count(); i++) {
                    var selectnode = xx.ElementAt(i);
                    var pid = int.Parse(selectnode.data.placeholdid);
                    var parent = selectnode.Parent;
                    parent.Remove(selectnode);
                    parent.Add(trees[pid]);
                    trees.Remove(pid);
                }
            }
            return tree;
        }
        /// <summary>
        /// 处理括号
        /// </summary>
        /// <param name="emmet"></param>
        /// <returns></returns>
        public string ParenthesisHandle(string emmet) {
            var t1 = emmet.IndexOf(")");
            if (t1 == -1) {
                parenthesis.Add(emmet);
                return emmet;
            }
            var tstr = emmet.Substring(0, t1);
            var t2 = new string(tstr.Reverse().ToArray()).IndexOf("(");
            var target = emmet.Substring(t1 - t2, t2);
            var key = $"_{parenthesis.Count}";
            parenthesis.Add(target);
            var replaceemmet = emmet.Replace($"({target})", key);
            var reemmet = ParenthesisHandle(replaceemmet);
            return reemmet;
        }
        public struct EmmetNode {
            public string open;
            public string close;
            public string content;
            public bool isSourceBinding;
            public string sourceStr;
            public string placeholdid;
            //public int times;
            public string htmlfragment;
            public string tag;
            public override string ToString() => $"{open}{content}{close}";
        }
        public Tree<EmmetNode> Parse(string basicEmmet) {
            var root = new Tree<EmmetNode>();
            var current = root;
            var currentstr = string.Empty;
            var currentnode = default(EmmetNode);
            foreach (var item in basicEmmet) {
                if (item == '>') {
                    currentnode = ParseCell(currentstr);
                    if (current.data.Equals(default(EmmetNode))) {
                        current.data = currentnode;
                    } else {
                        current = current.Addat(currentnode);
                    }
                    currentstr = string.Empty;
                } else if (item == '^') {
                    if (string.IsNullOrEmpty(currentstr)) {
                        current = current.Upon();
                    } else {
                        currentnode = ParseCell(currentstr);
                        if (current.data.Equals(default(EmmetNode))) {
                            current.data = currentnode;
                        } else {
                            current = current.Add(currentnode);
                            current = current.Upon();
                        }
                        currentstr = string.Empty;
                    }
                } else if (item == '+') {
                    currentnode = ParseCell(currentstr);
                    if (current.data.Equals(default(EmmetNode))) {
                        current.data = currentnode;
                    } else {
                        current = current.Add(currentnode);
                    }
                    currentstr = string.Empty;
                } else {
                    currentstr += item;
                }
            }
            currentnode = ParseCell(currentstr);
            current.Add(currentnode);
            return root;
        }
        static public string tagpattern = @"^[a-z]\w*";
        static public string classpattern = @"(?<=\.)(?<!\w)\w+";
        static public string idpattern = @"(?<=#)(?<!\w)\w+";
        static public string itemTemplatepattern = @"(?<!\*)\{\w+\}";
        static public string sourceTemplatepattern = @"(?<=\*\{)\w+(?=\})";
        //static public string timeTemplatepattern = @"(?<=\*)\d+";
        static public string attributepattern = @"(?<=\[).+(?=\])";
        static public string placeholdpattern = @"(?<=_)\d+";
        public Dictionary<string, Hsource> bindingnote;
        EmmetNode ParseCell(string emmetcell) {
            var r = default(EmmetNode);
            var tag = Regex.Match(emmetcell, tagpattern);
            var _class = Regex.Match(emmetcell, classpattern);
            var id = Regex.Match(emmetcell, idpattern);
            var item = Regex.Match(emmetcell, itemTemplatepattern);
            var source = Regex.Match(emmetcell, sourceTemplatepattern);
            var attrs = Regex.Match(emmetcell, attributepattern);
            var placehold = Regex.Match(emmetcell, placeholdpattern);
            //var times = Regex.Match(emmetcell, timeTemplatepattern);
            var attrlist = attrs.Value.Split(' ').ToList();
            if (_class.Success) attrlist.Insert(0, $"class={_class.Value}");
            if (id.Success) attrlist.Insert(0, $"id={id.Value}");
            r.tag = "div";
            if (tag.Success) {
                r.tag = tag.Value;
            }
            if (source.Success) {
                r.sourceStr = source.Value;
                r.isSourceBinding = true;
            }
            var attrliststr = string.Join(" ", attrlist);
            if (!string.IsNullOrWhiteSpace(attrliststr)) {
                attrliststr = $" {attrliststr}";
            }
            r.open = $"<{r.tag}{attrliststr}>";
            r.close = $"</{r.tag}>";
            if (item.Success) {
                r.content = item.Value.Trim();
            }
            if (placehold.Success) {
                r.placeholdid = placehold.Value;
            }
            return r;
        }
        public string GetTreeStr(Tree<EmmetNode> tree) {
            if (!string.IsNullOrWhiteSpace(tree.data.htmlfragment)) {
                return tree.data.htmlfragment;
            }
            var r = new StringBuilder();
            switch (tree.data.tag) {
                case "input":
                    var input = tree.data.open.Insert(6, $" placeholder='{tree.data.content}'");
                    r.Append(input);
                    //foreach (var item in tree.Childen) {
                    //    r.Append(GetTreeStr(item));
                    //}
                    break;
                case "img":
                    var img = tree.data.open.Insert(4, $" src='{tree.data.content}'");
                    r.Append(img);
                    break;
                case "hr":
                case "br":
                    var hr = tree.data.open;
                    r.Append(hr);
                    break;
                default:
                    r.Append(tree.data.open);
                    r.Append(tree.data.content);
                    foreach (var item in tree.Childen) {
                        r.Append(GetTreeStr(item));
                    }
                    r.Append(tree.data.close);
                    break;
            }
            return r.ToString();
        }
    }
    public class Hsource {
        internal object source;
        public string key;
        public enum SourceType {
            nothing, entity, entities, dic, dics, table, row, meta
        }
        public Hsource(object source, SourceType stype) {
            this.source = source; this.stype = stype;
        }
        public Hsource(object source, SourceType stype, bool unGeneric) {
            this.source = source; this.stype = stype; this.unGeneric = unGeneric;
        }
        public SourceType stype { get; private set; }
        #region data port
        public void SetEntity<T>(T s) {
            source = s;
            stype = SourceType.entity;
        }
        public IDictionary<string, object> Dic {
            get {
                var temp = default(IDictionary<string, object>);
                switch (stype) {
                    case SourceType.dic: temp = source as IDictionary<string, object>; break;
                    case SourceType.entity: temp = this.source.GetEoo() as IDictionary<string, object>; break;
                }
                return temp;
            }
            set {
                source = value;
                stype = SourceType.dic;
            }
        }
        public IEnumerable<object> Entities {
            get => source as IEnumerable<object>;
            set {
                source = value;
                stype = SourceType.entities;
            }
        }
        public DataTable Table {
            get => source as DataTable;
            set {
                source = value;
                stype = SourceType.table;
            }
        }
        public DataRow Row {
            get => source as DataRow;
            set {
                source = value;
                stype = SourceType.row;
            }
        }
        public IEnumerable<IDictionary<string, object>> Dics {
            get => source as IEnumerable<IDictionary<string, object>>;
            set {
                source = value;
                stype = SourceType.dics;
            }
        }
        public IEnumerable<PropertyInfo> MetaData {
            get {
                if (source is IEnumerable<PropertyInfo>) {
                    return source as IEnumerable<PropertyInfo>;
                } else {
                    return source.GetType().GetProperties();
                }
            }
            set {
                source = value;
                stype = SourceType.meta;
            }
        }
        #endregion
        public object this[string key] {
            get {
                var temp = default(object);
                switch (stype) {
                    case SourceType.entity:
                    case SourceType.dic: temp = Dic[key]; break;
                    case SourceType.row: temp = Row[key]; break;
                    case SourceType.meta:
                        temp = MetaData.Single(i => i.Name == key);
                        break;
                }
                return temp;
            }
        }
        public IEnumerable<string> Keys {
            get {
                switch (stype) {
                    case SourceType.entity:
                    case SourceType.dic:
                        foreach (var item in Dic) {
                            yield return item.Key;
                        }
                        break;
                    case SourceType.row:
                        foreach (DataColumn item in Row.Table.Columns) {
                            yield return item.ColumnName;
                        }
                        break;
                    case SourceType.meta:
                        foreach (var item in MetaData) {
                            yield return item.Name;
                        }
                        break;
                }
            }
        }
        //
        /// <summary>
        /// 标记 emmet串的 数据模板是否是通用的{item}模板，
        /// 是{item} 为false,否则为true ,缺省不赋值为false，这个极大影响
        /// 绑定方式
        /// </summary>
        public bool unGeneric;
    }
    /// <summary>
    /// C#后端emmet表达式
    /// </summary>
    public class Emmet {
        readonly Hwriter writer;
        internal async Task<string> Build() {
            var result = await Task.Run(() => {
                var tree = writer.Merge();
                writer.Binding(tree);
                return writer.GetTreeStr(tree);
            });
            return result;
        }
        public Emmet() {
            writer = new Hwriter();
        }
        static public Emmet Binding(string emmet, params Hsource[] sources) {
            var that = new Emmet();
            var x = Regex.Matches(emmet, Hwriter.sourceTemplatepattern);
            var dic = new Dictionary<string, Match>();
            var idx = 0;
            foreach (Match item in x) {
                if (!dic.ContainsKey(item.Value))
                    dic.Add(item.Value, item);
            }
            foreach (var item in dic) {
                that.writer.bindingnote.Add(item.Value.Value, sources[idx++]);
            }
            that.writer.ParenthesisHandle(emmet);
            return that;
        }
    }
}
