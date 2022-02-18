// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Util.Ex;

namespace Util.Generator {
    public delegate bool BoolExpressionHandler(params dynamic[] ps);
    public enum FunctionType {
        None = 0,
        GreaterThan,
        LessThan,
        Equal,
        NotEqual,
        OrElse,
        GreaterEqual,
        LessEqual,
        Or,
        And,
        Not,
        In,
    }
    public enum ValueType {
        None = 0,
        Int,
        Double,
        Float,
        Short,
        Long,
        Byte,
        Boolean,
        DateTime,
        Timespan,
        String,
        IntCollection,
        DoubleCollection,
        FloatCollection,
        ShortCollection,
        LongCollection,
        ByteCollection,
        StringCollection,
        TimespanCollection,
        DateTimeCollection,
        BooleanCollection,
        Entity,
        EntityCollection
    }
    public class BoolExpression {
        public class Node {
            public FunctionType type;
            public Node before;
            public Node after;
            public int id;
            public bool isParameter;
            public int priority;
            public string entityField;
            public Expression exp;
            public Node(int id) {
                this.id = id;
                this.entityField = "";
            }
        }
        public ValueType MapptingTo(object value) {
            var vt = ValueType.None;
            var temp = value.GetType();
            var isCollection = false;
            if (temp.IsCollection()) {
                if (temp.IsArray) {
                    temp = temp.GetElementType();
                } else if (temp != typeof(string)) {
                    temp = temp.GenericTypeArguments[0];
                }
                if (temp != typeof(string))
                    value = Activator.CreateInstance(temp);
                else
                    value = string.Empty;
                isCollection = true;
            }
            vt = (value, isCollection) switch {
                (int, false) => ValueType.Int,
                (int, true) => ValueType.IntCollection,
                (byte, false) => ValueType.Byte,
                (byte, true) => ValueType.ByteCollection,
                (short, false) => ValueType.Short,
                (short, true) => ValueType.ShortCollection,
                (long, false) => ValueType.Long,
                (long, true) => ValueType.LongCollection,
                (double, false) => ValueType.Double,
                (double, true) => ValueType.DoubleCollection,
                (float, false) => ValueType.Float,
                (float, true) => ValueType.FloatCollection,
                (bool, false) => ValueType.Boolean,
                (bool, true) => ValueType.BooleanCollection,
                (DateTime, false) => ValueType.DateTime,
                (DateTime, true) => ValueType.DateTimeCollection,
                (string, false) => ValueType.String,
                (string, true) => ValueType.StringCollection,
                (TimeSpan, false) => ValueType.Timespan,
                (TimeSpan, true) => ValueType.TimespanCollection,
                (_, false) => ValueType.Entity,
                (_, true) => ValueType.EntityCollection,
            };
            return vt;
        }
        int no;
        Node root;
        Node current;
        public BoolExpression() {
            no = 0;
            typeDic = new Dictionary<int, ValueType>();
            valueDic = new Dictionary<int, object>();
        }
        static BoolExpression() {
        }
        public void RegistryOperate(FunctionType ft, int priority = 0) {
            if (root == null) {
                root = new Node(no);
                current = root;
            } else {
                var temp = new Node(no);
                temp.before = current;
                current.after = temp;
                current = temp;
            }
            valueDic.Add(no, null);
            typeDic.Add(no, ValueType.None);
            current.type = ft;
            current.priority = priority;
            no++;
        }
        public void RegistryNode(object value, ValueType vt = ValueType.None, string entityField = "", int priority = 0) {
            if (root == null) {
                root = new Node(no);
                current = root;
            } else {
                var temp = new Node(no);
                temp.before = current;
                current.after = temp;
                current = temp;
            }
            valueDic.Add(no, value);
            if (vt == ValueType.None) {
                vt = MapptingTo(value);
            } else {
                //这里约定假设，使用时，如果传递了ValueType.None之外的其它值，那么它就是一个参数
                current.isParameter = true;
            }
            typeDic.Add(no, vt);
            current.type = FunctionType.None;
            current.priority = priority;
            current.entityField = entityField;
            no++;
        }
        void Add(int id, FunctionType ft, object value, bool isParameter = false, string entityFiled = "", int priority = 0) {
            if (root == null) {
                root = new Node(id);
                current = root;
            } else {
                var temp = new Node(id);
                temp.before = current;
                current.after = temp;
                current = temp;
            }
            // valueDic.Add(id, value);
            current.type = ft;
            current.priority = priority;
            current.isParameter = isParameter;
            current.entityField = entityFiled;
            no++;
        }
        Node Find(int id) {
            var temp = default(Node);
            foreach (var i in TraversalNode()) {
                if (i.id == id) {
                    temp = i;
                    break;
                }
            }
            return temp;
        }
        public IEnumerable<Node> TraversalNode() {
            var temp = root;
            while (temp != null) {
                yield return temp;
                temp = temp.after;
            }
        }
        public event Func<int, Type> Entity;
        protected virtual Type OnEntity(object sender, int id) {
            return Entity?.Invoke(id);
        }
        public Dictionary<int, ValueType> typeDic;
        public Dictionary<int, object> valueDic;
        public string Serialization() {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append($"\"{nameof(valueDic)}\":");
            sb.Append("[");
            foreach (var i in valueDic) {
                sb.Append("{");
                sb.Append($"\"{nameof(i.Key)}\":{i.Key},");
                sb.Append($"\"{nameof(i.Value)}\":\"{i.Value}\"");
                sb.Append("}");
                sb.Append(",");
            }
            sb.Append("],");
            sb.Append($"\"{nameof(typeDic)}\":");
            sb.Append("[");
            foreach (var i in typeDic) {
                sb.Append("{");
                sb.Append($"\"{nameof(i.Key)}\":{i.Key},");
                sb.Append($"\"{nameof(i.Value)}\":\"{i.Value}\"");
                sb.Append("}");
                sb.Append(",");
            }
            sb.Append("],");
            sb.Append("\"nodes\":");
            sb.Append("[");
            foreach (var i in TraversalNode()) {
                sb.Append("{");
                sb.Append($"\"{nameof(i.id)}\":{i.id},");
                sb.Append($"\"{nameof(i.isParameter)}\":{i.isParameter.ToString().ToLower()},");
                sb.Append($"\"{nameof(i.priority)}\":{i.priority},");
                sb.Append($"\"{nameof(i.type)}\":\"{i.type}\",");
                sb.Append($"\"{nameof(i.entityField)}\":\"{i.entityField}\",");
                sb.Append("},");
            }
            sb.Append("]");
            sb.Append("}");
            return sb.ToString().Replace(",]", "]").Replace(",}", "}");
            //return System.Text.Json.JsonSerializer.Serialize(this);
        }
        public static BoolExpression Deserialization(string json) {
            var x = new {
                valueDic = new[]
                {
                    new { Key = 0, Value = default(dynamic)}
                },
                typeDic = new[]
                {
                    new { Key = 0, Value = "" }
                },
                nodes = new[]
                {
                    new { id = 0, isParameter = false, priority = 0, type = "" ,entityField=""}
                }
            };
            dynamic xins = System.Text.Json.JsonSerializer.Deserialize(json, x.GetType());
            //var be = System.Text.Json.JsonSerializer.Deserialize<BoolExpression>(json);
            var be = new BoolExpression();
            foreach (var i in xins.typeDic) {
                be.typeDic.Add(i.Key, Enum.Parse<ValueType>(i.Value));
            }
            foreach (var i in xins.nodes) {
                be.Add(i.id, Enum.Parse<FunctionType>(i.type), xins.valueDic[i.id].Value, i.isParameter, i.entityField, i.priority);
            }
            foreach (var i in xins.valueDic) {
                var je = (System.Text.Json.JsonElement)i.Value;
                if (be.Find((int)i.Key).isParameter) {
                    be.valueDic.Add(i.Key, i.Value.GetString());
                } else {
                    be.valueDic.Add(i.Key, ParseTo(be.typeDic[i.Key], i.Value.GetString()));
                }
            }
            return be;
        }
        static dynamic ParseTo(ValueType vt, string je) {
            return vt switch {
                ValueType.Boolean => Convert.ToBoolean(je),
                ValueType.Int => Convert.ToInt32(je),
                ValueType.Byte => Convert.ToByte(je),
                ValueType.Short => Convert.ToInt16(je),
                ValueType.Long => Convert.ToInt64(je),
                ValueType.Double => Convert.ToDouble(je),
                ValueType.Float => Convert.ToSingle(je),
                ValueType.DateTime => Convert.ToDateTime(je),
                ValueType.Timespan => TimeSpan.Parse(je),
                ValueType.String => je,
                ValueType.None => je,
            };
        }
        int GetParameterCount() {
            var count = 0;
            foreach (var i in this.TraversalNode()) {
                if (typeDic[i.id] != ValueType.None && i.type == FunctionType.None) count++;
            }
            return count;
        }
        int GetOrder(FunctionType ft) {
            var i = ft switch {
                FunctionType.None => -1,
                FunctionType.In => 0,
                FunctionType.Or => 1,
                FunctionType.OrElse => 2,
                FunctionType.And => 3,
                FunctionType.Equal => 4,
                FunctionType.NotEqual => 4,
                FunctionType.GreaterEqual => 5,
                FunctionType.GreaterThan => 5,
                FunctionType.LessEqual => 5,
                FunctionType.LessThan => 5,
                FunctionType.Not => 6,
                _ => throw new Exception("不支持的bool运算符")
            };
            return i;
        }
        Type MappingToType(ValueType vt) =>
        vt switch {
            ValueType.String => typeof(string),
            ValueType.StringCollection => typeof(IEnumerable<string>),
            ValueType.Boolean => typeof(bool),
            ValueType.BooleanCollection => typeof(IEnumerable<bool>),
            ValueType.Double => typeof(double),
            ValueType.DoubleCollection => typeof(IEnumerable<double>),
            ValueType.Int => typeof(int),
            ValueType.IntCollection => typeof(IEnumerable<int>),
            ValueType.Float => typeof(float),
            ValueType.FloatCollection => typeof(IEnumerable<float>),
            ValueType.Short => typeof(short),
            ValueType.ShortCollection => typeof(IEnumerable<short>),
            ValueType.Byte => typeof(byte),
            ValueType.ByteCollection => typeof(IEnumerable<byte>),
            ValueType.Long => typeof(long),
            ValueType.LongCollection => typeof(IEnumerable<long>),
            ValueType.DateTime => typeof(DateTime),
            ValueType.DateTimeCollection => typeof(IEnumerable<DateTime>),
            ValueType.Timespan => typeof(TimeSpan),
            ValueType.TimespanCollection => typeof(IEnumerable<TimeSpan>),
        };
        Expression GetConstentEntityExp(Node node) {
            var result = default(Expression);
            // var entity = OnEntity(this, node.id);
            //是否存在属性访问
            if (string.IsNullOrWhiteSpace(node.entityField)) {
                result = Expression.Constant(valueDic[node.id]);
            } else {
                var x = Expression.Constant(valueDic[node.id]);
                result = Expression.PropertyOrField(x, node.entityField);
            }
            return result;
        }
        Expression GetParameterEntityExp(Node node, List<ParameterExpression> pslist) {
            var result = default(Expression);
            var entityType = OnEntity(this, node.id);
            //是否存在属性访问
            if (string.IsNullOrWhiteSpace(node.entityField)) {
                result = Expression.Parameter(entityType);
                pslist.Add(result as ParameterExpression);
            } else {
                var x = Expression.Parameter(entityType);
                result = Expression.PropertyOrField(x, node.entityField);
                pslist.Add(x);
            }
            return result;
        }
        Expression GetParameterEntityCollectionExp(Node node, List<ParameterExpression> pslist) {
            var result = default(Expression);
            var entityType = OnEntity(this, node.id);
            //是否存在属性访问
            if (string.IsNullOrWhiteSpace(node.entityField)) {
                result = Expression.Parameter(entityType);
                pslist.Add(result as ParameterExpression);
            } else {
                var x = Expression.Parameter(entityType);
                result = Expression.PropertyOrField(x, node.entityField);
                pslist.Add(x as ParameterExpression);
            }
            return result;
        }
        public BoolExpressionHandler Compile() {
            var boolexp = default(Expression);
            var pslist = new List<ParameterExpression>();
            //第一轮遍历在于构造出常量和参数表达式基础节点
            foreach (var i in from j in this.TraversalNode() where j.type == FunctionType.None select j) {
                if (i.isParameter) {
                    i.exp = typeDic[i.id] switch {
                        ValueType.Int => Expression.Parameter(typeof(int)),
                        ValueType.IntCollection => Expression.Parameter(typeof(IEnumerable<int>)),
                        ValueType.Short => Expression.Parameter(typeof(short)),
                        ValueType.ShortCollection => Expression.Parameter(typeof(IEnumerable<short>)),
                        ValueType.Byte => Expression.Parameter(typeof(byte)),
                        ValueType.ByteCollection => Expression.Parameter(typeof(IEnumerable<byte>)),
                        ValueType.Long => Expression.Parameter(typeof(long)),
                        ValueType.LongCollection => Expression.Parameter(typeof(IEnumerable<long>)),
                        ValueType.Float => Expression.Parameter(typeof(float)),
                        ValueType.FloatCollection => Expression.Parameter(typeof(IEnumerable<float>)),
                        ValueType.Boolean => Expression.Parameter(typeof(bool)),
                        ValueType.BooleanCollection => Expression.Parameter(typeof(IEnumerable<bool>)),
                        ValueType.DateTime => Expression.Parameter(typeof(DateTime)),
                        ValueType.DateTimeCollection => Expression.Parameter(typeof(IEnumerable<DateTime>)),
                        ValueType.Double => Expression.Parameter(typeof(double)),
                        ValueType.DoubleCollection => Expression.Parameter(typeof(IEnumerable<double>)),
                        ValueType.String => Expression.Parameter(typeof(string)),
                        ValueType.StringCollection => Expression.Parameter(typeof(IEnumerable<string>)),
                        ValueType.Timespan => Expression.Parameter(typeof(TimeSpan)),
                        ValueType.TimespanCollection => Expression.Parameter(typeof(IEnumerable<TimeSpan>)),
                        ValueType.Entity => GetParameterEntityExp(i, pslist),
                        ValueType.EntityCollection => GetParameterEntityCollectionExp(i, pslist),
                    };
                    if (typeDic[i.id] != ValueType.Entity && typeDic[i.id] != ValueType.EntityCollection)
                        pslist.Add(i.exp as ParameterExpression);
                } else {
                    // 常量
                    i.exp = typeDic[i.id] switch {
                        ValueType.Entity => GetConstentEntityExp(i),
                        _ => Expression.Constant(valueDic[i.id], MappingToType(typeDic[i.id])),
                    };
                }
            }
            var boolNodes = from i in this.TraversalNode()
                            where i.type != FunctionType.None
                            orderby i.priority descending
                            orderby GetOrder(i.type) descending
                            select i;
            foreach (var item in boolNodes) {
                if (item.before == null && item.after == null)
                    throw new Exception("bool 表达式错误");
                item.exp = item.type switch {
                    FunctionType.And => Expression.And(item.before.exp, item.after.exp),
                    FunctionType.OrElse => Expression.OrElse(item.before.exp, item.after.exp),
                    FunctionType.GreaterEqual => Expression.GreaterThanOrEqual(item.before.exp, item.after.exp),
                    FunctionType.GreaterThan => Expression.GreaterThan(item.before.exp, item.after.exp),
                    FunctionType.LessThan => Expression.LessThan(item.before.exp, item.after.exp),
                    FunctionType.LessEqual => Expression.LessThanOrEqual(item.before.exp, item.after.exp),
                    FunctionType.Not => Expression.Not(item.after.exp),
                    FunctionType.NotEqual => Expression.NotEqual(item.before.exp, item.after.exp),
                    FunctionType.Equal => Expression.Equal(item.before.exp, item.after.exp),
                    FunctionType.Or => Expression.Or(item.before.exp, item.after.exp),
                    FunctionType.In => Expression.Or(item.before.exp, item.after.exp),
                };
                boolexp = item.exp;
            }
            // return result.Compile();
            return objs => {
                var result = false;
                var idx = 0;
                // dynamic value;
                List<BinaryExpression> assigns = new();
                foreach (var item in objs) {
                    // value = ConvertTo(pslist[idx].Type, item);
                    assigns.Add(Expression.Assign(pslist[idx], Expression.Constant(item)));
                    idx++;
                }
                var x = Expression.Lambda<Func<bool>>(
                    Expression.Block(
                        pslist,
                        assigns.Union(new[] { boolexp })
                    // boolexp
                    )
                );
                result = x.Compile()();
                return result;
            };
        }
        public Expression<BoolExpressionHandler> Build() {
            var boolexp = default(Expression);
            var pslist = new List<ParameterExpression>();
            //第一轮遍历在于构造出常量和参数表达式基础节点
            foreach (var i in from j in this.TraversalNode() where j.type == FunctionType.None select j) {
                if (i.isParameter) {
                    i.exp = typeDic[i.id] switch {
                        ValueType.Int => Expression.Parameter(typeof(int)),
                        ValueType.IntCollection => Expression.Parameter(typeof(IEnumerable<int>)),
                        ValueType.Short => Expression.Parameter(typeof(short)),
                        ValueType.ShortCollection => Expression.Parameter(typeof(IEnumerable<short>)),
                        ValueType.Byte => Expression.Parameter(typeof(byte)),
                        ValueType.ByteCollection => Expression.Parameter(typeof(IEnumerable<byte>)),
                        ValueType.Long => Expression.Parameter(typeof(long)),
                        ValueType.LongCollection => Expression.Parameter(typeof(IEnumerable<long>)),
                        ValueType.Float => Expression.Parameter(typeof(float)),
                        ValueType.FloatCollection => Expression.Parameter(typeof(IEnumerable<float>)),
                        ValueType.Boolean => Expression.Parameter(typeof(bool)),
                        ValueType.BooleanCollection => Expression.Parameter(typeof(IEnumerable<bool>)),
                        ValueType.DateTime => Expression.Parameter(typeof(DateTime)),
                        ValueType.DateTimeCollection => Expression.Parameter(typeof(IEnumerable<DateTime>)),
                        ValueType.Double => Expression.Parameter(typeof(double)),
                        ValueType.DoubleCollection => Expression.Parameter(typeof(IEnumerable<double>)),
                        ValueType.String => Expression.Parameter(typeof(string)),
                        ValueType.StringCollection => Expression.Parameter(typeof(IEnumerable<string>)),
                        ValueType.Timespan => Expression.Parameter(typeof(TimeSpan)),
                        ValueType.TimespanCollection => Expression.Parameter(typeof(IEnumerable<TimeSpan>)),
                        ValueType.Entity => GetParameterEntityExp(i, pslist),
                        ValueType.EntityCollection => GetParameterEntityCollectionExp(i, pslist),
                    };
                    if (typeDic[i.id] != ValueType.Entity && typeDic[i.id] != ValueType.EntityCollection)
                        pslist.Add(i.exp as ParameterExpression);
                } else {
                    // 常量
                    i.exp = typeDic[i.id] switch {
                        ValueType.Entity => GetConstentEntityExp(i),
                        _ => Expression.Constant(valueDic[i.id], MappingToType(typeDic[i.id])),
                    };
                }
            }
            var boolNodes = from i in this.TraversalNode()
                            where i.type != FunctionType.None
                            orderby i.priority descending
                            orderby GetOrder(i.type) descending
                            select i;
            foreach (var item in boolNodes) {
                if (item.before == null && item.after == null)
                    throw new Exception("bool 表达式错误");
                item.exp = item.type switch {
                    FunctionType.And => Expression.And(item.before.exp, item.after.exp),
                    FunctionType.OrElse => Expression.OrElse(item.before.exp, item.after.exp),
                    FunctionType.GreaterEqual => Expression.GreaterThanOrEqual(item.before.exp, item.after.exp),
                    FunctionType.GreaterThan => Expression.GreaterThan(item.before.exp, item.after.exp),
                    FunctionType.LessThan => Expression.LessThan(item.before.exp, item.after.exp),
                    FunctionType.LessEqual => Expression.LessThanOrEqual(item.before.exp, item.after.exp),
                    FunctionType.Not => Expression.Not(item.after.exp),
                    FunctionType.NotEqual => Expression.NotEqual(item.before.exp, item.after.exp),
                    FunctionType.Equal => Expression.Equal(item.before.exp, item.after.exp),
                    FunctionType.Or => Expression.Or(item.before.exp, item.after.exp),
                    FunctionType.In => Expression.Or(item.before.exp, item.after.exp),
                };
                boolexp = item.exp;
            }
            var ps = Expression.Parameter(typeof(dynamic[]));
            var idx = 0;
            var assexp = new List<Expression>();
            foreach (var i in pslist) {
                assexp.Add(
                Expression.Assign(i,
                    Expression.Convert(
                        Expression.ArrayAccess(ps, Expression.Constant(idx))
                        , i.Type
                    )
                )
                ); idx++;
            }
            // var idxexp = Expression.Variable(typeof(int));
            var result = Expression.Lambda<BoolExpressionHandler>(
                Expression.Block(
                    pslist,
                    assexp.Union(new[] { boolexp })

                ), ps
            );
            return result;
        }
        public Expression<Func<dynamic, bool>> Build1() {
            var boolexp = default(Expression);
            var pslist = new List<ParameterExpression>();
            //第一轮遍历在于构造出常量和参数表达式基础节点
            foreach (var i in from j in this.TraversalNode() where j.type == FunctionType.None select j) {
                if (i.isParameter) {
                    i.exp = typeDic[i.id] switch {
                        ValueType.Int => Expression.Parameter(typeof(int)),
                        ValueType.IntCollection => Expression.Parameter(typeof(IEnumerable<int>)),
                        ValueType.Short => Expression.Parameter(typeof(short)),
                        ValueType.ShortCollection => Expression.Parameter(typeof(IEnumerable<short>)),
                        ValueType.Byte => Expression.Parameter(typeof(byte)),
                        ValueType.ByteCollection => Expression.Parameter(typeof(IEnumerable<byte>)),
                        ValueType.Long => Expression.Parameter(typeof(long)),
                        ValueType.LongCollection => Expression.Parameter(typeof(IEnumerable<long>)),
                        ValueType.Float => Expression.Parameter(typeof(float)),
                        ValueType.FloatCollection => Expression.Parameter(typeof(IEnumerable<float>)),
                        ValueType.Boolean => Expression.Parameter(typeof(bool)),
                        ValueType.BooleanCollection => Expression.Parameter(typeof(IEnumerable<bool>)),
                        ValueType.DateTime => Expression.Parameter(typeof(DateTime)),
                        ValueType.DateTimeCollection => Expression.Parameter(typeof(IEnumerable<DateTime>)),
                        ValueType.Double => Expression.Parameter(typeof(double)),
                        ValueType.DoubleCollection => Expression.Parameter(typeof(IEnumerable<double>)),
                        ValueType.String => Expression.Parameter(typeof(string)),
                        ValueType.StringCollection => Expression.Parameter(typeof(IEnumerable<string>)),
                        ValueType.Timespan => Expression.Parameter(typeof(TimeSpan)),
                        ValueType.TimespanCollection => Expression.Parameter(typeof(IEnumerable<TimeSpan>)),
                        ValueType.Entity => GetParameterEntityExp(i, pslist),
                        ValueType.EntityCollection => GetParameterEntityCollectionExp(i, pslist),
                    };
                    if (typeDic[i.id] != ValueType.Entity && typeDic[i.id] != ValueType.EntityCollection)
                        pslist.Add(i.exp as ParameterExpression);
                } else {
                    // 常量
                    i.exp = typeDic[i.id] switch {
                        ValueType.Entity => GetConstentEntityExp(i),
                        _ => Expression.Constant(valueDic[i.id], MappingToType(typeDic[i.id])),
                    };
                }
            }
            var boolNodes = from i in this.TraversalNode()
                            where i.type != FunctionType.None
                            orderby i.priority descending
                            orderby GetOrder(i.type) descending
                            select i;
            foreach (var item in boolNodes) {
                if (item.before == null && item.after == null)
                    throw new Exception("bool 表达式错误");
                item.exp = item.type switch {
                    FunctionType.And => Expression.And(item.before.exp, item.after.exp),
                    FunctionType.OrElse => Expression.OrElse(item.before.exp, item.after.exp),
                    FunctionType.GreaterEqual => Expression.GreaterThanOrEqual(item.before.exp, item.after.exp),
                    FunctionType.GreaterThan => Expression.GreaterThan(item.before.exp, item.after.exp),
                    FunctionType.LessThan => Expression.LessThan(item.before.exp, item.after.exp),
                    FunctionType.LessEqual => Expression.LessThanOrEqual(item.before.exp, item.after.exp),
                    FunctionType.Not => Expression.Not(item.after.exp),
                    FunctionType.NotEqual => Expression.NotEqual(item.before.exp, item.after.exp),
                    FunctionType.Equal => Expression.Equal(item.before.exp, item.after.exp),
                    FunctionType.Or => Expression.Or(item.before.exp, item.after.exp),
                    FunctionType.In => Expression.Or(item.before.exp, item.after.exp),
                };
                boolexp = item.exp;
            }
            var p = Expression.Parameter(typeof(object));
            // var idx = 0;
            var assexp = new List<Expression>();
            // foreach (var i in pslist)
            // {
            //      idx++;
            // }
            assexp.Add(
                Expression.Assign(pslist[0],
                    Expression.Convert(
                        p
                        , pslist[0].Type
                    )
                )
                );
            // var idxexp = Expression.Variable(typeof(int));
            var result = Expression.Lambda<Func<dynamic, bool>>(
                Expression.Block(
                    new[] { pslist[0] },
                    assexp.Union(new[] { boolexp })

                ), p
            );
            return result;
        }

