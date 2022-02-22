// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Util.BizPort;
using Util.Ex;

namespace Util.Generator {
    public class Configh {
        static Configh cc;
        public static Configh Deputy {
            get { if (cc == null) cc = new Configh(); return cc; }
        }
        public void ResetProtobufExpressionT(string path) {
            var x1 = typeof(Expression<>).Assembly;
            var ts = (from i in x1.GetTypes()
                      where i.IsSubclassOf(typeof(Expression))
                      select i).Union(new[] { typeof(Expression) });
            var psps = new ProtoNamespace();
            psps.package = "synario.protobuf.linqtree";
            psps.imports.Add("design/data.proto");
            //psps.LoadLocalProto();
            foreach (var item in ts) {
                ProtoUtil.GenDesignProtoMessage(item, psps);
            }
            ProtoUtil.ProtoInherit(ts, psps);
            psps.GetEnum("GotoExpressionKind").refowner = psps.GetMessage("GotoExpression");
            SaveTo(path, psps.ToString());
        }
        public void ResetProtobufDataSource(string path) {
            var psps = new ProtoNamespace();
            psps.package = "synario.protobuf.data";
            var ts = new[] {
                typeof(DataSource.Column),
                typeof(DataSource.Row),
                typeof(DataSource),
            };
            foreach (var item in ts) {
                ProtoUtil.GenDesignProtoMessage(item, psps);
            }
            ProtoUtil.ProtoInherit(ts, psps);
            SaveTo(path, psps.ToString().Replace("Items1s", "Cell").Replace("Items1", "Item"));
        }
        /// <summary>
        /// 返回所有最里层平衡组的模式
        /// </summary>
        /// <param name="open_exp">开始串 例如:(,[,{;等</param>
        /// <param name="close_exp">结束串 例如:),],}等</param>
        /// <returns></returns>
        public string BalancedGroupPattern(char open_exp = '<', char close_exp = '>', bool inorout = false) {
            var r = "<((?'x'<)|(?'-x'>)|[^<>]*)(?(x)(?!))>";
            if (inorout) r = "<((?'x'<)|(?'-x'>)|[^<>]*)+(?(x)(?!))>";
            if (open_exp != '<' && close_exp != '>')
                r = r.Replace('<', open_exp).Replace('>', close_exp);
            return r;
        }
        public string BalancedGroupPattern(string open_exp, string close_exp, bool inorout = false) {
            var r = "<((?'x'<)|(?'-x'>)|[^<>]*)(?(x)(?!))>";
            if (inorout) r = "<((?'x'<)|(?'-x'>)|[^<>]*)+(?(x)(?!))>";
            r = r.Replace("<", open_exp).Replace(">", close_exp);
            return r;
        }
        /// <summary>
        /// 从一个xsd文档生成其对应的cs代码
        /// </summary>
        /// <param name="target_xsdpath">xsd文档内容</param>
        /// <param name="outdir">输出位置</param>
        /// <param name="xsdpath">xsd.exe工具位置</param>
        public void GenerateClassByXsd(string target_xsdpath, string outdir, string xsdpath) {
            var p = new System.Diagnostics.Process();
            p.StartInfo.FileName = xsdpath;
            p.StartInfo.ErrorDialog = true;
            p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            p.StartInfo.Arguments = string.Format("{0} /c /l:cs /f /o:{1}", target_xsdpath, outdir);
            p.Start();
            p.Close();
        }
        /// <summary>
        /// 生成cs 类从对应的 xsd文件
        /// </summary>
        /// <param name="outdir">输出位置</param>
        /// <param name="xsdpath">xsd.exe工具路径</param>
        /// <param name="target_xsdpaths">若干个xsd文档的路径</param>
        public void GenerateClassByXsds(string outdir, string xsdpath, params string[] target_xsdpaths) {
            if (target_xsdpaths == null || target_xsdpaths.Length < 1) {
                return;
            }
            var p = new System.Diagnostics.Process();
            p.StartInfo.FileName = xsdpath;
            p.StartInfo.ErrorDialog = true;
            p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            p.StartInfo.Arguments = string.Format("{0} /c /l:cs /f /o:{1}", string.Join(" ", target_xsdpaths), outdir);
            p.Start();
            p.Close();
        }
        /*Application Name（应用程序名称）：应用程序的名称。如果没有被指定的话，它的值为.NET SqlClient Data Provider（数据提供程序）.
         •AttachDBFilename／extended properties（扩展属性）／Initial File Name（初始文件名）：可连接数据库的主要文件的名称，包括完整路径名称。数据库名称必须用关键字数据库指定。
         •Connect Timeout（连接超时）／Connection Timeout（连接超时）：一个到服务器的连接在终止之前等待的时间长度（以秒计），缺省值为15。
         •Connection Lifetime（连接生存时间）：当一个连接被返回到连接池时，它的创建时间会与当前时间进行对比。如果这个时间跨度超过了连接的有效期的话，连接就被取消。其缺省值为0。
         •Connection Reset（连接重置）：表示一个连接在从连接池中被移除时是否被重置。一个伪的有效在获得一个连接的时候就无需再进行一个额外的服务器来回运作，其缺省值为真。
         •Current Language（当前语言）：SQL Server语言记录的名称。
         •Data Source（数据源）／Server（服务器）／Address（地址）／Addr（地址）／Network Address（网络地址）：SQL Server实例的名称或网络地址。
         •Encrypt（加密）：当值为真时，如果服务器安装了授权证书，SQL Server就会对所有在客户和服务器之间传输的数据使用SSL加密。被接受的值有true（真）、false（伪）、yes（是）和no（否）。
         •Enlist（登记）：表示连接池程序是否会自动登记创建线程的当前事务语境中的连接，其缺省值为真。
         •Database（数据库）／Initial Catalog（初始编目）：数据库的名称。
         •Integrated Security（集成安全）／Trusted Connection（受信连接）：表示Windows认证是否被用来连接数据库。它可以被设置成真、伪或者是和真对等的sspi，其缺省值为伪。
         •Max Pool Size（连接池的最大容量）：连接池允许的连接数的最大值，其缺省值为100。
         •Min Pool Size（连接池的最小容量）：连接池允许的连接数的最小值，其缺省值为0。
         •Network Library（网络库）／Net（网络）：用来建立到一个SQL Server实例的连接的网络库。支持的值包括： dbnmpntw (Named Pipes)、dbmsrpcn (Multiprotocol／RPC)、dbmsvinn(Banyan Vines)、dbmsspxn (IPX／SPX)和dbmssocn (TCP／IP)。协议的动态链接库必须被安装到适当的连接，其缺省值为TCP／IP。
         •Packet Size（数据包大小）：用来和数据库通信的网络数据包的大小。其缺省值为8192。
         •Password（密码）／Pwd：与帐户名相对应的密码。
         •Persist Security Info（保持安全信息）：用来确定一旦连接建立了以后安全信息是否可用。如果值为真的话，说明像用户名和密码这样对安全性比较敏感的数据可用，而如果值为伪则不可用。重置连接字符串将重新配置包括密码在内的所有连接字符串的值。其缺省值为伪。
         •Pooling（池）：确定是否使用连接池。如果值为真的话，连接就要从适当的连接池中获得，或者，如果需要的话，连接将被创建，然后被加入合适的连接池中。其缺省值为真。
         •User ID（用户ID）：用来登陆数据库的帐户名。
         •Workstation ID（工作站ID）：连接到SQL Server的工作站的名称。其缺省值为本地计算机的名称。 
         */

