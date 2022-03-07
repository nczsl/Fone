// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Util.Ex;

namespace Util.Generator {
    public class FactoryDbCode {
        #region common
        static string ProcessSqlStr(string sqlstr) {
            return sqlstr.
            Replace("[dbo].", string.Empty).
            Replace("[", string.Empty).
            Replace("CREATE TABLE", "create table").
            Replace("NOT NULL", string.Empty).
            Replace("NULL", string.Empty).
            Replace("]", string.Empty);
        }

        public static bool IsSampleType(string t) {
            var r = false;
            var dotNetTypes = new string[] {
                "int?",
                "string",
                "bool?",
                "DateTime",
                "Decimal?",
                "Double?",
                "Byte[]",
                "Single?",
                "string",
                "string",
                "DateTime?",
                "Int16?",
                "string",
                "Int64?",
                "Byte[]",
                "string",
                "string",
                "Decimal?",
                "Single?",
                "Single?",
                "Object",
                "Byte[]?",
                "Byte?",
                "Guid",
                "Byte[]",
                "string"
            }; //new string[] { "Int32", "Int64", "long", "UInt64", "byte", "int", "uint", "UInt32", "String", "Boolen", "bool", "DateTime", "Decimal", "Double", "doublue", "Byte", "Byte[]", "Single", "String", "string", "Int8", "UInt8", "Int16", "UInt16", "float", "short", "ushort", "Guid" };
            if ((from it in dotNetTypes.Distinct() where it == t select it).Count() == 1) r = true;
            return r;
        }
        static Type SqlType2NetType(string sqlType, bool isnull = false) => sqlType.ToLower()
        switch {
            "bigint" => isnull ? typeof(long?) : typeof(long),
            "binary" => typeof(byte[]),
            "bit" => isnull ? typeof(bool?) : typeof(bool),
            "char" => typeof(string),
            "date" => isnull ? typeof(DateTime?) : typeof(DateTime),
            "datetime" => isnull ? typeof(DateTime?) : typeof(DateTime),
            "datetime2" => isnull ? typeof(DateTime?) : typeof(DateTime),
            "datetimeoffset" => isnull ? typeof(DateTimeOffset) : typeof(DateTimeOffset),
            "decimal" => isnull ? typeof(decimal?) : typeof(decimal),
            "float" => isnull ? typeof(double?) : typeof(double),
            "image" => typeof(byte[]),
            "int" => isnull ? typeof(int?) : typeof(int),
            "integer" => isnull ? typeof(int?) : typeof(int),
            "money" => isnull ? typeof(decimal?) : typeof(decimal),
            "nchar" => typeof(string),
            "ntext" => typeof(string),
            "numeric" => isnull ? typeof(decimal?) : typeof(decimal),
            "nvarchar" => typeof(string),
            "real" => isnull ? typeof(float?) : typeof(float),
            "rowversion" => typeof(byte[]),
            "smalldatetime" => isnull ? typeof(DateTime?) : typeof(DateTime),
            "smallint" => isnull ? typeof(short?) : typeof(short),
            "smallmoney" => isnull ? typeof(decimal?) : typeof(decimal),
            "sql_variant" => typeof(object),
            "text" => typeof(string),
            "time" => isnull ? typeof(TimeSpan?) : typeof(TimeSpan),
            "timestamp" => typeof(byte[]),
            "tinyint" => isnull ? typeof(byte?) : typeof(byte),
            "uniqueidentifier" => isnull ? typeof(Guid?) : typeof(Guid),
            "varbinary" => typeof(byte[]),
            "varchar" => typeof(string),
            "xml" => typeof(System.Xml.Linq.XElement),
            _ =>
            throw new Exception("undefind sqltype")
        };
        public static string NetType2SqlType(Type netType, int i = 0) => netType
        switch {
            Type _ when netType == typeof(long) => "bigint",
            Type _ when netType == typeof(long?) => "bigint null",
            Type _ when netType == typeof(byte[]) && i == 0 => "binary",
            Type _ when netType == typeof(bool) => "bit",
            Type _ when netType == typeof(bool?) => "bit null",
            Type _ when netType == typeof(string) && i == 0 => "char",
            Type _ when netType == typeof(DateTime) && i == 0 => "date",
            Type _ when netType == typeof(DateTime?) && i == 0 => "date null",
            Type _ when netType == typeof(DateTime) && i == 1 => "datetime",
            Type _ when netType == typeof(DateTime?) && i == 1 => "datetime null",
            Type _ when netType == typeof(DateTime) && i == 2 => "datetime2",
            Type _ when netType == typeof(DateTime?) && i == 2 => "datetime2 null",
            Type _ when netType == typeof(DateTimeOffset) => "datetimeoffset",
            Type _ when netType == typeof(DateTimeOffset?) => "datetimeoffset null",
            Type _ when netType == typeof(decimal) && i == 0 => "decimal",
            Type _ when netType == typeof(decimal?) && i == 0 => "decimal null",
            Type _ when netType == typeof(double) => "float",
            Type _ when netType == typeof(double?) => "float null",
            Type _ when netType == typeof(byte[]) && i == 1 => "image",
            Type _ when netType == typeof(int) => "int",
            Type _ when netType == typeof(int?) => "int null",
            Type _ when netType == typeof(decimal) && i == 1 => "money",
            Type _ when netType == typeof(decimal?) && i == 1 => "money null",
            Type _ when netType == typeof(string) && i == 1 => "nchar",
            Type _ when netType == typeof(string) && i == 2 => "next",
            Type _ when netType == typeof(decimal) && i == 2 => "numeric",
            Type _ when netType == typeof(decimal?) && i == 2 => "numeric null",
            Type _ when netType == typeof(string) && i == 3 => "nvarchar",
            Type _ when netType == typeof(float) => "real",
            Type _ when netType == typeof(float?) => "real null",
            Type _ when netType == typeof(byte[]) && i == 2 => "rowversion",
            Type _ when netType == typeof(DateTime) && i == 3 => "smalldatetime",
            Type _ when netType == typeof(DateTime?) && i == 3 => "smalldatetime null",
            Type _ when netType == typeof(short) => "smallint",
            Type _ when netType == typeof(short?) => "smallint null",
            Type _ when netType == typeof(decimal) && i == 3 => "smallmoney",
            Type _ when netType == typeof(decimal?) && i == 3 => "smallmoney null",
            Type _ when netType == typeof(object) => "sql_variant",
            Type _ when netType == typeof(string) && i == 4 => "text",
            Type _ when netType == typeof(TimeSpan) => "time",
            Type _ when netType == typeof(TimeSpan) => "time null",
            Type _ when netType == typeof(byte[]) && i == 3 => "timestamp",
            Type _ when netType == typeof(byte) => "tinyint",
            Type _ when netType == typeof(byte?) => "tinyint null",
            Type _ when netType == typeof(Guid) => "uniqueidentifier",
            Type _ when netType == typeof(Guid?) => "uniqueidentifier null",
            Type _ when netType == typeof(byte[]) && i == 4 => "varbinary",
            Type _ when netType == typeof(string) && i == 5 => "varchar",
            Type _ when netType == typeof(System.Xml.Linq.XElement) => "xml",
            _ =>
            throw new Exception("undefind sqltype")
        };
        #endregion
        #region case entityframework
        //static string classvm = @"(Data|Ui|Xml|Json|Trans)_\w+";
        static string classent = @"(Role|Lc|Res|Note|R|State|Sys|Log)_\w+";
        public static string Classent {
            get {
                return classent;
            }
            set {
                classent = value;
                pattern_ent = $@"(?<=create\s+table\s+)(?<class>{classent})\((?<props>(([^\(\)])|(?<open>\()|(?<-open>\)))*)(?(open)(?!))\)";
            }
        }
        static string classall = @"(Role|Lc|Res|Note|R|State|Sys|Log|Data|Ui|Xml|Json|Trans)_\w+";
        public static string Classall {
            get {
                return classall;
            }
            set {
                classall = value;
                pattern_ctx = $@"(?<=create\s+table\s+)(?<class>{classall})\((?<props>(([^\(\)])|(?<open>\()|(?<-open>\)))*)(?(open)(?!))\)";
            }
        }
        static string pattern_ent = $@"(?<!\s*--\s*.*)(?<=create\s+table\s+)(?<class>{Classall})\((?<props>(?:([^\(\)])|(?:--)|(?<o>\()|(?<-o>\)))*)(?(o)(?!))\)";
        static string pattern_ctx = $@"(?<!\s*--\s*.*)(?<=create\s+table\s+)(?<class>{Classent})\((?<props>(?:([^\(\)])|(?:--)|(?<o>\()|(?<-o>\)))*)(?(o)(?!))\)";
        static readonly string patternp = @"(?<!\s*--\s*.*)\[?(?<pname>\w+)\]?\s+\[?(?<type>(?:int|integer|tinyint|varchar|bit|datetime|date|decimal|float|image|money|ntext|nvarchar|smalldatetime|smallint|text|bigint|binary|char|nchar|numeric|real|smallmoney|sql_variant|timestamp|uniqueidentifier|varbinary|xml|INT|INTEGER|TINYINT|VARCHAR|BIT|DATETIME|DATE|DECIMAL|FLOAT|IMAGE|MONEY|NTEXT|NVARCHAR|SMALLDATETIME|SMALLINT|TEXT|BIGINT|BINARY|CHAR|NCHAR|NUMERIC|REAL|SMALLMONEY|SQL_VARIANT|TIMESTAMP|UNIQUEIDENTIFIER|VARBINARY|XML)){1}\]?(?<suffix>\((\d+|max|MAX|Max|\d+\s?,\s?\d+)\))?(\s+(null|not\snull|NULL|NOT\sNULL))?(?<pk>(\s+primary\s+key|\s+PRIMARY\sKEY))?(\s+(default|DEFAULT)\s+(?<default>\'?\w+\.?\w+\'?(\(\))?))?(?!.*\s?(?:references|REFERENCES).*)";
        static readonly string patternfk = @"(?<!\s*--\s*.*)\[?(?<pname>\w+)\]?\s+\[?(?<type>(?:int|integer|tinyint|varchar|bit|datetime|date|decimal|float|image|money|ntext|nvarchar|smalldatetime|smallint|text|bigint|binary|char|nchar|numeric|real|smallmoney|sql_variant|timestamp|uniqueidentifier|varbinary|xml|INT|INTEGER|TINYINT|VARCHAR|BIT|DATETIME|DATE|DECIMAL|FLOAT|IMAGE|MONEY|NTEXT|NVARCHAR|SMALLDATETIME|SMALLINT|TEXT|BIGINT|BINARY|CHAR|NCHAR|NUMERIC|REAL|SMALLMONEY|SQL_VARIANT|TIMESTAMP|UNIQUEIDENTIFIER|VARBINARY|XML)){1}\]?(?<suffx>\((\d+|max|MAX|Max|\d+\s?,\s?\d+)\))?(\s+(null|not\snull|NULL|NOT\sNULL))?(?<pk>(\s+primary\s+key|\s+PRIMARY\s+KEY))?(?:\s+references\s+|\s+REFERENCES\s+)(?<table>\w+_?\w+)\s*\(\w+\)";
        static readonly string propsplit = @"(?<!\(\s?\d{0,3}\s?),(?!\s?\d{0,3}\s?\))";
        static readonly string lcinsert = @"\binsert\s+into\s+([\w_\d]+)\s*\((.*)\)\s+values\s*\((.*)\)";
        //
        static CsType GenerateNotifyBase(CsNamespace nps) {
            var nb = nps.StartClass("NotifyBase", inhlist: "INotifyPropertyChanged");
            nb.Event("PropertyChanged", "PropertyChangedEventHandler")
                .StartMethod("OnPropertyChanged", "string ps", "void", "protected virtual")
                .If("PropertyChanged != null")
                .Sentence("PropertyChanged.Invoke(this,new PropertyChangedEventArgs(ps))");
            nb.SetAttribute<CsType>("DataContract");
            return nb;
        }
        public static string GenerateEntitys(string sqlstr, string nps = null, bool iswcfserial = false) {
            sqlstr = ProcessSqlStr(sqlstr);
            var regfk = new Regex(patternfk);
            var regp = new Regex(patternp);
            //
            var r = new Regex(pattern_ent);
            var x = r.Matches(sqlstr);
            var sps = new CsNamespace();
            sps.Using(".", ".Collections.Generic", ".IO", ".Runtime.Serialization");
            foreach (Match item in x) {
                var typ = default(CsType);
                foreach (Capture item2 in item.Groups["class"].Captures) {
                    typ = sps.StartClass(item2.Value);
                }
                foreach (Capture item2 in item.Groups["props"].Captures) {
                    foreach (var item3 in item2.Value.Split(',')) {
                        if (!string.IsNullOrEmpty(item3)) {
                            // var mark1 = regp.IsMatch(item3);
                            // var mark2 = regfk.IsMatch(item3);
                            if (regp.IsMatch(item3)) {
                                var pn = regp.Match(item3).Groups["pname"].Value;
                                var pt = regp.Match(item3).Groups["type"].Value;
                                if (Regex.IsMatch(item3, @"\b(primary\s+key|PRIMARY\s+KEY)\b")) {
                                    typ.Property(pn, SqlType2NetType(pt).Name);
                                } else {
                                    typ.Property(pn, SqlType2NetType(pt).Name);
                                }
                            } else if (regfk.IsMatch(item3)) {
                                var pn = regfk.Match(item3).Groups["pname"].Value;
                                var pt = regfk.Match(item3).Groups["type"].Value;
                                var tn = regfk.Match(item3).Groups["table"].Value;
                                typ.Property(pn, SqlType2NetType(pt).Name);
                                var pkt = sps.FindClass(i => i.name == tn).First();
                                SetVirtualProp(regfk, typ, item3, pn, pkt);
                            }
                        }
                    }
                }
            }
            if (iswcfserial)
                SetEntityDataContract(sps);
            currentCsn = sps;
            var outstr = sps.ToString(string.Format("{0}", nps));
            return outstr;
        }
        static void SetVirtualProp(Regex regfk, CsType typ, string source, string pn, CsType pkt) {
            if (pkt != null) {
                var pk = regfk.Match(source).Groups["pk"].Success;
                var isu = source.ToLower().Contains("unique");
                if (pk || isu) {
                    pkt.Property(string.Format("{0}_{1}", typ.name, pn), string.Format("{0}", typ.name), "public virtual");
                } else {
                    pkt.Property(string.Format("{0}s", typ.name), string.Format("HashSet<{0}>", typ.name), "public virtual");
                }
                if (isu) {
                    typ.Property(string.Format("{0}Unique", pn), string.Format("{0}", pkt.name), "public virtual");
                } else {
                    typ.Property(string.Format("{0}Pk", pn), string.Format("{0}", pkt.name), "public virtual");
                }
            }
        }
        internal static string GenerateNotifyEntitys(string sqlstr, string nps = null, bool iswcfserial = false) {
            sqlstr = ProcessSqlStr(sqlstr);
            var regfk = new Regex(patternfk); //外建
            var regp = new Regex(patternp);
            //
            var r = new Regex(pattern_ent);
            var x = r.Matches(sqlstr);
            var sps = new CsNamespace();
            sps.Using(".", ".Collections.Generic", ".IO", ".Linq", ".Text", ".ComponentModel", ".Collections.ObjectModel", ".Runtime.Serialization", ".ServiceModel");
            //GenerateNotifyBaseNode(sps);
            var nb = GenerateNotifyBase(sps);
            foreach (Match item in x) {
                var typ = default(CsType);
                //每一次匹配实际上只有一个类，虽然下面写的是foreach
                foreach (Capture item2 in item.Groups["class"].Captures) {
                    typ = sps.StartClass(item2.Value, inhlist: nb.name);
                }
                //每次只会有一个props组被匹配
                foreach (Capture item2 in item.Groups["props"].Captures) {
                    foreach (var item3 in Regex.Split(item2.Value, propsplit)) {
                        if (!string.IsNullOrEmpty(item3)) {
                            var mark1 = regp.IsMatch(item3);
                            var mark2 = regfk.IsMatch(item3);
                            //常规属性
                            if (regp.IsMatch(item3)) {
                                var pn = regp.Match(item3).Groups["pname"].Value;
                                var pt = regp.Match(item3).Groups["type"].Value;
                                var ppt = SqlType2NetType(pt).ToString();
                                var field = typ.StartField(pn.ToLower(), ppt, "");
                                typ.StartProperty(pn, ppt, true)
                                    .Get()
                                    .Sentence(string.Format("return this.{0}", field.name))
                                    .ReturnProperty()
                                    .Set()
                                    .Sentence("this.{0} = value", field.name)
                                    .Sentence("base.OnPropertyChanged(nameof({0}))", pn);
                            } else if (regfk.IsMatch(item3)) { //外键属性
                                var pn = regfk.Match(item3).Groups["pname"].Value;
                                var pt = regfk.Match(item3).Groups["type"].Value;
                                var tn = regfk.Match(item3).Groups["table"].Value;
                                typ.Property(pn, SqlType2NetType(pt).ToString());
                                var pkt = sps.FindClass(i => i.name == tn).First();
                                SetVirtualProp(regfk, typ, item3, pn, pkt);
                            }
                        }
                    }
                }
            }
            if (iswcfserial)
                SetEntityDataContract(sps);
            var outstr = sps.ToString(string.Format("{0}", nps));
            return outstr;
        } //
        private static void SetEntityDataContract(CsNamespace sps) {
            //是否进行wcf 序列化标记 默认为否
            foreach (var item in sps.FindClass(c => Regex.IsMatch(c.name, @"\w+_\w+"))) {
                item.SetAttribute<CsType>("DataContract");
                //item.BaseClass?.SetAttribute<Typ>(string.Format("KnownType(typeof({0}))", item.name));
                foreach (var it in from i in item.props where !i.visit.Contains("virtual") select i) {
                    it.SetAttribute<CsProperty>("DataMember");
                }
            }
        }
        public static string GenerateBizCtx(string sqlstr, string dbc, string nps = null, string nps_entity = null) {
            sqlstr = ProcessSqlStr(sqlstr);
            var r = new Regex(pattern_ctx);
            var x = r.Matches(sqlstr);
            var sps = new CsNamespace();
            sps.Using(".,Microsoft.EntityFrameworkCore,Microsoft.Extensions.Configuration");
            var cls = sps.StartClass(dbc, inhlist: "DbContext");
            cls.Field("config", "IConfiguration", "private readonly");
            cls.StartConstructor("")
                .Sentence("//todo entity framework core initial code");
            cls.StartConstructor("IConfiguration config,DbContextOptions dco", inhlist: "base(dco)")
                .Sentence("this.config = config");
            cls.StartMethod("OnConfiguring", "DbContextOptionsBuilder optionsBuilder", "void", "protected override")
                .Sentence("//optionsBuilder.UseNpgsql(config[\"connectstring:dbname\"]);")
                .Sentence("base.OnConfiguring(optionsBuilder)");
            foreach (Match item in x) {
                foreach (Capture item2 in item.Groups["class"].Captures) {
                    cls.Property(item2.Value, string.Format("DbSet<{0}>", item2.Value));
                }
            }
            string outstr = sps.ToString(string.Format("{0}", nps));
            return outstr;
        }
        /// <summary>
        /// 生成efcore 实体上下文，基于sql脚本,
        /// 适用于2.x和3.x
        /// </summary>
        /// <param name="sqlstr">sql脚本内容</param>
        /// <param name="dbc">dbcontext 继承类名称</param>
        /// <param name="nps">名称空间</param>
        /// <param name="nps_entity">引用的实体名称空间</param>
        /// <returns></returns>
        internal static string GenerateBizCtxConfigEfc(string sqlstr, string dbc, string nps = null, string nps_entity = null) {
            sqlstr = ProcessSqlStr(sqlstr);
            var outstr = string.Empty;
            var regfk = new Regex(patternfk);
            var regp = new Regex(patternp);
            var r = new Regex(pattern_ctx);
            var x = r.Matches(sqlstr);
            var sps = new CsNamespace();
            sps.Using(".", "Microsoft.EntityFrameworkCore", "Microsoft.EntityFrameworkCore.Metadata.Builders,.Linq,.Extensions.Configuration", nps_entity);
            var cls = sps.StartClass(dbc, inhlist: $"DbContext");
            cls.Field("config", "IConfiguration", "private readonly");
            cls.StartConstructor("")
                .Sentence("//todo entity framework core 3.x.x initial code");
            cls.StartConstructor("IConfiguration config")
                .Sentence("this.config = config");
            cls.StartMethod("OnConfiguring", "DbContextOptionsBuilder optionsBuilder", "void", "protected override")
                .Sentence("//optionsBuilder.UseSqlServer(\"db_connection_string\");")
                .Sentence("base.OnConfiguring(optionsBuilder)");
            var omc = cls.StartMethod("OnModelCreating", "ModelBuilder mb", "void", "protected override");
            omc.Sentence($"base.OnModelCreating(mb);");
            foreach (Match item in x) {
                var className = string.Empty;
                var typefield = string.Empty;
                foreach (Capture item2 in item.Groups["class"].Captures) {
                    className = item2.Value;
                    cls.Property(className, $"DbSet<{className}>");
                    typefield = string.Format("_{0}", className.ToLower());
                    omc.Sentence("var {0} = mb.Entity<{1}>();", typefield, className);

                }
                foreach (Capture item2 in item.Groups["props"].Captures) {
                    foreach (var item3 in Regex.Split(item2.Value, propsplit)) {
                        if (!string.IsNullOrEmpty(item3)) {
                            if (regp.IsMatch(item3)) {
                                var groups = regp.Match(item3).Groups;
                                var pn = groups["pname"].Value;
                                var pt = groups["type"].Value;
                                var len = groups["suffix"].Value;
                                len = Regex.Match(len, @"(?<=\()(\d+|\d+\s?,\s?\d+)(?=\))").Value;
                                len = len.Split(',').Last();
                                var coltype = "";
                                if (!string.IsNullOrWhiteSpace(len)) {
                                    coltype = $"{pt}({len})";
                                } else {
                                    coltype = $"{pt}";
                                }
                                var pk = groups["pk"].Success;
                                var isu = item3.ToLower().Contains("unique");
                                var clrtype = string.Format("typeof({0})", SqlType2NetType(pt));
                                var defaullt = groups["default"].Value;
                                if (pk) {
                                    omc.Sentence($"{typefield}.HasKey(x=>x.{pn})");
                                    if (!string.IsNullOrEmpty(len)) {
                                        omc.Sentence($"{typefield}.Property(x => x.{pn}).HasColumnType(\"{pt}\").HasMaxLength({len}).IsRequired()");
                                    } else {
                                        omc.Sentence($"{typefield}.Property(x => x.{pn}).HasColumnType(\"{coltype}\").IsRequired()");
                                    }
                                } else if (isu) {
                                    omc.Sentence($"{typefield}.HasAlternateKey(x=>x.{pn})");
                                    if (!string.IsNullOrEmpty(len)) {
                                        omc.Sentence($"{typefield}.Property(x => x.{pn}).HasColumnType(\"{pt}\").HasMaxLength({len})");
                                    } else {
                                        omc.Sentence($"{typefield}.Property(x => x.{pn}).HasColumnType(\"{coltype}\")");
                                    }
                                } else {
                                    if (!string.IsNullOrEmpty(len)) {
                                        omc.Sentence($"{typefield}.Property(x => x.{pn}).HasColumnType(\"{pt}\").HasMaxLength({len}).IsRequired(false)");
                                    } else {
                                        omc.Sentence($"{typefield}.Property(x => x.{pn}).HasColumnType(\"{coltype}\").IsRequired(false)");
                                    }
                                }
                            } else if (regfk.IsMatch(item3)) {
                                var groups = regfk.Match(item3).Groups;
                                var pn = groups["pname"].Value;
                                var pt = groups["type"].Value;
                                var len = groups["suffix"].Value;
                                len = Regex.Match(len, @"(?<=\()(\d+|\d+\s?,\s?\d+)(?=\))").Value;
                                len = len.Split(',').Last();
                                var coltype = "";
                                if (!string.IsNullOrWhiteSpace(len)) {
                                    coltype = $"{pt}({len})";
                                } else {
                                    coltype = $"{pt}";
                                }
                                var tn = groups["table"].Value;
                                var pk = groups["pk"].Success;
                                var isu = item3.ToLower().Contains("unique");
                                var clrtype = string.Format("typeof({0})", SqlType2NetType(pt));
                                var pkt = cls.FindProps(i => i.name == $"{tn}")?.First();
                                if (pk) {
                                    if (!string.IsNullOrEmpty(len)) {
                                        omc.Sentence($"{typefield}.Property(x => x.{pn}).HasColumnType(\"{pt}\").HasMaxLength({len}).IsRequired()");
                                    } else {
                                        omc.Sentence($"{typefield}.Property(x => x.{pn}).HasColumnType(\"{coltype}\").IsRequired()");
                                    }
                                    omc.Sentence($"{typefield}.HasOne(x => x.{pn}Pk).WithOne(x => x.{className}_{pn}).HasForeignKey(\"{pn}\")");
                                } else if (isu) {
                                    if (!string.IsNullOrEmpty(len)) {
                                        omc.Sentence($"{typefield}.Property(x => x.{pn}).HasColumnType(\"{pt}\").HasMaxLength({len}).IsUnique()");
                                    } else {
                                        omc.Sentence($"{typefield}.Property(x => x.{pn}).HasColumnType(\"{coltype}\").IsUnique()");
                                    }
                                    omc.Sentence($"{typefield}.HasOne(x => x.{pn}Unique).WithOne(x => x.{className}s).HasForeignKey(x=>x.{pn})");
                                } else {
                                    if (!string.IsNullOrEmpty(len)) {
                                        omc.Sentence($"{typefield}.Property(x => x.{pn}).HasColumnType(\"{pt}\").HasMaxLength({len}).IsRequired(false)");
                                    } else {
                                        omc.Sentence($"{typefield}.Property(x => x.{pn}).HasColumnType(\"{coltype}\").IsRequired(false)");
                                    }
                                    omc.Sentence($"{typefield}.HasOne(x => x.{pn}Pk).WithMany(x => x.{className}s).HasForeignKey(x=>x.{pn})");
                                }
                            }
                        }
                    }
                }
            }
            currentCsn = sps;
            outstr = sps.ToString(string.Format("{0}", nps));
            return outstr;
        }
        public static CsNamespace GenerateEntity(string sqlstr) {
            sqlstr = ProcessSqlStr(sqlstr);
            var regfk = new Regex(patternfk);
            var regp = new Regex(patternp);
            //
            var r = new Regex(pattern_ent);
            var x = r.Matches(sqlstr);
            var sps = new CsNamespace();
            sps.Using(".", ".Collections.Generic", ".IO", ".Runtime.Serialization");
            foreach (Match item in x) {
                var typ = default(CsType);
                foreach (Capture item2 in item.Groups["class"].Captures) {
                    typ = sps.StartClass(item2.Value);
                }
                foreach (Capture item2 in item.Groups["props"].Captures) {
                    foreach (var item3 in item2.Value.Split(',')) {
                        if (!string.IsNullOrEmpty(item3)) {
                            var mark1 = regp.IsMatch(item3);
                            var mark2 = regfk.IsMatch(item3);
                            if (regp.IsMatch(item3)) {
                                var pn = regp.Match(item3).Groups["pname"].Value;
                                var pt = regp.Match(item3).Groups["type"].Value;
                                if (Regex.IsMatch(item3, @"\bprimary\s*key\b")) {
                                    typ.Property(pn, SqlType2NetType(pt).ToString());
                                } else {
                                    typ.Property(pn, SqlType2NetType(pt).ToString());
                                }
                            } else if (regfk.IsMatch(item3)) {
                                var pn = regfk.Match(item3).Groups["pname"].Value;
                                var pt = regfk.Match(item3).Groups["type"].Value;
                                var tn = regfk.Match(item3).Groups["table"].Value;
                                typ.Property(pn, SqlType2NetType(pt).ToString());
                                var pkt = sps.FindClass(i => i.name == tn).First();
                                SetVirtualProp(regfk, typ, item3, pn, pkt);
                            }
                        }
                    }
                }
            }
            return sps;
        }
        public static CsNamespace GenerateEfCtx(string sqlstr, string dbctxName) {
            sqlstr = ProcessSqlStr(sqlstr);
            var r = new Regex(pattern_ctx);
            var x = r.Matches(sqlstr);
            var sps = new CsNamespace();
            sps.Using(".,Microsoft.EntityFrameworkCore,Microsoft.Extensions.Configuration");
            var cls = sps.StartClass(dbctxName, inhlist: "DbContext");
            cls.Field("config", "IConfiguration", "private readonly");
            cls.StartConstructor("")
                .Sentence("//todo entity framework core 3.x.x initial code");
            cls.StartConstructor("IConfiguration config,DbContextOptions dco", inhlist: "base(dco)")
                .Sentence("this.config = config");
            cls.StartMethod("OnConfiguring", "DbContextOptionsBuilder optionsBuilder", "void", "protected override")
                .Sentence("//optionsBuilder.UseNpgsql(config[\"connectstring:dbname\"]);")
                .Sentence("base.OnConfiguring(optionsBuilder)");
            foreach (Match item in x) {
                foreach (Capture item2 in item.Groups["class"].Captures) {
                    cls.Property(item2.Value, string.Format("DbSet<{0}>", item2.Value));
                }
            }
            return sps;
        }
        public static ProtoNamespace GenerateProtoBySql(string sqlstr, string svcName, string pkg) {
            sqlstr = ProcessSqlStr(sqlstr);
            var regfk = new Regex(patternfk); //外建
            var regp = new Regex(patternp);
            //
            var r = new Regex(pattern_ent);
            var x = r.Matches(sqlstr);
            var psps = new ProtoNamespace { package = pkg };
            foreach (Match item in x) {
                var msg = default(ProtoMessage);
                //每一次匹配实际上只有一个类，虽然下面写的是foreach
                foreach (Capture item2 in item.Groups["class"].Captures) {
                    // typ = sps.StartClass (item2.Value, inhlist : nb.name);
                    msg = new ProtoMessage {
                        Name = item2.Value
                    };
                    psps.AddMessage(msg);
                }
                //每次只会有一个props组被匹配
                foreach (Capture item2 in item.Groups["props"].Captures) {
                    foreach (var item3 in Regex.Split(item2.Value, propsplit)) {
                        if (!string.IsNullOrEmpty(item3)) {
                            var mark1 = regp.IsMatch(item3);
                            var mark2 = regfk.IsMatch(item3);
                            //常规属性
                            if (regp.IsMatch(item3)) {
                                var pn = regp.Match(item3).Groups["pname"].Value;
                                var pt = regp.Match(item3).Groups["type"].Value;
                                var ppt = SqlType2NetType(pt).ToString();
                                var pfield = new ProtoField {
                                    Name = pn,
                                    Key = ProtoFieldType.GetByType(SqlType2NetType(pt)),
                                };
                                msg.AddField(pfield);
                            } else if (regfk.IsMatch(item3)) { //外键属性 引用了别的复合message的字段
                                var pn = regfk.Match(item3).Groups["pname"].Value;
                                var pt = regfk.Match(item3).Groups["type"].Value;
                                var tn = regfk.Match(item3).Groups["table"].Value;
                                msg.AddField(new ProtoField { Name = pn, Key = ProtoFieldType.GetByType(SqlType2NetType(pt)) });
                                var pkt = psps.GetMessage(tn);
                                msg.AddField(new ProtoField { Name = "ref" + pn, Key = ProtoFieldType.RefMessage(pkt) });
                            }
                        }
                    }
                }
            }
            var queryDescriptor = new ProtoMessage { Name = "QueryDescriptor" };
            var operateDescriptor = new ProtoMessage { Name = "OperateDescriptor" };
            var operateCallback = new ProtoMessage { Name = "OperateCallback" };
            var queryCallback = new ProtoMessage { Name = "QueryCallback" };
            var _table = new ProtoMessage { Name = "EntityCatalog" };
            _table.AddToOneof("EntityPointer",
            (from i in psps.messages
             select new ProtoField { Name = $"_{i.Name}", Key = ProtoFieldType.RefMessage(i) }).ToArray());
            var tableType = new ProtoEnum { Name = "TableType" };
            var operType = new ProtoEnum { Name = "OperType" };
            var psvc = new ProtoService { name = svcName };
            foreach (var item in psps.messages) {
                tableType.Add("_" + item.Name);
            }
            psps.AddEnum(tableType);
            psps.AddEnum(operType);
            psps.AddMessage(_table);
            psps.AddMessage(operateCallback);
            psps.AddMessage(queryCallback);
            psps.AddMessage(queryDescriptor);
            psps.AddMessage(operateDescriptor);
            psps.AddService(psvc);
            operateCallback.AddField(new ProtoField { Name = "IsSuceess", Key = ProtoFieldType._bool });
            operateCallback.AddField(new ProtoField { Name = "EffectRows", Key = ProtoFieldType.int32 });
            operateCallback.AddField(new ProtoField { Name = "Message", Key = ProtoFieldType._string });
            operateCallback.AddField(new ProtoField { Name = "Code", Key = ProtoFieldType._string });
            queryCallback.AddField(new ProtoField { Name = "MixData", Key = ProtoFieldType._string });
            queryCallback.AddField(new ProtoField { Name = "Message", Key = ProtoFieldType._string });
            queryCallback.AddField(new ProtoField { Name = "Source", Key = ProtoFieldType.RefMessage(_table), indicia = ProtoFieldMark.repeated });
            queryCallback.AddField(new ProtoField { Name = "TablePointer", Key = ProtoFieldType.RefEnum(tableType) });
            queryCallback.AddField(new ProtoField { Name = "Code", Key = ProtoFieldType._string });
            queryCallback.AddField(new ProtoField { Name = "IsSuceess", Key = ProtoFieldType._bool });
            operType.Add("Insert");
            operType.Add("Update");
            operType.Add("Delete");
            operType.Add("Other");
            queryDescriptor.AddField(new ProtoField { Name = "EntityType", Key = ProtoFieldType.RefEnum(tableType), indicia = ProtoFieldMark.repeated });
            // QueryInfo设计 基于 System.Linq.Dynamic.Core
            queryDescriptor.AddField(new ProtoField { Name = "QueryInfo", Key = ProtoFieldType._string });
            operateDescriptor.AddField(new ProtoField { Name = "Source", Key = ProtoFieldType.RefMessage(_table), indicia = ProtoFieldMark.repeated });
            operateDescriptor.AddField(new ProtoField { Name = "TablePointer", Key = ProtoFieldType.RefEnum(tableType) });
            // OperInfo设计 基于 System.Linq.Dynamic.Core
            operateDescriptor.AddField(new ProtoField { Name = "OperInfo", Key = ProtoFieldType._string });
            operateDescriptor.AddField(new ProtoField { Name = "OperateType", Key = ProtoFieldType.RefEnum(operType) });

            psvc.AddRpc(new ProtoRpc { name = "Operate", input = operateDescriptor, output = operateCallback });
            psvc.AddRpc(new ProtoRpc { name = "Query", input = queryDescriptor, output = queryCallback });

            psps.imports.Add("data.proto");
            return psps;
        }
        //
        internal static string GenerateMixTable(string sqlstr) {
            sqlstr = ProcessSqlStr(sqlstr);
            var outstr = string.Empty;
            var regfk = new Regex(patternfk);
            var regp = new Regex(patternp);
            var r = new Regex(pattern_ctx);
            var x = r.Matches(sqlstr);
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("create table State_Mix("));
            foreach (Match item in x) {
                var colName = item.Groups["class"].Value;
                foreach (Capture item2 in item.Groups["props"].Captures) {
                    foreach (var item3 in Regex.Split(item2.Value, propsplit)) {
                        if (!string.IsNullOrEmpty(item3)) {
                            if (regp.IsMatch(item3)) {
                                var groups = regp.Match(item3).Groups;
                                var pn = groups["pname"].Value;
                                var pt = groups["type"].Value;
                                var len = groups["suffix"].Value;
                                var pk = groups["pk"].Success;
                                var id = groups["id"].Success;
                                if (!pk && !regfk.IsMatch(item3)) {
                                    sb.AppendLine(string.Format("{0}_{1} {2},", colName, pn, pt));
                                }
                            }
                        }
                    }
                }
            }
            sb.AppendLine(string.Format(")"));
            outstr = sb.ToString();
            return outstr;
        }
        static (bool isGenaricArrayType, Type genaric, Type parameter) AnalysePropertyType(Type pt) {
            if (!pt.IsGenericType) {
                return (false, null, pt);
            } else {
                return (true, pt.GetGenericTypeDefinition(), pt.GetGenericArguments()[0]);
            }
        }
        #endregion
        static CsNamespace currentCsn;
        /// <summary>
        /// 生成各个 biz 项目的 IDbStore 接口
        /// </summary>
        /// <returns></returns>
        public static string GenDbStore(string nsps, bool isOrleans = false) {
            var sps = new CsNamespace();
            var iif = default(CsType);
            if (isOrleans) {
                sps.Using(".,.Threading.Tasks,.Collections.Generic,.Linq.Expressions,Orleans,Util.BizPort");
                iif = sps.StartInterface("IDbStore", inhlist: "IGrainWithIntegerKey");
            } else {
                sps.Using(".,.Threading.Tasks,.Collections.Generic,.Linq.Expressions,Util.BizPort");
                iif = sps.StartInterface("IDbStore");
            }

            foreach (var item in currentCsn.typs) {
                iif.StartMethod($"Operate{item.name}", $"Expression<Predicate<{item.name}>>condition = null,params {item.name}[]  entities", "Task<bool>").isInterface = true;
                iif.StartMethod($"Query{item.name}", $"Expression<Predicate<{item.name}>>condition = null", $"Task<IEnumerable<{item.name}>>").isInterface = true;
                iif.StartMethod($"QueryStream{item.name}", $"Expression<Predicate<{item.name}>>condition = null", $"Task<IAsyncEnumerable<{item.name}>>").isInterface = true;
            }
            iif.StartMethod($"Query", $"string sql", $"Task<DataSource>").isInterface = true;
            iif.StartMethod($"Execute", $"string sql", $"Task<int>").isInterface = true;
            currentCsn = sps;
            return sps.ToString(nsps);
        }
        internal static string GenOrleansWrapper(string entitynsps, string dbc) {
            var nsps = new CsNamespace();
            nsps.Using(".,.Linq,.Linq.Expressions,.Threading.Tasks,.Collections.Generic,Orleans,Util.BizPort");
            var idbs = currentCsn.typs[0];
            var dbs = nsps.StartClass("DbStoreOrleans", inhlist: "Grain,IDbStore,IGrainWithIntegerKey");
            dbs.Field("dbc", dbc, "private readonly");
            dbs.StartConstructor($"{dbc} dbc")
                .Sentence($"this.dbc = dbc");
            foreach (var item in idbs.methods) {
                dbs.StartMethod(item.name, item.paramlist, $"async {item.typ}")
                    .Sentence("throw new NotImplementedException()");
            }
            currentCsn = nsps;
            return nsps.ToString(entitynsps);
        }
        internal static string GenGrpcWrapper(string entitynsps, string dbc) {
            var nsps = new CsNamespace();
            nsps.Using(".,.Linq,.Linq.Expressions,.Threading.Tasks,.Collections.Generic,Util.BizPort");
            var idbs = currentCsn.typs[0];
            var dbs = nsps.StartClass("DbStoreGrpc", inhlist: "IDbStore");
            dbs.Field("dbc", dbc, "private readonly");
            dbs.StartConstructor($"{dbc} dbc")
                .Sentence($"this.dbc = dbc");
            foreach (var item in idbs.methods) {
                dbs.StartMethod(item.name, item.paramlist, $"{item.typ}")
                    .Sentence("throw new NotImplementedException()");
            }
            currentCsn = nsps;
            return nsps.ToString(entitynsps);
        }
        [Obsolete]
        internal static string GenGrpcProto(string assemblyName, string nugetPkgDir) {
            var assemblyDir = System.IO.Path.Combine(
                nugetPkgDir,
                assemblyName.ToLower()
            );
            var assemblyPath = System.IO.Directory.GetFiles(assemblyDir, "*.dll", System.IO.SearchOption.AllDirectories)[0];
            var x = Assembly.LoadFile(assemblyPath);
            var r = ProtoUtil.GenGrpcProtoByInterface(x.GetType($"{assemblyName}.IDbStore"));
            return r.ToString();
        }
        internal static string GenLcInsert(string sqlstr, string dbc, string nps = null, string nps_entity = null) {
            sqlstr = ProcessSqlStr(sqlstr);
            var lcinserts = Regex.Matches(sqlstr, lcinsert);
            var sps = new CsNamespace();
            sps.Using(".", "Microsoft.EntityFrameworkCore", "Microsoft.EntityFrameworkCore.Metadata.Builders,.Linq,Microsoft.Extensions.Configuration", nps_entity);
            var cls = sps.StartClass($"Lc{dbc}", visit: "static public");
            var group = from i in lcinserts group i by i.Groups[1].Value;
            foreach (var item in group) {
                var m = cls.StartMethod($"Insert_{item.Key}", $"Db{dbc} ctx", "void", "static public");
                m.Sentence($"ctx.{item.Key}.RemoveRange(ctx.{item.Key})");
                foreach (var it in item) {
                    var cols = it.Groups[2].Value.Split(",");
                    var vals = it.Groups[3].Value.Split(",");
                    var iu = new List<string>();
                    for (int i = 0; i < cols.Length; i++) {
                        iu.Add($"{cols[i]} = {vals[i]}");
                    }
                    m.Sentence($"ctx.{item.Key}.Add(new {item.Key}{{{string.Join(",", iu).Replace("'", "\"")}}})");
                }
            }
            currentCsn = sps;
            return sps.ToString(nps);
        }
        internal static string GenBizHandler(string fn, string entitynsps) {
            var sps = new CsNamespace();
            sps.Using(".,.Collections.Generic,.Threading.Tasks");
            var t = sps.StartClass($"{fn}Handler");
            var f1 = t.StartField("storeOrleans", "IDbStore");
            t.Property(f1);
            t.StartMethod("TestConnect", "", "async Task<dynamic>")
                .Sentence("throw new NotImplementedException ()");
            return sps.ToString(entitynsps);
        }
    }
}
