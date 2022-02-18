// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Util.Ex;
namespace Util.Generator {
    //
    public class XmlFormat : CodeFormat {
        internal override string Indent => "    ";
        public bool isDoubleQuotation;
        public XmlFormat Attribute(string name, object value) {
            var x = new XmlFormatAttribute(name, value);
            x.owner = this;
            x.isDoubleQuotation = isDoubleQuotation;
            attributes.Add(x);
            return this;
        }
        public List<XmlFormatAttribute> attributes;
        public List<XmlFormat> children;
        public List<string> comments;
        public bool IsEmpty { get; private set; }
        internal string name, namesps;
        public string Name {
            get { return name; }
            set { name = value; }
        }
        public object Value { get; set; }
        public bool isSameLineByParent;
        protected string childContent;
        public string ChildContent {
            get {
                if (string.IsNullOrWhiteSpace(childContent)) {
                    ToString();
                }
                return childContent;
            }
        }
        public XmlFormat() {
            children = new List<XmlFormat>();
            attributes = new List<XmlFormatAttribute>();
            comments = new List<string>();
            this.name = "root";
        }
        public XmlFormat(string name) {
            children = new List<XmlFormat>();
            attributes = new List<XmlFormatAttribute>();
            comments = new List<string>();
            this.name = name;
        }
        /// <summary>
        /// 构造xmlformat  
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isd">属性是否使用双引号，默认为单引号</param>
        public XmlFormat(string name, bool isd) {
            children = new List<XmlFormat>();
            attributes = new List<XmlFormatAttribute>();
            comments = new List<string>();
            isDoubleQuotation = isd;
            this.name = name;
        }
        public void ResetFormat() {
            foreach (var item in children) {
                base.Push(ref item.indenton, Indent);
                item.ResetFormat();
            }
        }
        public IEnumerable<XmlFormat> Traversal() {
            return Traversal(this);
        }
        IEnumerable<XmlFormat> Traversal(XmlFormat root) {
            foreach (var item in root.children) {
                foreach (var it in Traversal(item)) {
                    yield return it;
                }
                yield return item;
            }
        }
        public XmlFormat Element(string name) {
            var x = new XmlFormat(name);
            x.isDoubleQuotation = isDoubleQuotation;
            Push(ref x.indenton, Indent);
            children.Add(x);
            return this;
        }
        public XmlFormat Element(XmlFormat child) {
            child.isDoubleQuotation = isDoubleQuotation;
            Push(ref child.indenton, Indent);
            children.Add(child);
            return this;
        }
        public XmlFormat ElementNs(string name, string namesps) {
            var x = new XmlFormat(name) { namesps = namesps };
            x.isDoubleQuotation = isDoubleQuotation;
            Push(ref x.indenton, Indent); children.Add(x);
            return this;
        }
        public XmlFormat Element(string name, string namesps, object value) {
            var x = new XmlFormat(name) { namesps = namesps, Value = value };
            x.isDoubleQuotation = isDoubleQuotation;
            Push(ref x.indenton, Indent); children.Add(x);
            return this;
        }
        public XmlFormat Element(string name, object value) {
            var x = new XmlFormat(name) { Value = value };
            x.isDoubleQuotation = isDoubleQuotation;
            Push(ref x.indenton, Indent); children.Add(x);
            return this;
        }
        //
        public XmlFormat ElementSameLine(string name) {
            var x = new XmlFormat(name);
            x.isDoubleQuotation = isDoubleQuotation;
            x.isSameLineByParent = true;
            Push(ref x.indenton, Indent);
            children.Add(x);
            return this;
        }
        public XmlFormat ElementSameLineNs(string name, string namesps) {
            var x = new XmlFormat(name) { namesps = namesps, isSameLineByParent = true };
            x.isDoubleQuotation = isDoubleQuotation;
            Push(ref x.indenton, Indent); children.Add(x);
            return this;
        }
        public XmlFormat ElementSameLine(string name, string namesps, object value) {
            var x = new XmlFormat(name) { namesps = namesps, Value = value, isSameLineByParent = true };
            x.isDoubleQuotation = isDoubleQuotation;
            Push(ref x.indenton, Indent); children.Add(x);
            return this;
        }
        public XmlFormat ElementSameLine(string name, object value) {
            var x = new XmlFormat(name) { Value = value, isSameLineByParent = true };
            x.isDoubleQuotation = isDoubleQuotation;
            Push(ref x.indenton, Indent); children.Add(x);
            return this;
        }
        //
        protected XmlFormat upon;
        public new StringBuilder root => base.root;
        public XmlFormat ElementAtChild(string name) {
            var x = new XmlFormat(name);
            x.isDoubleQuotation = isDoubleQuotation;
            Push(ref x.indenton, Indent);
            children.Add(x);
            x.upon = this;
            return x;
        }
        public XmlFormat ElementAtChildNs(string name, string namesps) {
            var x = new XmlFormat(name) { namesps = namesps };
            x.isDoubleQuotation = isDoubleQuotation;
            Push(ref x.indenton, Indent); children.Add(x);
            x.upon = this;
            return x;
        }
        public XmlFormat InsertAtChild(string name) {
            var x = new XmlFormat(name);
            x.isDoubleQuotation = isDoubleQuotation;
            Push(ref x.indenton, Indent);
            children.Insert(0, x);
            x.upon = this;
            return x;
        }
        public XmlFormat InsertAtChildNs(string name, string namesps) {
            var x = new XmlFormat(name) { namesps = namesps };
            x.isDoubleQuotation = isDoubleQuotation;
            Push(ref x.indenton, Indent);
            children.Insert(0, x);
            x.upon = this;
            return x;
        }
        public XmlFormat ElementAtChild(string name, string namesps, object value) {
            var x = new XmlFormat(name) { namesps = namesps, Value = value };
            x.isDoubleQuotation = isDoubleQuotation;
            Push(ref x.indenton, Indent); children.Add(x);
            upon = this;
            return x;
        }
        public XmlFormat ElementAtChild(string name, object value) {
            var x = new XmlFormat(name) { Value = value };
            x.isDoubleQuotation = isDoubleQuotation;
            Push(ref x.indenton, Indent); children.Add(x);
            x.upon = this;
            return x;
        }
        public XmlFormat InsertAtChild(string name, object value) {
            var x = new XmlFormat(name) { Value = value };
            x.isDoubleQuotation = isDoubleQuotation;
            Push(ref x.indenton, Indent);
            children.Insert(0, x);
            x.upon = this;
            return x;
        }
        public XmlFormat ElementAtChildPostValue(string name, object value) {
            var x = new XmlFormat(name) { Value = value, valuePosionPost = true };
            x.isDoubleQuotation = isDoubleQuotation;
            Push(ref x.indenton, Indent); children.Add(x);
            x.upon = this;
            return x;
        }
        //
        public XmlFormat ElementAtChildSameLine(string name) {
            var x = new XmlFormat(name);
            x.isSameLineByParent = true;
            x.isDoubleQuotation = isDoubleQuotation;
            Push(ref x.indenton, Indent);
            children.Add(x);
            x.upon = this;
            return x;
        }
        public XmlFormat ElementAtChildSameLineNs(string name, string namesps) {
            var x = new XmlFormat(name) { namesps = namesps, isSameLineByParent = true };
            x.isDoubleQuotation = isDoubleQuotation;
            Push(ref x.indenton, Indent); children.Add(x);
            x.upon = this;
            return x;
        }
        public XmlFormat ElementAtChildSameLine(string name, string namesps, object value) {
            var x = new XmlFormat(name) { namesps = namesps, Value = value, isSameLineByParent = true };
            x.isDoubleQuotation = isDoubleQuotation;
            Push(ref x.indenton, Indent); children.Add(x);
            upon = this;
            return x;
        }
        public XmlFormat ElementAtChildSameLine(string name, object value) {
            var x = new XmlFormat(name) { Value = value, isSameLineByParent = true };
            x.isDoubleQuotation = isDoubleQuotation;
            Push(ref x.indenton, Indent); children.Add(x);
            x.upon = this;
            return x;
        }
        public XmlFormat ElementAtChildSameLinePostValue(string name, object value) {
            var x = new XmlFormat(name) { Value = value, isSameLineByParent = true, valuePosionPost = true };
            x.isDoubleQuotation = isDoubleQuotation;
            Push(ref x.indenton, Indent); children.Add(x);
            x.upon = this;
            return x;
        }
        //
        public XmlFormat ElementAtChild(params XmlFormat[] ines) {
            if (ines.Length == 1) {
                ines[0].isDoubleQuotation = isDoubleQuotation;
                Push(ref ines[0].indenton);
                children.Add(ines[0]);
                ines[0].upon = this;
                return ines[0];
            } else if (ines.Length > 1) {
                foreach (var item in ines) {
                    item.isDoubleQuotation = isDoubleQuotation;
                    Push(ref item.indenton);
                    children.Add(item);
                    item.upon = this;
                }
            }
            return this;
        }
        public XmlFormat Upon(byte i = 1) {
            var r = default(XmlFormat);
            for (var j = 0; j < i; j++) {
                if (r == null) {
                    r = upon;
                } else {
                    r = r.upon;
                }
            }
            return r;
        }
        protected string GetAttributesFormat() {
            var r = string.Empty;
            r = string.Join(" ", from i in attributes select i.ToString());
            return r;
        }
        public XmlFormat Comment(string comment) {
            comments.Add(comment);
            return this;
        }
        public bool valuePosionPost;
        public override string ToString() {
            childContent = string.Empty;
            foreach (var item in comments) {
                root.AppendLine(string.Format("{0}<!--{1}-->", indenton, item));
            }
            if (upon != null && isSameLineByParent && children.Count == 0) {
                if (attributes.Count == 0) {
                    if (Value == null) {
                        if (string.IsNullOrEmpty(namesps)) {
                            upon.root.AppendFormat("<{0}/>", name);
                        } else {
                            upon.root.AppendFormat("<{0}:{1}/>", namesps, name);
                        }
                    } else {
                        if (string.IsNullOrEmpty(namesps)) {
                            upon.root.AppendFormat("<{0}>{1}</{2}>", name, Value, name);
                        } else {
                            upon.root.AppendFormat("<{0}:{1}>{2}</{3}:{4}>", namesps, name, Value, namesps, name);
                        }
                    }
                } else {
                    var attstr = GetAttributesFormat();
                    if (Value == null) {
                        if (string.IsNullOrEmpty(namesps)) {
                            upon.root.AppendFormat("<{0} {1}/>", name, attstr);
                        } else {
                            upon.root.AppendFormat("<{0}:{1} {2}/>", namesps, name, attstr);
                        }
                    } else {
                        if (string.IsNullOrEmpty(namesps)) {
                            upon.root.AppendFormat("<{0} {1}>{2}</{3}>", name, attstr, Value, name);
                        } else {
                            upon.root.AppendFormat("<{0}:{1} {2}>{3}</{4}:{5}>", namesps, name, attstr, Value, namesps, name);
                        }
                    }
                }
            } else if (children.Count == 0) {
                if (attributes.Count == 0) {
                    if (Value == null) {
                        if (string.IsNullOrEmpty(namesps)) {
                            root.AppendLine(string.Format("{0}<{1}/>", indenton, name));
                        } else {
                            root.AppendLine(string.Format("{0}<{1}:{2}/>", indenton, namesps, name));
                        }
                    } else {
                        if (string.IsNullOrEmpty(namesps)) {
                            root.AppendLine(string.Format("{0}<{1}>{2}</{3}>", indenton, name, Value, name));
                        } else {
                            root.AppendLine(string.Format("{0}<{1}:{2}>{3}</{4}:{5}>", indenton, namesps, name, Value, namesps, name));
                        }
                    }
                } else {
                    var attstr = GetAttributesFormat();
                    if (Value == null) {
                        if (string.IsNullOrEmpty(namesps)) {
                            root.AppendLine(string.Format("{0}<{1} {2}/>", indenton, name, attstr));
                        } else {
                            root.AppendLine(string.Format("{0}<{1}:{2} {3}/>", indenton, namesps, name, attstr));
                        }
                    } else {
                        if (string.IsNullOrEmpty(namesps)) {
                            root.AppendLine(string.Format("{0}<{1} {2}>{3}</{4}>", indenton, name, attstr, Value, name));
                        } else {
                            root.AppendLine(string.Format("{0}<{1}:{2} {3}>{4}</{5}:{6}>", indenton, namesps, name, attstr, Value, namesps, name));
                        }
                    }
                }
            } else if (children.Count == 1 && isSameLineByParent) {
                if (attributes.Count == 0) {
                    if (Value == null) {
                        if (string.IsNullOrEmpty(namesps)) {
                            root.AppendFormat("{0}<{1}>", indenton, name);
                            foreach (var item in children) {
                                childContent += root.Append(item.ToString().Trim()).ToString();
                            }
                            root.AppendLine(string.Format("</{0}>", name));
                        } else {
                            root.AppendFormat("{0}<{1}:{2}>", indenton, namesps, name);
                            foreach (var item in children) {
                                childContent += root.Append(item.ToString().Trim()).ToString();
                            }
                            root.AppendLine(string.Format("</{0}>", name));
                        }
                    } else {
                        if (string.IsNullOrEmpty(namesps)) {
                            root.AppendFormat("{0}<{1}>", indenton, name);
                            if (!valuePosionPost)
                                root.AppendFormat("{0}", Value);
                            foreach (var item in children) {
                                childContent += root.Append(item.ToString().Trim()).ToString();
                            }
                            if (valuePosionPost)
                                root.AppendFormat("{0}", Value);
                            root.AppendLine(string.Format("</{0}>", name));
                        } else {
                            root.AppendFormat("{0}<{1}:{2}>", indenton, namesps, name);
                            if (!valuePosionPost)
                                root.AppendFormat("{0}", Value);
                            foreach (var item in children) {
                                childContent += root.Append(item.ToString().Trim()).ToString();
                            }
                            if (valuePosionPost)
                                root.AppendFormat("{0}", Value);
                            root.AppendLine(string.Format("</{0}:{1}>", namesps, name));
                        }
                    }
                } else {
                    var attstr = GetAttributesFormat();
                    if (Value == null) {
                        if (string.IsNullOrEmpty(namesps)) {
                            root.AppendFormat("{0}<{1} {2}>", indenton, name, attstr);
                            foreach (var item in children) {
                                childContent += root.Append(item.ToString().Trim()).ToString();
                            }
                            root.AppendLine(string.Format("</{0}>", name));
                        } else {
                            root.AppendFormat("{0}<{1}:{2} {3}>", indenton, namesps, name, attstr);
                            foreach (var item in children) {
                                childContent += root.Append(item.ToString().Trim()).ToString();
                            }
                            root.AppendLine(string.Format("</{0}>", name));
                        }
                    } else {
                        if (string.IsNullOrEmpty(namesps)) {
                            root.AppendFormat("{0}<{1} {2}>", indenton, name, attstr);
                            if (!valuePosionPost)
                                root.AppendFormat("{0}", Value);
                            foreach (var item in children) {
                                childContent += root.Append(item.ToString().Trim()).ToString();
                            }
                            if (valuePosionPost)
                                root.AppendFormat("{0}", Value);
                            root.AppendLine(string.Format("</{0}>", name));
                        } else {
                            root.AppendFormat("{0}<{1}:{2} {3}>", indenton, namesps, name, attstr);
                            if (!valuePosionPost)
                                root.AppendFormat("{0}", Value);
                            foreach (var item in children) {
                                childContent += root.Append(item.ToString().Trim()).ToString();
                            }
                            if (valuePosionPost)
                                root.AppendFormat("{0}", Value);
                            root.AppendLine(string.Format("</{0}:{1}>", namesps, name));
                        }
                    }
                }
            } else {
                if (attributes.Count == 0) {
                    if (Value == null) {
                        if (string.IsNullOrEmpty(namesps)) {
                            root.AppendLine(string.Format("{0}<{1}>", indenton, name));
                            foreach (var item in children) {
                                childContent += root.Append(item.ToString());
                            }
                            root.AppendLine(string.Format("{0}</{1}>", indenton, name));
                        } else {
                            root.AppendLine(string.Format("{0}<{1}:{2}>", indenton, namesps, name));
                            foreach (var item in children) {
                                childContent += root.Append(item.ToString());
                            }
                            root.AppendLine(string.Format("{0}</{1}>", indenton, name));
                        }
                    } else {
                        if (string.IsNullOrEmpty(namesps)) {
                            root.AppendLine(string.Format("{0}<{1}>", indenton, name));
                            if (!valuePosionPost)
                                root.AppendLine(string.Format("{0}  {1}", indenton, Value));
                            foreach (var item in children) {
                                childContent += root.Append(item.ToString());
                            }
                            if (valuePosionPost)
                                root.AppendLine(string.Format("{0}  {1}", indenton, Value));
                            root.AppendLine(string.Format("{0}</{1}>", indenton, name));
                        } else {
                            root.AppendLine(string.Format("{0}<{1}:{2}>", indenton, namesps, name));
                            if (!valuePosionPost)
                                root.AppendLine(string.Format("{0}  {1}", indenton, Value));
                            foreach (var item in children) {
                                childContent += root.Append(item.ToString());
                            }
                            if (valuePosionPost)
                                root.AppendLine(string.Format("{0}  {1}", indenton, Value));
                            root.AppendLine(string.Format("{0}</{1}:{2}>", indenton, namesps, name));
                        }
                    }
                } else {
                    var attstr = GetAttributesFormat();
                    if (Value == null) {
                        if (string.IsNullOrEmpty(namesps)) {
                            root.AppendLine(string.Format("{0}<{1} {2}>", indenton, name, attstr));
                            foreach (var item in children) {
                                childContent += root.Append(item.ToString());
                            }
                            root.AppendLine(string.Format("{0}</{1}>", indenton, name));
                        } else {
                            root.AppendLine(string.Format("{0}<{1}:{2} {3}>", indenton, namesps, name, attstr));
                            foreach (var item in children) {
                                childContent += root.Append(item.ToString());
                            }
                            root.AppendLine(string.Format("{0}</{1}>", indenton, name));
                        }
                    } else {
                        if (string.IsNullOrEmpty(namesps)) {
                            root.AppendLine(string.Format("{0}<{1} {2}>", indenton, name, attstr));
                            if (!valuePosionPost)
                                root.AppendLine(string.Format("{0}  {1}", indenton, Value));
                            foreach (var item in children) {
                                childContent += root.Append(item.ToString());
                            }
                            if (valuePosionPost)
                                root.AppendLine(string.Format("{0}  {1}", indenton, Value));
                            root.AppendLine(string.Format("{0}</{1}>", indenton, name));
                        } else {
                            root.AppendLine(string.Format("{0}<{1}:{2} {3}>", indenton, namesps, name, attstr));
                            if (!valuePosionPost)
                                root.AppendLine(string.Format("{0}  {1}", indenton, Value));
                            foreach (var item in children) {
                                childContent += root.Append(item.ToString());
                            }
                            if (valuePosionPost)
                                root.AppendLine(string.Format("{0}  {1}", indenton, Value));
                            root.AppendLine(string.Format("{0}</{1}:{2}>", indenton, namesps, name));
                        }
                    }
                }
            }
            return base.ToString();
        }
        public static XmlFormat Load(string path) => CodeFormat.ReadDocumentByPath<XmlFormat>(path);
        protected override void OnLoad(string doc) {
            var tags = new List<(string value, int key)>();
            var words = new List<string>();
            //var idx = 0;
            //在script 内部 有 >,< => 这些东西需要处理，遇到以后需要忽略，
            //因为这些部分都存在于内容里所以只在内容加以验证项即可
            //用html传入文本预处理来解决这个问题，加入正则替换
            doc = doc.Replace("&", "&amp;");
            doc = Regex.Replace(doc, @"<script[^>]*>([\s\S](?!<script))*?</script>", "");
            //doc = Regex.Replace(doc, @"<script[^>]*>([\s\S](?!<script))*?</script>", "&gt;");
            for (var i = 0; i < doc.Length; i++) {
                if (doc[i] == '<') {
                    var tag = string.Empty;
                    var idx2 = 0;
                    var idx3 = 0;
                    foreach (var it in doc.Substring(i)) {
                        tag += it;
                        if (it == '/') {
                            idx3 = idx2;
                        }
                        if (it == '>') {
                            if (idx3 > 1 && idx3 != idx2 - 1) idx3 = 0;
                            tags.Add((tag, idx3));
                            i += idx2 - 1;
                            break;
                        }
                        idx2++;
                    }
                }
                if (doc[i] == '>') {
                    var idx4 = 0;
                    var word = string.Empty;
                    foreach (var it in doc.Substring(i)) {
                        word += it;
                        if (it == '<') {
                            words.Add(word);
                            i += idx4 - 1;
                            break;
                        }
                        idx4++;
                    }
                }
            }
            words.Add("");
            words.Reverse();
            tags.Reverse();
            //const string wordPatten = @"(?<=</?)\w+(?=>)";
            const string wordPatten = @"(?<=</?)[\w-_]+\b(?=[\s\S]*>)";
            //\b(\w+)\s{0,2}=\s{0,2}(?:(?:\"([^\"]*)\")|(?:'([^']*)'))  这条是调好的正则
            const string attributePattern = @"\b([A-Za-z\-:]+)\s{0,2}=\s{0,2}(?:(?:""([^""]*)"")|(?:'([^']*)')|(?:([^""'>]*)))";
            const string attributePattern2 = @"(?<=\s+\b)\w+(?=\b\s*)(?![=\-])";
            const string contentPattern = @"(?<=[><])[\s\S]*(?=[><])";
            const string annotationPattern = @"<!--[\s\S]*-->";
            var current = this;
            for (var i = 0; i < tags.Count; i++) {
                var item = tags[i];
                var word = words[i];
                var tag = Regex.Match(item.value, wordPatten).Value;
                var attrs = Regex.Matches(item.value, attributePattern);
                var attrs2 = Regex.Matches(item.value, attributePattern2);
                var content = Regex.Match(word, contentPattern).Value;
                if (string.IsNullOrWhiteSpace(tag)) {
                    var annotation = Regex.Match(item.value, annotationPattern).Value;
                    Value += content.Replace("<", "").Replace(">", "");
                    continue;
                }
                if (item.key == 1) {
                    //current = current.AddInChild(new HtmlNode(tag) { tagdomain = item.value });
                    current = current.ElementAtChild(tag);
                } else if (item.key == 0 && tag == current?.name) {
                    //current = current?.Upon();
                    SetAttribute(current, attrs, attrs2, item.value);
                    current.Value += content;
                    current = current?.Upon();
                } else if (item.key == 0) {
                    SetAttribute(current, attrs, attrs2, item.value);
                    current.Value = null;
                } else {
                    //current.Add(new HtmlNode(tag) { tagdomain = item.value });
                    var tempxf = current.ElementAtChild(tag);
                    SetAttribute(tempxf, attrs, attrs2, item.value);
                }
            }
            //nodes.DeepOrBreadthFirst = false;
            //foreach (var item in nodes.Traversal()) {
            //    item.Childen.Reverse();
            //}
            foreach (var item in current.Traversal()) {
                if (item.children?.Count > 0)
                    item.children.Reverse();
            }
            //Console.WriteLine(current.ToString());
            //xdoc = XDocument.Parse(this.children[0].ToString());
            //Console.WriteLine(this);
        }
        private static void SetAttribute(XmlFormat xf, MatchCollection attrs, MatchCollection attrs2, string value) {
            foreach (Match it in attrs) {
                if (!xf.attributes.Exists(iu => iu.name.Trim() == it.Groups[1].Value.Trim())) {
                    var pv = string.IsNullOrWhiteSpace(it.Groups[2].Value) ? it.Groups[3].Value : it.Groups[2].Value;
                    pv = string.IsNullOrWhiteSpace(pv) ? it.Groups[4].Value : pv;
                    xf.Attribute(it.Groups[1].Value, pv);
                }
            }
            foreach (Match it in attrs2) {
                //排查是否处于引号之内,采用的方法是判断前子串中的单双引号是否为偶数
                if (value.Substring(0, value.IndexOf(it.Value)).SubstringCount("\"") % 2 != 0) continue;
                if (!xf.attributes.Exists(iu => iu.name.Trim() == it.Value.Trim())) {
                    xf.Attribute(it.Value, "true");
                }
            }
        }
        public XmlFormat Find(string name) {
            foreach (var item in Traversal()) {
                if (name.Trim() == item.name) {
                    return item;
                }
            }
            return null;
        }
        public XmlFormatAttribute FindAttribute(string name) {
            if (attributes.Count > 0)
                return (from i in attributes where i.name == name select i).First();
            return null;
        }
    }
    public class XmlFormatAttribute {
        internal XmlFormat owner;
        public string name; public object value;
        public bool isDoubleQuotation;
        public XmlFormatAttribute(bool idq) {
            isDoubleQuotation = idq;
        }
        public XmlFormatAttribute(string name, object value) {
            this.name = name; this.value = value;
        }
        public override string ToString() {
            var r = string.Empty;
            if (isDoubleQuotation) {
                //如果value中包含"则改为'
                var v = value?.ToString().Replace("\"", "'");
                if (name.Contains("xmlns")) {
                    r = string.Format("{0}{1}{2}=\"{3}\"", Environment.NewLine, owner.indenton + owner.Indent, name, v);
                } else {
                    r = string.Format("{0}=\"{1}\"", name, v);
                }
            } else {
                //如果value中包含'则改为"
                var v = value?.ToString().Replace("'", "\"");
                if (name.Contains("xmlns")) {
                    r = string.Format("{0}{1}{2}='{3}'", Environment.NewLine, owner.indenton + owner.Indent, name, v);
                } else {
                    r = string.Format("{0}='{1}'", name, v);
                }
            }
            return r;
        }
    }
}