        public string CreateSqlConstr(string dbname, string security = null, string datasource = null) {
            var r = default(string);
            datasource = datasource ?? ".";
            if (!new[] { "true", "false", "sspi", "Ture", "False", "SSPI", "TRUE", "FALSE" }.Contains(security)) security = "SSPI";
            r = string.Format("Data Source={0};Initial Catalog={1};Integrated Security={2}", datasource ?? ".", dbname, security);

            return r;
        }
        public string GetSqlconstrHelp() {
            var sb = new StringBuilder();
            sb.AppendLine("Application Name（应用程序名称）：应用程序的名称。如果没有被指定的话，它的值为.NET SqlClient Data Provider（数据提供程序）. ");
            sb.AppendLine("•AttachDBFilename／extended properties（扩展属性）／Initial File Name（初始文件名）：可连接数据库的主要文件的名称，包括完整路径名称。数据库名称必须用关键字数据库指定.");
            sb.AppendLine("•Connect Timeout（连接超时）／Connection Timeout（连接超时）：一个到服务器的连接在终止之前等待的时间长度（以秒计），缺省值为15。");
            sb.AppendLine("•Connection Lifetime（连接生存时间）：当一个连接被返回到连接池时，它的创建时间会与当前时间进行对比。如果这个时间跨度超过了连接的有效期的话，连接就被取消。其缺省值为0。");
            sb.AppendLine("•Connection Reset（连接重置）：表示一个连接在从连接池中被移除时是否被重置。一个伪的有效在获得一个连接的时候就无需再进行一个额外的服务器来回运作，其缺省值为真。");
            sb.AppendLine("•Current Language（当前语言）：SQL Server语言记录的名称。");
            sb.AppendLine("•Data Source（数据源）／Server（服务器）／Address（地址）／Addr（地址）／Network Address（网络地址）：SQL Server实例的名称或网络地址。");
            sb.AppendLine("•Encrypt（加密）：当值为真时，如果服务器安装了授权证书，SQL Server就会对所有在客户和服务器之间传输的数据使用SSL加密。被接受的值有true（真）、false（伪）、yes（是）和no（否）。");
            sb.AppendLine("•Enlist（登记）：表示连接池程序是否会自动登记创建线程的当前事务语境中的连接，其缺省值为真。");
            sb.AppendLine("•Database（数据库）／Initial Catalog（初始编目）：数据库的名称。");
            sb.AppendLine("•Integrated Security（集成安全）／Trusted Connection（受信连接）：表示Windows认证是否被用来连接数据库。它可以被设置成真、伪或者是和真对等的sspi，其缺省值为伪。");
            sb.AppendLine("•Max Pool Size（连接池的最大容量）：连接池允许的连接数的最大值，其缺省值为100。");
            sb.AppendLine("•Min Pool Size（连接池的最小容量）：连接池允许的连接数的最小值，其缺省值为0。");
            sb.AppendLine("•Network Library（网络库）／Net（网络）：用来建立到一个SQL Server实例的连接的网络库。支持的值包括： dbnmpntw (Named Pipes)、dbmsrpcn (Multiprotocol／RPC)、dbmsvinn(Banyan Vines)、dbmsspxn (IPX／SPX)和dbmssocn (TCP／IP)。协议的动态链接库必须被安装到适当的连接，其缺省值为TCP／IP。");
            sb.AppendLine("•Packet Size（数据包大小）：用来和数据库通信的网络数据包的大小。其缺省值为8192。");
            sb.AppendLine("•Password（密码）／Pwd：与帐户名相对应的密码。");
            sb.AppendLine("•Persist Security Info（保持安全信息）：用来确定一旦连接建立了以后安全信息是否可用。如果值为真的话，说明像用户名和密码这样对安全性比较敏感的数据可用，而如果值为伪则不可用。重置连接字符串将重新配置包括密码在内的所有连接字符串的值。其缺省值为伪。");
            sb.AppendLine("•Pooling（池）：确定是否使用连接池。如果值为真的话，连接就要从适当的连接池中获得，或者，如果需要的话，连接将被创建，然后被加入合适的连接池中。其缺省值为真。");
            sb.AppendLine("•User ID（用户ID）：用来登陆数据库的帐户名。");
            sb.AppendLine("•Workstation ID（工作站ID）：连接到SQL Server的工作站的名称。其缺省值为本地计算机的名称。 ");
            return sb.ToString();
        }
        //
        //public void GWcfConfig(WcfParam wp) {
        //    var np = wp.dirTargetAssembly.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
        //    FactoryCommunity.GWcfProxy40(wp);
        //}
        //public void GWcfAsyncConfig(WcfParam wp) {
        //    var np = wp.dirTargetAssembly.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
        //    FactoryCommunity.GWcfProxyAsync40(wp);
        //}
        //public void GWcfTaskConfig(WcfParam wp) {
        //    var np = wp.dirTargetAssembly.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
        //    FactoryCommunity.GWcfProxyTask40(wp);
        //}
        //public void GenerateWebapiConfig(string webapidll_path,string todir) {
        //    var np1 = Regex.Match(webapidll_path,@"(?<=\\)[^\\]*(?=\\Vmh.cs)").Value;
        //    var np2 = Regex.Match(todir,@"(?<=\\)[^\\]*(?=\\VmStyleConverter.cs)").Value;
        //    FactoryCommunity.GenerateWebapiProxyPage(webapidll_path,np1,np2,todir);
        //}
        //
        /// <summary>
        /// 根据输入sql脚本 文件 在目标上生成一个xml文档用于配置仿真数据
        /// </summary>
        /// <param name="sqlpath">sql脚本</param>
        /// <param name="targetpath">目标xml</param>
        //public void GenerateSimuXsdSetting(string sqlpath, string targetpath) {
        //    var x = System.Windows.Forms.MessageBox.Show("若选择 '是' 当你的'仿真数据配置文件'不存在或需要被刷新时!刷新后原数据将被删除!否则请选择 '否'!!", "警告！", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning);
        //    if (x == System.Windows.Forms.DialogResult.Yes) {
        //        var sqlscript = File.ReadAllText(sqlpath);
        //        if (!string.IsNullOrEmpty(sqlscript))
        //            FactorySimulation.GenerateSimulateXsdSetting(sqlscript, targetpath);
        //    }
        //}
        //public void GenerateBizSchema(string sqlpath, string targetpath, string schema_id, string schema_name) {
        //    var sqlscript = File.ReadAllText(sqlpath);
        //    FactorySimulation.GenerateBizSchema(sqlscript, targetpath, schema_id, schema_name);
        //}
        /// <summary>
        /// 通过传入的sql脚本文件生成仿真架构文件(xsd)文件，此架构用于从用户回传的仿真配置xml做解析
        /// 并且通过仿真xml数据文件生成数据集，用于对此仿真档案xml文件的操作
        /// </summary>
        /// <param name="sqlscriptpath">case sql</param>
        /// <param name="sqlscriptpath">config xsd file</param>
        /// <param name="sqlscriptpath"></param>
        //public void GenerateSimuXsdModel(string sqlscriptpath, string targetxsddir, string xsdtoolpath) {
        //    var simuresxml = System.IO.Path.Combine(Common.DirProject, "design\\simulation.xml");
        //    var simuresxsd = System.IO.Path.Combine(Common.DirTools, @"simulation.xsd");
        //    var targetxsdpath = System.IO.Path.Combine(targetxsddir, "currentsimuset.xsd");
        //    var p = new System.Diagnostics.Process();
        //    p.StartInfo.FileName = xsdtoolpath;
        //    p.StartInfo.ErrorDialog = true;
        //    p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        //    var dbsql = File.ReadAllText(sqlscriptpath);
        //    if (!string.IsNullOrEmpty(dbsql)) {
        //        FactorySimulation.GenerateSimulateXsdClass(dbsql, targetxsdpath);//在tools 路径下面根据sql脚本生成对应的xsd架构
        //        p.StartInfo.Arguments = string.Format("{0} /c /l:cs /f /o:{1}", targetxsdpath, targetxsddir);
        //        //以下这句生成不了DataSet的原因是 不支持架构中的 simpleType 中的 list 和 union 元素
        //        //p.StartInfo.Arguments = string.Format("{0} /d /l:cs /o:{1}", targetxsdpath, outdir);
        //        p.Start();
        //    }
        //    if (!File.Exists(simuresxsd)) {
        //        p.StartInfo.Arguments = string.Format("{0} /d /l:cs /f /o:{1}", simuresxml, targetxsddir);
        //        p.Start();
        //    }
        //    p.Close();
        //}
        /// <summary>
        /// 根据仿真配置文档(xml文件)生成一个sql插入脚本 将仿真数据插入到目标数据库
        /// 生成的sql插入脚本位置为调用程序的输入目录(Environment.Current)
        /// </summary>
        /// <param name="usersimusetpath"></param>
        /// <param name="sqlscriptpath"></param>
        //public void GenerateSimuSqlScript<T>(string usersimusetpath, string sqlscriptpath) where T : class {
        //    var _usersetting = File.ReadAllText(usersimusetpath, Encoding.UTF8);
        //    var xdoc = XDocument.Parse(_usersetting);
        //    var nodes = (from item in xdoc.Root.Nodes() where item is XElement select item).Cast<XElement>();
        //    var usersetting = nodes.Single(n => n.Name.LocalName == "simudbsetting");
        //    var simusetobj = usersetting.XmlDeserialize<T>();
        //    FactorySimulation.ParseAndGenerateSqlScript(simusetobj, File.ReadAllText(sqlscriptpath, Encoding.Default), Common.dirbase);
        //}
        
