// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Util.Ex;

namespace Util.Generator {
    public class CsCommentAbility : CodeFormat {
        public string comment;
        protected string formatcomment;
        public T SetComment<T>(string commen, bool ismultiline = false, string tag = "summary") where T : CsCommentAbility {
            this.comment = commen;
            var lenmark = 70;
            var comlist = new List<string>();
            comlist = commen.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (comlist.Count == 1 && commen.Length > lenmark) {
                comlist.Clear();
                var count = commen.Length / lenmark;
                var idx = 0;
                for (var i = 0; i < count; i++) {
                    comlist.Add(commen.Substring(idx, lenmark));
                    idx += lenmark;
                }
                var lastcom = commen.Substring(idx, commen.Length - idx);
                if (!string.IsNullOrEmpty(lastcom)) {
                    comlist.Add(lastcom);
                }
            }
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(tag)) {
                if (ismultiline) {
                    sb.AppendLine($"{indenton}/// <{tag}>");
                    foreach (var item in comlist) {
                        sb.AppendLine($"{indenton}/// {item}");
                    }
                    sb.AppendLine($"{indenton}/// </{tag}>");
                } else {
                    sb.Append($"{indenton}///<{tag}>");
                    sb.Append($"{string.Concat(comlist)}");
                    sb.Append($"</{tag.Split(' ').First()}>{Environment.NewLine}");
                }
                formatcomment += sb.ToString();
            } else {
                formatcomment = null;
                if (comlist.Count > 1) {
                    sb.AppendLine($"{indenton}/*");
                    foreach (var item in comlist) {
                        sb.AppendLine($"{indenton}* {item}");
                    }
                    sb.AppendLine($"{indenton}*/");
                } else {
                    sb.AppendLine($"{indenton}// {this.comment}");
                }
                formatcomment = sb.ToString();
            }
            return this as T;
        }
    }
    /// <summary>
    /// 一个C# 代码格式化生成工具
    /// 注意里面使用了大量的Start开关命名的成员
    /// 不要一时兴起加以改变，因为这完全是在链式调用
    /// 语境下的产物，本来在一般情况下使用Get更加贴切的
    /// </summary>
    public class CsNamespace : CsCommentAbility {
        string usingarea;
        public List<string> usings;
        public List<CsDelegate> delegats;
        public List<CsEnum> enums;
        public CsType StartClass(string namePanttern, params object[] namePart) {
            var name = string.Format(namePanttern, namePart);
            return StartClass(name);
        }
        public CsType StartClass(string name, string visit = "public", string inhlist = "") {
            var cls = new CsType(name, visit, "class", inhlist: inhlist);
            this.Push(ref cls.indenton);
            cls.namesps = this;
            typs.Add(cls);
            return cls;
        }
        public CsType StartInterface(string name, string visit = "public", string inhlist = "") {
            if (name[0] != 'I')
                name = "I" + name;
            var interfs = new CsType(name, visit, "interface", inhlist: inhlist);
            this.Push(ref interfs.indenton);
            interfs.namesps = this;
            typs.Add(interfs);
            return interfs;
        }
        public CsType StartStruct(string name, string visit = "public", string inhlist = "") {
            var struc = new CsType(name, visit, "struct", inhlist: inhlist);
            this.Push(ref struc.indenton);
            struc.namesps = this;
            typs.Add(struc);
            return struc;
        }
        public CsNamespace Using(params string[] importns) {
            if (importns.Length == 1) {
                var x = importns[0];
                var x2 = Regex.Replace(x, @"(?<=,|^)[\.]", "System.");
                x2 = Regex.Replace(x2, @"[\.](?=\s*?,|\s*?$)", "");
                var x3 = x2.Split(',');
                foreach (var item in x3) {
                    this.usings.Add(string.Format("{0}using {1};", Indent, item));
                }
            } else {
                var x = string.Join(",", importns);
                var x2 = Regex.Replace(x, @"(?<=,|^)[\.]", "System.");
                x2 = Regex.Replace(x2, @"[\.](?=\s*?,|\s*?$)", "");
                var x3 = x2.Split(',');
                foreach (var item in x3) {
                    this.usings.Add(string.Format("{0}using {1};", Indent, item));
                }
            }
            usingarea = string.Join(Environment.NewLine, this.usings);
            return this;
        }
        public CsNamespace Delegat(string name, string paramlist, string rtype = "void", string visit = "public") {
            var x = new CsDelegate(name, paramlist, rtype);
            Push(ref x.indenton);
            delegats.Add(x);
            return this;
        }
        public CsNamespace Enum(string name, params string[] evs) {
            var x = new CsEnum(name, evs);
            Push(ref x.indenton);
            enums.Add(x);
            return this;
        }
        public CsEnum StartEnum(string name) {
            var x = new CsEnum(name);
            Push(ref x.indenton);
            enums.Add(x);
            return x;
        }
        public List<CsType> typs;
        public IEnumerable<CsType> FindClass(Func<CsType, bool> filter = null) {
            var x = from item in typs
                    where item.typ.Contains("class")
                    select item;
            if (filter != null)
                x = from item in x where filter(item) select item;
            return x;
        }
        public IEnumerable<CsType> FindInterface(Func<CsType, bool> filter = null) {
            var x = from item in typs
                    where item.typ.Contains("interface")
                    select item;
            if (filter != null)
                x = from item in x where filter(item) select item;
            return x;
        }
        public IEnumerable<CsType> FindStruct(Func<CsType, bool> filter = null) {
            var x = from item in typs
                    where item.typ.Contains("struct")
                    select item;
            if (filter != null)
                x = from item in x where filter(item) select item;
            return x;
        }
        public CsNamespace() {
            typs = new List<CsType>();
            delegats = new List<CsDelegate>();
            enums = new List<CsEnum>();
            usings = new List<string>();
            usingarea = Indent + "//";
        }
        public string ToString(string name) {
            root.Clear();
            if (!string.IsNullOrEmpty(comment)) {
                root.Append(formatcomment);
            }
            root.AppendLine(string.Format("namespace {0} {1}", name, "{"));
            root.AppendLine(usingarea);
            foreach (var item in enums) {
                root.Append(item.ToString());
            }
            foreach (var item in delegats) {
                root.Append(item.ToString());
            }
            foreach (var item in typs) {
                root.Append(item.ToString());
            }
            root.Append("}");
            return base.ToString();
        }
    }
    public abstract class CsMember : CsCommentAbility {
        public string visit;
        /// <summary>
        /// 方法的返回类型
        /// </summary>
        public string typ;
        public string name;
        public string attribute;
        public T SetAttribute<T>(params string[] attributes) where T : CsMember {
            var list = attributes;
            if (attributes != null && !string.IsNullOrEmpty(this.attribute)) {
                list = attributes.Concat(this.attribute.Split(new[] { ",", Environment.NewLine, base.indenton }, StringSplitOptions.RemoveEmptyEntries)).ToArray();
            }
            this.attribute = string.Join(string.Format(",{0}{1}", Environment.NewLine, base.indenton), list);
            return this as T;
        }
        public T SetAttributeOne<T>(params string[] attributes) where T : CsMember {
            this.attribute = string.Join(string.Format(",{0}{1}", Environment.NewLine, base.indenton), attributes);
            return this as T;
        }
        //
        protected abstract string ToStringDefault();
        public override string ToString() {
            if (!string.IsNullOrEmpty(comment)) {
                root.Append(formatcomment);
            }
            if (!string.IsNullOrEmpty(attribute)) {
                root.AppendFormat("{0}[", indenton);
                root.Append(this.attribute);
                root.Append("]");
                root.AppendLine();
            }
            return ToStringDefault();
        }
    }
    public class CsType : CsMember {
        internal CsType(string name, string visit = "public", string typ = "class", string attribute = null, string inhlist = null) {
            this.name = name; this.typ = typ; this.visit = visit;
            this.inhlist = inhlist;
            this.attribute = attribute;
            //
            fields = new List<CsField>();
            props = new List<CsProperty>();
            methods = new List<CsMethod>();
            events = new List<CsEvent>();
            indxs = new List<CsIndx>();
        }
        public CsNamespace EndType() {
            return this.namesps;
        }
        internal CsNamespace namesps;
        public string inhlist;
        //string constructorarea, destructorarea;
        internal List<CsField> fields;
        internal List<CsProperty> props;
        internal List<CsEvent> events;
        internal List<CsMethod> methods;
        internal List<CsIndx> indxs;
        //
        public CsMethod StartConstructor(string paramestr, string inhlist = null, string visit = "public") {
            var meth = default(CsMethod);
            if (this.name.IndexOf('<') > 0) {
                meth = new CsMethod(this.name.Substring(0, this.name.IndexOf('<')), "", paramestr, inhlist, visit);
            } else {
                meth = new CsMethod(this.name, "", paramestr, inhlist, visit);
            }
            meth.isctor = true;
            base.Push(ref meth.indenton);
            meth.InitialLogicMangement();
            meth.owner = this;
            methods.Add(meth);
            return meth;
        }
        public CsMethod StartConstructor(string paramsPanttern, params object[] ps) {
            var paramestr = string.Format(paramsPanttern, ps);
            return StartConstructor(paramestr);
        }
        public CsType Destructor() { return this; }
        public CsType Field(string fname, string type, string visit = "private") {
            var field = new CsField(fname, type, visit);
            Push(ref field.indenton);
            field.visit = visit;
            this.fields.Add(field);
            return this;
        }
        public CsField StartField(string fname, string type, string visit = "private") {
            var field = new CsField(fname, type, visit);
            field.owner = this;
            Push(ref field.indenton);
            this.fields.Add(field);
            return field;
        }
        public CsProperty StartProperty(string name, string type, bool isoneline = false) {
            var curp = new CsProperty(name, type);
            curp.owner = this;
            Push(ref curp.indenton);
            props.Add(curp);
            curp.IsOneLine = isoneline;
            return curp;
        }
        public CsProperty StartProperty(string name, string type, string visit) {
            var curp = new CsProperty(name, type, visit);
            curp.owner = this;
            Push(ref curp.indenton);
            props.Add(curp);
            return curp;
        }
        public CsProperty StartProperty(CsField f) {
            var pn = string.Empty;
            if (f.name.First() == '_')
                pn = f.name.Replace("_", "").First().ToString().ToUpper() + f.name.Substring(2);
            else
                pn = f.name.First().ToString().ToUpper() + f.name.Substring(1);
            var curp = new CsProperty(pn, f.typ);
            curp.owner = this;
            Push(ref curp.indenton);
            props.Add(curp);
            return curp;
        }
        public CsIndx StartIndex(string returntype, string paramlist, string visit = "public") {
            var x = new CsIndx(returntype, paramlist, visit);
            x.owner = this;
            Push(ref x.indenton);
            indxs.Add(x);
            return x;
        }
        public CsType Property(CsField f) {
            var pn = string.Empty;
            if (f.name.First() == '_')
                pn = f.name.Replace("_", "").First().ToString().ToUpper() + f.name.Substring(2);
            else
                pn = f.name.First().ToString().ToUpper() + f.name.Substring(1);
            var curp = new CsProperty(pn, f.typ);
            base.Push(ref curp.indenton);
            props.Add(curp);
            return this;
        }
        public CsType Property(CsField f, string visit) {
            var pn = string.Empty;
            if (f.name.First() == '_')
                pn = f.name.Replace("_", "").First().ToString().ToUpper() + f.name.Substring(2);
            else
                pn = f.name.First().ToString().ToUpper() + f.name.Substring(1);
            var curp = new CsProperty(pn, f.typ, visit: visit);
            base.Push(ref curp.indenton);
            props.Add(curp);
            return this;
        }
        public CsType Property(string name, string type) {
            var curp = new CsProperty(name, type);
            base.Push(ref curp.indenton);
            props.Add(curp);
            return this;
        }
        public CsType Property(string name, string type, string visit) {
            var curp = new CsProperty(name, type, visit);
            base.Push(ref curp.indenton);
            props.Add(curp);
            return this;
        }
        public CsType PropertyReadOnly(string name, string type) {
            var curp = new CsProperty(name, type);
            curp.isReadOnly = true;
            base.Push(ref curp.indenton);
            props.Add(curp);
            return this;
        }
        public CsEvent StartEvent(string name, string type) {
            var x = new CsEvent(name, type);
            x.owner = this;
            Push(ref x.indenton);
            events.Add(x);
            return x;
        }
        public CsType Event(string name, string type) {
            var x = new CsEvent(name, type);
            Push(ref x.indenton);
            events.Add(x);
            return this;
        }
        public CsMethod StartMethod(CsMethod method) {
            method.Push(this);
            method.owner = this;
            methods.Add(method);
            return method;
        }
        public CsMethod StartMethod(string mname) {
            var meth = new CsMethod(mname, "void");
            base.Push(ref meth.indenton);
            meth.InitialLogicMangement();
            meth.owner = this;
            methods.Add(meth);
            return meth;
        }
        public CsMethod StartMethod(string mname, string paramestr) {
            var meth = new CsMethod(mname, "void", paramestr);
            base.Push(ref meth.indenton);
            meth.InitialLogicMangement();
            meth.owner = this;
            methods.Add(meth);
            return meth;
        }
        public CsMethod StartMethod(string mname, string paramestr, string returntype) {
            var meth = new CsMethod(mname, returntype, paramestr);
            base.Push(ref meth.indenton);
            meth.InitialLogicMangement();
            meth.owner = this;
            methods.Add(meth);
            return meth;
        }
        public CsMethod StartMethod(string mname, string paramestr, string returntype, string visit) {
            var meth = new CsMethod(mname, returntype, paramestr, visit);
            base.Push(ref meth.indenton);
            meth.InitialLogicMangement();
            meth.owner = this;
            methods.Add(meth);
            return meth;
        }
        public CsMethod StartMethod(string mname, string returntype, string visit, string paramPanttern, params object[] ps) {
            var paramestr = string.Format(paramPanttern, ps);
            var meth = new CsMethod(mname, returntype, paramestr, visit);
            base.Push(ref meth.indenton);
            meth.InitialLogicMangement();
            meth.owner = this;
            methods.Add(meth);
            return meth;
        }
        public CsMethod StartMethod(string mname, string returntype, string paramPanttern, params object[] ps) {
            var paramestr = string.Format(paramPanttern, ps);
            var meth = new CsMethod(mname, returntype, paramestr);
            base.Push(ref meth.indenton);
            meth.InitialLogicMangement();
            meth.owner = this;
            methods.Add(meth);
            return meth;
        }
        public CsType DeclareMethod(string mname, string returntype, string paramestr, string visit = "", string[] attributes = null) {
            var meth = new CsMethod(mname, returntype, paramestr, visit);
            base.Push(ref meth.indenton);
            meth.isInterface = true;
            methods.Add(meth);
            if (attributes != null)
                meth.SetAttribute<CsMethod>(attributes);
            return this;
        }
        public CsType DeclareProperty(string pname, string type, string[] attributes = null) {
            var prop = new CsProperty(pname, type);
            base.Push(ref prop.indenton);
            prop.isInterface = true;
            props.Add(prop);
            if (attributes != null)
                prop.SetAttribute<CsProperty>(attributes);
            return this;
        }
        public IEnumerable<CsMethod> FindMethod(Func<CsMethod, bool> filter) {
            return from it in methods where filter.Invoke(it) select it;
        }
        public CsMethod FindMethod(string name, string paramestr) {
            var pstr = string.Join(",", paramestr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            return (from it in methods
                    let itpstr = string.Join(",", it.paramlist.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    where it.name == name && itpstr == pstr
                    select it).First();
        }
        public IEnumerable<CsProperty> FindProps(Func<CsProperty, bool> filter) => from it in this.props where filter.Invoke(it) select it;
        public IEnumerable<CsField> FindFields(Func<CsField, bool> filter) => from it in this.fields where filter.Invoke(it) select it;
        public IEnumerable<CsMethod> FindMethods(Func<CsMethod, bool> filter) => from it in this.methods where filter.Invoke(it) select it;
        string[] InheritList => this.inhlist.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        public CsType BaseClass {
            get {
                if (InheritList == null) return null;
                var x = this.namesps.FindClass(i => InheritList.Contains(i.name));
                if (x.Count() >= 1)
                    return x.First();
                return null;
            }
        }
        public CsType[] BaseInterfaces => this.namesps.FindInterface(i => InheritList.Contains(i.name)).ToArray();
        public CsType SetInherit(params string[] inhlist) {
            this.inhlist = string.Join(",", inhlist);
            return this;
        }
        public CsGenericFragment gf;
        /// <summary>
        /// T struct,U,W new() ICustomer
        /// =>
        /// <T,U,W> where T:struct where W new(),ICustomer
        /// </summary>
        /// <param name="gps">generic parameters</param>
        public CsType SetGenericParameters(string gps) {
            if (string.IsNullOrEmpty(gps)) {
                return this;
            }
            gf = new CsGenericFragment(gps); return this;
        }
        protected override string ToStringDefault() {
            if (string.IsNullOrEmpty(this.inhlist)) {
                if (gf == null)
                    root.AppendLine(string.Format("{0}{1} {2} {3} {4}", indenton, visit, typ, name, "{"));
                else
                    root.AppendLine(string.Format("{0}{1} {2} {3}{4} {5}{6}", indenton, visit, typ, name, gf.GParams, gf.GWheres, "{"));
                foreach (var item in fields) {
                    root.Append(item.ToString());
                }
                foreach (var item in props) {
                    root.Append(item.ToString());
                }
                foreach (var item in indxs) {
                    root.Append(item.ToString());
                }
                foreach (var item in events) {
                    root.Append(item.ToString());
                }
                foreach (var item in methods) {
                    root.Append(item.ToString());
                }
                root.AppendLine(string.Format("{0}{1}", indenton, "}"));
            } else {
                if (gf == null)
                    root.AppendLine(string.Format("{0}{1} {2} {3} : {4} {5}", indenton, visit, typ, name, inhlist, "{"));
                else
                    root.AppendLine(string.Format("{0}{1} {2} {3}{4} : {5} {6} {7}", indenton, visit, typ, name, gf.GParams, inhlist, gf.GWheres, "{"));
                foreach (var item in fields) {
                    root.Append(item.ToString());
                }
                foreach (var item in props) {
                    root.Append(item.ToString());
                }
                foreach (var item in indxs) {
                    root.Append(item.ToString());
                }
                foreach (var item in events) {
                    root.Append(item.ToString());
                }
                foreach (var item in methods) {
                    root.Append(item.ToString());
                }
                root.AppendLine(string.Format("{0}{1}", indenton, "}"));
            }
            return root.ToString();
        }
    }
    public class CsEnum : CsMember {
        readonly List<string> values;
        public CsEnum(string name, string[] values = null) {
            this.name = name;
            if (values != null)
                this.values = new List<string>(values);
            else
                this.values = new List<string>();
            this.visit = "public";
        }
        public CsEnum AddItem(string value) {
            this.values.Add(value);
            return this;
        }
        public CsEnum AddItem(string value, string command) {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("///<summary>{0}</summary>", command));
            sb.Append(string.Format("{0}{1}", base.indenton + Indent, value));
            this.values.Add(sb.ToString());
            return this;
        }
        protected override string ToStringDefault() {
            root.Clear();
            root.AppendLine(string.Format("{0}public enum {1} {2}", this.indenton, name, "{"));
            foreach (var item in values) {
                root.AppendLine(string.Format("{0}{1},", this.indenton + Indent, item));
            }
            root.AppendLine(string.Format("{0}{1}", indenton, "}"));
            return root.ToString();
        }
    }
    public class CsDelegate : CsMember {
        public CsDelegate(string name, string parames, string rtype) {
            this.name = name;
            this.visit = "public";
            this.paramlist = parames;
            this.typ = rtype;
        }
        public string paramlist;

        protected override string ToStringDefault() {
            root.AppendLine($"{this.indenton}{this.visit} delegate {this.typ} {this.name}({this.paramlist});");
            return root.ToString();
        }
    }
    public class CsField : CsMember {
        public CsField(string name, string type, string visit = "private") {
            this.name = name; this.typ = type; this.visit = visit;
        }
        public void Init(string exp) {
            this.exp = exp;
        }
        public CsType owner;
        private string exp;

        public CsType EndField() { return owner; }
        protected override string ToStringDefault() {
            if (string.IsNullOrWhiteSpace(this.visit)) {
                root.AppendLine(string.Format("{0}{1} {2};", indenton, typ, name));
            } else {
                root.AppendLine(string.Format("{0}{1} {2} {3};", indenton, visit, typ, name));
            }
            if (!string.IsNullOrWhiteSpace(this.exp)) {
                root.Insert(root.Length - 3, $" = {this.exp}");
            }
            return root.ToString();
        }
    }
    public class CsProperty : CsMember {
        public CsProperty(string name, string type, string visit = "public") {
            this.name = name; this.typ = type; this.visit = visit;
            isget = isset = false;
            this.isReadOnly = false;
        }
        internal CsType owner;
        public CsType EndProperty() { return owner; }
        bool isget, isset;
        public bool isInterface;
        CsMethod get, set;
        public CsBlock Get() {
            get = new CsMethod("get");
            Push(ref get.indenton);
            get.InitialLogicMangement(this);
            isget = true;
            return get.logicManager;
        }
        public CsBlock Set() {
            set = new CsMethod("set");
            Push(ref set.indenton);
            set.InitialLogicMangement(this);
            isset = true;
            return set.logicManager;
        }
        internal bool IsOneLine { get; set; }
        internal bool isReadOnly;
        private string lambda;
        public CsProperty SetLambda(string value) {
            var p = "^(=>)[^=>;]*(;)$";
            if (Regex.IsMatch(value, p)) {
                lambda = value;
            } else {
                lambda = $"=>{value.Replace("=>", "").Replace(";", "")};";
            }
            return this;
        }
        public CsProperty SetLambda(CsComplexNew cn) {
            cn.leftsite = null;
            Push(ref cn.indenton);
            foreach (var item in cn.Sentents) {
                Push(ref item.indenton);
            }
            this.lambda = cn.ToString().Replace("=", "=>").Trim();
            return this;
        }
        protected override string ToStringDefault() {
            if (string.IsNullOrEmpty(this.lambda)) {
                if (isInterface) {
                    var interstr = string.Empty;
                    if (isReadOnly) interstr += "get;";
                    else interstr += "get; set;";
                    if (visit.Trim() != "partial") visit = null;
                    if (!string.IsNullOrEmpty(visit))
                        root.AppendLine(string.Format("{0}{1} {2} {3}{4} {5} {6}", base.indenton, visit, typ, name, "{", interstr, "}"));
                    else
                        root.AppendLine(string.Format("{0}{1} {2}{3} {4} {5}", base.indenton, typ, name, "{", interstr, "}"));
                } else if (isget || isset) {
                    root.AppendLine(string.Format("{0}{1} {2} {3} {4}", indenton, visit, typ, name, "{"));
                    if (isget) { root.Append(get.ToString()); }
                    if (isset) { root.Append(set.ToString()); }
                    root.AppendLine(string.Format("{0}{1}", indenton, "}"));
                } else {
                    if (this.isReadOnly) {
                        root.AppendLine(string.Format("{0}{1} {2} {3} {4} get; private set; {5}", indenton, visit, typ, name, "{", "}"));
                    } else {
                        root.AppendLine(string.Format("{0}{1} {2} {3} {4} get; set; {5}", indenton, visit, typ, name, "{", "}"));
                    }
                }
                if (IsOneLine) {
                    IsOneLine = false;
                    var rootstr = root.ToString();
                    var propbody = rootstr.Substring(rootstr.IndexOf(']') + 1);
                    propbody = propbody.Replace(Environment.NewLine, "").Replace(Indent, "");
                }
            } else {
                root.AppendLine(string.Format("{0}{1} {2} {3}{4}", indenton, visit, typ, name, this.lambda));
            }
            return root.ToString();
        }
    }
    public class CsIndx : CsMember {
        public string paramlist;
        internal CsType owner;
        public CsType EndIndex() { return owner; }
        bool isget, isset;
        CsMethod get, set;
        public CsIndx(string returntype, string paramlist, string visit = "public") {
            this.paramlist = paramlist; this.typ = returntype; this.visit = visit;
            isget = isset = false;
        }
        public CsBlock Get() {
            get = new CsMethod("get");
            Push(ref get.indenton);
            get.InitialLogicMangement(this);
            isget = true;
            return get.logicManager;
        }
        public CsBlock Set() {
            set = new CsMethod("set");
            Push(ref set.indenton);
            set.InitialLogicMangement(this);
            isset = true;
            return set.logicManager;
        }
        protected override string ToStringDefault() {
            root.Clear();
            if (isget || isset) {
                root.AppendLine(string.Format("{0}{1} {2} this[{3}] {4}", indenton, visit, typ, paramlist, "{"));
                if (isget) { root.Append(get.ToString()); }
                if (isset) { root.Append(set.ToString()); }
                root.AppendLine(string.Format("{0}{1}", indenton, "}"));
            }
            return root.ToString();
        }
    }
    public class CsEvent : CsMember {
        public CsEvent(string name, string type, string visit = "public") {
            this.name = name; this.typ = type; this.visit = visit;
            isadd = isremove = false;
        }
        internal CsType owner;
        public CsType EndEvent() { return owner; }
        bool isadd, isremove;
        CsMethod add, remove;
        public CsBlock Add() {
            add = new CsMethod("add");
            Push(ref add.indenton);
            add.InitialLogicMangement(this);
            isadd = true;
            add.logicManager.Sentence("_{0} += value;", name);
            return add.logicManager;
        }
        public CsBlock Remove() {
            remove = new CsMethod("remove");
            Push(ref remove.indenton);
            remove.InitialLogicMangement(this);
            isremove = true;
            remove.logicManager.Sentence("_{0} -= value;", name);
            return remove.logicManager;
        }
        protected override string ToStringDefault() {
            var isaddorremove = isadd || isremove;
            var _name = name;
            if (isaddorremove) {
                _name = "_" + name;
                root.AppendLine(string.Format("{0}event {1} {2};", indenton, typ, _name));
            } else {
                root.AppendLine(string.Format("{0}{1} event {2} {3};", indenton, visit, typ, _name));
            }
            var sb = new StringBuilder();
            if (isaddorremove) {
                sb.AppendLine(string.Format("{0}{1} event {2} {3} {4}", indenton, visit, typ, name, "{"));
                if (isadd) { sb.Append(add.ToString()); }
                if (isremove) { sb.Append(remove.ToString()); }
                sb.AppendLine(string.Format("{0}{1}", indenton, "}"));
                if (add.logicManager.Sentents.Count == 1 || remove.logicManager.Sentents.Count == 1) {
                    sb = sb.Replace(Environment.NewLine, " ").Replace(Indent, "");
                    sb = sb.Insert(0, base.indenton);
                    sb.AppendLine();
                }
            }
            root.Append(sb.ToString());
            return root.ToString();
        }
    }
    public class CsMethod : CsMember {
        public string inhlist;
        public string paramlist;
        public bool isctor;
        private string lambda;
        public CsMethod SetLambda(string value) {
            var p = "^(=>)[^=>;]*(;)$";
            if (Regex.IsMatch(value, p)) {
                lambda = value;
            } else {
                lambda = $"=>{value.Replace("=>", "").Replace(";", "")};";
            }
            return this;
        }
        public CsType owner;
        public CsType EndMethod() {
            return owner;
        }
        public bool isInterface, isprop;
        #region initial and constructs
        public CsMethod(string name) {
            this.name = name; this.paramlist = null; this.typ = null;
            isprop = true; this.visit = "public";
            isInterface = false;
        }
        public CsMethod(string name, string returntype) {
            this.name = name; this.typ = returntype; this.visit = "public";
            paramlist = "";
            isInterface = false;
            isctor = false;
            isprop = false;
        }
        public CsMethod(string name, string returntype, string paramestr) {
            this.name = name; this.typ = returntype; this.visit = "public";
            paramlist = paramestr;
            isInterface = false;
            isctor = false;
            isprop = false;
        }
        public CsMethod(string name, string returntype, string paramestr, string visit = "public") {
            this.name = name; this.typ = returntype; this.visit = visit;
            paramlist = paramestr;
            isInterface = false;
            isctor = false;
            isprop = false;
        }
        public CsMethod(string name, string returntype, string paramestr, string inhlist, string visit = "public") {
            this.name = name; this.typ = returntype; this.visit = visit;
            paramlist = paramestr;
            isInterface = false;
            this.inhlist = inhlist;
            isctor = false;
            isprop = false;
        }
        //
        internal CsBlock logicManager;
        internal CsMethod InitialLogicMangement() {
            logicManager = new CsBlock();
            logicManager.indenton = this.indenton;
            logicManager.method = this;
            return this;
        }
        internal CsMethod InitialLogicMangement(CsProperty prop) {
            logicManager = new CsBlock();
            logicManager.indenton = this.indenton;
            logicManager.method = this;
            logicManager.property = prop;
            return this;
        }
        internal CsMethod InitialLogicMangement(CsIndx idx) {
            logicManager = new CsBlock();
            logicManager.indenton = this.indenton;
            logicManager.method = this;
            logicManager.idx = idx;
            return this;
        }
        internal CsMethod InitialLogicMangement(CsEvent e) {
            logicManager = new CsBlock();
            logicManager.indenton = this.indenton;
            logicManager.method = this;
            logicManager.evnt = e;
            return this;
        }
        #endregion
        public CsMethod Sentence(string pattern, params string[] ps) {
            Sentence(string.Format(pattern, ps));
            return this;
        }
        public CsMethod Sentence(string sen, bool comma = false) {
            logicManager.Sentence(sen, comma);
            return this;
        }
        public void Concat(params CodeFormat[] cfs) {
            this.logicManager.Concat(cfs);
        }
        #region block
        public CsBlock If(string panttern, params object[] ps) {
            return logicManager.If(panttern, ps);
        }
        public CsBlock If(string exp) {
            return logicManager.If(exp);
        }
        public CsBlock Do(string panttern, params object[] ps) {
            return logicManager.Do(panttern, ps);
        }
        public CsBlock Do(string exp) {
            return logicManager.Do(exp);
        }
        public CsBlock While(string panttern, params object[] ps) {
            return logicManager.While(panttern, ps);
        }
        public CsBlock While(string exp) {
            return logicManager.While(exp);
        }
        public CsBlock For(string exp, string item) {
            return logicManager.For(exp, item);
        }
        public CsBlock Foreach(string array) {
            return logicManager.Foreach(array);
        }
        public CsBlock Using(string panttern, params object[] ps) {
            return logicManager.Using(panttern, ps);
        }
        public CsBlock Using(string exp) {
            return logicManager.Using(exp);
        }
        public CsSwitch Switch(string panttern, params object[] ps) {
            return logicManager.Switch(panttern, ps);
        }
        public CsSwitch Switch(string exp) {
            return logicManager.Switch(exp);
        }
        public CsBlock Try(CsTry.Type type, string errstr = "") {
            return logicManager.Try(type, errstr);
        }
        public CsBlock ComplexNew(string leftsite, string newtype) {
            return logicManager.ComplexNew(leftsite, newtype);
        }
        public CsBlock ComplexLambda(string leftsite, string parameters, bool isEvent = false) {
            return logicManager.ComplexLambda(leftsite, parameters, isEvent);
        }
        #endregion
        public string[] InheritList => this.inhlist.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        public CsMethod SetInherit(params string[] inhlist) {
            this.inhlist = string.Join(",", inhlist);
            return this;
        }
        public CsGenericFragment gf;
        /// <summary>
        /// T struct,U,W new() ICustomer
        /// =>
        /// <T,U,W> where T:struct where W new(),ICustomer
        /// </summary>
        /// <param name="gps">generic parameters</param>
        public CsMethod SetGenericParameters(string gps) {
            if (string.IsNullOrEmpty(gps)) {
                return this;
            }
            gf = new CsGenericFragment(gps); return this;
        }
        protected override string ToStringDefault() {
            if (string.IsNullOrEmpty(this.lambda)) {
                if (isInterface) {
                    if (visit.Trim() != "partial") visit = null;
                    if (!string.IsNullOrEmpty(visit))
                        root.AppendLine(string.Format("{0}{1} {2} {3}({4});", base.indenton, visit, typ, name, paramlist));
                    else
                        root.AppendLine(string.Format("{0}{1} {2}({3});", base.indenton, typ, name, paramlist));
                } else {
                    if (!string.IsNullOrEmpty(inhlist) && isctor) {
                        root.AppendLine(string.Format("{0}{1} {2}({3}) : {4} {5}", base.indenton, visit, name, paramlist, inhlist, "{"));
                    } else {
                        if (isctor) {
                            root.AppendLine(string.Format("{0}{1} {2}({3}) {4}", base.indenton, visit, name, paramlist, "{"));
                        } else {
                            if (isprop) {
                                root.AppendLine(string.Format("{0}{1} {2}", base.indenton, name, "{"));
                            } else {
                                if (gf == null) {
                                    root.AppendLine(string.Format("{0}{1} {2} {3}({4}) {5}", base.indenton, visit, typ, name, paramlist, "{"));
                                } else {
                                    root.AppendLine(string.Format("{0}{1} {2} {3}{4}({5}){6} {7}", base.indenton, visit, typ, name, gf.GParams, paramlist, gf.GWheres, "{"));
                                }
                            }
                        }
                    }
                    root.Append(logicManager.ToString());
                    root.AppendLine(string.Format("{0}{1}", base.indenton, "}"));
                }
            } else {
                root.AppendLine(string.Format("{0}{1} {2} {3}({4}) {5}", base.indenton, visit, typ, name, paramlist, this.lambda));
            }
            return root.ToString();
        }
    }
    //
    public class CsBlock : CodeFormat {
        internal CsMethod method;
        internal CsProperty property;
        internal CsIndx idx;
        internal CsEvent evnt;
        internal CsBlock upon;//上级块
        public CsBlock() {
            Sentents = new List<CsSentence>();
            Children = new List<CsBlock>();
            SwitchBlocks = new List<CsSwitch>();
            orders = new List<CodeFormat>();
        }
        public void Concat(params CodeFormat[] cfs) {
            foreach (var item in cfs) {
                Push(ref item.indenton);
            }
            orders = orders.Concat(cfs).ToList();
        }
        public CsMethod ReturnMethod() {
            return method;
        }
        public CsProperty ReturnProperty() {
            return this.method.logicManager.property;
        }
        public CsIndx ReturnIndex() {
            return this.method.logicManager.idx;
        }
        public CsEvent ReturnEvent() {
            return this.method.logicManager.evnt;
        }
        public List<CsSentence> Sentents { get; protected set; }
        public List<CsBlock> Children { get; protected set; }
        public List<CsSwitch> SwitchBlocks { get; protected set; }
        internal List<CodeFormat> orders;
        #region port
        public CsBlock Sentence(string pattern, params string[] ps) {
            Sentence(string.Format(pattern, ps));
            return this;
        }
        public virtual CsBlock Sentence(string sen, bool comma = false) {
            var x = new CsSentence(sen, comma);
            Push(ref x.indenton);
            Sentents.Add(x);
            orders.Add(x);
            return this;
        }
        public virtual CsCaseBlock CaseSentence(string sen, params object[] ps) {
            var x = new CsSentence(sen, ps);
            Push(ref x.indenton, "");
            Sentents.Add(x);
            orders.Add(x);
            return this as CsCaseBlock;
        }
        public virtual CsBlock If(string panttern, params object[] ps) {
            var exp = string.Format(panttern, ps);
            return If(exp);
        }
        public virtual CsBlock If(string exp) {
            var x = new CsIfEles(exp);
            x.method = this.method;
            Push(ref x.indenton);
            x.upon = this;
            Children.Add(x);
            orders.Add(x);
            return x;
        }
        public virtual CsBlock Else() {
            var x = new CsIfEles("", false);
            x.method = this.method;
            Push(ref x.indenton);
            x.upon = this;
            Children.Add(x);
            orders.Add(x);
            return x;
        }
        public virtual CsBlock ElseIf(string exp) {
            var x = new CsIfEles(exp, null);
            x.method = this.method;
            Push(ref x.indenton);
            x.upon = this;
            Children.Add(x);
            orders.Add(x);
            return x;
        }
        public virtual CsBlock Do(string panttern, params object[] ps) {
            var exp = string.Format(panttern, ps);
            return Do(exp);
        }
        public virtual CsBlock Do(string exp) {
            var x = new CsDoWhile(exp);
            x.method = this.method;
            Push(ref x.indenton);
            x.upon = this;
            Children.Add(x);
            orders.Add(x);
            return x;
        }
        public virtual CsBlock While(string panttern, params object[] ps) {
            var exp = string.Format(panttern, ps);
            return While(exp);
        }
        public virtual CsBlock While(string exp) {
            var x = new CsWhile(exp);
            x.method = this.method;
            Push(ref x.indenton);
            x.upon = this;
            Children.Add(x);
            orders.Add(x);
            return x;
        }
        public virtual CsBlock For(string exp, string item) {
            var x = new CsFor(exp, item);
            x.method = this.method;
            Push(ref x.indenton);
            x.upon = this;
            Children.Add(x);
            orders.Add(x);
            return x;
        }
        public virtual CsBlock Foreach(string arrayorexp, string obj = "var", string item = "item") {
            var x = new CsForeach(arrayorexp, obj, item);
            x.method = this.method;
            Push(ref x.indenton);
            x.upon = this;
            Children.Add(x);
            orders.Add(x);
            return x;
        }
        public virtual CsBlock Using(string panttern, params object[] ps) {
            var exp = string.Format(panttern, ps);
            return Using(exp);
        }
        public virtual CsBlock Using(string exp) {
            var x = new CsUsing(exp);
            x.method = this.method;
            Push(ref x.indenton);
            x.upon = this;
            Children.Add(x);
            orders.Add(x);
            return x;
        }
        public virtual CsSwitch Switch(string panttern, params object[] ps) {
            var exp = string.Format(panttern, ps);
            return Switch(exp);
        }
        public virtual CsSwitch Switch(string exp) {
            var x = new CsSwitch(exp);
            x.method = this.method;
            Push(ref x.indenton);
            x.upon = this;
            SwitchBlocks.Add(x);
            orders.Add(x);
            return x;
        }
        public virtual CsBlock Try(CsTry.Type type, string errstr = "") {
            var x = new CsTry(type);
            x.errorexp = errstr;
            x.method = this.method;
            Push(ref x.indenton);
            x.upon = this;
            Children.Add(x);
            orders.Add(x);
            return x;
        }
        public virtual CsBlock ComplexNew(string leftsite, string newtype) {
            var x = new CsComplexNew(leftsite, newtype);
            x.method = this.method;
            Push(ref x.indenton);
            x.upon = this;
            Children.Add(x);
            orders.Add(x);
            return x;
        }
        public virtual CsBlock ComplexLambda(string leftsite, string parameters, bool isEvent) {
            var x = new CsComplexLambda(leftsite, parameters, isEvent);
            x.method = this.method;
            Push(ref x.indenton);
            x.upon = this;
            Children.Add(x);
            orders.Add(x);
            return x;
        }
        //
        public virtual CsBlock EndBlock(int level = 1) {
            var up = this;
            while (up != null && level >= 1) {
                up = up.upon;
                level--;
            }
            return up;
        }
        #endregion
        public override string ToString() {
            foreach (var item in orders) {
                root.Append(item.ToString());
            }
            return root.ToString();
        }
    }
    public class CsDoWhile : CsBlock {
        public string exp;
        public CsDoWhile(string exp) {
            this.exp = exp;
        }
        public override string ToString() {
            root.AppendLine(string.Format("{0}do {1}", base.indenton, "{"));
            base.ToString();
            root.AppendLine(string.Format("{0}{1} while({2});", base.indenton, "}", exp));
            return root.ToString();
        }
    }
    public class CsWhile : CsBlock {
        public string exp;
        public CsWhile(string exp) {
            this.exp = exp;
        }
        public override string ToString() {
            root.AppendLine(string.Format("{0}while ({1}) {2}", base.indenton, exp, "{"));
            base.ToString();
            root.AppendLine(string.Format("{0}{1}", base.indenton, "}"));
            return root.ToString();
        }
    }
    public class CsForeach : CsBlock {
        readonly string obj, item, array;
        public string exp;
        public CsForeach(string arrayorexp, string obj, string item = "item") {
            this.obj = obj; this.item = item;
            if (string.IsNullOrEmpty(obj) && Regex.Match(arrayorexp, @"\w+\s+in\s+.+").Success)
                this.exp = arrayorexp;
            else
                this.array = arrayorexp;
        }
        public CsForeach(string exp) {
            this.exp = exp;
        }
        public override string ToString() {
            if (!string.IsNullOrEmpty(this.exp)) {
                root.AppendLine(string.Format("{0}foreach({1}) {2}", base.indenton, this.exp, "{"));
                base.ToString();
                root.AppendLine(string.Format("{0}{1}", base.indenton, "}"));
            } else {
                root.AppendLine(string.Format("{0}foreach({1} {2} in {3}) {4}", base.indenton, obj, item, array, "{"));
                base.ToString();
                root.AppendLine(string.Format("{0}{1}", base.indenton, "}"));
            }
            return root.ToString();
        }
    }
    public class CsFor : CsBlock {
        public string exp;
        readonly string item;
        public CsFor(string exporlen, string item) {
            this.item = item;
            this.exp = exporlen;
        }
        public override string ToString() {
            if (!string.IsNullOrEmpty(this.item)) {
                root.AppendLine(string.Format("{0}for(int {1} = 0; {1} < {2}; {1}++) {3}", base.indenton, item, exp, "{"));
                base.ToString();
                root.AppendLine(string.Format("{0}{1}", base.indenton, "}"));
            } else {
                root.AppendLine(string.Format("{0}for({1}) {2}", base.indenton, exp, "{"));
                base.ToString();
                root.AppendLine(string.Format("{0}{1}", base.indenton, "}"));
            }
            return root.ToString();
        }
    }
    public class CsIfEles : CsBlock {
        public string exp;
        readonly bool? iforelse;
        public CsIfEles(string exp, bool? iforelse = true) {
            this.exp = exp;
            this.iforelse = iforelse;
        }
        public override string ToString() {
            if (iforelse == null) {
                root.AppendLine(string.Format("{0}else if({1}) {2}", base.indenton, exp, "{"));
                base.ToString();
                root.AppendLine(string.Format("{0}{1}", base.indenton, "}"));
            } else {
                if (iforelse.Value) {
                    root.AppendLine(string.Format("{0}if ({1}) {2}", base.indenton, exp, "{"));
                    base.ToString();
                    root.AppendLine(string.Format("{0}{1}", base.indenton, "}"));
                } else {
                    root.AppendLine(string.Format("{0}else {1}", base.indenton, "{"));
                    base.ToString();
                    root.AppendLine(string.Format("{0}{1}", base.indenton, "}"));
                }
            }
            return root.ToString();
            //return null;
        }
    }
    public class CsSwitch : CodeFormat {
        public string exp;
        internal CsMethod method;
#pragma warning disable CS0649 // 从未对字段“Switch.property”赋值，字段将一直保持其默认值 null
        internal CsProperty property;
#pragma warning restore CS0649 // 从未对字段“Switch.property”赋值，字段将一直保持其默认值 null
        internal CsBlock upon;//上级块
        public CsSwitch(string exp) {
            this.exp = exp;
            Cases = new List<CsCaseBlock>();
        }
        public CsMethod ReturnMethod() {
            return method;
        }
        public CsProperty ReturnProperty() {
            return this.method.logicManager.property;
        }
        internal CsCaseBlock current;
        public List<CsCaseBlock> Cases { get; private set; }
        public CsCaseBlock CaseBlock(string tag) {
            var x = new CsCaseBlock(tag, this);
            x.method = this.method;
            Push(ref x.indenton);
            current = x;
            Cases.Add(x);
            x.isdefault = false;
            return x;
        }
        public CsCaseBlock CaseGoto(string tag) {
            var x = new CsCaseBlock(tag, this);
            x.method = this.method;
            Push(ref x.indenton);
            current = x;
            Cases.Add(x);
            x.isgoto = true;
            return x;
        }
        public CsCaseBlock CasePass(string tag) {
            var x = new CsCaseBlock(tag, this);
            x.method = this.method;
            Push(ref x.indenton);
            current = x;
            Cases.Add(x);
            x.ispass = true;
            return x;
        }
        public CsCaseBlock Default(string tag) {
            var x = new CsCaseBlock(tag, this);
            x.method = this.method;
            Push(ref x.indenton);
            current = x;
            Cases.Add(x);
            x.isdefault = true;
            return x;
        }
        public CsBlock EndSwitch() {
            return upon;
        }
        public virtual CsCaseBlock CaseSentence(string sen) {
            current.CaseSentence(sen);
            return current;
        }
        public override string ToString() {
            root.AppendLine(string.Format("{0}switch({1}) {2}", base.indenton, exp, "{"));
            foreach (var item in Cases) {
                root.Append(item.ToString());
            }
            root.AppendLine(string.Format("{0}{1}", base.indenton, "}"));
            return root.ToString();
        }
    }
    public class CsCaseBlock : CsBlock {
        readonly string tag; internal CsSwitch owner;
        internal bool isdefault;
        internal bool isgoto;
        internal bool ispass;
        internal CsCaseBlock(string tag, CsSwitch owner) {
            this.tag = tag; this.owner = owner; isdefault = false;
        }
        public CsCaseBlock CaseBreak(string tag) {
            owner.CaseBlock(tag);
            return this;
        }
        public CsCaseBlock CaseGoto(string tag) {
            owner.CaseGoto(tag);
            return this;
        }
        public CsCaseBlock CasePass(string tag) {
            owner.CasePass(tag);
            return this;
        }
        public CsSwitch Default() {
            owner.Default(null);
            return owner;
        }
        public CsSwitch EndCase() {
            return owner;
        }
        public CsBlock EndSwitch() {
            return owner.EndSwitch();
        }
        public override string ToString() {
            if (isdefault) {
                root.AppendLine(string.Format("{0}default: ", base.indenton));
                base.ToString();
                root.AppendLine(string.Format("{0}{1}", base.indenton, "break;"));
            } else {
                root.AppendLine(string.Format("{0}case {1}: ", base.indenton, tag));
                base.ToString();
                if (!ispass) {
                    root.AppendLine(string.Format("{0}{1}", base.indenton, "break;"));
                }
                if (isgoto) {
                    root.AppendLine(string.Format("{0}{1}", base.indenton, "goto;"));
                }
            }
            return root.ToString();
        }
    }
    public class CsUsing : CsBlock {
        public string exp;
        public CsUsing(string exp) {
            this.exp = exp;
        }
        public override string ToString() {
            root.AppendLine(string.Format("{0}using({1}) {2}", base.indenton, exp, "{"));
            base.ToString();
            root.AppendLine(string.Format("{0}{1}", base.indenton, "}"));
            return base.ToString();
        }
    }
    public class CsTry : CsBlock {
        public enum Type {
            _try, _catch, _finally
        }

        readonly Type type;
        public CsTry(Type type) {
            this.type = type;
        }
        internal string errorexp;
        public override string ToString() {
            switch (this.type) {
                case Type._try:
                    root.AppendLine(string.Format("{0}try {1}", base.indenton, "{"));
                    base.ToString();
                    root.AppendLine(string.Format("{0}{1}", base.indenton, "}"));
                    break;
                case Type._catch:
                    root.AppendLine(string.Format("{0}catch({1}) {2}", base.indenton, this.errorexp, "{"));
                    base.ToString();
                    root.AppendLine(string.Format("{0}{1}", base.indenton, "}"));
                    break;
                case Type._finally:
                    root.AppendLine(string.Format("{0}finally {1}", base.indenton, "{"));
                    base.ToString();
                    root.AppendLine(string.Format("{0}{1}", base.indenton, "}"));
                    break;
            }
            return root.ToString();
        }
    }
    public class CsComplexNew : CsBlock {
        public string leftsite, newtype;
        public CsComplexNew(string leftsite, string newtype) {
            this.leftsite = leftsite; this.newtype = newtype;
        }
        public string Variable { get { return leftsite.Split(' ').Last(); } }
        public override string ToString() {
            var x = string.IsNullOrEmpty(newtype);
            if (x) {
                root.AppendLine(string.Format("{0}{1} = new {2}", base.indenton, leftsite, "{"));
                base.ToString();
                root.AppendLine(string.Format("{0}{1};", base.indenton, "}"));
            } else if (newtype.Trim() == "[]") {
                root.AppendLine(string.Format("{0}{1} = new[] {2}", base.indenton, leftsite, "{"));
                base.ToString();
                root.AppendLine(string.Format("{0}{1};", base.indenton, "}"));
            } else {
                root.AppendLine(string.Format("{0}{1} = new {2}{3}", base.indenton, leftsite, newtype, "{"));
                base.ToString();
                root.AppendLine(string.Format("{0}{1};", base.indenton, "}"));
            }
            return root.ToString();
        }
    }
    public class CsComplexLambda : CsBlock {
        public string leftsite, parameters;
        readonly bool isEvent;
        public CsComplexLambda(string leftsite, string parameters, bool isEvent = false) {
            this.leftsite = leftsite; this.parameters = parameters;
            this.isEvent = isEvent;
        }
        public override string ToString() {
            var x = string.IsNullOrEmpty(parameters);
            if (this.isEvent) {
                if (x) {
                    root.AppendLine(string.Format("{0}{1} += () => {2}", base.indenton, leftsite, "{"));
                    base.ToString();
                    root.AppendLine(string.Format("{0}{1};", base.indenton, "}"));
                } else if (parameters.Trim().Split(',').Count() == 1) {
                    root.AppendLine(string.Format("{0}{1} += {2} => {3}", base.indenton, leftsite, parameters, "{"));
                    base.ToString();
                    root.AppendLine(string.Format("{0}{1};", base.indenton, "}"));
                } else {
                    root.AppendLine(string.Format("{0}{1} += ({2}) => {3}", base.indenton, leftsite, parameters, "{"));
                    base.ToString();
                    root.AppendLine(string.Format("{0}{1};", base.indenton, "}"));
                }
            } else {
                //此种情况用于函数调用时的lambda表达式做为参数的情况
                //这种情况下格式也不一样，第一个空格项没有，最后一行不能换行
                if (string.IsNullOrWhiteSpace(leftsite)) {
                    if (x) {
                        root.AppendLine(string.Format("() => {0}", "{"));
                        base.ToString();
                        root.Append(string.Format("{0}{1}", base.indenton, "}"));
                    } else if (parameters.Trim().Split(',').Count() == 1) {
                        root.AppendLine(string.Format("{0} => {1}", parameters, "{"));
                        base.ToString();
                        root.Append(string.Format("{0}{1}", base.indenton, "}"));
                    } else {
                        root.AppendLine(string.Format("({0}) => {1}", parameters, "{"));
                        base.ToString();
                        root.Append(string.Format("{0}{1}", base.indenton, "}"));
                    }
                } else {
                    if (x) {
                        root.AppendLine(string.Format("{0}{1} = () => {2}", base.indenton, leftsite, "{"));
                        base.ToString();
                        root.AppendLine(string.Format("{0}{1};", base.indenton, "}"));
                    } else if (parameters.Trim().Split(',').Count() == 1) {
                        root.AppendLine(string.Format("{0}{1} = {2} => {3}", base.indenton, leftsite, parameters, "{"));
                        base.ToString();
                        root.AppendLine(string.Format("{0}{1};", base.indenton, "}"));
                    } else {
                        root.AppendLine(string.Format("{0}{1} = ({2}) => {3}", base.indenton, leftsite, parameters, "{"));
                        base.ToString();
                        root.AppendLine(string.Format("{0}{1};", base.indenton, "}"));
                    }
                }
            }
            return root.ToString();
        }
    }
    //
    public class CsSentence : CodeFormat {
        public string sentens;
        public CsSentence(string sentens, bool comma = false) {
            if (!comma && sentens[sentens.Length - 1] != ';') {
                this.sentens = sentens + ";";
            } else if (comma && sentens[sentens.Length - 1] != ';') {
                this.sentens = sentens + ",";
            } else {
                this.sentens = sentens;
            }
            this.sentens = this.sentens.Trim();
        }
        public CsSentence(string pattern, params object[] ps) {
            if (pattern.Last() != ';') {
                pattern += ";";
            }
            this.sentens = string.Format(pattern, ps);
        }
        public override string ToString() {
            root.AppendLine(string.Format("{0}{1}", indenton, this.sentens));
            return root.ToString();
        }
    }
    public class CsGenericFragment {
        public Dictionary<string, string[]> genericParams;
        public CsGenericFragment(string genericParams) {
            this.genericParams = new Dictionary<string, string[]>();
            var x = genericParams.Split(',');
            foreach (var item in x) {
                var x2 = item.Split(' ');
                var key = x2.First();
                var x3 = x2.ToList();
                x3.Remove(key);
                this.genericParams.Add(key, x3.ToArray());
            }
        }
        public string GParams => $"<{string.Join(",", this.genericParams.Keys.ToArray())}>";
        public string GWheres {
            get {
                var r = new List<string>();
                foreach (var item in this.genericParams) {
                    if (item.Value != null && item.Value.Length > 0) {
                        r.Add($"where {item.Key}:{string.Join(",", item.Value)}");
                    }
                }
                return string.Join(" ", r);
            }
        }
    }

}
