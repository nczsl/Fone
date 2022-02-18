// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Util.BizPort;
using Util.Ex;
namespace Util.Generator {
    public class ProtoUtil {
        public static void GenDesignProtoMessage(Type t, ProtoNamespace pn) {
            var mname = GetTypeName(t);
            if (pn.GetMessage(mname) != null) {
                return;
            }
            var msg = new ProtoMessage { Name = mname };
            pn.AddMessage(msg);
            foreach (var item in t.GetProperties()) {
                ProtobufHandler(msg, item.PropertyType, new ProtoField { Name = item.Name });
            }
        }
        public static void ProtoInherit(IEnumerable<Type> ts, ProtoNamespace psps) {
            var childname = "Umbra";
            foreach (var item in ts) {
                if (ts.Contains(item.BaseType)) {
                    var bt = psps.GetMessage(GetTypeName(item.BaseType));
                    var msg = psps.GetMessage(GetTypeName(item));
                    var btselfname = GetTypeName(item.BaseType) + "Self";
                    if (psps.GetMessage(btselfname) == null) {
                        var btself = new ProtoMessage { Name = btselfname };
                        btself.fields.AddRange(bt.fields);
                        psps.AddMessage(btself);
                        bt.AddToOneof(childname,
                            new ProtoField {
                                Name = btselfname.LowFirst(),
                                Key = btself.Name
                            });
                    }
                    bt.AddToOneof(childname,
                        new ProtoField {
                            Name = msg.Name.LowFirst(),
                            Key = msg.Name
                        });
                    bt.fields.Clear();
                }
            }
        }
        public static ProtoNamespace GenGrpcProtoByInterface(Type bizInterface) {
            var pnsps = new ProtoNamespace();
            pnsps.LoadLocalProto();
            var ms = bizInterface.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            pnsps.package = bizInterface.Namespace + ".Grpc";
            //每一个方法的返回类型封装成一个message, 参数列表封装成一个meesage
            //在这一过程中，检查出poco类型单独分裂出来
            var svc = new ProtoService { name = $"{bizInterface.Name}" };
            pnsps.AddService(svc);
            foreach (var item in ms) {
                var outmsg = new ProtoMessage { Name = $"Out{item.Name}" };
                var inmsg = new ProtoMessage { Name = $"In{item.Name}" };
                pnsps.AddMessage(outmsg);
                pnsps.AddMessage(inmsg);
                ProtobufHandler(outmsg, item.ReturnType, new ProtoField { Name = $"{item.Name}Response" });
                var i = 0;
                foreach (var it in item.GetParameters()) {
                    ProtobufHandler(inmsg, it.ParameterType, new ProtoField { Name = $"{item.Name}Request{++i}" });
                }
                var rpc = new ProtoRpc { name = $"{item.Name}" };
                rpc.input = inmsg;
                rpc.output = outmsg;
                svc.AddRpc(rpc);
            }
            return pnsps;
        }
        static int collectionNumber;
        static bool ismap;
        /// <summary>
        /// 处理类型映射和泛型追踪
        /// </summary>
        /// <param name="pm"></param>
        /// <param name="t"></param>
        /// <param name="fname"></param>
        /// <param name="pfm"></param>
        public static ProtoFieldType ProtobufHandler(ProtoMessage pm, Type t, ProtoField pf) {
            var _t = GetProtobufType(t);
            switch (_t) {
                case _Type.normal:
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                        t = t.GenericTypeArguments[0];
                    }
                    pf.Key = ProtoFieldType.GetByType(t);
                    if (!ismap)
                        pm.AddField(pf);
                    break;
                case _Type.datasource:
                    // pf.Key = ProtoFieldType.QuerySet;
                    // if (!ismap)
                    //     pm.AddField(pf);
                    break;
                case _Type.poco:
                    var msg = new ProtoMessage { Name = GetTypeName(t) };
                    if (pm.owner.GetMessage(msg.Name) == null) {
                        pm.owner.AddMessage(msg);
                        foreach (var item in t.GetProperties()) {
                            ProtobufHandler(msg, item.PropertyType, new ProtoField { Name = item.Name });
                        }
                    }
                    pf.Key = ProtoFieldType.RefMessage(msg);
                    if (!ismap)
                        pm.AddField(pf);
                    break;
                case _Type.collection:
                    var tjudge = default(Type);
                    if (t.IsArray) {
                        tjudge = t.GetElementType();
                    } else {
                        tjudge = t.GenericTypeArguments[0];
                    }
                    pf.indicia = ProtoFieldMark.repeated;
                    if (GetProtobufType(tjudge) == _Type.collection) {
                        var msg2 = new ProtoMessage { Name = $"{pf.Name}{++collectionNumber}s" };
                        if (pm.owner.GetMessage(msg2.Name) == null) {
                            pm.owner.AddMessage(msg2);
                        }
                        pf.Key = ProtoFieldType.RefMessage(msg2);
                        if (!ismap)
                            pm.AddField(pf);
                        var pf2 = new ProtoField { Name = $"{pf.Name}{collectionNumber}", indicia = ProtoFieldMark.repeated };
                        ProtobufHandler(msg2, tjudge, pf2);
                        collectionNumber = 0;
                    } else {
                        ProtobufHandler(pm, tjudge, pf);
                    }
                    break;
                case _Type.map:
                    ismap = true;
                    var keytype = t.GenericTypeArguments[0];
                    var valuetype = t.GenericTypeArguments[1];
                    pf.Key = ProtobufHandler(pm, keytype, pf);
                    pf.Value = ProtobufHandler(pm, valuetype, pf);
                    pm.AddField(pf);
                    ismap = false;
                    break;
                case _Type.stream:
                    pm.IsStream = true;
                    ProtobufHandler(pm, t.GenericTypeArguments[0], pf);
                    break;
                case _Type.task:
                    ProtobufHandler(pm, typeof(void), pf);
                    break;
                case _Type.taskg:
                    ProtobufHandler(pm, t.GenericTypeArguments[0], pf);
                    break;
                case _Type.type:
                    goto case _Type.normal;
                case _Type.expression:
                    goto case _Type.poco;
                case _Type.lambdaexpression:
                    //ProtobufHandler(pm,t.GenericTypeArguments[0].GenericTypeArguments[0],pf);
                    goto case _Type.normal;
                case _Type._enum:
                    var _enname = GetTypeName(t);
                    if (pm.owner.GetEnum(_enname) == null) {
                        var _en = new ProtoEnum { Name = _enname };
                        pm.owner.AddEnum(_en);
                        foreach (var item in Enum.GetNames(t)) {
                            _en.Add(item);
                        }
                    }
                    pf.Key = ProtoFieldType.RefEnum(pm.owner.GetEnum(_enname));
                    pm.AddField(pf);
                    break;
                case _Type.basic:
                    if (pf.Name.ToLower().Contains("item")) {
                        if (pf.indicia == ProtoFieldMark.repeated) {
                            var msg3 = new ProtoMessage { Name = $"{pf.Name}{++collectionNumber}s" };
                            if (pm.owner.GetMessage(msg3.Name) == null) {
                                pm.owner.AddMessage(msg3);
                            }
                            pf.Key = ProtoFieldType.RefMessage(msg3);
                            if (!ismap)
                                pm.AddField(pf);
                            var pf3 = new ProtoField { Name = $"{pf.Name}{collectionNumber}" };
                            ProtobufHandler(msg3, t, pf3);
                        } else {
                            if (!ismap)
                                pm.AddToOneof(pf.Name,
                                    new ProtoField { Name = "Int32Value", Key = ProtoFieldType.int32 },
                                    new ProtoField { Name = "Int64Value", Key = ProtoFieldType.int64 },
                                    new ProtoField { Name = "FloatValue", Key = ProtoFieldType._float },
                                    new ProtoField { Name = "DoubleValue", Key = ProtoFieldType._double },
                                    new ProtoField { Name = "BoolValue", Key = ProtoFieldType._bool },
                                    new ProtoField { Name = "DateTime", Key = ProtoFieldType.DateTime },
                                    new ProtoField { Name = "TimeSpan", Key = ProtoFieldType.TimeSpan },
                                    new ProtoField { Name = "StringValue", Key = ProtoFieldType._string },
                                    new ProtoField { Name = "BytesValue", Key = ProtoFieldType.bytes });
                        }
                    } else {
                        pf.Key = ProtoFieldType.Any;
                        if (!ismap)
                            pm.AddField(pf);
                    }
                    break;
                case _Type.other:
                    if (pm.owner.GetMessage(GetTypeName(t)) != null) {
                        pf.Key = ProtoFieldType.RefMessage(pm.owner.GetMessage(GetTypeName(t)));
                        if (!ismap)
                            pm.AddField(pf);
                    }
                    break;
            }
            return pf.Key;
        }
        public static string GetTypeName(Type t) =>
            t
        switch {
            Type x when x.HasElementType => x.Name.Replace("[]", ""),
            Type x when x.IsGenericType => x.GetGenericTypeDefinition().Name.Replace("`", "T"),
            _ => t.Name
        };
        //
        public enum _Type {
            normal,
            poco,
            collection,
            map,
            stream,
            task,
            taskg,
            type,
            lambdaexpression,
            expression,
            _enum,
            other,
            datasource,
            basic
        }
        public static _Type GetProtobufType(Type t) =>
            t
        switch {
            Type x when x.IsPrimaryType() => _Type.normal,
            Type x when x == typeof(DataSource) => _Type.datasource,
            Type x when x.IsTypeDefinition && x.BaseType == typeof(object) &&
            !x.IsAbstract && x.IsPublic && x.GetInterfaces().Length == 0 => _Type.poco,
            Type x when (
            x.IsGenericType &&
            typeof(System.Collections.IEnumerable).IsAssignableFrom(x) &&
            !(
            (x.GetGenericTypeDefinition() == typeof(IDictionary<,>)) ||
            (x.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            )) ||
            (!x.IsGenericType &&
            typeof(System.Collections.IEnumerable).IsAssignableFrom(x)
            ) => _Type.collection,
            Type x when (from i in x.GetInterfaces() where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>) select i).Count() == 1 => _Type.map,
            Type x when (x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>)) ||
            (from i in x.GetInterfaces() where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>) select i).Count() == 1 => _Type.stream,
            Type x when typeof(Task) == x => _Type.task,
            Type x when x.IsGenericType &&
            typeof(Task<>) == x.GetGenericTypeDefinition() => _Type.taskg,
            Type x when x == typeof(Type) => _Type.type,
            Type x when x.IsGenericType &&
            x.GetGenericTypeDefinition() == typeof(Expression<>) &&
            x.GenericTypeArguments[0].IsSubclassOf(typeof(Delegate)) => _Type.lambdaexpression,
            Type x when x == typeof(Expression) ||
            x.BaseType == typeof(Expression) => _Type.expression,
            Type x when x.IsEnum => _Type._enum,
            Type x when x == typeof(object) => _Type.basic,
            _ => _Type.other,
        };
    }
    public class ProtoNamespace : CodeFormat {
        /// <summary>
        /// 包名
        /// </summary>
        public string name;
        public List<string> options;
        public List<string> imports;
        public string package;
        public List<ProtoEnum> enums;
        public List<ProtoMessage> messages;
        public List<ProtoService> services;
        public ProtoNamespace AddEnum(ProtoEnum pe) {
            pe.Push(this);
            enums.Add(pe);
            return this;
        }
        public void DelEnum(ProtoEnum pe) {
            enums.Remove(pe);
        }
        public ProtoEnum GetEnum(string name) => enums.Find(i => i.Name == name);
        public ProtoNamespace AddMessage(ProtoMessage msg) {
            msg.Push(this);
            msg.owner = this;
            messages.Add(msg);
            return this;
        }
        public void DelMessage(ProtoMessage msg) {
            messages.Remove(msg);
        }
        public ProtoService GetService(string name) => services.Find(i => i.name == name);
        /// <summary>
        /// 可以通过 全名匹配找自定义类型
        /// 也可以通过 忽略名称空间的引用消息名，忽略 大小写
        /// 来进行匹配例如 google.protobuf.Any 可以使用
        /// any的传入值 匹配到
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ProtoMessage GetMessage(string name) =>
            (from i in messages select i).ToList()
            .Find(i => i.Name == name);
        public ProtoNamespace AddService(ProtoService svc) {
            svc.Push(this);
            services.Add(svc);
            return this;
        }
        public void DelService(ProtoService svc) {
            services.Remove(svc);
        }
        public ProtoNamespace() {
            enums = new List<ProtoEnum>();
            messages = new List<ProtoMessage>();
            services = new List<ProtoService>();
            //要使用google定义的日期类型，空类型和 any类型做为默认情况
            //要导入，并加入相应的类型
            imports = new List<string> {
                "google/protobuf/duration.proto",
                "google/protobuf/timestamp.proto",
                "google/protobuf/empty.proto",
                "google/protobuf/any.proto",
                "google/protobuf/wrappers.proto"
            };
        }
        public void LoadLocalProto() {
            imports.Add("design/data.proto");
            imports.Add("design/expression.proto");
        }
        public override string ToString() {
            AppendLine("syntax = \"proto3\";");
            foreach (var item in imports) {
                AppendLine($"import \"{item}\";");
            }
            AppendLine($"package {package};");
            root.AppendLine();
            root.AppendLine("//--enums define part");
            foreach (var item in from i in enums where i.refowner == null select i) {
                root.AppendLine(item.ToString());
            }
            root.AppendLine("//--messages define part");
            foreach (var item in messages) {
                root.AppendLine(item.ToString());
            }
            root.AppendLine("//--services define part");
            foreach (var item in services) {
                root.AppendLine(item.ToString());
            }
            return base.ToString();
        }
    }
    public class ProtoBase : CodeFormat {
        public string Name { get; set; }
    }
    public class ProtoEnum : ProtoBase {
        public ProtoMessage refowner;
        public readonly List<ProtoEnumItem> items;
        public ProtoEnum() {
            items = new List<ProtoEnumItem>();
            // IndentonChenaged += ProtoEnum_IndentonChenaged;
        }
        private void ProtoEnum_IndentonChenaged(object sender, string e) {
            foreach (var item in items) {
                item.indenton = e + Indent;
            }
        }
        public ProtoEnum Add(string it) {
            var item = new ProtoEnumItem { name = it };
            item.Push(this);
            items.Add(item);
            return this;
        }
        public override string ToString() {
            root.Append(indenton);
            root.Append("enum");
            root.Append(space);
            root.Append(Name);
            root.Append(space);
            root.Append("{");
            root.Append(NewLine);
            var idx = 0;
            foreach (var item in items) {
                item.idx = idx++;
                root.AppendLine(item.ToString());
            }
            root.Append(indenton);
            root.Append("}");
            return base.ToString();
        }
    }
    public class ProtoEnumItem : CodeFormat {
        public string name;
        public int idx;
        public override string ToString() {
            root.Append(indenton);
            root.Append(name);
            root.Append(space);
            root.Append("=");
            root.Append(space);
            root.Append(idx);
            root.Append(";");
            //
            return base.ToString();
        }
    }
    public class ProtoService : CodeFormat {
        public string name;
        public List<ProtoRpc> rpcs;
        public ProtoService AddRpc(ProtoRpc rpc) {
            rpc.Push(this);
            rpcs.Add(rpc);
            return this;
        }
        public void DelRpc(ProtoRpc rpc) {
            rpcs.Remove(rpc);
        }
        public ProtoRpc GetRpc(string name) => rpcs.Find(i => i.name == name);
        public ProtoService() {
            rpcs = new List<ProtoRpc>();
        }
        public override string ToString() {
            root.Append(indenton);
            root.Append("service");
            root.Append(space);
            root.Append(name);
            root.Append(space);
            root.Append("{");
            root.Append(Environment.NewLine);
            foreach (var item in rpcs) {
                root.AppendLine(item.ToString());
            }
            root.Append(indenton);
            root.Append("}");
            return base.ToString();
        }
    }
    public class ProtoMessage : ProtoBase {
        internal ProtoNamespace owner;
        public List<ProtoField> fields;
        public bool IsStream { get; set; }
        public Dictionary<string, List<ProtoField>> oneofs;
        public ProtoMessage AddField(ProtoField field) {
            field.Push(this);
            fields.Add(field);
            return this;
        }
        public ProtoMessage AddToOneof(string name, params ProtoField[] fields) {
            if (oneofs == null) {
                oneofs = new Dictionary<string, List<ProtoField>>();
            }
            var of = new List<ProtoField>();
            foreach (var field in fields) {
                Push(ref field.indenton, Indent + Indent); of.Add(field);
            }
            if (oneofs.ContainsKey(name)) {
                oneofs[name].AddRange(of);
            } else {
                oneofs.Add(name, of);
            }
            return this;
        }
        public void Remove(ProtoField field) {
            fields.Remove(field);
        }
        public ProtoField GetField(string name) => fields.Find(i => i.Name == name);
        public ProtoMessage() {
            fields = new List<ProtoField>();
        }
        IEnumerable<ProtoEnum> IncludeEnum =>
            from i in owner.enums
            where i.refowner == this
            select i;
        public override string ToString() {
            root.Append(indenton);
            root.Append("message"); //proto 关键字
            root.Append(space);
            root.Append(Name);
            root.Append(space);
            root.Append("{");
            root.Append(NewLine);
            foreach (var item in IncludeEnum) {
                item.Indenton += Indent;
                root.AppendLine(item.ToString());
            }
            var idx = 0;
            foreach (var item in fields) {
                item.idx = ++idx;
                root.AppendLine(item.ToString());
            }
            if (oneofs != null) {
                idx = 0;
                foreach (var item in oneofs) {
                    var indent = $"{base.indenton + base.Indent}";
                    root.Append($"{indent}oneof");
                    root.Append(space);
                    root.Append(item.Key);
                    root.Append("{");
                    root.Append(NewLine);
                    foreach (var it in item.Value) {
                        it.idx = ++idx;
                        root.AppendLine(it.ToString());
                    }
                    root.Append(indent);
                    root.Append("}");
                    root.Append(NewLine);
                }
            }
            root.Append(indenton);
            root.Append("}");
            //
            return base.ToString();
        }
    }
    public class ProtoField : ProtoBase {
        public ProtoFieldMark indicia = ProtoFieldMark.singular;
        public ProtoFieldType Key { get; set; }
        /// <summary>
        /// 当字段是字典map时使用，没有其它判断依据
        /// 当mapMinor为一个有效protobuf值时即断定此
        /// 字段为，map
        /// </summary>
        public ProtoFieldType Value { get; set; }
        internal int idx;
        public override string ToString() {
            root.Append(indenton);
            root.Append(indicia);
            if (indicia == ProtoFieldMark.repeated) {
                root.Append(space);
            }
            if (Value.HasValue) {
                root.Append("map<");
                root.Append(Key);
                root.Append(",");
                root.Append(Value);
                root.Append(">");
            } else {
                root.Append(Key);
            }
            root.Append(space);
            root.Append(Name);
            root.Append(space);
            root.Append("=");
            root.Append(space);
            root.Append(idx);
            root.Append(";");
            //root.Append(Environment.NewLine);
            return base.ToString();
        }
    }
    public class ProtoRpc : CodeFormat {
        public string name;
        public ProtoMessage input;
        public ProtoMessage output;
        public override string ToString() {
            root.Append(indenton);
            root.Append("rpc");
            root.Append(space);
            root.Append(name);
            root.Append("(");
            if (input.IsStream) {
                root.Append("stream");
                root.Append(space);
            }
            root.Append(input.Name);
            root.Append(")");
            root.Append(space);
            root.Append("returns");
            root.Append("(");
            if (output.IsStream) {
                root.Append("stream");
                root.Append(space);
            }
            root.Append(output.Name);
            root.Append(")");
            root.Append(";");
            return base.ToString();
        }
    }
    /// <summary>
    /// 标明是单个还是多个的，多个就是数组，默认为none在protobuf上就是单个的
    /// </summary>
    public struct ProtoFieldMark {
        string value;
        public static implicit operator string(ProtoFieldMark pfm) =>
            pfm
        switch {
            ProtoFieldMark { value: nameof(singular) } => "",
            ProtoFieldMark { value: nameof(repeated) } => nameof(repeated),
            _ =>
            throw new Exception("error value of ProtFieldMark")
        };
        public static implicit operator ProtoFieldMark(string value) {
            if (!Check(value)) {
                throw new Exception("error setting of ProtoFieldMark");
            }
            var pfm = default(ProtoFieldMark);
            pfm.value = value;
            return pfm;
        }
        //
        private static bool Check(string value) =>
            value == nameof(ProtoFieldMark.singular) ||
            value == nameof(ProtoFieldMark.repeated);
        public static ProtoFieldMark singular = nameof(singular);
        public static ProtoFieldMark repeated = nameof(repeated);
    }
    public struct ProtoFieldType {
        string value;
        public static implicit operator string(ProtoFieldType pft) => pft.value;
        public bool IsBasicValue =>
            !string.IsNullOrWhiteSpace(value) &&
            value != ProtoFieldType.none &&
            CheckBasic(this);
        public bool HasValue =>
            !string.IsNullOrWhiteSpace(value);
        public static ProtoFieldType RefMessage(ProtoMessage msg) => msg.Name;
        public static ProtoFieldType RefEnum(ProtoEnum penum) => penum.Name;
        public static ProtoFieldType GetByType(Type t) =>
            t
        switch {
            Type x when x == typeof(double) => ProtoFieldType._double,
            Type x when x == typeof(float) => ProtoFieldType._float,
            Type x when x == typeof(int) => ProtoFieldType.int32,
            Type x when x == typeof(long) => ProtoFieldType.int64,
            Type x when x == typeof(uint) => ProtoFieldType.uint32,
            Type x when x == typeof(ulong) => ProtoFieldType.uint64,
            Type x when x == typeof(bool) => ProtoFieldType._bool,
            Type x when x == typeof(string) => ProtoFieldType._string,
            Type x when x == typeof(byte[]) => ProtoFieldType.bytes,
            Type x when x == typeof(decimal) => ProtoFieldType.Decimal,
            Type x when x == typeof(DateTime) => ProtoFieldType.DateTime,
            Type x when x == typeof(TimeSpan) => ProtoFieldType.TimeSpan,
            Type x when x == typeof(DateTimeOffset) => ProtoFieldType.DateTimeOffset,
            Type x when x == typeof(void) => ProtoFieldType.Void,
            Type x when x == typeof(bool?) => ProtoFieldType.BoolNull,
            Type x when x == typeof(double?) => ProtoFieldType.DoubleNull,
            Type x when x == typeof(float?) => ProtoFieldType.FloatNull,
            Type x when x == typeof(int?) => ProtoFieldType.IntNull,
            Type x when x == typeof(long?) => ProtoFieldType.LongNull,
            Type x when x == typeof(uint?) => ProtoFieldType.UIntNull,
            Type x when x == typeof(ulong?) => ProtoFieldType.ULongNull,
            Type x when x == typeof(string) => ProtoFieldType.StringNull,
            Type x when x == typeof(string) => ProtoFieldType.ByteString,
            Type x when x == typeof(Type) => ProtoFieldType.Type,
            Type x when x.GetGenericTypeDefinition() == typeof(Expression<>) => ProtoFieldType.Expression,
            _ =>
            throw new Exception($"Unresolved Type:{t}")
        };
        public static ProtoFieldType GetFixedByType(Type t) =>
            t
        switch {
            Type x when x == typeof(double) => ProtoFieldType._double,
            Type x when x == typeof(float) => ProtoFieldType._float,
            Type x when x == typeof(int) => ProtoFieldType.sfixed32,
            Type x when x == typeof(long) => ProtoFieldType.sfixed64,
            Type x when x == typeof(uint) => ProtoFieldType.fixed32,
            Type x when x == typeof(ulong) => ProtoFieldType.fixed64,
            Type x when x == typeof(bool) => ProtoFieldType._bool,
            Type x when x == typeof(string) => ProtoFieldType._string,
            Type x when x == typeof(byte[]) => ProtoFieldType.bytes,
            Type x when x == typeof(DateTime) => ProtoFieldType.DateTime,
            Type x when x == typeof(TimeSpan) => ProtoFieldType.TimeSpan,
            Type x when x == typeof(DateTimeOffset) => ProtoFieldType.DateTimeOffset,
            Type x when x == typeof(void) => ProtoFieldType.Void,
            Type x when x == typeof(Type) => ProtoFieldType.Type,
            Type x when x.GetGenericTypeDefinition() == typeof(Expression<>) => ProtoFieldType.Expression,
            _ =>
            throw new Exception($"Unresolved Type:{t}")
        };
        public static ProtoFieldType GetVariableByType(Type t) =>
            t
        switch {
            Type x when x == typeof(double) => ProtoFieldType._double,
            Type x when x == typeof(float) => ProtoFieldType._float,
            Type x when x == typeof(int) => ProtoFieldType.sint32,
            Type x when x == typeof(long) => ProtoFieldType.sint64,
            Type x when x == typeof(uint) => ProtoFieldType.uint32,
            Type x when x == typeof(ulong) => ProtoFieldType.uint64,
            Type x when x == typeof(bool) => ProtoFieldType._bool,
            Type x when x == typeof(string) => ProtoFieldType._string,
            Type x when x == typeof(byte[]) => ProtoFieldType.bytes,
            Type x when x == typeof(DateTime) => ProtoFieldType.DateTime,
            Type x when x == typeof(TimeSpan) => ProtoFieldType.TimeSpan,
            Type x when x == typeof(DateTimeOffset) => ProtoFieldType.DateTimeOffset,
            Type x when x == typeof(void) => ProtoFieldType.Void,
            Type x when x == typeof(Type) => ProtoFieldType.Type,
            Type x when x.GetGenericTypeDefinition() == typeof(Expression<>) => ProtoFieldType.Expression,
            _ =>
            throw new Exception($"Unresolved Type:{t}")
        };
        public static implicit operator ProtoFieldType(string value) {
            var pft = default(ProtoFieldType);
            pft.value = value;
            return pft;
        }
        static bool CheckBasic(ProtoFieldType pft) =>
            pft.value == ProtoFieldType.bytes ||
            pft.value == ProtoFieldType.fixed32 ||
            pft.value == ProtoFieldType.fixed64 ||
            pft.value == ProtoFieldType.int32 ||
            pft.value == ProtoFieldType.int64 ||
            pft.value == ProtoFieldType.sfixed32 ||
            pft.value == ProtoFieldType.sfixed64 ||
            pft.value == ProtoFieldType.sint32 ||
            pft.value == ProtoFieldType.sint64 ||
            pft.value == ProtoFieldType.uint32 ||
            pft.value == ProtoFieldType.uint64 ||
            pft.value == ProtoFieldType._bool ||
            pft.value == ProtoFieldType._double ||
            pft.value == ProtoFieldType._float ||
            pft.value == ProtoFieldType._string ||
            pft.value == ProtoFieldType.none ||
            pft.value == ProtoFieldType.DateTime ||
            pft.value == ProtoFieldType.DateTimeOffset ||
            pft.value == ProtoFieldType.TimeSpan ||
            pft.value == ProtoFieldType.Void ||
            pft.value == ProtoFieldType.Any ||
            pft.value == ProtoFieldType.Type ||
            pft.value == ProtoFieldType.Expression;
        #region registry fields
        public static ProtoFieldType none = nameof(none);
        public static ProtoFieldType _double = nameof(_double)[1..];
        public static ProtoFieldType _float = nameof(_float)[1..];
        public static ProtoFieldType int32 = nameof(int32);
        public static ProtoFieldType int64 = nameof(int64);
        public static ProtoFieldType uint32 = nameof(uint32);
        public static ProtoFieldType uint64 = nameof(uint64);
        public static ProtoFieldType sint32 = nameof(sint32);
        public static ProtoFieldType sint64 = nameof(sint64);
        public static ProtoFieldType fixed32 = nameof(fixed32);
        public static ProtoFieldType fixed64 = nameof(fixed64);
        public static ProtoFieldType sfixed32 = nameof(sfixed32);
        public static ProtoFieldType sfixed64 = nameof(sfixed64);
        public static ProtoFieldType _bool = nameof(_bool).Substring(1);
        public static ProtoFieldType _string = nameof(_string).Substring(1);
        public static ProtoFieldType bytes = nameof(bytes);
        public static ProtoFieldType DateTime = "google.protobuf.Timestamp";
        public static ProtoFieldType TimeSpan = "google.protobuf.Duration";
        public static ProtoFieldType DateTimeOffset = "google.protobuf.Timestamp";
        public static ProtoFieldType Void = "google.protobuf.Empty";
        public static ProtoFieldType Any = "google.protobuf.Any";
        public static ProtoFieldType Type = nameof(_string)[1..];
        public static ProtoFieldType Expression = "synario.protobuf.linqtree.Expression";
        public static ProtoFieldType Decimal = "synario.protobuf.base.DecimalValue";
        public static ProtoFieldType BoolNull = "google.protobuf.BoolValue";
        public static ProtoFieldType DoubleNull = "google.protobuf.DoubleValue";
        public static ProtoFieldType FloatNull = "google.protobuf.FloatValue";
        public static ProtoFieldType IntNull = "google.protobuf.Int32Value";
        public static ProtoFieldType LongNull = "google.protobuf.Int64Value";
        public static ProtoFieldType UIntNull = "google.protobuf.UInt32Value";
        public static ProtoFieldType ULongNull = "google.protobuf.UInt64Value";
        public static ProtoFieldType StringNull = "google.protobuf.StringValue";
        public static ProtoFieldType ByteString = "google.protobuf.BytesValue";
        public static ProtoFieldType AnyScalar = "google.protobuf.Value";
        public static ProtoFieldType DecimalValue = "synario.protobuf.base.DecimalValue";
        public static ProtoFieldType Field = "synario.protobuf.base.Field";
        #endregion
    }
}
