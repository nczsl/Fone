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
        public string CreateSqlConstr(string dbname, string security = null, string datasource = null) {
            var r = default(string);
            datasource = datasource ?? ".";
            if (!new[] { "true", "false", "sspi", "Ture", "False", "SSPI", "TRUE", "FALSE" }.Contains(security)) security = "SSPI";
            r = string.Format("Data Source={0};Initial Catalog={1};Integrated Security={2}", datasource ?? ".", dbname, security);

            return r;
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
