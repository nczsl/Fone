using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Util.Ex;
using Util.Generator;
//常见接口实现
namespace Fone {
    static public class Tool {
        /// <summary>
        /// 这个转换性能高于泛型+反射的方案
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        static public Expression<Func<IEnumerable<T>, DataTable>> GetTable<T>() {
            var objType = typeof(T);
            var dcType = typeof(DataColumn);
            var tableExprs = new List<Expression>();
            var tableType = typeof(DataTable);
            var tableExpr = Expression.Variable(tableType, "dt");
            var tableInstanceExpr = Expression.Assign(tableExpr,
                Expression.New(tableType));
            tableExprs.Add(tableInstanceExpr);
            var tableCols = Expression.Property(tableExpr, "Columns");
            //判断依据，属性里又有get，又有set的为可以翻译为DataColumn的属性，否则 视为计算列
            var ps = from i in objType.GetProperties()
                     where i.CanRead && i.CanWrite
                     && i.PropertyType.IsPrimaryType()
                     select i;
            foreach (var item in ps) {
                var constName = Expression.Constant(item.Name, typeof(string));
                var constType = Expression.Constant(item.PropertyType, typeof(Type));
                var colNew = Expression.New(dcType.GetConstructor(new[] { typeof(string), typeof(Type) }), constName, constType);
                var callAddCol = Expression.Call(
                    tableCols,
                    typeof(DataColumnCollection).GetMethod("Add", new[] { dcType }),
                    colNew
                    );
                tableExprs.Add(callAddCol);
            }
            var source = Expression.Parameter(typeof(IEnumerable<T>), "source");
            var loop = source.Foreach<T>(i => {
                var rowExp = Expression.Call(tableExpr, tableType.GetMethod("NewRow"));
                var drowExp = Expression.Variable(typeof(DataRow), "dr");
                var rowSettingExps = new List<Expression>();
                var drowInit = Expression.Assign(drowExp, rowExp);
                rowSettingExps.Add(drowInit);
                foreach (var item in ps) {
                    var propConst = Expression.Constant(item.Name, typeof(string));
                    //var rowCollectionExp=Expression.Property(tableExpr,"Rows");
                    var rowIndex = Expression.MakeIndex(drowExp,
                        typeof(DataRow).GetProperty("Item", new[] { typeof(string) }),
                        new[] { propConst });
                    var valueExp = Expression.Property(i, item.Name);
                    var assignExp = Expression.Assign(rowIndex,
                        Expression.TypeAs(valueExp, typeof(object))
                        );
                    rowSettingExps.Add(assignExp);
                }
                var rowsPropExp = Expression.Property(tableExpr, tableType.GetProperty("Rows"));
                var rowAddExp = Expression.Call(rowsPropExp, typeof(DataRowCollection).GetMethod("Add", new[] { typeof(DataRow) }), drowExp);
                rowSettingExps.Add(rowAddExp);
                var inBody = Expression.Block(new[] { drowExp }, rowSettingExps);
                return inBody;
            });
            tableExprs.Add(loop);
            tableExprs.Add(tableExpr);
            var body = Expression.Block(new[] { tableExpr }, tableExprs);
            var lam = Expression.Lambda<Func<IEnumerable<T>, DataTable>>(body, source);
            return lam;
        }
        /// <summary>
        /// 转换对象
        /// </summary>
        /// <typeparam name="Tin"></typeparam>
        /// <typeparam name="Tout"></typeparam>
        /// <returns></returns>
        static public Expression<Func<Tin, Tout>> GetPocoConverter<Tin, Tout>() {
            var t = typeof(Tin);
            var x = typeof(Tout);
            //忽略 field 只限定property
            var ps1 = from i in t.GetProperties() where i.PropertyType.IsPrimaryType() select i;
            var ps2 = from i in x.GetProperties() where i.PropertyType.IsPrimaryType() select i;
            var source = from i in ps1
                         join j in ps2
                         on new { i.PropertyType, i.Name }
                         equals new { j.PropertyType, j.Name }
                         select new { i, j };
            var binds = new List<MemberAssignment>();//用于初始化对象成员
            var inExp = Expression.Parameter(t, "tin");
            foreach (var item in source) {
                var propExp = Expression.Property(inExp, item.i);
                var mabind = Expression.Bind(item.j, propExp);
                binds.Add(mabind);
            }
            var convertExp = Expression.MemberInit(
                Expression.New(x),
                binds
                );
            var lambda = Expression.Lambda<Func<Tin, Tout>>(convertExp, new[] { inExp });
            return lambda;
        }
        /// <summary>
        /// 将指定的目标接口功业务功能因，转成orleans中的IGrainxx形态的子接口
        /// </summary>
        /// <param name="x"></param>
        /// <param name="pofuFullClassName"></param>
        /// <returns></returns>
        static public string GetOleansCsStr(Assembly x, string pofuFullClassName) {
            var targetType = x.GetType(pofuFullClassName);
            var flag = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;
            var ms = targetType.GetMethods(flag);   
            var nsps = new CsNamespace();
            var targetNamespace = targetType.Namespace + ".Orlean";
            var name=targetType.Name[0]=='I'? targetType.Name:$"I{targetType.Name}";
            var orleansInterface = nsps.StartInterface(name, inhlist: "IGrainWithIntegerKey");
            foreach (var item in ms) {
                var m = new CsMethod(item.Name);
                m.paramlist = string.Join(",",
                    from i in item.GetParameters()
                    select $"{i.ParameterType.GetStandardTypeName()} {i.Name}"
                    );
                var rt = item.ReturnType.GetStandardTypeName();
                m.typ = item.ReturnType.Name == "Void" ? "Task<Result<string>>" : $"Task<Result<{rt}>>";
                m.isInterface = true;
                orleansInterface.StartMethod(m);
            }
            nsps.Using($".,.Collections.Generic,.Linq,.Linq.Expressions,.Threading.Tasks,Orleans,Fone,{targetType.Namespace}");
            return nsps.ToString(targetNamespace);
        }
    }
    /// <summary>
    /// .net core 常用mime 类型，可供HttpClient使用
    /// 如果这个类型选择错误将很可能被报 http 415 错误代码
    /// </summary>
    public struct Mime {
        static public Mime json {
            get { Mime ct; ct.value = "application/json"; return ct; }
        }
        static public Mime bytes {
            get { Mime ct; ct.value = "application/octet-stream"; return ct; }
        }
        static public Mime text {
            get { Mime ct; ct.value = "text/plain"; return ct; }
        }
        static public Mime multi {
            get { Mime ct; ct.value = "multipart/form-data"; return ct; }
        }
        static public Mime fome {
            get { Mime ct; ct.value = "x-www-form-urlencoded"; return ct; }
        }
        static public Mime txtjson {
            get { Mime ct; ct.value = "text/json"; return ct; }
        }
        static public Mime txtxml {
            get { Mime ct; ct.value = "text/xml"; return ct; }
        }
        static public Mime xml {
            get { Mime ct; ct.value = "application/xml"; return ct; }
        }
        string value;
        static public implicit operator string(Mime n) {
            return n.value;
        }
        static public implicit operator Mime(string s) {
            var x = default(Mime);
            x.value = s;
            return x;
        }
        public bool Exist => !string.IsNullOrWhiteSpace(value);
    }
    static public class ColorTable {
        ///<summary>black（黑）</summary>
        public const string black = nameof(black);
        public const string black16 = "#000000";
        ///<summary>silver（银）</summary>
        public const string silver = nameof(silver);
        public const string silver16 = "#c0c0c0";
        ///<summary>gray[*]（灰）</summary>
        public const string gray = nameof(gray);
        public const string gray16 = "#808080";
        ///<summary>white（白）</summary>
        public const string white = nameof(white);
        public const string white16 = "#ffffff";
        ///<summary>maroon（褐）</summary>
        public const string maroon = nameof(maroon);
        public const string maroon16 = "#800000";
        ///<summary>red（红）</summary>
        public const string red = nameof(red);
        public const string red16 = "#ff0000";
        ///<summary>purple（紫）</summary>
        public const string purple = nameof(purple);
        public const string purple16 = "#800080";
        ///<summary>fuchsia（紫红）</summary>
        public const string fuchsia = nameof(fuchsia);
        public const string fuchsia16 = "#ff00ff";
        ///<summary>green（绿）</summary>
        public const string green = nameof(green);
        public const string green16 = "#008000";
        ///<summary>lime（绿黄）</summary>
        public const string lime = nameof(lime);
        public const string lime16 = "#00ff00";
        ///<summary>olive（橄榄绿）</summary>
        public const string olive = nameof(olive);
        public const string olive16 = "#808000";
        ///<summary>yellow（黄）</summary>
        public const string yellow = nameof(yellow);
        public const string yellow16 = "#ffff00";
        ///<summary>navy（藏青）</summary>
        public const string navy = nameof(navy);
        public const string navy16 = "#000080";
        ///<summary>blue（蓝）</summary>
        public const string blue = nameof(blue);
        public const string blue16 = "#0000ff";
        ///<summary>teal（青）</summary>
        public const string teal = nameof(teal);
        public const string teal16 = "#008080";
        ///<summary>aqua（水绿）</summary>
        public const string aqua = nameof(aqua);
        public const string aqua16 = "#00ffff";
        ///<summary>orange（橙）</summary>
        public const string orange = nameof(orange);
        public const string orange16 = "#ffa500";
        ///<summary>aliceblue（浅灰蓝）</summary>
        public const string aliceblue = nameof(aliceblue);
        public const string aliceblue16 = "#f0f8ff";
        ///<summary>antiquewhite（古董白）</summary>
        public const string antiquewhite = nameof(antiquewhite);
        public const string antiquewhite16 = "#faebd7";
        ///<summary>aquamarine（海蓝）</summary>
        public const string aquamarine = nameof(aquamarine);
        public const string aquamarine16 = "#7fffd4";
        ///<summary>azure（蔚蓝）</summary>
        public const string azure = nameof(azure);
        public const string azure16 = "#f0ffff";
        ///<summary>beige（浅褐）</summary>
        public const string beige = nameof(beige);
        public const string beige16 = "#f5f5dc";
        ///<summary>bisque（橘黄）</summary>
        public const string bisque = nameof(bisque);
        public const string bisque16 = "#ffe4c4";
        ///<summary>blanchedalmond（杏仁白）</summary>
        public const string blanchedalmond = nameof(blanchedalmond);
        public const string blanchedalmond16 = "#ffe4c4";
        ///<summary>blueviolet（蓝紫）</summary>
        public const string blueviolet = nameof(blueviolet);
        public const string blueviolet16 = "#8a2be2";
        ///<summary>brown（褐）</summary>
        public const string brown = nameof(brown);
        public const string brown16 = "#a52a2a";
        ///<summary>burlywood（原木色）</summary>
        public const string burlywood = nameof(burlywood);
        public const string burlywood16 = "#deb887";
        ///<summary>cadetblue（灰蓝）</summary>
        public const string cadetblue = nameof(cadetblue);
        public const string cadetblue16 = "#5f9ea0";
        ///<summary>chartreuse（黄绿）</summary>
        public const string chartreuse = nameof(chartreuse);
        public const string chartreuse16 = "#7fff00";
        ///<summary>chocolate（巧克力色）</summary>
        public const string chocolate = nameof(chocolate);
        public const string chocolate16 = "#d2691e";
        ///<summary>coral（珊瑚红）</summary>
        public const string coral = nameof(coral);
        public const string coral16 = "#ff7f50";
        ///<summary>cornflowerblue（矢车菊蓝）</summary>
        public const string cornflowerblue = nameof(cornflowerblue);
        public const string cornflowerblue16 = "#6495ed";
        ///<summary>cornsilk（玉米穗黄）</summary>
        public const string cornsilk = nameof(cornsilk);
        public const string cornsilk16 = "#fff8dc";
        ///<summary>crimson（深红）</summary>
        public const string crimson = nameof(crimson);
        public const string crimson16 = "#dc143c";
        ///<summary>darkblue（深蓝）</summary>
        public const string darkblue = nameof(darkblue);
        public const string darkblue16 = "#00008b";
        ///<summary>darkcyan（深青）</summary>
        public const string darkcyan = nameof(darkcyan);
        public const string darkcyan16 = "#008b8b";
        ///<summary>darkgoldenrod（暗金）</summary>
        public const string darkgoldenrod = nameof(darkgoldenrod);
        public const string darkgoldenrod16 = "#b8860b";
        ///<summary>darkgray[*]（深灰）</summary>
        public const string darkgray = nameof(darkgray);
        public const string darkgray16 = "#a9a9a9";
        ///<summary>darkgreen（深绿）</summary>
        public const string darkgreen = nameof(darkgreen);
        public const string darkgreen16 = "#006400";
        ///<summary>darkgrey[*]（深灰）</summary>
        public const string darkgrey = nameof(darkgrey);
        public const string darkgrey16 = "#a9a9a9";
        ///<summary>darkkhaki（暗黄褐）</summary>
        public const string darkkhaki = nameof(darkkhaki);
        public const string darkkhaki16 = "#bdb76b";
        ///<summary>darkmagenta（深紫）</summary>
        public const string darkmagenta = nameof(darkmagenta);
        public const string darkmagenta16 = "#8b008b";
        ///<summary>darkolivegreen（深橄榄绿）</summary>
        public const string darkolivegreen = nameof(darkolivegreen);
        public const string darkolivegreen16 = "#556b2f";
        ///<summary>darkorange（深橙）</summary>
        public const string darkorange = nameof(darkorange);
        public const string darkorange16 = "#ff8c00";
        ///<summary>darkorchid（深兰花紫）</summary>
        public const string darkorchid = nameof(darkorchid);
        public const string darkorchid16 = "#9932cc";
        ///<summary>darkred（深红）</summary>
        public const string darkred = nameof(darkred);
        public const string darkred16 = "#8b0000";
        ///<summary>darksalmon（深橙红）</summary>
        public const string darksalmon = nameof(darksalmon);
        public const string darksalmon16 = "#e9967a";
        ///<summary>darkseagreen（深海绿）</summary>
        public const string darkseagreen = nameof(darkseagreen);
        public const string darkseagreen16 = "#8fbc8f";
        ///<summary>darkslateblue（暗灰蓝）</summary>
        public const string darkslateblue = nameof(darkslateblue);
        public const string darkslateblue16 = "#483d8b";
        ///<summary>darkslategray[*]（墨绿）</summary>
        public const string darkslategray = nameof(darkslategray);
        public const string darkslategray16 = "#2f4f4f";
        ///<summary>darkslategrey[*]（墨绿）</summary>
        public const string darkslategrey = nameof(darkslategrey);
        public const string darkslategrey16 = "#2f4f4f";
        ///<summary>darkturquoise（暗宝石绿）</summary>
        public const string darkturquoise = nameof(darkturquoise);
        public const string darkturquoise16 = "#00ced1";
        ///<summary>darkviolet（深紫罗兰）</summary>
        public const string darkviolet = nameof(darkviolet);
        public const string darkviolet16 = "#9400d3";
        ///<summary>deeppink（深粉红）</summary>
        public const string deeppink = nameof(deeppink);
        public const string deeppink16 = "#ff1493";
        ///<summary>deepskyblue（深天蓝）</summary>
        public const string deepskyblue = nameof(deepskyblue);
        public const string deepskyblue16 = "#00bfff";
        ///<summary>dimgray[*]（暗灰）</summary>
        public const string dimgray = nameof(dimgray);
        public const string dimgray16 = "#696969";
        ///<summary>dimgrey[*]（暗灰）</summary>
        public const string dimgrey = nameof(dimgrey);
        public const string dimgrey16 = "#696969";
        ///<summary>dodgerblue（遮板蓝）</summary>
        public const string dodgerblue = nameof(dodgerblue);
        public const string dodgerblue16 = "#1e90ff";
        ///<summary>firebrick（砖红）</summary>
        public const string firebrick = nameof(firebrick);
        public const string firebrick16 = "#b22222";
        ///<summary>floralwhite（花白）</summary>
        public const string floralwhite = nameof(floralwhite);
        public const string floralwhite16 = "#fffaf0";
        ///<summary>forestgreen（丛林绿）</summary>
        public const string forestgreen = nameof(forestgreen);
        public const string forestgreen16 = "#228b22";
        ///<summary>gainsboro（浅灰）</summary>
        public const string gainsboro = nameof(gainsboro);
        public const string gainsboro16 = "#dcdcdc";
        ///<summary>ghostwhite（幽灵白）</summary>
        public const string ghostwhite = nameof(ghostwhite);
        public const string ghostwhite16 = "#f8f8ff";
        ///<summary>gold（金）</summary>
        public const string gold = nameof(gold);
        public const string gold16 = "#ffd700";
        ///<summary>goldenrod（橘黄）</summary>
        public const string goldenrod = nameof(goldenrod);
        public const string goldenrod16 = "#daa520";
        ///<summary>greenyellow（黄绿）</summary>
        public const string greenyellow = nameof(greenyellow);
        public const string greenyellow16 = "#adff2f";
        ///<summary>grey（灰）</summary>
        public const string grey = nameof(grey);
        public const string grey16 = "#808080";
        ///<summary>honeydew（蜜瓜色）</summary>
        public const string honeydew = nameof(honeydew);
        public const string honeydew16 = "#f0fff0";
        ///<summary>hotpink（亮粉）</summary>
        public const string hotpink = nameof(hotpink);
        public const string hotpink16 = "#ff69b4";
        ///<summary>indianred（印第安红）</summary>
        public const string indianred = nameof(indianred);
        public const string indianred16 = "#cd5c5c";
        ///<summary>indigo（靛蓝）</summary>
        public const string indigo = nameof(indigo);
        public const string indigo16 = "#4b0082";
        ///<summary>ivory（象牙白）</summary>
        public const string ivory = nameof(ivory);
        public const string ivory16 = "#fffff0";
        ///<summary>khaki（卡其色）</summary>
        public const string khaki = nameof(khaki);
        public const string khaki16 = "#f0e68c";
        ///<summary>lavender（淡紫）</summary>
        public const string lavender = nameof(lavender);
        public const string lavender16 = "#e6e6fa";
        ///<summary>lavenderblush（淡紫红）</summary>
        public const string lavenderblush = nameof(lavenderblush);
        public const string lavenderblush16 = "#fff0f5";
        ///<summary>lawngreen（草绿）</summary>
        public const string lawngreen = nameof(lawngreen);
        public const string lawngreen16 = "#7cfc00";
        ///<summary>lemonchiffon（粉黄）</summary>
        public const string lemonchiffon = nameof(lemonchiffon);
        public const string lemonchiffon16 = "#fffacd";
        ///<summary>lightblue（淡蓝）</summary>
        public const string lightblue = nameof(lightblue);
        public const string lightblue16 = "#add8e6";
        ///<summary>lightcoral（浅珊瑚色）</summary>
        public const string lightcoral = nameof(lightcoral);
        public const string lightcoral16 = "#f08080";
        ///<summary>lightcyan（淡青）</summary>
        public const string lightcyan = nameof(lightcyan);
        public const string lightcyan16 = "#e0ffff";
        ///<summary>lightgoldenrodyellow（浅金黄）</summary>
        public const string lightgoldenrodyellow = nameof(lightgoldenrodyellow);
        public const string lightgoldenrodyellow16 = "#fafad2";
        ///<summary>lightgray[*]（浅灰）</summary>
        public const string lightgray = nameof(lightgray);
        public const string lightgray16 = "#d3d3d3";
        ///<summary>lightgreen（浅绿）</summary>
        public const string lightgreen = nameof(lightgreen);
        public const string lightgreen16 = "#90ee90";
        ///<summary>lightgrey[*]（浅灰）</summary>
        public const string lightgrey = nameof(lightgrey);
        public const string lightgrey16 = "#d3d3d3";
        ///<summary>lightpink（淡粉）</summary>
        public const string lightpink = nameof(lightpink);
        public const string lightpink16 = "#ffb6c1";
        ///<summary>lightsalmon（浅肉色）</summary>
        public const string lightsalmon = nameof(lightsalmon);
        public const string lightsalmon16 = "#ffa07a";
        ///<summary>lightseagreen（浅海绿）</summary>
        public const string lightseagreen = nameof(lightseagreen);
        public const string lightseagreen16 = "#20b2aa";
        ///<summary>lightskyblue（浅天蓝）</summary>
        public const string lightskyblue = nameof(lightskyblue);
        public const string lightskyblue16 = "#87cefa";
        ///<summary>lightslategray[*]（浅青灰）</summary>
        public const string lightslategray = nameof(lightslategray);
        public const string lightslategray16 = "#778899";
        ///<summary>lightslategrey[*]（浅青灰）</summary>
        public const string lightslategrey = nameof(lightslategrey);
        public const string lightslategrey16 = "#778899";
        ///<summary>lightsteelblue（浅钢蓝）</summary>
        public const string lightsteelblue = nameof(lightsteelblue);
        public const string lightsteelblue16 = "#b0c4de";
        ///<summary>lightyellow（浅黄）</summary>
        public const string lightyellow = nameof(lightyellow);
        public const string lightyellow16 = "#ffffe0";
        ///<summary>limegreen（酸橙绿）</summary>
        public const string limegreen = nameof(limegreen);
        public const string limegreen16 = "#32cd32";
        ///<summary>linen（亚麻色）</summary>
        public const string linen = nameof(linen);
        public const string linen16 = "#faf0e6";
        ///<summary>mediumaquamarine（中绿）</summary>
        public const string mediumaquamarine = nameof(mediumaquamarine);
        public const string mediumaquamarine16 = "#66cdaa";
        ///<summary>mediumblue（中蓝）</summary>
        public const string mediumblue = nameof(mediumblue);
        public const string mediumblue16 = "#0000cd";
        ///<summary>mediumorchid（间兰花紫）</summary>
        public const string mediumorchid = nameof(mediumorchid);
        public const string mediumorchid16 = "#ba55d3";
        ///<summary>mediumpurple（中紫）</summary>
        public const string mediumpurple = nameof(mediumpurple);
        public const string mediumpurple16 = "#9370db";
        ///<summary>mediumseagreen（间海绿）</summary>
        public const string mediumseagreen = nameof(mediumseagreen);
        public const string mediumseagreen16 = "#3cb371";
        ///<summary>mediumslateblue（中暗蓝）</summary>
        public const string mediumslateblue = nameof(mediumslateblue);
        public const string mediumslateblue16 = "#7b68ee";
        ///<summary>mediumspringgreen（中春绿）</summary>
        public const string mediumspringgreen = nameof(mediumspringgreen);
        public const string mediumspringgreen16 = "#00fa9a";
        ///<summary>mediumturquoise（中海湖蓝）</summary>
        public const string mediumturquoise = nameof(mediumturquoise);
        public const string mediumturquoise16 = "#48d1cc";
        ///<summary>mediumvioletred（中紫罗兰）</summary>
        public const string mediumvioletred = nameof(mediumvioletred);
        public const string mediumvioletred16 = "#c71585";
        ///<summary>midnightblue（午夜蓝）</summary>
        public const string midnightblue = nameof(midnightblue);
        public const string midnightblue16 = "#191970";
        ///<summary>mintcream（薄荷乳白）</summary>
        public const string mintcream = nameof(mintcream);
        public const string mintcream16 = "#f5fffa";
        ///<summary>mistyrose（粉玫瑰红）</summary>
        public const string mistyrose = nameof(mistyrose);
        public const string mistyrose16 = "#ffe4e1";
        ///<summary>moccasin（鹿皮色）</summary>
        public const string moccasin = nameof(moccasin);
        public const string moccasin16 = "#ffe4b5";
        ///<summary>navajowhite（纳瓦白）</summary>
        public const string navajowhite = nameof(navajowhite);
        public const string navajowhite16 = "#ffdead";
        ///<summary>oldlace（浅米色）</summary>
        public const string oldlace = nameof(oldlace);
        public const string oldlace16 = "#fdf5e6";
        ///<summary>olivedrab（橄榄褐）</summary>
        public const string olivedrab = nameof(olivedrab);
        public const string olivedrab16 = "#6b8e23";
        ///<summary>orangered（橙红）</summary>
        public const string orangered = nameof(orangered);
        public const string orangered16 = "#ff4500";
        ///<summary>orchid（兰花紫）</summary>
        public const string orchid = nameof(orchid);
        public const string orchid16 = "#da70d6";
        ///<summary>palegoldenrod（灰菊黄）</summary>
        public const string palegoldenrod = nameof(palegoldenrod);
        public const string palegoldenrod16 = "#eee8aa";
        ///<summary>palegreen（苍绿）</summary>
        public const string palegreen = nameof(palegreen);
        public const string palegreen16 = "#98fb98";
        ///<summary>paleturquoise（苍宝石绿）</summary>
        public const string paleturquoise = nameof(paleturquoise);
        public const string paleturquoise16 = "#afeeee";
        ///<summary>palevioletred（苍紫罗兰）</summary>
        public const string palevioletred = nameof(palevioletred);
        public const string palevioletred16 = "#db7093";
        ///<summary>papayawhip（木瓜色）</summary>
        public const string papayawhip = nameof(papayawhip);
        public const string papayawhip16 = "#ffefd5";
        ///<summary>peachpuff（桃色）</summary>
        public const string peachpuff = nameof(peachpuff);
        public const string peachpuff16 = "#ffdab9";
        ///<summary>peru（秘鲁色）</summary>
        public const string peru = nameof(peru);
        public const string peru16 = "#cd853f";
        ///<summary>pink（粉）</summary>
        public const string pink = nameof(pink);
        public const string pink16 = "#ffc0cb";
        ///<summary>plum（李子色）</summary>
        public const string plum = nameof(plum);
        public const string plum16 = "#dda0dd";
        ///<summary>powderblue（粉蓝）</summary>
        public const string powderblue = nameof(powderblue);
        public const string powderblue16 = "#b0e0e6";
        ///<summary>rosybrown（玫瑰粽）</summary>
        public const string rosybrown = nameof(rosybrown);
        public const string rosybrown16 = "#bc8f8f";
        ///<summary>royalblue（宝蓝）</summary>
        public const string royalblue = nameof(royalblue);
        public const string royalblue16 = "#4169e1";
        ///<summary>saddlebrown（马鞍棕）</summary>
        public const string saddlebrown = nameof(saddlebrown);
        public const string saddlebrown16 = "#8b4513";
        ///<summary>salmon（鲑肉色）</summary>
        public const string salmon = nameof(salmon);
        public const string salmon16 = "#fa8072";
        ///<summary>sandybrown（沙褐色）</summary>
        public const string sandybrown = nameof(sandybrown);
        public const string sandybrown16 = "#f4a460";
        ///<summary>seagreen（海绿）</summary>
        public const string seagreen = nameof(seagreen);
        public const string seagreen16 = "#2e8b57";
        ///<summary>seashell（贝壳白）</summary>
        public const string seashell = nameof(seashell);
        public const string seashell16 = "#fff5ee";
        ///<summary>sienna（赭）</summary>
        public const string sienna = nameof(sienna);
        public const string sienna16 = "#a0522d";
        ///<summary>skyblue（天蓝）</summary>
        public const string skyblue = nameof(skyblue);
        public const string skyblue16 = "#87ceeb";
        ///<summary>slateblue（青蓝）</summary>
        public const string slateblue = nameof(slateblue);
        public const string slateblue16 = "#6a5acd";
        ///<summary>slategray[*]（青灰）</summary>
        public const string slategray = nameof(slategray);
        public const string slategray16 = "#708090";
        ///<summary>slategrey[*]（青灰）</summary>
        public const string slategrey = nameof(slategrey);
        public const string slategrey16 = "#708090";
        ///<summary>snow（雪白）</summary>
        public const string snow = nameof(snow);
        public const string snow16 = "#fffafa";
        ///<summary>springgreen（春绿）</summary>
        public const string springgreen = nameof(springgreen);
        public const string springgreen16 = "#00ff7f";
        ///<summary>steelblue（铁青）</summary>
        public const string steelblue = nameof(steelblue);
        public const string steelblue16 = "#4682b4";
        ///<summary>tan（棕褐）</summary>
        public const string tan = nameof(tan);
        public const string tan16 = "#d2b48c";
        ///<summary>thistle（苍紫）</summary>
        public const string thistle = nameof(thistle);
        public const string thistle16 = "#d8bfd8";
        ///<summary>tomato（番茄红）</summary>
        public const string tomato = nameof(tomato);
        public const string tomato16 = "#ff6347";
        ///<summary>turquoise（蓝绿）</summary>
        public const string turquoise = nameof(turquoise);
        public const string turquoise16 = "#40e0d0";
        ///<summary>violet（紫罗兰色）</summary>
        public const string violet = nameof(violet);
        public const string violet16 = "#ee82ee";
        ///<summary>wheat（麦色）</summary>
        public const string wheat = nameof(wheat);
        public const string wheat16 = "#f5deb3";
        ///<summary>whitesmoke（烟白）</summary>
        public const string whitesmoke = nameof(whitesmoke);
        public const string whitesmoke16 = "#f5f5f5";
        ///<summary>yellowgreen（黄绿）</summary>
        public const string yellowgreen = nameof(yellowgreen);
        public const string yellowgreen16 = "#9acd32";
        ///<summary>rebeccapurple（利百加紫）</summary>
        public const string rebeccapurple = nameof(rebeccapurple);
        public const string rebeccapurple16 = "#663399";
    }
    static public class InsInputTypes {
        ///<summary>定义可点击的按钮（大多与 JavaScript 使用来启动脚本）</summary>
        public const string button = nameof(button);
        ///<summary>定义复选框。</summary>
        public const string checkbox = nameof(checkbox);
        ///<summary>定义拾色器。</summary>
        public const string color = nameof(color);
        ///<summary>定义日期字段（带有 calendar 控件）</summary>
        public const string date = nameof(date);
        ///<summary>定义日期字段（带有 calendar 和 time 控件）</summary>
        public const string datetime = nameof(datetime);
        ///<summary>定义日期字段（带有 calendar 和 time 控件）</summary>
        public const string datetime_local = "datetime-local";
        ///<summary>定义日期字段的月（带有 calendar 控件）</summary>
        public const string month = nameof(month);
        ///<summary>定义日期字段的周（带有 calendar 控件）</summary>
        public const string week = nameof(week);
        ///<summary>定义日期字段的时、分、秒（带有 time 控件）</summary>
        public const string time = nameof(time);
        ///<summary>定义用于 e-mail 地址的文本字段</summary>
        public const string email = nameof(email);
        ///<summary>定义输入字段和 "浏览..." 按钮，供文件上传</summary>
        public const string file = nameof(file);
        ///<summary>定义隐藏输入字段</summary>
        public const string hidden = nameof(hidden);
        ///<summary>定义图像作为提交按钮</summary>
        public const string image = nameof(image);
        ///<summary>定义带有 spinner 控件的数字字段</summary>
        public const string number = nameof(number);
        ///<summary>定义密码字段。字段中的字符会被遮蔽。</summary>
        public const string password = nameof(password);
        ///<summary>定义单选按钮。</summary>
        public const string radio = nameof(radio);
        ///<summary>定义带有 slider 控件的数字字段。</summary>
        public const string range = nameof(range);
        ///<summary>定义重置按钮。重置按钮会将所有表单字段重置为初始值。</summary>
        public const string reset = nameof(reset);
        ///<summary>定义用于搜索的文本字段。</summary>
        public const string search = nameof(search);
        ///<summary>定义提交按钮。提交按钮向服务器发送数据。</summary>
        public const string submit = nameof(submit);
        ///<summary>定义用于电话号码的文本字段。</summary>
        public const string tel = nameof(tel);
        ///<summary>默认。定义单行输入字段，用户可在其中输入文本。默认是 20 个字符。</summary>
        public const string text = nameof(text);
        ///<summary>定义用于 URL 的文本字段。</summary>
        public const string url = nameof(url);
    }
    class CommonEqualityComparer<T, Key> : IEqualityComparer<T> {
        private readonly Func<T, Key> keySelector;
        private readonly IEqualityComparer<Key> comparer;
        public CommonEqualityComparer(Func<T, Key> keySelector, IEqualityComparer<Key> comparer) {
            this.keySelector = keySelector;
            this.comparer = comparer;
        }
        public CommonEqualityComparer(Func<T, Key> keySelector)
            : this(keySelector, EqualityComparer<Key>.Default) { }
        public bool Equals(T x, T y) {
            return comparer.Equals(keySelector(x), keySelector(y));
        }
        public int GetHashCode(T obj) {
            return comparer.GetHashCode(keySelector(obj));
        }
    }
}
namespace Fone.Wmi {
    //
    public struct Cooling {
        /// <summary>
        /// 风扇
        /// </summary>
        static public string Win32_Fan => nameof(Win32_Fan);
        /// <summary>
        /// 热管
        /// </summary>
        static public string Win32_HeatPipe => nameof(Win32_HeatPipe);
        /// <summary>
        /// 致冷
        /// </summary>
        static public string Win32_Refrigeration => nameof(Win32_Refrigeration);
        /// <summary>
        /// 温度传感
        /// </summary>
        static public string Win32_TemperatureProbe => nameof(Win32_TemperatureProbe);
    }
    public struct InputDevice {
        /// <summary>
        /// 键盘　
        /// </summary>
        static public string Win32_Keyboard => nameof(Win32_Keyboard);
        /// <summary>
        /// 指示设备（如鼠标）　
        /// </summary>
        static public string Win32_PointingDevice => nameof(Win32_PointingDevice);
    }
    public struct BulkStorage {
        /// <summary>
        /// 磁盘自动检查操作设置　
        /// </summary>
        static public string Win32_AutochkSetting => nameof(Win32_AutochkSetting);
        /// <summary>
        /// 光盘驱动器　
        /// </summary>
        static public string Win32_CDROMDrive => nameof(Win32_CDROMDrive);
        /// <summary>
        /// 硬盘驱动器　
        /// </summary>
        static public string Win32_DiskDrive => nameof(Win32_DiskDrive);
        /// <summary>
        /// 软盘驱动器　
        /// </summary>
        static public string Win32_FloppyDrive => nameof(Win32_FloppyDrive);
        /// <summary>
        /// 物理媒体　
        /// </summary>
        static public string Win32_PhysicalMedia => nameof(Win32_PhysicalMedia);
        /// <summary>
        /// 磁带驱动器　
        /// </summary>
        static public string Win32_TapeDrive => nameof(Win32_TapeDrive);
    }
    public struct MainBoard_Controller_Port {
        /// <summary>
        /// 1394控制器　
        /// </summary>
        static public string Win32_1394Controller => nameof(Win32_1394Controller);
        /// <summary>
        /// 1394控制器设备
        /// </summary>
        static public string Win32_1394ControllerDevice => nameof(Win32_1394ControllerDevice);
        /// <summary>
        /// 已分配的资源
        /// </summary>
        static public string Win32_AllocatedResource => nameof(Win32_AllocatedResource);
        /// <summary>
        /// 处理器和高速缓冲存储器
        /// </summary>
        static public string Win32_AssociatedProcessorMemory => nameof(Win32_AssociatedProcessorMemory);
        /// <summary>
        /// 主板
        /// </summary>
        static public string Win32_BaseBoard => nameof(Win32_BaseBoard);
        /// <summary>
        /// BIOS--基本输入输出系统
        /// </summary>
        static public string Win32_BIOS => nameof(Win32_BIOS);
        /// <summary>
        /// 总线
        /// </summary>
        static public string Win32_Bus => nameof(Win32_Bus);
        /// <summary>
        /// 缓存内存
        /// </summary>
        static public string Win32_CacheMemory => nameof(Win32_CacheMemory);
        /// <summary>
        /// USB控制器
        /// </summary>
        static public string Win32_ControllerHasHub => nameof(Win32_ControllerHasHub);
        /// <summary>
        /// 设备总线
        /// </summary>
        static public string Win32_DeviceBus => nameof(Win32_DeviceBus);
        /// <summary>
        /// 设备存储器地址
        /// </summary>
        static public string Win32_DeviceMemoryAddress => nameof(Win32_DeviceMemoryAddress);
        /// <summary>
        /// 设备设置
        /// </summary>
        static public string Win32_DeviceSettings => nameof(Win32_DeviceSettings);
        /// <summary>
        /// DMA通道
        /// </summary>
        static public string Win32_DMAChannel => nameof(Win32_DMAChannel);
        /// <summary>
        /// 软盘控制器
        /// </summary>
        static public string Win32_FloppyController => nameof(Win32_FloppyController);
        /// <summary>
        /// IDE控制器
        /// </summary>
        static public string Win32_IDEController => nameof(Win32_IDEController);
        /// <summary>
        /// IDE控制器设备
        /// </summary>
        static public string Win32_IDEControllerDevice => nameof(Win32_IDEControllerDevice);
        /// <summary>
        /// 红外线设备
        /// </summary>
        static public string Win32_InfraredDevice => nameof(Win32_InfraredDevice);
        /// <summary>
        /// 中断IRQ资源
        /// </summary>
        static public string Win32_IRQResource => nameof(Win32_IRQResource);
        /// <summary>
        /// 内存数组
        /// </summary>
        static public string Win32_MemoryArray => nameof(Win32_MemoryArray);
        /// <summary>
        /// 内存数组位置
        /// </summary>
        static public string Win32_MemoryArrayLocation => nameof(Win32_MemoryArrayLocation);
        /// <summary>
        /// 内存设备
        /// </summary>
        static public string Win32_MemoryDevice => nameof(Win32_MemoryDevice);
        /// <summary>
        /// 内存设备数组
        /// </summary>
        static public string Win32_MemoryDeviceArray => nameof(Win32_MemoryDeviceArray);
        /// <summary>
        /// 内存设备位置
        /// </summary>
        static public string Win32_MemoryDeviceLocation => nameof(Win32_MemoryDeviceLocation);
        /// <summary>
        /// 主板设备
        /// </summary>
        static public string Win32_MotherboardDevice => nameof(Win32_MotherboardDevice);
        /// <summary>
        /// 插件设备
        /// </summary>
        static public string Win32_OnBoardDevice => nameof(Win32_OnBoardDevice);
        /// <summary>
        /// 并行端口
        /// </summary>
        static public string Win32_ParallelPort => nameof(Win32_ParallelPort);
        /// <summary>
        /// PCMCIA控制器
        /// </summary>
        static public string Win32_PCMCIAController => nameof(Win32_PCMCIAController);
        /// <summary>
        /// 物理内存
        /// </summary>
        static public string Win32_PhysicalMemory => nameof(Win32_PhysicalMemory);
        /// <summary>
        /// 物理内存数组
        /// </summary>
        static public string Win32_PhysicalMemoryArray => nameof(Win32_PhysicalMemoryArray);
        /// <summary>
        /// 物理内存位置
        /// </summary>
        static public string Win32_PhysicalMemoryLocation => nameof(Win32_PhysicalMemoryLocation);
        /// <summary>
        /// PNP保留资源
        /// </summary>
        static public string Win32_PNPAllocatedResource => nameof(Win32_PNPAllocatedResource);
        /// <summary>
        /// PNP设备
        /// </summary>
        static public string Win32_PNPDevice => nameof(Win32_PNPDevice);
        /// <summary>
        /// PNP实体
        /// </summary>
        static public string Win32_PNPEntity => nameof(Win32_PNPEntity);
        /// <summary>
        /// 端口连接器
        /// </summary>
        static public string Win32_PortConnector => nameof(Win32_PortConnector);
        /// <summary>
        /// 端口资源
        /// </summary>
        static public string Win32_PortResource => nameof(Win32_PortResource);
        /// <summary>
        /// CPU处理器
        /// </summary>
        static public string Win32_Processor => nameof(Win32_Processor);
        /// <summary>
        /// SCSI控制器
        /// </summary>
        static public string Win32_SCSIController => nameof(Win32_SCSIController);
        /// <summary>
        /// SCSI控制器设备
        /// </summary>
        static public string Win32_SCSIControllerDevice => nameof(Win32_SCSIControllerDevice);
        /// <summary>
        /// 串行端口
        /// </summary>
        static public string Win32_SerialPort => nameof(Win32_SerialPort);
        /// <summary>
        /// 串行端口配置
        /// </summary>
        static public string Win32_SerialPortConfiguration => nameof(Win32_SerialPortConfiguration);
        /// <summary>
        /// 串行端口设置
        /// </summary>
        static public string Win32_SerialPortSetting => nameof(Win32_SerialPortSetting);
        /// <summary>
        /// 内存有关的设备的管理
        /// </summary>
        static public string Win32_SMBIOSMemory => nameof(Win32_SMBIOSMemory);
        /// <summary>
        /// 声卡
        /// </summary>
        static public string Win32_SoundDevice => nameof(Win32_SoundDevice);
        /// <summary>
        /// 系统BIOS
        /// </summary>
        static public string Win32_SystemBIOS => nameof(Win32_SystemBIOS);
        /// <summary>
        /// 系统驱动器PNP实体
        /// </summary>
        static public string Win32_SystemDriverPNPEntity => nameof(Win32_SystemDriverPNPEntity);
        /// <summary>
        /// 系统封闭
        /// </summary>
        static public string Win32_SystemEnclosure => nameof(Win32_SystemEnclosure);
        /// <summary>
        /// 系统内存资源
        /// </summary>
        static public string Win32_SystemMemoryResource => nameof(Win32_SystemMemoryResource);
        /// <summary>
        /// 系统插槽
        /// </summary>
        static public string Win32_SystemSlot => nameof(Win32_SystemSlot);
        /// <summary>
        /// USB控制器
        /// </summary>
        static public string Win32_USBController => nameof(Win32_USBController);
        /// <summary>
        /// USB控制器设备
        /// </summary>
        static public string Win32_USBControllerDevice => nameof(Win32_USBControllerDevice);
        /// <summary>
        /// USB集线器
        /// </summary>
        static public string Win32_USBHub => nameof(Win32_USBHub);
    }
    public struct AteEquipment {
        /// <summary>
        /// 网络适配器
        /// </summary>
        static public string Win32_NetworkAdapter => nameof(Win32_NetworkAdapter);
        /// <summary>
        /// 网络适配器配置
        /// </summary>
        static public string Win32_NetworkAdapterConfiguration => nameof(Win32_NetworkAdapterConfiguration);
        /// <summary>
        /// 网络适配器设置
        /// </summary>
        static public string Win32_NetworkAdapterSetting => nameof(Win32_NetworkAdapterSetting);
    }
    public struct PowerSupply {
        /// <summary>
        /// 联合电池组
        /// </summary>
        static public string Win32_Associateery => nameof(Win32_Associateery);
        /// <summary>
        /// 电池
        /// </summary>
        static public string Win32_Battery => nameof(Win32_Battery);
        /// <summary>
        /// 当前传感
        /// </summary>
        static public string Win32_CurrentProbe => nameof(Win32_CurrentProbe);
        /// <summary>
        /// 便携式电池
        /// </summary>
        static public string Win32_PortableBattery => nameof(Win32_PortableBattery);
        /// <summary>
        /// 电池事件管理
        /// </summary>
        static public string Win32_PowerManagementEvent => nameof(Win32_PowerManagementEvent);
        /// <summary>
        /// UPS电源
        /// </summary>
        static public string Win32_UninterruptiblePowerSupply => nameof(Win32_UninterruptiblePowerSupply);
        /// <summary>
        /// 电压探测
        /// </summary>
        static public string Win32_VoltageProbe => nameof(Win32_VoltageProbe);
    }
    public struct Print {
        /// <summary>
        /// 驱动器设备
        /// </summary>
        static public string Win32_DriverForDevice => nameof(Win32_DriverForDevice);
        /// <summary>
        /// 打印机
        /// </summary>
        static public string Win32_Printer => nameof(Win32_Printer);
        /// <summary>
        /// 打印机配置
        /// </summary>
        static public string Win32_PrinterConfiguration => nameof(Win32_PrinterConfiguration);
        /// <summary>
        /// 打印机控制器
        /// </summary>
        static public string Win32_PrinterController => nameof(Win32_PrinterController);
        /// <summary>
        /// 打印机驱动器
        /// </summary>
        static public string Win32_PrinterDriver => nameof(Win32_PrinterDriver);
        /// <summary>
        /// 打印机驱动器DLL
        /// </summary>
        static public string Win32_PrinterDriverDll => nameof(Win32_PrinterDriverDll);
        /// <summary>
        /// 打印机设置
        /// </summary>
        static public string Win32_PrinterSetting => nameof(Win32_PrinterSetting);
        /// <summary>
        /// 打印工作
        /// </summary>
        static public string Win32_PrintJob => nameof(Win32_PrintJob);
        /// <summary>
        /// TCPIP打印机端口
        /// </summary>
        static public string Win32_TCPIPPrinterPort => nameof(Win32_TCPIPPrinterPort);
    }
    public struct Telephone {
        /// <summary>
        /// POTS调制解调器Modem
        /// </summary>
        static public string Win32_POTSModem => nameof(Win32_POTSModem);
        /// <summary>
        /// POTS调制解调器串行端口
        /// </summary>
        static public string Win32_POTSModemToSerialPort => nameof(Win32_POTSModemToSerialPort);
    }
    public struct VideoMonitor {
        /// <summary>
        /// 即插即用监视器
        /// </summary>
        static public string Win32_DesktopMonitor => nameof(Win32_DesktopMonitor);
        /// <summary>
        /// 显示配置
        /// </summary>
        static public string Win32_DisplayConfiguration => nameof(Win32_DisplayConfiguration);
        /// <summary>
        /// 显示控制器配置
        /// </summary>
        static public string Win32_DisplayControllerConfiguration => nameof(Win32_DisplayControllerConfiguration);
        /// <summary>
        /// 视频配置
        /// </summary>
        static public string Win32_VideoConfiguration => nameof(Win32_VideoConfiguration);
        /// <summary>
        /// 视频控制器
        /// </summary>
        static public string Win32_VideoController => nameof(Win32_VideoController);
        /// <summary>
        /// 视频设置
        /// </summary>
        static public string Win32_VideoSettings => nameof(Win32_VideoSettings);
    }
    public struct Com {
        static public string Win32_ClassicCOMApplicationClasses => nameof(Win32_ClassicCOMApplicationClasses);
        static public string Win32_ClassicCOMClass => nameof(Win32_ClassicCOMClass);
        static public string Win32_ClassicCOMClassSettings => nameof(Win32_ClassicCOMClassSettings);
        static public string Win32_ClientApplicationSetting => nameof(Win32_ClientApplicationSetting);
        /// <summary>
        /// COM应用
        /// </summary>
        static public string Win32_COMApplication => nameof(Win32_COMApplication);
        static public string Win32_COMApplicationClasses => nameof(Win32_COMApplicationClasses);
        static public string Win32_COMApplicationSettings => nameof(Win32_COMApplicationSettings);
        static public string Win32_COMClass => nameof(Win32_COMClass);
        static public string Win32_ComClassAutoEmulator => nameof(Win32_ComClassAutoEmulator);
        static public string Win32_ComClassEmulator => nameof(Win32_ComClassEmulator);
        static public string Win32_ComponentCategory => nameof(Win32_ComponentCategory);
        static public string Win32_COMSetting => nameof(Win32_COMSetting);
        /// <summary>
        /// DCOM应用
        /// </summary>
        static public string Win32_DCOMApplication => nameof(Win32_DCOMApplication);
        static public string Win32_DCOMApplicationAccessAllowedSetting => nameof(Win32_DCOMApplicationAccessAllowedSetting);
        static public string Win32_DCOMApplicationSetting => nameof(Win32_DCOMApplicationSetting);
        static public string Win32_ImplementedCategory => nameof(Win32_ImplementedCategory);
    }
    public struct Desktop {
        /// <summary>
        /// 桌面
        /// </summary>
        static public string Win32_Desktop => nameof(Win32_Desktop);
        /// <summary>
        /// 环境
        /// </summary>
        static public string Win32_Environment => nameof(Win32_Environment);
        /// <summary>
        /// 时区
        /// </summary>
        static public string Win32_TimeZone => nameof(Win32_TimeZone);
        /// <summary>
        /// 使用者桌面
        /// </summary>
        static public string Win32_UserDesktop => nameof(Win32_UserDesktop);
    }
    public struct DriverProgram {
        static public string Win32_DriverVXD => nameof(Win32_DriverVXD);
        /// <summary>
        /// 系统驱动程序
        /// </summary>
        static public string Win32_SystemDriver => nameof(Win32_SystemDriver);
    }
    public struct FileSystem {
        static public string Win32_CIMLogicalDeviceCIMDataFile => nameof(Win32_CIMLogicalDeviceCIMDataFile);
        static public string Win32_Directory => nameof(Win32_Directory);
        static public string Win32_DirectorySpecification => nameof(Win32_DirectorySpecification);
        static public string Win32_DiskDriveToDiskPartition => nameof(Win32_DiskDriveToDiskPartition);
        /// <summary>
        /// 磁盘逻辑分区
        /// </summary>
        static public string Win32_DiskPartition => nameof(Win32_DiskPartition);
        /// <summary>
        /// NTFS磁盘分区定额
        /// </summary>
        static public string Win32_DiskQuota => nameof(Win32_DiskQuota);
        /// <summary>
        /// 逻辑磁盘分区
        /// </summary>
        static public string Win32_LogicalDisk => nameof(Win32_LogicalDisk);
        static public string Win32_LogicalDiskRootDirectory => nameof(Win32_LogicalDiskRootDirectory);
        static public string Win32_LogicalDiskToPartition => nameof(Win32_LogicalDiskToPartition);
        /// <summary>
        /// 映射逻辑磁盘
        /// </summary>
        static public string Win32_MappedLogicalDisk => nameof(Win32_MappedLogicalDisk);
        static public string Win32_OperatingSystemAutochkSetting => nameof(Win32_OperatingSystemAutochkSetting);
        static public string Win32_QuotaSetting => nameof(Win32_QuotaSetting);
        static public string Win32_ShortcutFile => nameof(Win32_ShortcutFile);
        static public string Win32_SubDirectory => nameof(Win32_SubDirectory);
        static public string Win32_SystemPartitions => nameof(Win32_SystemPartitions);
        static public string Win32_Volume => nameof(Win32_Volume);
        static public string Win32_VolumeQuota => nameof(Win32_VolumeQuota);
        static public string Win32_VolumeQuotaSetting => nameof(Win32_VolumeQuotaSetting);
        static public string Win32_VolumeUserQuota => nameof(Win32_VolumeUserQuota);
    }
    public struct TaskObject {
        static public string Win32_CollectionStatistics => nameof(Win32_CollectionStatistics);
        static public string Win32_LUID => nameof(Win32_LUID);
        static public string Win32_LUIDandAttributes => nameof(Win32_LUIDandAttributes);
        static public string Win32_NamedJobObject => nameof(Win32_NamedJobObject);
        static public string Win32_NamedJobObjectActgInfo => nameof(Win32_NamedJobObjectActgInfo);
        static public string Win32_NamedJobObjectLimit => nameof(Win32_NamedJobObjectLimit);
        static public string Win32_NamedJobObjectLimitSetting => nameof(Win32_NamedJobObjectLimitSetting);
        static public string Win32_NamedJobObjectProcess => nameof(Win32_NamedJobObjectProcess);
        static public string Win32_NamedJobObjectSecLimit => nameof(Win32_NamedJobObjectSecLimit);
        static public string Win32_NamedJobObjectSecLimitSetting => nameof(Win32_NamedJobObjectSecLimitSetting);
        static public string Win32_NamedJobObjectStatistics => nameof(Win32_NamedJobObjectStatistics);
        static public string Win32_SIDandAttributes => nameof(Win32_SIDandAttributes);
        static public string Win32_TokenGroups => nameof(Win32_TokenGroups);
        static public string Win32_TokenPrivileges => nameof(Win32_TokenPrivileges);
    }
    public struct StorePageFile {
        /// <summary>
        /// 逻辑内存配置
        /// </summary>
        static public string Win32_LogicalMemoryConfiguration => nameof(Win32_LogicalMemoryConfiguration);
        /// <summary>
        /// 页面文件
        /// </summary>
        static public string Win32_PageFile => nameof(Win32_PageFile);
        static public string Win32_PageFileElementSetting => nameof(Win32_PageFileElementSetting);
        /// <summary>
        /// 页面文件设置
        /// </summary>
        static public string Win32_PageFileSetting => nameof(Win32_PageFileSetting);
        /// <summary>
        /// 页面文件使用
        /// </summary>
        static public string Win32_PageFileUsage => nameof(Win32_PageFileUsage);
        static public string Win32_SystemLogicalMemoryConfiguration => nameof(Win32_SystemLogicalMemoryConfiguration);
    }
    public struct Mutimedia {
        /// <summary>
        /// 编解码器文件
        /// </summary>
        static public string Win32_CodecFile => nameof(Win32_CodecFile);
    }
    public struct BuildNet {
        /// <summary>
        /// 活动路由
        /// </summary>
        static public string Win32_ActiveRoute => nameof(Win32_ActiveRoute);
        static public string Win32_IP4PersistedRouteTable => nameof(Win32_IP4PersistedRouteTable);
        /// <summary>
        /// 路由表
        /// </summary>
        static public string Win32_IP4RouteTable => nameof(Win32_IP4RouteTable);
        static public string Win32_IP4RouteTableEvent => nameof(Win32_IP4RouteTableEvent);
        static public string Win32_NetworkClient => nameof(Win32_NetworkClient);
        static public string Win32_NetworkConnection => nameof(Win32_NetworkConnection);
        /// <summary>
        /// 网络协议
        /// </summary>
        static public string Win32_NetworkProtocol => nameof(Win32_NetworkProtocol);
        static public string Win32_NTDomain => nameof(Win32_NTDomain);
        static public string Win32_PingStatus => nameof(Win32_PingStatus);
        /// <summary>
        /// 协议绑定
        /// </summary>
        static public string Win32_ProtocolBinding => nameof(Win32_ProtocolBinding);
    }
    public struct OperationSystemEvent {
        static public string Win32_ComputerShutdownEvent => nameof(Win32_ComputerShutdownEvent);
        static public string Win32_ComputerSystemEvent => nameof(Win32_ComputerSystemEvent);
        static public string Win32_DeviceChangeEvent => nameof(Win32_DeviceChangeEvent);
        static public string Win32_ModuleLoadTrace => nameof(Win32_ModuleLoadTrace);
        static public string Win32_ModuleTrace => nameof(Win32_ModuleTrace);
        static public string Win32_ProcessStartTrace => nameof(Win32_ProcessStartTrace);
        static public string Win32_ProcessStopTrace => nameof(Win32_ProcessStopTrace);
        static public string Win32_ProcessTrace => nameof(Win32_ProcessTrace);
        static public string Win32_SystemConfigurationChangeEvent => nameof(Win32_SystemConfigurationChangeEvent);
        static public string Win32_SystemTrace => nameof(Win32_SystemTrace);
        static public string Win32_ThreadStartTrace => nameof(Win32_ThreadStartTrace);
        static public string Win32_ThreadStopTrace => nameof(Win32_ThreadStopTrace);
        static public string Win32_ThreadTrace => nameof(Win32_ThreadTrace);
        static public string Win32_VolumeChangeEvent => nameof(Win32_VolumeChangeEvent);
        /// <summary>
        /// 引导配置
        /// </summary>
        static public string Win32_BootConfiguration => nameof(Win32_BootConfiguration);
        /// <summary>
        /// 计算机系统
        /// </summary>
        static public string Win32_ComputerSystem => nameof(Win32_ComputerSystem);
        /// <summary>
        /// 计算机系统处理器
        /// </summary>
        static public string Win32_ComputerSystemProcessor => nameof(Win32_ComputerSystemProcessor);
        /// <summary>
        /// 计算机系统产品
        /// </summary>
        static public string Win32_ComputerSystemProduct => nameof(Win32_ComputerSystemProduct);
        /// <summary>
        /// 信任的服务
        /// </summary>
        static public string Win32_DependentService => nameof(Win32_DependentService);
        /// <summary>
        /// 装载顺序组
        /// </summary>
        static public string Win32_LoadOrderGroup => nameof(Win32_LoadOrderGroup);
        static public string Win32_LoadOrderGroupServiceDependencies => nameof(Win32_LoadOrderGroupServiceDependencies);
        static public string Win32_LoadOrderGroupServiceMembers => nameof(Win32_LoadOrderGroupServiceMembers);
        /// <summary>
        /// 操作系统
        /// </summary>
        static public string Win32_OperatingSystem => nameof(Win32_OperatingSystem);
        static public string Win32_OperatingSystemQFE => nameof(Win32_OperatingSystemQFE);
        /// <summary>
        /// 操作系统恢复配置
        /// </summary>
        static public string Win32_OSRecoveryConfiguration => nameof(Win32_OSRecoveryConfiguration);
        static public string Win32_QuickFixEngineering => nameof(Win32_QuickFixEngineering);
        /// <summary>
        /// 启动命令
        /// </summary>
        static public string Win32_StartupCommand => nameof(Win32_StartupCommand);
        static public string Win32_SystemBootConfiguration => nameof(Win32_SystemBootConfiguration);
        static public string Win32_SystemDesktop => nameof(Win32_SystemDesktop);
        static public string Win32_SystemDevices => nameof(Win32_SystemDevices);
        static public string Win32_SystemLoadOrderGroups => nameof(Win32_SystemLoadOrderGroups);
        static public string Win32_SystemNetworkConnections => nameof(Win32_SystemNetworkConnections);
        static public string Win32_SystemOperatingSystem => nameof(Win32_SystemOperatingSystem);
        static public string Win32_SystemProcesses => nameof(Win32_SystemProcesses);
        /// <summary>
        /// Windows开始程序组
        /// </summary>
        static public string Win32_SystemProgramGroups => nameof(Win32_SystemProgramGroups);
        static public string Win32_SystemResources => nameof(Win32_SystemResources);
        /// <summary>
        /// 系统服务
        /// </summary>
        static public string Win32_SystemServices => nameof(Win32_SystemServices);
        static public string Win32_SystemSetting => nameof(Win32_SystemSetting);
        static public string Win32_SystemSystemDriver => nameof(Win32_SystemSystemDriver);
        /// <summary>
        /// 系统时区
        /// </summary>
        static public string Win32_SystemTimeZone => nameof(Win32_SystemTimeZone);
        /// <summary>
        /// 系统用户
        /// </summary>
        static public string Win32_SystemUsers => nameof(Win32_SystemUsers);
    }
    public struct Process {
        /// <summary>
        /// 进程
        /// </summary>
        static public string Win32_Process => nameof(Win32_Process);
        static public string Win32_ProcessStartup => nameof(Win32_ProcessStartup);
        /// <summary>
        /// 线程
        /// </summary>
        static public string Win32_Thread => nameof(Win32_Thread);
    }
    public struct Registry {
        /// <summary>
        /// 注册表
        /// </summary>
        static public string Win32_Registry => nameof(Win32_Registry);
        /// <summary>
        /// 当前时间
        /// </summary>
        static public string Win32_CurrentTime => nameof(Win32_CurrentTime);
        static public string Win32_ScheduledJob => nameof(Win32_ScheduledJob);
    }
    public struct Security {
        static public string Win32_AccountSID => nameof(Win32_AccountSID);
        static public string Win32_ACE => nameof(Win32_ACE);
        static public string Win32_LogicalFileAccess => nameof(Win32_LogicalFileAccess);
        static public string Win32_LogicalFileAuditing => nameof(Win32_LogicalFileAuditing);
        static public string Win32_LogicalFileGroup => nameof(Win32_LogicalFileGroup);
        static public string Win32_LogicalFileOwner => nameof(Win32_LogicalFileOwner);
        static public string Win32_LogicalFileSecuritySetting => nameof(Win32_LogicalFileSecuritySetting);
        static public string Win32_LogicalShareAccess => nameof(Win32_LogicalShareAccess);
        static public string Win32_LogicalShareAuditing => nameof(Win32_LogicalShareAuditing);
        static public string Win32_LogicalShareSecuritySetting => nameof(Win32_LogicalShareSecuritySetting);
        static public string Win32_PrivilegesStatus => nameof(Win32_PrivilegesStatus);
        static public string Win32_SecurityDescriptor => nameof(Win32_SecurityDescriptor);
        static public string Win32_SecuritySetting => nameof(Win32_SecuritySetting);
        static public string Win32_SecuritySettingAccess => nameof(Win32_SecuritySettingAccess);
        static public string Win32_SecuritySettingAuditing => nameof(Win32_SecuritySettingAuditing);
        static public string Win32_SecuritySettingGroup => nameof(Win32_SecuritySettingGroup);
        static public string Win32_SecuritySettingOfLogicalFile => nameof(Win32_SecuritySettingOfLogicalFile);
        static public string Win32_SecuritySettingOfLogicalShare => nameof(Win32_SecuritySettingOfLogicalShare);
        static public string Win32_SecuritySettingOfObject => nameof(Win32_SecuritySettingOfObject);
        static public string Win32_SecuritySettingOwner => nameof(Win32_SecuritySettingOwner);
        static public string Win32_SID => nameof(Win32_SID);
        static public string Win32_Trustee => nameof(Win32_Trustee);
    }
    public struct Service {
        /// <summary>
        /// 基本服务
        /// </summary>
        static public string Win32_BaseService => nameof(Win32_BaseService);
        /// <summary>
        /// 服务
        /// </summary>
        static public string Win32_Service => nameof(Win32_Service);
    }
    public struct Share {
        static public string Win32_DFSNode => nameof(Win32_DFSNode);
        static public string Win32_DFSNodeTarget => nameof(Win32_DFSNodeTarget);
        static public string Win32_DFSTarget => nameof(Win32_DFSTarget);
        static public string Win32_ServerConnection => nameof(Win32_ServerConnection);
        static public string Win32_ServerSession => nameof(Win32_ServerSession);
        static public string Win32_ConnectionShare => nameof(Win32_ConnectionShare);
        static public string Win32_PrinterShare => nameof(Win32_PrinterShare);
        static public string Win32_SessionConnection => nameof(Win32_SessionConnection);
        static public string Win32_SessionProcess => nameof(Win32_SessionProcess);
        static public string Win32_ShareToDirectory => nameof(Win32_ShareToDirectory);
        /// <summary>
        /// 共享文件夹
        /// </summary>
        static public string Win32_Share => nameof(Win32_Share);
    }
    public struct StartMenu {
        /// <summary>
        /// Windows开始逻辑程序组
        /// </summary>
        static public string Win32_LogicalProgramGroup => nameof(Win32_LogicalProgramGroup);
        /// <summary>
        /// Windows开始逻辑程序组目录
        /// </summary>
        static public string Win32_LogicalProgramGroupDirectory => nameof(Win32_LogicalProgramGroupDirectory);
        /// <summary>
        /// Windows开始逻辑程序组项
        /// </summary>
        static public string Win32_LogicalProgramGroupItem => nameof(Win32_LogicalProgramGroupItem);
        /// <summary>
        /// Windows开始逻辑程序组项数据文件
        /// </summary>
        static public string Win32_LogicalProgramGroupItemDataFile => nameof(Win32_LogicalProgramGroupItemDataFile);
        /// <summary>
        /// Windows程序组
        /// </summary>
        static public string Win32_ProgramGroup => nameof(Win32_ProgramGroup);
        /// <summary>
        /// Windows程序组内容
        /// </summary>
        static public string Win32_ProgramGroupContents => nameof(Win32_ProgramGroupContents);
        /// <summary>
        /// Windows程序组或项
        /// </summary>
        static public string Win32_ProgramGroupOrItem => nameof(Win32_ProgramGroupOrItem);
    }
    public struct Storage {
        static public string Win32_ShadowBy => nameof(Win32_ShadowBy);
        static public string Win32_ShadowContext => nameof(Win32_ShadowContext);
        static public string Win32_ShadowCopy => nameof(Win32_ShadowCopy);
        static public string Win32_ShadowDiffVolumeSupport => nameof(Win32_ShadowDiffVolumeSupport);
        static public string Win32_ShadowFor => nameof(Win32_ShadowFor);
        static public string Win32_ShadowOn => nameof(Win32_ShadowOn);
        static public string Win32_ShadowProvider => nameof(Win32_ShadowProvider);
        static public string Win32_ShadowStorage => nameof(Win32_ShadowStorage);
        static public string Win32_ShadowVolumeSupport => nameof(Win32_ShadowVolumeSupport);
        static public string Win32_Volume => nameof(Win32_Volume);
        static public string Win32_VolumeUserQuota => nameof(Win32_VolumeUserQuota);
    }
    public struct User {
        /// <summary>
        /// 帐户
        /// </summary>
        static public string Win32_Account => nameof(Win32_Account);
        /// <summary>
        /// 组
        /// </summary>
        static public string Win32_Group => nameof(Win32_Group);
        /// <summary>
        /// 域中的组
        /// </summary>
        static public string Win32_GroupInDomain => nameof(Win32_GroupInDomain);
        /// <summary>
        /// 组用户
        /// </summary>
        static public string Win32_GroupUser => nameof(Win32_GroupUser);
        /// <summary>
        /// 登录会话
        /// </summary>
        static public string Win32_LogonSession => nameof(Win32_LogonSession);
        static public string Win32_LogonSessionMappedDisk => nameof(Win32_LogonSessionMappedDisk);
        static public string Win32_NetworkLoginProfile => nameof(Win32_NetworkLoginProfile);
        /// <summary>
        /// 系统账户
        /// </summary>
        static public string Win32_SystemAccount => nameof(Win32_SystemAccount);
        /// <summary>
        /// 使用账户
        /// </summary>
        static public string Win32_UserAccount => nameof(Win32_UserAccount);
        /// <summary>
        /// 域中的用户
        /// </summary>
        static public string Win32_UserInDomain => nameof(Win32_UserInDomain);
        /// <summary>
        /// 事件日志文件
        /// </summary>
        static public string Win32_NTEventlogFile => nameof(Win32_NTEventlogFile);
        /// <summary>
        /// 日志事件
        /// </summary>
        static public string Win32_NTLogEvent => nameof(Win32_NTLogEvent);
        /// <summary>
        /// 日志事件计算机
        /// </summary>
        static public string Win32_NTLogEventComputer => nameof(Win32_NTLogEventComputer);
        /// <summary>
        /// 日志事件日志
        /// </summary>
        static public string Win32_NTLogEventLog => nameof(Win32_NTLogEventLog);
        static public string Win32_NTLogEventUser => nameof(Win32_NTLogEventUser);
    }
    public struct WinProduct {
        static public string Win32_ComputerSystemWindowsProductActivationSetting => nameof(Win32_ComputerSystemWindowsProductActivationSetting);
        /// <summary>
        /// 代理
        /// </summary>
        static public string Win32_Proxy => nameof(Win32_Proxy);
        /// <summary>
        /// Windows产品激活
        /// </summary>
        static public string Win32_WindowsProductActivation => nameof(Win32_WindowsProductActivation);
    }
    public struct InstallApplication {
        static public string Win32_ActionCheck => nameof(Win32_ActionCheck);
        static public string Win32_ApplicationCommandLine => nameof(Win32_ApplicationCommandLine);
        static public string Win32_ApplicationService => nameof(Win32_ApplicationService);
        static public string Win32_Binary => nameof(Win32_Binary);
        static public string Win32_BindImageAction => nameof(Win32_BindImageAction);
        static public string Win32_CheckCheck => nameof(Win32_CheckCheck);
        static public string Win32_ClassInfoAction => nameof(Win32_ClassInfoAction);
        static public string Win32_CommandLineAccess => nameof(Win32_CommandLineAccess);
        static public string Win32_Condition => nameof(Win32_Condition);
        static public string Win32_CreateFolderAction => nameof(Win32_CreateFolderAction);
        static public string Win32_DuplicateFileAction => nameof(Win32_DuplicateFileAction);
        static public string Win32_EnvironmentSpecification => nameof(Win32_EnvironmentSpecification);
        static public string Win32_ExtensionInfoAction => nameof(Win32_ExtensionInfoAction);
        static public string Win32_FileSpecification => nameof(Win32_FileSpecification);
        static public string Win32_FontInfoAction => nameof(Win32_FontInfoAction);
        static public string Win32_IniFileSpecification => nameof(Win32_IniFileSpecification);
        static public string Win32_InstalledSoftwareElement => nameof(Win32_InstalledSoftwareElement);
        static public string Win32_LaunchCondition => nameof(Win32_LaunchCondition);
        static public string Win32_ManagedSystemElementResource => nameof(Win32_ManagedSystemElementResource);
        static public string Win32_MIMEInfoAction => nameof(Win32_MIMEInfoAction);
        static public string Win32_MoveFileAction => nameof(Win32_MoveFileAction);
        static public string Win32_MSIResource => nameof(Win32_MSIResource);
        static public string Win32_ODBCAttribute => nameof(Win32_ODBCAttribute);
        static public string Win32_ODBCDataSourceAttribute => nameof(Win32_ODBCDataSourceAttribute);
        static public string Win32_ODBCDataSourceSpecification => nameof(Win32_ODBCDataSourceSpecification);
        static public string Win32_ODBCDriverAttribute => nameof(Win32_ODBCDriverAttribute);
        static public string Win32_ODBCDriverSoftwareElement => nameof(Win32_ODBCDriverSoftwareElement);
        static public string Win32_ODBCDriverSpecification => nameof(Win32_ODBCDriverSpecification);
        static public string Win32_ODBCSourceAttribute => nameof(Win32_ODBCSourceAttribute);
        static public string Win32_ODBCTranslatorSpecification => nameof(Win32_ODBCTranslatorSpecification);
        static public string Win32_Patch => nameof(Win32_Patch);
        static public string Win32_PatchFile => nameof(Win32_PatchFile);
        static public string Win32_PatchPackage => nameof(Win32_PatchPackage);
        static public string Win32_Product => nameof(Win32_Product);
        static public string Win32_ProductCheck => nameof(Win32_ProductCheck);
        static public string Win32_ProductResource => nameof(Win32_ProductResource);
        static public string Win32_ProductSoftwareFeatures => nameof(Win32_ProductSoftwareFeatures);
        static public string Win32_ProgIDSpecification => nameof(Win32_ProgIDSpecification);
        static public string Win32_Property => nameof(Win32_Property);
        static public string Win32_PublishComponentAction => nameof(Win32_PublishComponentAction);
        static public string Win32_RegistryAction => nameof(Win32_RegistryAction);
        static public string Win32_RemoveFileAction => nameof(Win32_RemoveFileAction);
        static public string Win32_RemoveIniAction => nameof(Win32_RemoveIniAction);
        static public string Win32_ReserveCost => nameof(Win32_ReserveCost);
        static public string Win32_SelfRegModuleAction => nameof(Win32_SelfRegModuleAction);
        static public string Win32_ServiceControl => nameof(Win32_ServiceControl);
        static public string Win32_ServiceSpecification => nameof(Win32_ServiceSpecification);
        static public string Win32_ServiceSpecificationService => nameof(Win32_ServiceSpecificationService);
        static public string Win32_SettingCheck => nameof(Win32_SettingCheck);
        static public string Win32_ShortcutAction => nameof(Win32_ShortcutAction);
        static public string Win32_ShortcutSAP => nameof(Win32_ShortcutSAP);
        static public string Win32_SoftwareElement => nameof(Win32_SoftwareElement);
        static public string Win32_SoftwareElementAction => nameof(Win32_SoftwareElementAction);
        static public string Win32_SoftwareElementCheck => nameof(Win32_SoftwareElementCheck);
        static public string Win32_SoftwareElementCondition => nameof(Win32_SoftwareElementCondition);
        static public string Win32_SoftwareElementResource => nameof(Win32_SoftwareElementResource);
        static public string Win32_SoftwareFeature => nameof(Win32_SoftwareFeature);
        static public string Win32_SoftwareFeatureAction => nameof(Win32_SoftwareFeatureAction);
        static public string Win32_SoftwareFeatureCheck => nameof(Win32_SoftwareFeatureCheck);
        static public string Win32_SoftwareFeatureParent => nameof(Win32_SoftwareFeatureParent);
        static public string Win32_SoftwareFeatureSoftwareElements => nameof(Win32_SoftwareFeatureSoftwareElements);
        static public string Win32_TypeLibraryAction => nameof(Win32_TypeLibraryAction);
    }
    public struct WmiConfig {
        /// <summary>
        /// 方法参数类
        /// </summary>
        static public string Win32_MethodParameterClass => nameof(Win32_MethodParameterClass);
        /// <summary>
        /// WMI设置
        /// </summary>
        static public string Win32_WMISetting => nameof(Win32_WMISetting);
        /// <summary>
        /// WMI单元设置
        /// </summary>
        static public string Win32_WMIElementSetting => nameof(Win32_WMIElementSetting);
    }
    public struct FormatPerformanceCounter {
        static public string Win32_PerfFormattedData => nameof(Win32_PerfFormattedData);
        static public string Win32_PerfFormattedData_ASP_ActiveServerPages => nameof(Win32_PerfFormattedData_ASP_ActiveServerPages);
        static public string Win32_PerfFormattedData_ContentFilter_IndexingServiceFilter => nameof(Win32_PerfFormattedData_ContentFilter_IndexingServiceFilter);
        static public string Win32_PerfFormattedData_ContentIndex_IndexingService => nameof(Win32_PerfFormattedData_ContentIndex_IndexingService);
        static public string Win32_PerfFormattedData_InetInfo_InternetInformationServicesGlobal => nameof(Win32_PerfFormattedData_InetInfo_InternetInformationServicesGlobal);
        static public string Win32_PerfFormattedData_ISAPISearch_HttpIndexingService => nameof(Win32_PerfFormattedData_ISAPISearch_HttpIndexingService);
        static public string Win32_PerfFormattedData_MSDTC_DistributedTransactionCoordinator => nameof(Win32_PerfFormattedData_MSDTC_DistributedTransactionCoordinator);
        static public string Win32_PerfFormattedData_NTFSDRV_SMTPNTFSStoreDriver => nameof(Win32_PerfFormattedData_NTFSDRV_SMTPNTFSStoreDriver);
        static public string Win32_PerfFormattedData_PerfDisk_LogicalDisk => nameof(Win32_PerfFormattedData_PerfDisk_LogicalDisk);
        static public string Win32_PerfFormattedData_PerfDisk_PhysicalDisk => nameof(Win32_PerfFormattedData_PerfDisk_PhysicalDisk);
        static public string Win32_PerfFormattedData_PerfNet_Browser => nameof(Win32_PerfFormattedData_PerfNet_Browser);
        static public string Win32_PerfFormattedData_PerfNet_Redirector => nameof(Win32_PerfFormattedData_PerfNet_Redirector);
        static public string Win32_PerfFormattedData_PerfNet_Server => nameof(Win32_PerfFormattedData_PerfNet_Server);
        static public string Win32_PerfFormattedData_PerfNet_ServerWorkQueues => nameof(Win32_PerfFormattedData_PerfNet_ServerWorkQueues);
        static public string Win32_PerfFormattedData_PerfOS_Cache => nameof(Win32_PerfFormattedData_PerfOS_Cache);
        static public string Win32_PerfFormattedData_PerfOS_Memory => nameof(Win32_PerfFormattedData_PerfOS_Memory);
        static public string Win32_PerfFormattedData_PerfOS_Objects => nameof(Win32_PerfFormattedData_PerfOS_Objects);
        static public string Win32_PerfFormattedData_PerfOS_PagingFile => nameof(Win32_PerfFormattedData_PerfOS_PagingFile);
        static public string Win32_PerfFormattedData_PerfOS_Processor => nameof(Win32_PerfFormattedData_PerfOS_Processor);
        static public string Win32_PerfFormattedData_PerfOS_System => nameof(Win32_PerfFormattedData_PerfOS_System);
        static public string Win32_PerfFormattedData_PerfProc_FullImage_Costly => nameof(Win32_PerfFormattedData_PerfProc_FullImage_Costly);
        static public string Win32_PerfFormattedData_PerfProc_Image_Costly => nameof(Win32_PerfFormattedData_PerfProc_Image_Costly);
        static public string Win32_PerfFormattedData_PerfProc_JobObject => nameof(Win32_PerfFormattedData_PerfProc_JobObject);
        static public string Win32_PerfFormattedData_PerfProc_JobObjectDetails => nameof(Win32_PerfFormattedData_PerfProc_JobObjectDetails);
        static public string Win32_PerfFormattedData_PerfProc_Process => nameof(Win32_PerfFormattedData_PerfProc_Process);
        static public string Win32_PerfFormattedData_PerfProc_ProcessAddressSpace_Costly => nameof(Win32_PerfFormattedData_PerfProc_ProcessAddressSpace_Costly);
        static public string Win32_PerfFormattedData_PerfProc_Thread => nameof(Win32_PerfFormattedData_PerfProc_Thread);
        static public string Win32_PerfFormattedData_PerfProc_ThreadDetails_Costly => nameof(Win32_PerfFormattedData_PerfProc_ThreadDetails_Costly);
        static public string Win32_PerfFormattedData_PSched_PSchedFlow => nameof(Win32_PerfFormattedData_PSched_PSchedFlow);
        static public string Win32_PerfFormattedData_PSched_PSchedPipe => nameof(Win32_PerfFormattedData_PSched_PSchedPipe);
        static public string Win32_PerfFormattedData_RemoteAccess_RASPort => nameof(Win32_PerfFormattedData_RemoteAccess_RASPort);
        static public string Win32_PerfFormattedData_RemoteAccess_RASTotal => nameof(Win32_PerfFormattedData_RemoteAccess_RASTotal);
        static public string Win32_PerfFormattedData_RSVP_ACSRSVPInterfaces => nameof(Win32_PerfFormattedData_RSVP_ACSRSVPInterfaces);
        static public string Win32_PerfFormattedData_RSVP_ACSRSVPService => nameof(Win32_PerfFormattedData_RSVP_ACSRSVPService);
        static public string Win32_PerfFormattedData_SMTPSVC_SMTPServer => nameof(Win32_PerfFormattedData_SMTPSVC_SMTPServer);
        static public string Win32_PerfFormattedData_Spooler_PrintQueue => nameof(Win32_PerfFormattedData_Spooler_PrintQueue);
        static public string Win32_PerfFormattedData_TapiSrv_Telephony => nameof(Win32_PerfFormattedData_TapiSrv_Telephony);
        static public string Win32_PerfFormattedData_Tcpip_ICMP => nameof(Win32_PerfFormattedData_Tcpip_ICMP);
        static public string Win32_PerfFormattedData_Tcpip_IP => nameof(Win32_PerfFormattedData_Tcpip_IP);
        static public string Win32_PerfFormattedData_Tcpip_NBTConnection => nameof(Win32_PerfFormattedData_Tcpip_NBTConnection);
        static public string Win32_PerfFormattedData_Tcpip_NetworkInterface => nameof(Win32_PerfFormattedData_Tcpip_NetworkInterface);
        static public string Win32_PerfFormattedData_Tcpip_TCP => nameof(Win32_PerfFormattedData_Tcpip_TCP);
        static public string Win32_PerfFormattedData_Tcpip_UDP => nameof(Win32_PerfFormattedData_Tcpip_UDP);
        static public string Win32_PerfFormattedData_TermService_TerminalServices => nameof(Win32_PerfFormattedData_TermService_TerminalServices);
        static public string Win32_PerfFormattedData_TermService_TerminalServicesSession => nameof(Win32_PerfFormattedData_TermService_TerminalServicesSession);
        static public string Win32_PerfFormattedData_W3SVC_WebService => nameof(Win32_PerfFormattedData_W3SVC_WebService);
    }
    public struct OriginalPerformanceCounter {
        /// <summary>
        /// Win32_PerfRawData_ASP_ActiveServerPages--
        /// </summary>
        static public string Win32_PerfRawData => nameof(Win32_PerfRawData);
        static public string Win32_PerfRawData_ContentFilter_IndexingServiceFilter => nameof(Win32_PerfRawData_ContentFilter_IndexingServiceFilter);
        static public string Win32_PerfRawData_ContentIndex_IndexingService => nameof(Win32_PerfRawData_ContentIndex_IndexingService);
        static public string Win32_PerfRawData_InetInfo_InternetInformationServicesGlobal => nameof(Win32_PerfRawData_InetInfo_InternetInformationServicesGlobal);
        static public string Win32_PerfRawData_ISAPISearch_HttpIndexingService => nameof(Win32_PerfRawData_ISAPISearch_HttpIndexingService);
        static public string Win32_PerfRawData_MSDTC_DistributedTransactionCoordinator => nameof(Win32_PerfRawData_MSDTC_DistributedTransactionCoordinator);
        static public string Win32_PerfRawData_NTFSDRV_SMTPNTFSStoreDriver => nameof(Win32_PerfRawData_NTFSDRV_SMTPNTFSStoreDriver);
        static public string Win32_PerfRawData_PerfDisk_LogicalDisk => nameof(Win32_PerfRawData_PerfDisk_LogicalDisk);
        static public string Win32_PerfRawData_PerfDisk_PhysicalDisk => nameof(Win32_PerfRawData_PerfDisk_PhysicalDisk);
        static public string Win32_PerfRawData_PerfNet_Browser => nameof(Win32_PerfRawData_PerfNet_Browser);
        static public string Win32_PerfRawData_PerfNet_Redirector => nameof(Win32_PerfRawData_PerfNet_Redirector);
        static public string Win32_PerfRawData_PerfNet_Server => nameof(Win32_PerfRawData_PerfNet_Server);
        static public string Win32_PerfRawData_PerfNet_ServerWorkQueues => nameof(Win32_PerfRawData_PerfNet_ServerWorkQueues);
        static public string Win32_PerfRawData_PerfOS_Cache => nameof(Win32_PerfRawData_PerfOS_Cache);
        static public string Win32_PerfRawData_PerfOS_Memory => nameof(Win32_PerfRawData_PerfOS_Memory);
        static public string Win32_PerfRawData_PerfOS_Objects => nameof(Win32_PerfRawData_PerfOS_Objects);
        static public string Win32_PerfRawData_PerfOS_PagingFile => nameof(Win32_PerfRawData_PerfOS_PagingFile);
        static public string Win32_PerfRawData_PerfOS_Processor => nameof(Win32_PerfRawData_PerfOS_Processor);
        static public string Win32_PerfRawData_PerfOS_System => nameof(Win32_PerfRawData_PerfOS_System);
        static public string Win32_PerfRawData_PerfProc_FullImage_Costly => nameof(Win32_PerfRawData_PerfProc_FullImage_Costly);
        static public string Win32_PerfRawData_PerfProc_Image_Costly => nameof(Win32_PerfRawData_PerfProc_Image_Costly);
        static public string Win32_PerfRawData_PerfProc_JobObject => nameof(Win32_PerfRawData_PerfProc_JobObject);
        static public string Win32_PerfRawData_PerfProc_JobObjectDetails => nameof(Win32_PerfRawData_PerfProc_JobObjectDetails);
        static public string Win32_PerfRawData_PerfProc_Process => nameof(Win32_PerfRawData_PerfProc_Process);
        static public string Win32_PerfRawData_PerfProc_ProcessAddressSpace_Costly => nameof(Win32_PerfRawData_PerfProc_ProcessAddressSpace_Costly);
        static public string Win32_PerfRawData_PerfProc_Thread => nameof(Win32_PerfRawData_PerfProc_Thread);
        static public string Win32_PerfRawData_PerfProc_ThreadDetails_Costly => nameof(Win32_PerfRawData_PerfProc_ThreadDetails_Costly);
        static public string Win32_PerfRawData_PSched_PSchedFlow => nameof(Win32_PerfRawData_PSched_PSchedFlow);
        static public string Win32_PerfRawData_PSched_PSchedPipe => nameof(Win32_PerfRawData_PSched_PSchedPipe);
        static public string Win32_PerfRawData_RemoteAccess_RASPort => nameof(Win32_PerfRawData_RemoteAccess_RASPort);
        static public string Win32_PerfRawData_RemoteAccess_RASTotal => nameof(Win32_PerfRawData_RemoteAccess_RASTotal);
        static public string Win32_PerfRawData_RSVP_ACSRSVPInterfaces => nameof(Win32_PerfRawData_RSVP_ACSRSVPInterfaces);
        static public string Win32_PerfRawData_RSVP_ACSRSVPService => nameof(Win32_PerfRawData_RSVP_ACSRSVPService);
        static public string Win32_PerfRawData_SMTPSVC_SMTPServer => nameof(Win32_PerfRawData_SMTPSVC_SMTPServer);
        static public string Win32_PerfRawData_Spooler_PrintQueue => nameof(Win32_PerfRawData_Spooler_PrintQueue);
        static public string Win32_PerfRawData_TapiSrv_Telephony => nameof(Win32_PerfRawData_TapiSrv_Telephony);
        static public string Win32_PerfRawData_Tcpip_ICMP => nameof(Win32_PerfRawData_Tcpip_ICMP);
        static public string Win32_PerfRawData_Tcpip_IP => nameof(Win32_PerfRawData_Tcpip_IP);
        static public string Win32_PerfRawData_Tcpip_NBTConnection => nameof(Win32_PerfRawData_Tcpip_NBTConnection);
        static public string Win32_PerfRawData_Tcpip_NetworkInterface => nameof(Win32_PerfRawData_Tcpip_NetworkInterface);
        static public string Win32_PerfRawData_Tcpip_TCP => nameof(Win32_PerfRawData_Tcpip_TCP);
        static public string Win32_PerfRawData_Tcpip_UDP => nameof(Win32_PerfRawData_Tcpip_UDP);
        static public string Win32_PerfRawData_TermService_TerminalServices => nameof(Win32_PerfRawData_TermService_TerminalServices);
        static public string Win32_PerfRawData_TermService_TerminalServicesSession => nameof(Win32_PerfRawData_TermService_TerminalServicesSession);
        static public string Win32_PerfRawData_W3SVC_WebService => nameof(Win32_PerfRawData_W3SVC_WebService);
    }
}
