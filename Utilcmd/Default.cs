using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Util.Ex;
using Util.Generator;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using System.Runtime.Loader;
using System.IO;

namespace Utilcmd {
    public class DefaultExcuter : ICmder {
        readonly Random ran;
        public DefaultExcuter() {
            ran = new Random();
        }
        #region svg controller
        void SvgViewer() {
            //var dir = @"D:\work\netcorework\apps\test20190207\server\test20190207.Mvc\wwwroot\font\keyamoon.svg";
            var dir = "./wwwroot/font/";
            //var xml = XDocument.Load(path);
            var svgFiles = System.IO.Directory.GetFiles(dir, "*.svg");
            //return View(svgFiles);
            //Clipboard
        }
        dynamic getSveContent(string path) {
            var xtxt = System.IO.File.ReadAllText(path).Replace("&#x", "\\").Replace(";", "");
            var xdoc = XDocument.Parse(xtxt);
            var font = xdoc.Root.Element(XName.Get("defs", "http://www.w3.org/2000/svg")).Element(XName.Get("font", "http://www.w3.org/2000/svg"));
            var ran = new Random();
            var style = new StringBuilder();
            style.Append("@font-face{");
            var name = path.Split("/").Last().Split(".").First();
            style.Append($"font-family:{name};");
            style.Append($"src:url(\'../../font/{name}.eot\');");
            style.Append($"src:url(\'../../font/{name}.woff\');");
            style.Append("}");
            style.Append(".icon{");
            style.Append($"font-family:{name};");
            style.Append("}");
            var ccontainer = new XElement("div");
            foreach (var item in font.Elements(XName.Get("glyph", "http://www.w3.org/2000/svg"))) {
                var spanx = new XElement("span");
                var className = "icon_";
                if (item.Attribute("glyph-name") != null)
                    className += item.Attribute("glyph-name").Value;
                style.Append($".{className}::before{{");
                var code = item.Attribute("unicode").Value;
                style.Append($"content:'{code}';");
                style.Append("font-size:'18px';");
                style.Append("margin-right:'10px';");
                style.Append("}");
                spanx.Add(new XAttribute("class", $"icon {className}"));
                spanx.Value = $"{className.Replace("icon_", "")},{code}";
                ccontainer.Add(spanx);
            }
            var xxc = ccontainer.ToString();
            return new { style = style.ToString(), content = xxc };
        }
        #endregion
        public void Help(IEnumerable<string> parameters) {
            Console.WriteLine();
            if (string.IsNullOrWhiteSpace(string.Join(" ", parameters))) {

                Console.WriteLine($"Utilcmd version:{config["version"]}");
                Console.WriteLine("---------------------------------------------");
                Console.WriteLine(Cmdx.CmdInfo);
            } else {

            }
        }
        public void Orm(IEnumerable<string> parameters) {
            if (parameters == null) {
                Console.WriteLine("generate need a file name but empty!");
                return;
            }
            var path = parameters.ToArray()[0];//System.IO.Path.Combine(Environment.CurrentDirectory, parameters);
            var nps = new CsNamespace();
            nps.Using(".,.ICollections,.Linq");
            nps.StartClass("Class1");
            System.IO.File.WriteAllText(path, nps.ToString("_" + ran.NextDouble().ToString().Replace(".", ".d")));
            var xs = new[] {
                new TestEn{Id=1,Name="aaa",Content="adlasdlasd" },
                new TestEn{Id=2,Name="bbb",Content="adlasdlasd2" }
            };
            // var x2 = Json(xs);
            // Console.WriteLine(x2);
        }
        public void Clip(IEnumerable<string> parameters) {
        }
        public void Crawler(IEnumerable<string> parameters) {
            /*
             * 约定为第1个为指定http地址，并且是一个网页不能是其它大资源
             * 约定为第2个为针对第一个参数所代表的网页的一个css 选择器
             */
            if (parameters == null || parameters.Count() == 0) return;
            var ps = parameters.ToArray();//.Split(Cmdx.parametersSplit, StringSplitOptions.RemoveEmptyEntries);
            var iscompleted = false;
            Action<Task> processer = async (Task t) => {
                var parser = new HtmlParser();
                var hc = new HttpClient();
                var htmlSource = await hc.GetStringAsync(ps[0]);
                var document = await parser.ParseDocumentAsync(htmlSource);
                //Console.WriteLine(document.DocumentElement.InnerHtml.Substring(200));
                var path = System.IO.Path.Combine(Environment.CurrentDirectory, "temp.html");
                var selectcontent = from i in document.QuerySelectorAll(ps[1]) select i;
                Console.WriteLine(selectcontent);
                iscompleted = true;
            };
            processer.Invoke(Task.Delay(0));
            while (!iscompleted) {
                Thread.Sleep(50);
                Console.Write('.');
            }
        }
        public void Actions(IEnumerable<string> parameters) {
            var assemblyPath = parameters.ToArray()[0];
            var asms = Assembly.LoadFrom(assemblyPath);
            var tps = asms.GetTypes();
            // var sb = new StringBuilder();
            //sb.AppendLine($"function initResponseArea(){{");
            //const string sps = "  ";
            var idx = 0;
            var controlleras = new List<ControllerRas>();
            foreach (var item in from i in tps where i.IsSubclassOf(typeof(Controller)) select i) {
                var dalist = new List<RequestAction>();
                foreach (var it in from j in item.GetMethods() where j.DeclaringType == item select j) {
                    var temp = new RequestAction();
                    //sb.AppendLine($"{sps}var da{idx} :IDataArea={{name:'',type:'main',url:'',httpMethod:'GET',header:{{}},isTagPackage:false,params:{{}},paramsFormat:'none'}};");
                    var method = "GET"; var paramFormat = "search";
                    if (it.GetCustomAttribute(typeof(HttpGetAttribute)) != null) { method = "GET"; paramFormat = "search"; }
                    if (it.GetCustomAttribute(typeof(HttpPostAttribute)) != null) { method = "POST"; paramFormat = "json"; }
                    if (it.GetCustomAttribute(typeof(HttpHeadAttribute)) != null) { method = "HEAD"; paramFormat = "none"; }
                    if (it.GetCustomAttribute(typeof(HttpPutAttribute)) != null) { method = "PUT"; paramFormat = "json"; }
                    if (it.GetCustomAttribute(typeof(HttpPatchAttribute)) != null) { method = "PATCH"; paramFormat = "json"; }
                    if (it.GetCustomAttribute(typeof(HttpDeleteAttribute)) != null) { method = "DELETE"; paramFormat = "json"; }
                    //sb.AppendLine($"{sps}da{idx}.httpMethod = '{method}';");
                    temp.httpMethod = $"'{method}'";
                    var pps = it.GetParameters();
                    if (pps.Count() == 0) {
                        paramFormat = "none";
                    }
                    //sb.AppendLine($"{sps}da{idx}.paramsFormat = '{paramFormat}';");
                    temp.paramsFormat = $"'{paramFormat}'";
                    var controller = item.Name.Replace("Controller", "");
                    //sb.AppendLine($"{sps}da{idx}.name='{controller}_{it.Name}';");
                    temp.name = $"'{controller}_{it.Name}'";
                    //sb.AppendLine($"{sps}da{idx}.url = '{controller}/{it.Name}';");
                    temp.url = $"'{controller}/{it.Name}'";
                    var _pp = new List<string>();
                    foreach (var iu in pps) {
                        Net2JsType(iu, out var ttype, out var dv);
                        //sb.AppendLine($"{sps}da{idx}.params['{iu.Name}'] = {dv};");
                        _pp.Add($" {iu.Name}: {dv} ");
                    }
                    temp._params = $"{{{string.Join(",", _pp)}}}";
                    var ist = "false";
                    //返回这个的只是有可能 是返回带标签的情况  
                    //也有 Ok,Json，File等各种函数，返回简单字符串，json，和文件blob对象的情况，
                    //这些情况，只能在ts端手动修改了
                    if (it.ReturnType.GetStandardTypeName().Contains("ActionResult"))
                        ist = "true";
                    //sb.AppendLine($"{sps}da{idx}.isTagPackage = {ist};");
                    temp.isTagPackage = ist;
                    //sb.AppendLine($"{sps}dataAreaConfig.push(da{idx});");                    
                    idx++;
                    dalist.Add(temp);
                }
                controlleras.Add(new ControllerRas { name = item.Name.Replace("Controller", ""), dalist = dalist.ToArray() });
            }
            //sb.AppendLine("}");
            //Console.WriteLine(sb.ToString());
            // Console.WriteLine(JsonConvert.SerializeObject(controlleras));
        }
        public void Entities(IEnumerable<string> parameters) {
            var assemblyPath = parameters.ToArray()[0];

            var asms = Assembly.LoadFrom(assemblyPath);
            var tps = asms.GetTypes();
            var sb = new StringBuilder();
            var ran = new Random();
            const string sps = "  ";
            foreach (var item in from i in tps where i.IsSubclassOf(typeof(DbContext)) select i) {
                sb.AppendLine($"namespace {item.GetGenericBaseName()}{{");
                foreach (var it in from i in item.GetProperties() where i.PropertyType.Name.Contains("DbSet") select i) {
                    var targetType = it.PropertyType.GenericTypeArguments[0];
                    sb.AppendLine($"{sps}export interface {targetType.GetStandardTypeName()}{{");
                    foreach (var iu in targetType.GetProperties()) {
                        Net2JsType(iu, out var ttype, out var dv);
                        if (!string.IsNullOrWhiteSpace(ttype))
                            sb.AppendLine($"{sps + sps}{iu.Name}:{ttype};");
                    }
                    sb.AppendLine($"{sps}}}");
                }
                sb.AppendLine("}");
            }
            Console.WriteLine(sb.ToString());
        }
        private static void Net2JsType<T>(T iu, out string ttype, out string dv) {
            var tempType = default(Type);
            ttype = dv = string.Empty;
            switch (iu) {
                case PropertyInfo i1:
                if ((from i in i1.GetCustomAttributes()
                     where i.GetType().Name.Contains("Ignore")
                     select i).Count() > 0) return;
                tempType = i1.PropertyType; break;
                case ParameterInfo i2:
                if ((from i in i2.GetCustomAttributes()
                     where i.GetType().Name.Contains("Ignore")
                     select i).Count() > 0) return;
                tempType = i2.ParameterType; break;
            }
            if (tempType.Name.Contains("Nullable")) {
                tempType = tempType.GenericTypeArguments[0];
            }
            if (
                tempType == typeof(int) || tempType == typeof(uint) ||
                tempType == typeof(short) || tempType == typeof(ushort) ||
                tempType == typeof(float) || tempType == typeof(double) ||
                tempType == typeof(sbyte) || tempType == typeof(byte) ||
                tempType == typeof(ulong) || tempType == typeof(long) ||
                tempType == typeof(decimal)
                ) {
                ttype = "number"; dv = "0";
            } else if (tempType == typeof(string)) {
                ttype = "string"; dv = "''";
            } else if (tempType == typeof(DateTime) || tempType == typeof(TimeSpan)) {
                ttype = "Date"; dv = "new Date()";
            } else if (tempType == typeof(bool)) {
                ttype = "boolean"; dv = "false";
            } else if (typeof(IEnumerable).IsAssignableFrom(tempType) && tempType.GenericTypeArguments.Length == 1) {
                ttype = $"Array<{tempType.GenericTypeArguments[0].GetStandardTypeName()}>";
                dv = "null";
            } else {
                //实体属性或其它传输属性
                // ttype = iu is PropertyInfo ? ((PropertyInfo)(object)iu).PropertyType.Name : ((ParameterInfo)(object)iu).ParameterType.Name;
                ttype = $"{tempType.GetStandardTypeName()}";
                dv = "null";
            }
        }
        /// <summary>
        /// 根据输入的js模块生成一个typescript文档
        /// </summary>
        /// <param name="jsmoduls">js文件所在的目录</param>
        public void GenerateDts(string jsmoduls) {
        }
        static IConfiguration config;
        static DefaultExcuter() {
            var configpath = System.IO.Path.Combine(
                   Regex.Replace(typeof(DefaultExcuter).Assembly.Location, @"\\\w+\.dll", ""),
                   "config.json");
            var cb = new ConfigurationBuilder();
            cb.AddJsonFile(configpath, true);
            config = cb.Build();
        }