        ///<summary>
        /// 生成entity framework core 实体和上下文
        ///</summary>
        public void G_BizEntityFramework(string entitydir, string ctxdir, string dbsqlscriptpath) {
            var dbsql = File.ReadAllText(dbsqlscriptpath);
            var sqlfile = dbsqlscriptpath.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Last().Split('.');
            var sqlfileName = sqlfile[0].UpFirst();
            var ctxname = $"Db{sqlfileName}";
            var entitynps = entitydir.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
            var ctxnps = ctxdir.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
            if (entitynps.Split('.').Last() == "cs" || ctxnps.Split('.').Last() == "cs")
                return;
            var outstr1 = FactoryDbCode.GenerateEntitys(dbsql, nps: entitynps);
            var outstr2 = FactoryDbCode.GenerateBizCtx(dbsql, ctxname, ctxnps, entitynps);
            var entitypath = string.Join("\\", entitydir.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Concat(new[] { sqlfileName + "_Ent.cs" }));
            var ctxpath = string.Join("\\", ctxdir.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Concat(new[] { sqlfileName + "_Ctx.cs" }));
            SaveTo(entitypath, outstr1);
            SaveTo(ctxpath, outstr2);
        }
        /// <summary>
        /// 根据poco C# 类型生成 ef620版本的，fluent api 配置的 继承DbContext上的 上下文类
        /// </summary>
        /// <param name="pathAndFullName">like this>> d:\yourdir1\yourdir2\Namespa.Ctx.cs</param>
        /// <param name="entitys">the poco class list</param>
        public void G_Ef620_CodeFirst(string namesps, string path, IEnumerable<Type> entitys) {
            var clsList = path.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Last().Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (clsList.Length < 2) {
                throw new Exception("path 参数错误，格式为>> pro?.yourClassName.cs");
            }
            var cls = clsList[clsList.Length - 2];
            if (string.IsNullOrWhiteSpace(namesps)) {
                namesps = $"X{new Random().NextDouble()}";
            }
            var content = FactoryDbCode.GenerateCtx620(namesps, cls, entitys);
            SaveTo(path, content);
        }
        public string G_Efc3x_CodeFirst(string namesps, string dllpath, string topath = null) {
            var assembly = Assembly.LoadFrom(dllpath);
            return G_Efc_CodeFirst(namesps, assembly, topath);
        }
        public string G_Efc_CodeFirst(string namesps, Assembly assembly, string topath = null) {
            var types = assembly.GetTypes();
            var entities = default(IEnumerable<Type>);
            entities = from i in types where i.Namespace == namesps select i;
            var nsps = namesps.Split('.').Last();
            var genNamespace = namesps;
            if (string.IsNullOrWhiteSpace(nsps)) {
                throw new Exception("实体集所在的名称空间不能为空");
            }
            var dbname = $"Db{System.IO.Path.GetExtension(namesps).Replace(".", "")}";
            if (topath == null) {
                topath = $"{Environment.CurrentDirectory}\\{dbname}.cs";
            }
            var content = FactoryDbCode.GenerateCtxCore3x(dbname, namesps, genNamespace, entities);
            SaveTo(topath, content);
            return content;
        }
        public void G_BizEfc_Config(string entitydir, string ctxdir, string dbsqlscriptpath, string fn = null, string classent = null, string classall = null, bool iswcfserial = false) {
            if (!string.IsNullOrEmpty(classall))
                FactoryDbCode.Classall = classall;
            if (!string.IsNullOrEmpty(classent))
                FactoryDbCode.Classent = classent;
            var dbsql = File.ReadAllText(dbsqlscriptpath);
            var sqlfile = dbsqlscriptpath.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Last().Split('.');
            fn = fn ?? sqlfile.ElementAt(sqlfile.Length - 2);
            var entitynsps = entitydir.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
            entitynsps = entitynsps ?? entitydir.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).TakeLast(1).ToArray()[0];
            var outstr1 = FactoryDbCode.GenerateEntitys(dbsql, nps: entitynsps, iswcfserial: iswcfserial);
            var outstr2 = FactoryDbCode.GenerateBizCtxConfigEfc(dbsql, "Db" + fn, entitynsps, entitynsps);
            var now = DateTime.Now;
            SaveTo($"{System.IO.Path.Combine(entitydir, fn)}.entity.cs", outstr1);
            SaveTo($"{System.IO.Path.Combine(ctxdir, fn)}.ctx.cs", outstr2);
        }
        public void G_BizOrleansFrame(string entitydir, string ctxdir, string soawrapperdir, string dbsqlscriptpath, string fn = null, string classent = null, string classall = null, bool iswcfserial = false) {
            if (!string.IsNullOrEmpty(classall))
                FactoryDbCode.Classall = classall;
            if (!string.IsNullOrEmpty(classent))
                FactoryDbCode.Classent = classent;
            var dbsql = File.ReadAllText(dbsqlscriptpath);
            var sqlfile = dbsqlscriptpath.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Last().Split('.');
            fn = fn ?? sqlfile.ElementAt(sqlfile.Length - 2);
            var entitynsps = entitydir.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
            entitynsps = entitynsps ?? entitydir.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).TakeLast(1).ToArray()[0];
            var outstr1 = FactoryDbCode.GenerateEntitys(dbsql, nps: entitynsps, iswcfserial: iswcfserial);
            var outstr2 = FactoryDbCode.GenDbStore(entitynsps, true);
            var outstr3 = FactoryDbCode.GenOrleansWrapper(entitynsps, $"Db{fn}");
            var outstr4 = FactoryDbCode.GenerateBizCtx(dbsql, $"Db{fn}", entitynsps, entitynsps);
            var outstr5 = FactoryDbCode.GenLcInsert(dbsql, fn, entitynsps, entitynsps);
            var outstr6 = FactoryDbCode.GenBizHandler(fn, entitynsps);
            SaveTo($"{System.IO.Path.Combine(entitydir, fn)}.entity.cs", outstr1);
            SaveTo($"{System.IO.Path.Combine(entitydir, fn)}.dbstore.cs", outstr2);
            SaveTo($"{System.IO.Path.Combine(soawrapperdir, fn)}.dbstore.cs", outstr3);
            SaveTo($"{System.IO.Path.Combine(ctxdir, fn)}.ctx.cs", outstr4);
            SaveTo($"{System.IO.Path.Combine(ctxdir, fn)}.lc.cs", outstr5);
            SaveTo($"{System.IO.Path.Combine(entitydir, fn)}.handler.cs", outstr6);
        }

        [Obsolete]
        public void G_BizGrpcFrame(string entitydir, string ctxdir, string soawrapperdir, string dbsqlscriptpath, Action dependency, string nugetPkgDir, string fn = null) {
            var dbsql = File.ReadAllText(dbsqlscriptpath);
            var sqlfile = dbsqlscriptpath.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Last().Split('.');
            fn = fn ?? sqlfile.ElementAt(sqlfile.Length - 2);
            var entitynsps = entitydir.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
            entitynsps = entitynsps ?? entitydir.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).TakeLast(1).ToArray()[0];
            var outstr1 = FactoryDbCode.GenerateEntitys(dbsql, nps: entitynsps, iswcfserial: false);
            var outstr2 = FactoryDbCode.GenDbStore(entitynsps);
            var outstr3 = FactoryDbCode.GenGrpcWrapper(entitynsps, $"Db{fn}");
            var outstr4 = FactoryDbCode.GenerateBizCtx(dbsql, $"Db{fn}", entitynsps, entitynsps);
            var outstr5 = FactoryDbCode.GenLcInsert(dbsql, fn, entitynsps, entitynsps);
            var outstr6 = FactoryDbCode.GenBizHandler(fn, entitynsps);
            SaveTo($"{System.IO.Path.Combine(entitydir, fn)}.entity.cs", outstr1);
            SaveTo($"{System.IO.Path.Combine(entitydir, fn)}.dbstore.cs", outstr2);
            SaveTo($"{System.IO.Path.Combine(soawrapperdir, fn)}.dbstore.cs", outstr3);
            SaveTo($"{System.IO.Path.Combine(ctxdir, fn)}.ctx.cs", outstr4);
            SaveTo($"{System.IO.Path.Combine(ctxdir, fn)}.lc.cs", outstr5);
            SaveTo($"{System.IO.Path.Combine(entitydir, fn)}.handler.cs", outstr6);
            dependency.Invoke();
            var outstr7 = FactoryDbCode.GenGrpcProto(entitydir.Split(new[] { '/', '\\' }).Last(), nugetPkgDir);
            SaveTo($"{System.IO.Path.Combine(soawrapperdir, fn)}.proto", outstr7);
        }
        ////
        //public DataTable SimuToTable(int rows, string[] colnames, params SimuType[] sts) {
        //    var dt = new DataTable();
        //    var simures = new tools.simudataroot();
        //    var xdoc = XDocument.Parse(Properties.Resources.simulation);
        //    simures.ReadXml(xdoc.CreateReader());
        //    var ran = new Random();
        //    var simlen = new Dictionary<string, int>();
        //    var simures_value = new Dictionary<string, string[]>();
        //    //
        //    foreach (DataTable item in simures.Tables) {
        //        simlen.Add(item.TableName, int.Parse(item.Rows[0]["len"].ToString()));
        //        var itemcol = string.Format("{0}_Text", item.TableName);
        //        simures_value.Add(item.TableName, simures.Tables[item.TableName].Rows[0][itemcol].ToString().Split(new[] { ',', ';' }));
        //    }
        //    simures.ReadXml(xdoc.CreateReader());
        //    foreach (var item in colnames ?? FactorySimulation.GetColName(sts)) {
        //        dt.Columns.Add(item.ToString());
        //    }
        //    for (int i = 0; i < rows; i++) {
        //        var row = dt.NewRow();
        //        for (int j = 0; j < sts.Length; j++) {
        //            row[j] = FactorySimulation.SimuTypeString(sts[j], simlen, simures_value, ran);
        //        }
        //        dt.Rows.Add(row);
        //    }
        //    return dt;
        //}
        //public void SimuToTable<T>(SqlBulkCopy bulk, int rows, Dictionary<string, SimuType> col_simutypes) {
        //    var dt = new DataTable();
        //    var simures = new tools.simudataroot();
        //    var xdoc = XDocument.Parse(Properties.Resources.simulation);
        //    simures.ReadXml(xdoc.CreateReader());
        //    var ran = new Random();
        //    var simlen = new Dictionary<string, int>();
        //    var simures_value = new Dictionary<string, string[]>();
        //    //
        //    foreach (DataTable item in simures.Tables) {
        //        simlen.Add(item.TableName, int.Parse(item.Rows[0]["len"].ToString()));
        //        var itemcol = string.Format("{0}_Text", item.TableName);
        //        simures_value.Add(item.TableName, simures.Tables[item.TableName].Rows[0][itemcol].ToString().Split(new[] { ',', ';' }));
        //    }
        //    //create columns at first
        //    var ps = from it in typeof(T).GetProperties()
        //             where Common.CanSqlType(it.PropertyType) && it.Name != "Id"
        //             && it.Name != "ID" && it.Name != "id"
        //             select it;
        //    var ps2 = from it in ps
        //              join it2 in col_simutypes
        //              on it.Name equals it2.Key
        //              select it2;
        //    dt.TableName = typeof(T).Name;
        //    foreach (var item in ps2) {
        //        dt.Columns.Add(item.Key);
        //    }
        //    for (int i = 0; i < rows; i++) {
        //        var row = dt.NewRow();
        //        foreach (DataColumn item in dt.Columns) {
        //            row[item] = FactorySimulation.SimuTypeString(col_simutypes[item.ColumnName], simlen, simures_value, ran);
        //        }
        //        dt.Rows.Add(row);
        //    }
        //    bulk.DestinationTableName = dt.TableName;
        //    bulk.WriteToServer(dt);
        //}
        //
        /// <summary>
        /// 按行抓取 分析提取按：
        /// insert into \b(?<g1>.*)\b values\s{0,5}\((?<g2>.*)\)
        /// insert into table1 values(xxx) 需要独点一行
        /// insert into table1(xxx) values(yyy) 这个可能有错
        /// </summary>
        /// <param name="entityname"></param>
        public void GSqlInsert(string sqlpath) {
            var regexpattern = @"insert\s+into\s+(?<g1>\w+)\s?\(.*\)\s+values\s?\((?<g2>.*)\)";
            var regexpattern2 = @"insert\s+into\s+(?<g1>\w+)\s+values\s?\((?<g2>.*)\)";
            var sb = new StringBuilder();
            sb.AppendLine("#region sql script insert segement");
            using (var sr = new StreamReader(File.Open(sqlpath, FileMode.Open), Encoding.Default)) {
                while (sr.Peek() > 0) {
                    var line = sr.ReadLine();
                    var m = Regex.Match(line, regexpattern);
                    var m2 = Regex.Match(line, regexpattern2);
                    if (m.Success) {
                        sb.AppendLine(string.Format("ctx.{0}.Add(new {0}({1}));", m.Groups["g1"].Value, m.Groups["g2"].Value.Replace("'", "\"")));
                    } else if (m2.Success) {
                        sb.AppendLine(string.Format("ctx.{0}.Add(new {0}({1}));", m2.Groups["g1"].Value, m2.Groups["g2"].Value.Replace("'", "\"")));
                    }
                }
            }
            sb.AppendLine("#endregion");
            Console.WriteLine(sb.ToString());
        }
        //public void GenerateContract(VmParam vp) {
        //    FactoryVmManage.GVmManage(vp);
        //}
        public string G_MixTable(string pathsql) {
            return FactoryDbCode.GenerateMixTable(File.ReadAllText(pathsql));
        }
        //public void SnippetXamlTurnOver(double width, double height, string page1, string page2) => Console.WriteLine(ForWpf.Deputy.CreateXamlTurnOver(width, height, page1, page2));
        #region code snippets
        public static void GenerateCodeSnippet(CsNamespace nps, string path, string nsps, bool isappend = false) {
            var code = nps.ToString(nsps);
            if (isappend) {
                using (var sr = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write))) {
                    sr.Write(code);
                    sr.Write(Environment.NewLine);
                }
            } else {
                File.WriteAllText(path, code);
            }
        }
        public static void GenerateCodeSnippet(XmlFormat xf, string path, bool isappend = false) {
            var code = xf.ToString();
            if (isappend)
                using (var sr = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write))) {
                    sr.WriteLine("<!-- by xmlformat 1.0-->");
                    sr.Write(code);
                    sr.Write(Environment.NewLine);
                }
            else
                File.WriteAllText(path, code);
        }
        /// <summary>
        /// 生成类型的所有virtual方法的override版本
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static CsNamespace GenAllOverrideMethod(Type t) {
            var sps = new CsNamespace();
            var mis = from i in t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                      where i.IsVirtual && i.Name != "Finalize"
                      select i;
            var subclass = sps.StartClass($"Sub{t.Name}", inhlist: t.Name);
            foreach (var item in mis) {
                var ps = from i in item.GetParameters()
                         select i;
                var ps2 = from i in ps select $"{i.ParameterType.GetStandardTypeName()} {i.Name}";
                var ps3 = from i in ps select $"{i.Name}";
                var method = default(Util.Generator.CsMethod);
                var x = from i in item.GetGenericArguments()
                        select i.GetStandardTypeName();
                var mname = "";
                if (x.Count() > 0) {
                    mname = $"{item.Name}<{string.Join(",", x)}>";
                } else {
                    mname = item.Name;
                }
                if (item.ReturnType == typeof(void)) {
                    method = subclass.StartMethod(mname, string.Join(",", ps2), "void", "protected override");
                    method.Sentence($"base.{item.Name}({string.Join(",", ps3)})");
                } else {
                    method = subclass.StartMethod(mname, string.Join(",", ps2), $"{item.ReturnType.GetStandardTypeName()}", "protected override");
                    method.Sentence($"return base.{item.Name}({string.Join(",", ps3)})");
                }
            }
            return sps;
        }
        #endregion
        internal static bool SaveTo(string path, string content) {
            var r = false;
            using (var sw = new StreamWriter(File.Open(path, FileMode.OpenOrCreate, FileAccess.Write))) {
                sw.BaseStream.SetLength(0);
                sw.Write(content); sw.Flush();
            }
            return r;
        }
    }
}
