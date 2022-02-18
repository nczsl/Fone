using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Util;
using Util.Generator;

namespace Netlibs.Test {
    public class Tool {
        
        static public void UpdateFileTo(string targetdir) {
            var dlldir = "./";
            foreach (var item in Directory.GetFiles(dlldir, "*.dll")) {
                Console.WriteLine(item);
                var toitem = System.IO.Path.Combine(targetdir, item.Split(new[] { '/', '\\' }).Last());
                File.Copy(item, toitem, true);
            }
            var jsondir = "../../../../Utilcmd/bin/Debug/netcoreapp2.2/";
            foreach (var item in Directory.GetFiles(jsondir, "*.json")) {
                Console.WriteLine(item);
                var toitem = System.IO.Path.Combine(targetdir, item.Split(new[] { '/', '\\' }).Last());
                File.Copy(item, toitem, true);
            }
        }
        /// <summary>
        /// 抓取图片
        /// </summary>
        static public void getimg(string url, string savedir, string name = null) {
            var hc = new HttpClient();
            var asyncer1Completed = false;
            Action asyncer1 = async () => {
                await Task.Delay(0);
                var res = await hc.GetAsync(url);
                //hc.PostAsync()
                if (!res.IsSuccessStatusCode) return;
                var content = await res.Content.ReadAsStreamAsync();
                //content
                //var buffer=new byte[content.Length];
                //content.Read(buffer);
                var ran = Guid.NewGuid();
                //var path = $@"C:\Users\hjjls\Pictures\download\{ran}.jpg";
                var segment = $"{name ?? ran.ToString()}.png";
                var path = System.IO.Path.Combine(savedir, segment);
                //using (var sw=new StreamWriter(File.Create(path))) {
                //    sw.Write(buffer);
                //    sw.Flush();
                //}
                var img = Image.FromStream(content);
                img.Save(path, ImageFormat.Png);
                //hc.PostAsync("http://localhost:5000/show",hcc);
                asyncer1Completed = true;
            };
            asyncer1.Invoke();
            while (!asyncer1Completed) {
                Thread.Sleep(100);
            }
        }
        //
        public enum semanticType {
            ContainerType_items,
            ContainerType_map,
            ElementType_items,
            ElementType_map
        }
        /// <summary>
        /// generate for containertype of fone project logic code 
        /// </summary>
        static public void Gf_SemanticeType<T>(semanticType st) {
            var t = typeof(T);
            var sprops = t.GetFields();
            var typename = t.Name;
            var nsps = new CsNamespace();
            var temp = nsps.StartClass("temp");
            switch (st) {
                case semanticType.ElementType_items:
                case semanticType.ContainerType_items:
                    var items = temp.StartProperty("items", "string[]", visit: "static public");
                    var cn = new CsComplexNew(null, "[]");
                    foreach (var item in sprops) {
                        cn.Sentence($"nameof({item.Name})", true);
                    }
                    items.SetLambda(cn);
                    break;
                case semanticType.ElementType_map:
                case semanticType.ContainerType_map:
                    var map = temp.StartMethod("Map", $"this {typename} it", "string", "static public");
                    map.Sentence("var tag = it.ToString()");
                    var mapsw = map.Switch("it");
                    foreach (var item in sprops) {
                        mapsw.CaseBlock($"nameof({typename}.{item.Name})");
                    }
                    map.Sentence("return tag");
                    break;
                default:
                    break;
            }
            var ran = new Random();
            Console.WriteLine(nsps.ToString($"_{ran.Next(1000, 10000)}"));
        }
    }
}