        public void Netlibs(IEnumerable<string> parameters) {
            var ps = parameters.ToArray();
            var c = "";
            var c2 = string.Join(".", parameters);
            System.Console.WriteLine("parameters:{0}", c2);
            string className, methodName;
            className = methodName = "";
            Assembly assem = default(Assembly);
            var actx = new AssemblyLoadContext("test", true);
            var _r = false;
            do {
                Console.WriteLine("type 'quit' is exit this service");
                Console.WriteLine();
                if (!string.IsNullOrWhiteSpace(c2))
                    c = c2;
                else
                    c = Console.ReadLine();
                var sp1 = c.Split(" ");
                var sp2 = sp1.First().Split(".");
                var templist = sp2.CutOut(0, sp2.Length - 1).ToList();
                templist.Insert(0, "Test");
                templist.Insert(0, "Netlibs");
                className = string.Join(".", templist.ToHashSet());
                methodName = sp2.Last();
                _r = sp1.Last().Trim() == "-r";
                if (_r) {
                    System.Console.WriteLine("c:{0}", c);
                    this.Restart = (psas) => {
                        System.Console.WriteLine("开始卸载程序集...");
                        actx.Unload();
                        System.Console.WriteLine("卸载完成");
                        PsInteraction.ExcutePs(pso => {
                            foreach (var item in pso) {
                                System.Console.WriteLine(item);
                            }
                            System.Console.WriteLine("重新开启一个对话");
                            Task.Run(() => {
                                var cmd = $"start-process -filepath dotnet -argumentlist \"{config["path:UtilcmdDll"]} -netlibs {className} {methodName}\"";
                                System.Console.WriteLine($"cmd:{cmd}");
                                PsInteraction.ExcutePs(null, $"Write-Host {className} {methodName}",
                                cmd).Wait();
                            });
                        },
                        // "Write-Host 'build Netlibs.Test.csproj ...'",
                        $"dotnet build {config["path:NetlibsProj"]}"
                        ).Wait();
                        this.Restart = null;
                    };
                    System.Console.WriteLine("退出本次对话");
                    break;
                }
                using var fs = File.Open($"{config["path:NetlibsDll"]}", FileMode.Open);
                assem = actx.LoadFromStream(fs);
                //Console.WriteLine(assem); //ok
                if (sp2.Count() >= 2) {
                    System.Console.WriteLine(className);
                    var t = assem.GetType(className);
                    var ins = Activator.CreateInstance(t);
                    t.GetMethod(methodName).Invoke(ins, null);
                }
                c2 = string.Empty;
            } while (c.Trim() != "quit");

        }
        void ExcuteDialog(IEnumerable<string> parameters, string dllPath, string projPath, string defns, string fcmd) {
            var ps = parameters.ToArray();
            var c = "";
            var c2 = string.Join(".", parameters);
            System.Console.WriteLine("parameters:{0}", c2);
            string className, methodName;
            className = methodName = "";
            Assembly assem = default(Assembly);
            var actx = new AssemblyLoadContext("test", true);
            var _r = false;
            do {
                Console.WriteLine("type 'quit' is exit this service");
                Console.WriteLine();
                if (!string.IsNullOrWhiteSpace(c2))
                    c = c2;
                else
                    c = Console.ReadLine();
                var sp1 = c.Split(" ");
                var sp2 = sp1.First().Split(".");
                var templist = sp2.CutOut(0, sp2.Length - 1).ToList();
                foreach (var it in defns.Split(".").Reverse()) {
                    if (!templist.Contains(it))
                        templist.Insert(0, it);
                }
                className = string.Join(".", templist.ToHashSet());
                methodName = sp2.Last();
                _r = sp1.Last().Trim() == "-r";
                if (_r) {
                    System.Console.WriteLine("c:{0}", c);
                    this.Restart = (psas) => {
                        System.Console.WriteLine("开始卸载程序集...");
                        actx.Unload();
                        System.Console.WriteLine("卸载完成");
                        PsInteraction.ExcutePs(pso => {
                            foreach (var item in pso) {
                                System.Console.WriteLine(item);
                            }
                            System.Console.WriteLine("重新开启一个对话");
                            Task.Run(() => {
                                var cmd = $"start-process -filepath dotnet -argumentlist \"{config["path:UtilcmdDll"]} {fcmd} {className} {methodName}\"";
                                System.Console.WriteLine($"cmd:{cmd}");
                                PsInteraction.ExcutePs(null, $"Write-Host {className} {methodName}",
                                cmd).Wait();
                            });
                        },
                        // "Write-Host 'build Netlibs.Test.csproj ...'",
                        $"dotnet build {projPath}"
                        ).Wait();
                        this.Restart = null;
                    };
                    System.Console.WriteLine("退出本次对话");
                    break;
                }
                using var fs = File.Open($"{dllPath}", FileMode.Open);
                assem = actx.LoadFromStream(fs);
                //Console.WriteLine(assem); //ok
                if (sp2.Count() >= 2) {
                    System.Console.WriteLine(className);
                    var t = assem.GetType(className);
                    var ins = Activator.CreateInstance(t);
                    t.GetMethod(methodName).Invoke(ins, null);
                }
                c2 = string.Empty;
            } while (c.Trim() != "quit");
        }
        public Action<IEnumerable<string>> Restart { get; set; }

        public void Bizallview(IEnumerable<string> parameters) {
            ExcuteDialog(parameters, config["path:BizallviewDll"], config["path:BizallviewProj"], "Bizallview.Test", "-bizallview");
        }

        public void GenerateDts(IEnumerable<string> jsmoduls) {
            System.Console.WriteLine("DTS");

            PsInteraction.ExcutePsRunspace(null,
            "write-output 223",
            $"dotnet {config["path:UtilcmdDll"]} -netlibs");//{className} {methodName}").Wait();
        }
    }
}
