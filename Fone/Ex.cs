using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace Fone {
    /// <summary>
    /// extention
    /// </summary>
    static public class Ex {
        static public Sqlitefo ConfigLogSqlite(this string dbpath) {
            if (!System.IO.File.Exists(dbpath)) {
                System.IO.File.Create(dbpath);
            }
            return Sqlitefo.Attach(dbpath);
        }
        static public void LogToSqlite(this Sqlitefo sfo,string msg, string note = "") {
            try {
                var slog = new SimpleLog();
                slog.Message = msg;
                slog.Note = note;
                slog.OnTime = DateTime.Now;
                sfo.Insert(slog);
            } catch (Exception e) {
                System.Console.WriteLine(e.Message);
            }
        }
        //static public void GrpcServerEndpontConfig(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder erp) {

        //}
        /// <summary>
        /// 主要用于没有ToString方法的DTO类的ToString方法,采用Json格式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="r"></param>
        /// <returns></returns>
        static public string ToStr<T>(this T r) => System.Text.Json.JsonSerializer.Serialize(r);
        /// <summary>
        /// 添加一个自定义简单日志，路径：logs/...
        /// </summary>
        /// <param name="logBuilder"></param>
        /// <returns></returns>
        static public ILoggingBuilder AddSimpleLogger(this ILoggingBuilder logBuilder) {
            logBuilder.ClearProviders();
            logBuilder.AddProvider(new FileLogProvider());
            return logBuilder;
        }
        /// <summary>
        /// 添加一个自定义简单日志，路径：采用指定绝对路径的片断比如:f:/abc
        /// </summary>
        /// <param name="logBuilder"></param>
        /// <param name="rootDir">最好指定绝对路径做为整个路径的起始片断</param>
        /// <returns></returns>
        static public ILoggingBuilder AddSimpleLogger(this ILoggingBuilder logBuilder, string rootDir) {
            logBuilder.ClearProviders();
            logBuilder.AddProvider(new FileLogProvider(rootDir));
            return logBuilder;
        }
        /// <summary>
        ///  “GrpcChannels":[{"Key":"xxx","Value":"xxx"},...]
        /// </summary>
        /// <param name="isc">~</param>
        /// <param name="addressDic">可选grpc地址配置字典，若存在值，则被注册为全局变量为后续grpc chaneel配置时使用</param>
        static public void RegistryGrpcClient(this IServiceCollection isc, Dictionary<string, string> addressDic = null) {
            if (addressDic != null && addressDic.Count > 0)
                isc.AddSingleton(addressDic);
            isc.AddTransient<IGrpcChannel, GrpcChannelDic>();
        }
        /// <summary>
        /// 注册一个后台服务代码参考了msdn官方文档
        /// </summary>
        /// <param name="isc"></param>
        static public void RegsitryBackgroundTask(this IServiceCollection isc) {
            isc.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            isc.AddHostedService<QueuedHostedService>();
        }
        /// <summary>
        /// 注册Orleans客户端，Orleans日志已经配置
        /// </summary>
        /// <param name="isc"></param>
        /// <param name="clientSetting">Orleans集群客户端配置</param>
        // static public void RegistryOrleansClient(this IServiceCollection isc, IConfiguration configuration, Action<ClientBuilder, ClusterConfig> clientSetting) {
        //     var clientBuilder = new ClientBuilder();
        //     var clusterConfig = configuration.GetSection("ClusterConfig").Get<ClusterConfig>();
        //     clientSetting.Invoke(clientBuilder, clusterConfig);
        //     isc.AddSingleton<IClusterClient>(clientBuilder.Build());
        //     isc.AddSingleton<ClusterClientHostedService>();
        //     isc.AddSingleton<IHostedService>(_ => _.GetService<ClusterClientHostedService>());
        // }
        /// <summary>
        /// 设置绝对或相对过期时间，默认为0分钟表示没有过期时间
        /// </summary>
        /// <param name="minutes">分钟为正是绝对时间，负是流动时间</param>
        /// <returns></returns>
        static public MemoryCacheEntryOptions GetOption(this IMemoryCache cache, int minutes = 0) {
            var op = new MemoryCacheEntryOptions();
            switch (minutes) {
                case int i when i > 0:
                op.AbsoluteExpiration = DateTime.Now.AddMinutes(minutes);
                break;
                case int i when i < 0:
                op.SlidingExpiration = TimeSpan.FromMinutes(Math.Abs(minutes));
                break;
            }
            //_memoryCache.Set<string>("timestamp", DateTime.Now.ToString(), options);    }
            return op;
        }
        /// <summary>
        /// 返回聚合查询的结果集，前端使用 net emmet 可以直接 解析
        /// </summary>
        /// <param name="db"></param>
        /// <param name="querysql"></param>
        /// <returns>临时结果集所构成的表，特别针对于多表组合成的动态结构的表</returns>
        static public DataTable MutiQuery(this DatabaseFacade db, string querysql, DbDataAdapter dda) {
            var r = new DataTable();
            var con = db.GetDbConnection();
            var cmd = con.CreateCommand();
            try {
                cmd.CommandText = querysql;
                dda.SelectCommand = cmd;
                dda.Fill(r);
            } catch (Exception e) {
                throw e;
            } finally {
                dda.Dispose();
                cmd.Dispose();
                con.Close();
            }
            return r;
        }
        /// <summary>
        /// 为类HttpClient 构造HttpContent，通常用于post参数
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        static public ByteArrayContent GetBytesContent(this Stream stream, string fileName, string contentType = "application/octet-stream") {
            var buffer = new byte[stream.Length];
            stream.Read(buffer);
            var fileContent = new ByteArrayContent(buffer);
            fileContent.Headers.Clear();
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") {
                Name = "\"file\"",
                FileName = "\"" + fileName + "\""
            }; // the extra quotes are key here
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return fileContent;
        }
        //static public IDictionary<string, object> Dyas(this object source) => source as IDictionary<string, object>;
        /// <summary>
        /// 将元组键值对数组转换成一个动态对象
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        static public dynamic GetEooAttatch(params (string id, object value)[] selector) {
            var r = new ExpandoObject();
            var r2 = (IDictionary<string, object>)r;
            foreach (var (id, value) in selector) {
                r2[id] = value;
            }
            return r2;
        }
        /// <summary>
        /// from i in source select GetEoo(i)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mainsource"></param>
        /// <returns></returns>
        static public dynamic GetEoo<T>(this T mainsource, bool isField = false, params (string id, object value)[] sencondarysource) {
            var r = new ExpandoObject();
            var r2 = (IDictionary<string, dynamic>)r;
            if (isField) {
                foreach (var item in mainsource.GetType().GetFields()) {
                    r2[item.Name] = item.GetValue(mainsource);
                }
            } else {
                foreach (var item in mainsource.GetType().GetProperties()) {
                    r2[item.Name] = item.GetValue(mainsource);
                }
            }
            foreach (var (id, value) in sencondarysource) {
                r2[id] = value;
            }
            return r;
        }
        /// <summary>
        /// 动态对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selector"></param>
        /// <param name="isField"></param>
        /// <returns></returns>
        static public dynamic GetEoo<T>(this T selector, bool isField = false) {
            var r = new ExpandoObject();
            var r2 = (IDictionary<string, dynamic>)r;
            if (isField) {
                foreach (var item in selector.GetType().GetFields()) {
                    r2[item.Name] = item.GetValue(selector);
                }
            } else {
                foreach (var item in selector.GetType().GetProperties()) {
                    r2[item.Name] = item.GetValue(selector);
                }
            }
            return r;
        }
        /// <summary>
        /// 集合=> DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="models"></param>
        /// <returns></returns>
        static public DataTable FromModels<T>(this IEnumerable<T> models) {
            var dt = new DataTable();
            var ps = typeof(T).GetProperties();
            foreach (var item in ps) {
                dt.Columns.Add(new DataColumn(item.Name, item.PropertyType));
            }
            foreach (var item in models) {
                var row = dt.NewRow();
                foreach (var it in ps) {
                    row[it.Name] = it.GetValue(item, null);
                }
                dt.Rows.Add(row);
            }
            return dt;
        }
        /// <summary>
        /// DataTable=>List<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        static public List<T> ToModels<T>(this DataTable dt) where T : new() {
            var ps = typeof(T).GetProperties();
            var xx = dt.Columns.Cast<DataColumn>();
            var ppp = from i in xx
                      join j in ps
                      on i.ColumnName equals j.Name
                      select j;
            var r = new List<T>();
            foreach (DataRow item in dt.Rows) {
                var t = new T();
                foreach (var it in ppp) {
                    it.SetValue(t, Convert.ChangeType(item[it.Name], it.PropertyType));
                }
                r.Add(t);
            }
            return r;
        }
        /// <summary>
        /// 使用了回传ref值，这是C#7.x以上的版本支持的特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matrix"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static ref T FindRef<T>(this T[,] matrix, Predicate<T> predicate) where T : struct {
            for (var i = 0; i < matrix.GetLength(0); i++)
                for (var j = 0; j < matrix.GetLength(1); j++)
                    if (predicate(matrix[i, j]))
                        return ref matrix[i, j];
            throw new InvalidOperationException("Not found");
        }
        /// <summary>
        /// 获取汇总求和数据
        /// var dt = GetReport(models.AsQueryable(), l => l.Name, l => l.Max(k => k.InCome), l => l.Min(k => k.InCome), l => l.Sum(k => k.InCome));
        /// </summary>
        /// <typeparam name="T">一般是 poco类型</typeparam>
        /// <param name="collection">需要进行分组统计的poco集合</param>
        /// <param name="groupby">linq中的Groupby根据T中某个字段进行分组</param>
        /// <param name="expressions">各种统计名目</param>
        /// <returns></returns>
        static public DataTable GetReport<T>(this IQueryable<T> collection, Expression<Func<T, String>> groupby, params Expression<Func<IQueryable<T>, double>>[] expressions) {
            var table = new DataTable();
            //  Message：利用表达式设置列名称 
            var memberExpression = groupby.Body as MemberExpression;
            var displayName = "";
            if (memberExpression.Member.CustomAttributes.Count() > 0) {
                displayName = (memberExpression.Member.GetCustomAttributes(false)[0] as System.ComponentModel.DescriptionAttribute).Description;
            } else {
                displayName = memberExpression.Member.Name;
            }
            table.Columns.Add(new DataColumn(displayName));
            foreach (var expression in expressions) {
                var dynamicExpression = expression.Body as MethodCallExpression;
                var groupName = dynamicExpression.Method.Name;
                var unaryexpression = dynamicExpression.Arguments[1] as UnaryExpression;
                var LambdaExpression = unaryexpression.Operand as LambdaExpression;
                memberExpression = LambdaExpression.Body as MemberExpression;
                if (memberExpression.Member.CustomAttributes.Count() > 0) {
                    displayName = (memberExpression.Member.GetCustomAttributes(false)[0] as System.ComponentModel.DescriptionAttribute).Description;
                } else {
                    displayName = memberExpression.Member.Name;
                }
                table.Columns.Add(new DataColumn(displayName + $"({groupName})"));
            }
            //  Message：通过表达式设置数据体 
            var groups = collection.GroupBy(groupby);
            foreach (var group in groups) {
                //  Message：设置分组列头
                var dataRow = table.NewRow();
                dataRow[0] = group.Key;
                //  Message：设置分组汇总数据
                for (var i = 0; i < expressions.Length; i++) {
                    var expression = expressions[i];
                    var fun = expression.Compile();
                    dataRow[i + 1] = fun(group.AsQueryable());
                }
                table.Rows.Add(dataRow);
            }
            return table;
        }
    }
}