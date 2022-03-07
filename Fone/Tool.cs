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