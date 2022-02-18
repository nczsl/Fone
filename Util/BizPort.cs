// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Util.BizPort {
    /// <summary>
    /// 包装从web后台向前端返回的业务对象的类
    /// </summary>
    /// <typeparam name="T">业务类</typeparam>
    public class Result<T> {
        public string Message { get; set; }
        /// <summary>
        /// 传输目标，比如前段查询后端处理后的返回结果等
        /// </summary>
        public T Content { get; set; }
        public string Token { get; set; }
        public bool Success { get; set; }
        /// <summary>
        /// 中转，从RequestT 到 ResultT
        /// </summary>     
        public string EmmetExpression { get; set; }
    }
    /// <summary>
    /// 封装请求
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Request<T> {
        public T Content { get; set; }
        public string Token { get; set; }
        public string EmmetExpression { get; set; }
    }
    public class DataSource {
        public enum ColumnType {
            Int32, Int64, Float, Double, Bool,
            String, Bytes, DateTime, TimeSpan,
        }
        public class Column {
            public ColumnType Type { get; set; }
            public string Name { get; set; }
        }
        public class Row {
            public List<object> Items { get; set; }
        }
        public List<string> SourceNames { get; set; }
        public List<Column> Columns { get; set; }
        public List<Row> Rows { get; set; }
        public int Length { get; set; }
    }
}