        Expression InFunc(Node i) {
            if (i.after == null)
                throw new Exception("In 操作后节点不能为空");
            bool isCollection = false;
            switch (typeDic[i.after.id]) {
                case ValueType.Boolean: isCollection = false; break;
                case ValueType.Double: isCollection = false; break;
                case ValueType.DateTime: isCollection = false; break;
                case ValueType.Timespan: isCollection = false; break;
                case ValueType.String: isCollection = false; break;
                case ValueType.BooleanCollection: isCollection = false; break;
                case ValueType.DoubleCollection: isCollection = false; break;
                case ValueType.DateTimeCollection: isCollection = false; break;
                case ValueType.TimespanCollection: isCollection = false; break;
                case ValueType.StringCollection: isCollection = false; break;
                    // case _:throw new Exception("In函数所需要的节点类型不在范围内");
            };
            if (!isCollection)
                throw new Exception("In操作需要后节点是一个集合");
            var mi = typeof(Enumerable)
            .GetMethod(
                nameof(Enumerable.Contains)
                , 1
                , new[]{
                    typeof(IEnumerable<>).MakeGenericType(Type.MakeGenericMethodParameter(0))
                    ,Type.MakeGenericMethodParameter(0)
                }
            );
            throw new NotImplementedException();
        }
    }
}
