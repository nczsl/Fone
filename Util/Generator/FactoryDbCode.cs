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
        static Type SqlType2NetType(string sqlType, bool isnull = false) => sqlType
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
        static string pattern_ent = $@"(?<=create\s+table\s+)(?<class>{Classall})\((?<props>(([^\(\)])|(?<open>\()|(?<-open>\)))*)(?(open)(?!))\)";
        static string pattern_ctx = $@"(?<=create\s+table\s+)(?<class>{Classent})\((?<props>(([^\(\)])|(?<open>\()|(?<-open>\)))*)(?(open)(?!))\)";
        static readonly string patternp = @"\[?(?<pname>\w+)\]?\s+\[?(?<type>(int|tinyint|varchar|bit|datetime|date|decimal|float|image|money|ntext|nvarchar|smalldatetime|smallint|text|bigint|binary|char|nchar|numeric|real|smallmoney|sql_variant|timestamp|uniqueidentifier|varbinary|xml)){1}\]?(?<suffix>\((\d+|max|MAX|Max|\d+\s?,\s?\d+)\))?(?<pk>\s+primary\s+key)?(\s+default\((?<default>\w+)\)\s{0,5})?(?!.*references)\s?";
        static readonly string patternfk = @"\[?(?<pname>\w+)\]?\s+\[?(?<type>(int|tinyint|varchar|bit|datetime|date|decimal|float|image|money|ntext|nvarchar|smalldatetime|smallint|text|bigint|binary|char|nchar|numeric|real|smallmoney|sql_variant|timestamp|uniqueidentifier|varbinary|xml)){1}\]?(?<suffx>\((\d+|max|MAX|Max|\d+\s?,\s?\d+)\))?(?<pk>\s+primary\s+key)?(\s+references\s+)(?<table>\w+_?\w+)\s*\(\w+\)";
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
        internal static string GenerateCtx620(string namesps, string clsStr, IEnumerable<Type> typs) {
            var sps = new CsNamespace();
            sps.Using(".", ".Collections.Generic", ".IO", ".Linq", ".Text", ".Threading.Tasks", ".Data.Entity", ".Data.Entity.ModelConfiguration");
            var cls = sps.StartClass(clsStr, inhlist: "DbContext");
            cls.StartConstructor("", string.Format("base(\"name={0}\")", clsStr));
            cls.StartConstructor("string constrName", "base(constrName)")
                .Sentence("this.Database.CreateIfNotExists()");
            var cls_onmodelcreating = cls.StartMethod("OnModelCreating", "DbModelBuilder mb", "void", "protected override");
            foreach (var item in typs) {
                var typ = default(CsType);
                var typcon = default(CsMethod);
                var className = "";
                className = item.Name;
                cls.Property(item.Name, string.Format("DbSet<{0}>", className));
                cls_onmodelcreating.Sentence($"mb.Configurations.Add(new {className}_Config());");
                typ = sps.StartClass($"{item.Name}_Config", "internal");
                typ.SetInherit($"EntityTypeConfiguration<{className}>");
                typcon = typ.StartConstructor("string schema=\"\"");
                typcon.Sentence($"ToTable(\"{className}\");");
                foreach (var it in item.GetProperties()) {
                    //看类型判断是不是外键
                    var pt = AnalysePropertyType(it.PropertyType);
                    var no = 0;
                    var columnType = "$columnType$";
                    if (pt.isGenaricArrayType) {
                        //判断是否是集合类型,说明是外建字段
                        if (pt.genaric.GetInterfaces().Contains(typeof(IEnumerable)) && typs.Contains(pt.parameter)) {
                            //外键分为，一对一，一对多，两种情况
                            //一对一，又分为，主键引用，和唯一键引用两种情况
                            //一对多
                            no = -1;
                        }
                    } else {
                        if (it.PropertyType.IsPrimaryType()) {
                            no = SwitchTypeByName(it.Name);
                        } else if (typs.Contains(it.PropertyType)) {
                            //一对一
                            no = -2;
                        }
                    }

                    if (it.PropertyType.IsPrimaryType())
                        columnType = NetType2SqlType(it.PropertyType);
                    var xlen = string.Empty;
                    var len = "100";
                    switch (no) {
                        case 1:
                            typcon.Sentence($"HasKey(x => x.{it.Name});");
                            typcon.Sentence($"Property(x => x.{it.Name}).HasColumnName(\"{it.Name}\").HasColumnType(\"{columnType}\").IsRequired();");
                            break;
                        case -1:
                            typcon.Sentence($"HasMany(x => x.{it.Name}).WithRequired();");
                            break;
                        case -2:
                            typcon.Sentence($"HasOptional(x => x.{it.Name}).WithRequired()");
                            break;
                        case 2:
                            //columnType = columnType.Replace("(100)", "(20)");
                            //typcon.Sentence($"Property(x => x.{it.Name}).HasColumnName(\"{it.Name}\").HasColumnType(\"{columnType}\").HasMaxLength(20).IsOptional();");
                            len = "20";
                            goto case 8;
                        case 3:
                            //columnType = columnType.Replace("varchar(100)", "varchar(max)");                        
                            xlen = $".IsMaxLength()";
                            typcon.Sentence($"Property(x => x.{it.Name}).HasColumnName(\"{it.Name}\").HasColumnType(\"{columnType}\"){xlen}.IsOptional();");
                            break;
                        case 4:
                        case 5:
                        case 6:
                            //typcon.Sentence($"Property(x => x.{it.Name}).HasColumnName(\"{it.Name}\").HasColumnType(\"{columnType}\").IsOptional();");
                            goto default;
                        case 7:
                            //typcon.Sentence($"Property(x => x.{it.Name}).HasColumnName(\"{it.Name}\").HasColumnType(\"{columnType}\").IsRequired();");
                            goto default;
                        case 8:
                            columnType = columnType.Replace("varchar", "nvarchar");
                            goto default;
                        case 9:
                            typcon.Sentence($"Ignore(x=>x.{it.Name})");
                            break;
                        case 10:
                            len = "500";
                            goto case 8;
                        default:
                            switch (it.PropertyType) {
                                case Type t when t == typeof(string):
                                    xlen = $".HasMaxLength({len})";
                                    break;
                                case Type t when t == typeof(byte[]):
                                    xlen = $".HasMaxLength({len})";
                                    break;
                                case Type t when t == typeof(DateTime) || t == typeof(TimeSpan) || t == typeof(DateTime?) || t == typeof(TimeSpan?):
                                    len = "8";
                                    xlen = $".HasPrecision({len})";
                                    break;
                                case Type t when t == typeof(decimal) || t == typeof(decimal?):
                                    len = "18,2";
                                    xlen = $".HasPrecision({len})";
                                    break;
                            }
                            typcon.Sentence($"Property(x => x.{it.Name}).HasColumnName(\"{it.Name}\").HasColumnType(\"{columnType}\"){xlen}.IsOptional();");
                            break;
                    }
                }
            }
            cls_onmodelcreating.Sentence($"base.OnModelCreating(mb);");
            cls_onmodelcreating.EndMethod();
            var outstr = sps.ToString(string.Format("{0}", namesps));
            return outstr;
        }
        /// <summary>
        /// 没有配置sql数据类型，因为考虑到需要适配到不同的数据库，那么数据类型会不一致
        /// </summary>
        /// <param name="ctxName"></param>
        /// <param name="entityNamespace"></param>
        /// <param name="genNamespace"></param>
        /// <param name="typs"></param>
        /// <returns></returns>
        public static string GenerateCtxCore2x(string ctxName, string entityNamespace, string genNamespace, IEnumerable<Type> typs) {
            var sps = new CsNamespace();
            sps.Using(".", ".Collections.Generic", ".IO", ".Linq", ".Text", ".Threading.Tasks", "Microsoft.EntityFrameworkCore", entityNamespace);
            var cls = sps.StartClass($"Db{ctxName}", inhlist: "DbContext");
            cls.StartConstructor("")
                .Sentence("//todo entity framework core 2.x.x initial code");
            cls.StartMethod("OnConfiguring", "DbContextOptionsBuilder optionsBuilder", "void", "protected override")
                .Sentence("//optionsBuilder.UseSqlServer(\"db_connection_string\");")
                .Sentence("base.OnConfiguring(optionsBuilder)");
            var omc = cls.StartMethod("OnModelCreating", "ModelBuilder mb", "void", "protected override");

            var typefield = string.Empty;
            foreach (var item in typs) {
                var className = "";
                className = item.Name;
                cls.Property(item.Name, string.Format("DbSet<{0}>", className));
                typefield = string.Format("_{0}", className.ToLower());
                omc.Sentence("var {0} = mb.Entity<{1}>();", typefield, className);
                var props = item.GetProperties();
                var fkname = "";
                var fkname2 = "";
                foreach (var it in props) {
                    /**看类型判断是不是外键
                     * 外键有命名约定 找不到报错，须要 自己去改实体
                     * 外键名=外键表名，或 外键表名+Id
                     * 外键分为，一对一，一对多，两种情况
                     *一对一，又分为，主键引用，和唯一键引用两种情况
                     * 一对多
                     * */
                    var pt = AnalysePropertyType(it.PropertyType);
                    var no = 0;
                    var clrtype = "";
                    if (pt.isGenaricArrayType) {
                        //判断是否是集合类型,说明是外建字段
                        if (pt.genaric.GetInterfaces().Contains(typeof(IEnumerable)) && typs.Contains(pt.parameter)) {
                            //找到集合实体键    
                            no = -1;
                        }
                    } else {
                        if (it.PropertyType.IsPrimaryType()) {
                            no = SwitchTypeByName(it.Name);
                            //clrtype = string.Format("typeof({0})", NetType2SqlType(it.PropertyType.Name));
                            clrtype = $"typeof({it.PropertyType.Name})";
                        } else if (typs.Contains(it.PropertyType)) {
                            //找到单一实体键
                            no = -2;
                        }
                    }
                    var xlen = string.Empty;
                    var len = "100";
                    switch (no) {
                        case 1:
                            omc.Sentence($"{typefield}.HasKey(x => x.{it.Name});");
                            omc.Sentence($"{typefield}.Property(x => x.{it.Name}).HasColumnName(\"{it.Name}\").IsRequired();");
                            break;
                        case -1:
                            //omc.Sentence($"{typefield}.HasOne(x => x.{it.Name}Pk).WithMany(x => x.{className}).HasForeignKey(x => x.{it.Name});");
                            //在集合导致属性表里不存在外键，只需要 配置即可，但需要 查找目标表的外键
                            //确定导航属性的目标实体中，有没有对应本表的导航键，只能是单一，而非集合否则 报错
                            var tlist = AnalysePropertyType(it.PropertyType).parameter.GetProperties().ToList();
                            var tk = from i in tlist
                                     where i.PropertyType == it.DeclaringType
                                     select i;

                            if (tk.Count() > 0) {
                                omc.Sentence($"{typefield}.HasMany(x => x.{it.Name}).WithOne(x => x.{tk.First().Name});");
                            } else {
                                omc.Sentence($"{typefield}.HasMany(x => x.{it.Name}).WithOne();");
                            }
                            break;
                        case -2:
                            //omc.Sentence($"{typefield}.HasOne(x => x.{it.Name}).WithOne(x => x.{className}_{it.Name}).HasForeignKey(\"{it.Name}\")");
                            //确定有没有外键，有才写导航属性，没有忽略 
                            //确定 是一对一，还是一对多，以hasone 开头 withone 或withmany 对应
                            fkname = $"{it.PropertyType.Name.Split('_').Last()}";
                            fkname2 = $"{it.PropertyType.Name.Split('_').Last()}Id";
                            var tlist2 = it.PropertyType.GetProperties().ToList();
                            var tk2 = from i in tlist2 where i.PropertyType == it.DeclaringType select i;
                            var tk3 = from i in tlist2
                                      let tkk = AnalysePropertyType(i.PropertyType)
                                      where tkk.isGenaricArrayType &&
                                          tkk.parameter == it.DeclaringType
                                      select i;
                            if (props.Select(i => i.Name).Contains(fkname)) {
                                //判断外键表里有没有对应的单一建
                                if (tk2.Count() > 0) {
                                    omc.Sentence($"{typefield}.HasOne(x => x.{it.Name}).WithOne(x => x.{tk2.First().Name}).HasForeignKey(x => x.{fkname});");
                                } else if (tk3.Count() > 0) {
                                    omc.Sentence($"{typefield}.HasOne(x => x.{it.Name}).WithMany(x=>x.{tk3.First().Name}).HasForeignKey(x => x.{fkname});");
                                } else {
                                    omc.Sentence($"{typefield}.HasOne(x => x.{it.Name}).WithMany().HasForeignKey(x => x.{fkname});");
                                }
                            } else if (props.Select(i => i.Name).Contains(fkname2)) {
                                if (tk2.Count() > 0) {
                                    omc.Sentence($"{typefield}.HasOne(x => x.{it.Name}).WithOne(x => x.{tk2.First().Name}).HasForeignKey(x => x.{fkname2});");
                                } else if (tk3.Count() > 0) {
                                    omc.Sentence($"{typefield}.HasOne(x => x.{it.Name}).WithMany(x=>x.{tk3.First().Name}).HasForeignKey(x => x.{fkname2});");
                                } else {
                                    omc.Sentence($"{typefield}.HasOne(x => x.{it.Name}).WithMany().HasForeignKey(x => x.{fkname2});");
                                }
                            } else {
                                //直接 用主键做连接
                                if (tk2.Count() > 0) {
                                    omc.Sentence($"{typefield}.HasOne(x => x.{it.Name}).WithOne(x => x.{tk2.First().Name}).HasForeignKey(x => x.Id);");
                                }
                            }
                            break;
                        case 2:
                            //columnType = columnType.Replace("(100)", "(20)");
                            //typcon.Sentence($"Property(x => x.{it.Name}).HasColumnName(\"{it.Name}\").HasColumnType(\"{columnType}\").HasMaxLength(20).IsOptional();");
                            len = "20";
                            goto case 8;
                        case 3:
                            //columnType = columnType.Replace("varchar(100)", "varchar(max)");                        
                            xlen = $"max";
                            omc.Sentence($"{typefield}.Property(x => x.{it.Name}).HasColumnName(\"{it.Name}\").HasMaxLength(\"{xlen}\").IsRequired(false);");
                            break;
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                            goto default;
                        case 9:
                            omc.Sentence($"{typefield}.Ignore(x=>x.{it.Name})");
                            break;
                        case 10:
                            len = "500";
                            goto case 8;
                        case 11:
                            len = "2000";
                            goto case 8;
                        default:
                            switch (it.PropertyType) {
                                case Type t when t == typeof(string):
                                    xlen = $".HasMaxLength({len})";
                                    break;
                                case Type t when t == typeof(byte[]):
                                    xlen = $".HasMaxLength({len})";
                                    break;
                                case Type t when t == typeof(DateTime) || t == typeof(TimeSpan) || t == typeof(DateTime?) || t == typeof(TimeSpan?):
                                    len = "8";
                                    xlen = $".HasMaxLength({len})";
                                    //xlen = $".HasPrecision({len})";
                                    break;
                                case Type t when t == typeof(decimal) || t == typeof(decimal?):
                                    len = "18,2";
                                    xlen = $".HasMaxLength({len})";
                                    //xlen = $".HasPrecision({len})";
                                    break;
                            }
                            omc.Sentence($"{typefield}.Property(x => x.{it.Name}).HasColumnName(\"{it.Name}\"){xlen}.IsRequired(false);");
                            break;
                    }
                }
            }
            omc.Sentence($"base.OnModelCreating(mb);");
            omc.EndMethod();
            var outstr = sps.ToString(string.Format("{0}", genNamespace));
            return outstr;
        }
        public static string GenerateCtxCore3x(string ctxName, string entityNamespace, string genNamespace, IEnumerable<Type> typs) {
            var sps = new CsNamespace();
            sps.Using(".", ".Collections.Generic", ".IO", ".Linq", ".Text", ".Threading.Tasks", "Microsoft.EntityFrameworkCore", entityNamespace);
            var cls = sps.StartClass(ctxName, inhlist: "DbContext");
            cls.Field("dbcon", "string");
            cls.Field("config", "IConfiguration", visit: "private readonly");
            cls.Field("logger", $"ILogger<{ctxName}>", visit: "private readonly");
            cls.StartConstructor("")
                .Sentence("//todo entity framework core 3.x.x initial code");
            cls.StartConstructor("IConfiguration config, ILogger<DbLearnManage> logger=null")
                .Sentence("//todo entity framework core 3.x.x initial code")
                .Sentence("this.config = config")
                .Sentence("this.logger = logger")
                .Sentence("//this.dbcon = config[\"dbcon:xxx\"]");
            cls.StartMethod("OnConfiguring", "DbContextOptionsBuilder optionsBuilder", "void", "protected override")
                .Sentence("//optionsBuilder.UseSqlServer(\"db_connection_string\");")
                .Sentence("base.OnConfiguring(optionsBuilder)");
            var omc = cls.StartMethod("OnModelCreating", "ModelBuilder mb", "void", "protected override");

            var typefield = string.Empty;
            foreach (var item in typs) {
                var className = "";
                className = item.Name;
                cls.Property(item.Name, string.Format("DbSet<{0}>", className));
                typefield = string.Format("_{0}", className.ToLower());
                omc.Sentence("var {0} = mb.Entity<{1}>();", typefield, className);
                var props = item.GetProperties();
                var fkname = "";
                var fkname2 = "";
                foreach (var it in props) {
                    /**看类型判断是不是外键
                     * 外键有命名约定 找不到报错，须要 自己去改实体
                     * 外键名=外键表名，或 外键表名+Id
                     * 外键分为，一对一，一对多，两种情况
                     *一对一，又分为，主键引用，和唯一键引用两种情况
                     * 一对多
                     * */
                    var pt = AnalysePropertyType(it.PropertyType);
                    var no = 0;
                    var clrtype = "";
                    if (pt.isGenaricArrayType) {
                        //判断是否是集合类型,说明是外建字段
                        if (pt.genaric.GetInterfaces().Contains(typeof(IEnumerable)) && typs.Contains(pt.parameter)) {
                            //找到集合实体键    
                            no = -1;
                        }
                    } else {
                        if (it.PropertyType.IsPrimaryType()) {
                            no = SwitchTypeByName(it.Name);
                            //clrtype = string.Format("typeof({0})", NetType2SqlType(it.PropertyType.Name));
                            clrtype = $"typeof({it.PropertyType.Name})";
                        } else if (typs.Contains(it.PropertyType)) {
                            //找到单一实体键
                            no = -2;
                        }
                    }
                    var xlen = string.Empty;
                    var len = "100";
                    switch (no) {
                        case 1:
                            omc.Sentence($"{typefield}.HasKey(x => x.{it.Name});");
                            omc.Sentence($"{typefield}.Property(x => x.{it.Name}).IsRequired();");
                            break;
                        case -1:
                            //omc.Sentence($"{typefield}.HasOne(x => x.{it.Name}Pk).WithMany(x => x.{className}).HasForeignKey(x => x.{it.Name});");
                            //在集合导致属性表里不存在外键，只需要 配置即可，但需要 查找目标表的外键
                            //确定导航属性的目标实体中，有没有对应本表的导航键，只能是单一，而非集合否则 报错
                            var tlist = AnalysePropertyType(it.PropertyType).parameter.GetProperties().ToList();
                            var tk = from i in tlist
                                     where i.PropertyType == it.DeclaringType
                                     select i;

                            if (tk.Count() > 0) {
                                omc.Sentence($"{typefield}.HasMany(x => x.{it.Name}).WithOne(x => x.{tk.First().Name});");
                            } else {
                                omc.Sentence($"{typefield}.HasMany(x => x.{it.Name}).WithOne();");
                            }
                            break;
                        case -2:
                            //omc.Sentence($"{typefield}.HasOne(x => x.{it.Name}).WithOne(x => x.{className}_{it.Name}).HasForeignKey(\"{it.Name}\")");
                            //确定有没有外键，有才写导航属性，没有忽略 
                            //确定 是一对一，还是一对多，以hasone 开头 withone 或withmany 对应
                            fkname = $"{it.PropertyType.Name.Split('_').Last()}";
                            fkname2 = $"{it.PropertyType.Name.Split('_').Last()}Id";
                            var tlist2 = it.PropertyType.GetProperties().ToList();
                            var tk2 = from i in tlist2 where i.PropertyType == it.DeclaringType select i;
                            var tk3 = from i in tlist2
                                      let tkk = AnalysePropertyType(i.PropertyType)
                                      where tkk.isGenaricArrayType &&
                                          tkk.parameter == it.DeclaringType
                                      select i;
                            if (props.Select(i => i.Name).Contains(fkname)) {
                                //判断外键表里有没有对应的单一建
                                if (tk2.Count() > 0) {
                                    omc.Sentence($"{typefield}.HasOne(x => x.{it.Name}).WithOne(x => x.{tk2.First().Name}).HasForeignKey(x => x.{fkname});");
                                } else if (tk3.Count() > 0) {
                                    omc.Sentence($"{typefield}.HasOne(x => x.{it.Name}).WithMany(x=>x.{tk3.First().Name}).HasForeignKey(x => x.{fkname});");
                                } else {
                                    omc.Sentence($"{typefield}.HasOne(x => x.{it.Name}).WithMany().HasForeignKey(x => x.{fkname});");
                                }
                            } else if (props.Select(i => i.Name).Contains(fkname2)) {
                                if (tk2.Count() > 0) {
                                    omc.Sentence($"{typefield}.HasOne(x => x.{it.Name}).WithOne(x => x.{tk2.First().Name}).HasForeignKey(x => x.{fkname2});");
                                } else if (tk3.Count() > 0) {
                                    omc.Sentence($"{typefield}.HasOne(x => x.{it.Name}).WithMany(x=>x.{tk3.First().Name}).HasForeignKey(x => x.{fkname2});");
                                } else {
                                    omc.Sentence($"{typefield}.HasOne(x => x.{it.Name}).WithMany().HasForeignKey(x => x.{fkname2});");
                                }
                            } else {
                                //直接 用主键做连接
                                if (tk2.Count() > 0) {
                                    omc.Sentence($"{typefield}.HasOne(x => x.{it.Name}).WithOne(x => x.{tk2.First().Name}).HasForeignKey(x => x.Id);");
                                }
                            }
                            break;
                        case 2:
                            //columnType = columnType.Replace("(100)", "(20)");
                            //typcon.Sentence($"Property(x => x.{it.Name}).HasField(\"{it.Name}\").HasColumnType(\"{columnType}\").HasMaxLength(20).IsOptional();");
                            len = "20";
                            goto case 8;
                        case 3:
                            //columnType = columnType.Replace("varchar(100)", "varchar(max)");                        
                            xlen = $"max";
                            omc.Sentence($"{typefield}.Property(x => x.{it.Name}).HasMaxLength(\"{xlen}\").IsRequired(false);");
                            break;
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                            goto default;
                        case 9:
                            omc.Sentence($"{typefield}.Ignore(x=>x.{it.Name})");
                            break;
                        case 10:
                            len = "500";
                            goto default;
                        case 11:
                            len = "2000";
                            goto default;
                        case 12:
                            len = "50";
                            goto default;
                        default:
                            switch (it.PropertyType) {
                                case Type t when t == typeof(string):
                                    xlen = $".HasMaxLength({len})";
                                    break;
                                case Type t when t == typeof(byte[]):
                                    xlen = $".HasMaxLength({len})";
                                    break;
                                case Type t when t == typeof(DateTime) || t == typeof(TimeSpan) || t == typeof(DateTime?) || t == typeof(TimeSpan?):
                                    len = "8";
                                    xlen = $".HasMaxLength({len})";
                                    //xlen = $".HasPrecision({len})";
                                    break;
                                case Type t when t == typeof(decimal) || t == typeof(decimal?):
                                    len = "18,2";
                                    xlen = $".HasMaxLength({len})";
                                    //xlen = $".HasPrecision({len})";
                                    break;
                            }
                            omc.Sentence($"{typefield}.Property(x => x.{it.Name}).IsRequired(false);");
                            break;
                    }
                }
            }
            omc.Sentence($"base.OnModelCreating(mb);");
            omc.EndMethod();
            var outstr = sps.ToString(string.Format("{0}", genNamespace));
            return outstr;
        }
        static int SwitchTypeByName(string name) {
            var r = 0;
            var key = name.ToLower().Trim();
            switch (key) {
                case string _ when key == "id":
                    r = 1;
                    break;
                case string _ when key.Contains("_key"):
                    r = 1;
                    break;
                case string _ when key.Contains("_pk"):
                    r = 1;
                    break;
                case string _ when key.Contains("name"):
                    r = 2;
                    break;
                case string _ when key.Contains("qq"):
                    r = 2;
                    break;
                case string _ when key.Contains("phone"):
                    r = 2;
                    break;
                case string _ when key.Contains("text"):
                    r = 3;
                    break;
                case string _ when key.Contains("article"):
                    r = 3;
                    break;
                case string _ when key.Contains("_binary"):
                    r = 3;
                    break;
                case string _ when key.Contains("vedio"):
                    r = 3;
                    break;
                case string _ when key.Contains("audio"):
                    r = 3;
                    break;
                case string _ when key.Contains("_no"):
                    r = 4;
                    break;
                case string _ when key.Contains("number"):
                    r = 4;
                    break;
                case string _ when key.Contains("num"):
                    r = 4;
                    break;
                case string _ when key.Contains("sex"):
                    r = 5;
                    break;
                case string _ when key.Contains("age"):
                    r = 4;
                    break;
                case string _ when key.Contains("price"):
                    r = 6;
                    break;
                case string _ when key.Contains("value"):
                    r = 6;
                    break;
                case string _ when key.Contains("_uk"):
                    r = 7;
                    break;
                case string _ when key.Contains("ukey"):
                    r = 7;
                    break;
                case string _ when key.Contains("_cn"):
                    r = 8;
                    break;
                case string _ when key.Contains("_nor"):
                    r = 9;
                    break;
                case string _ when key.Contains("_ig"):
                    r = 9;
                    break;
                case string _ when key.Contains("_ignore"):
                    r = 9;
                    break;
                case string _ when key.Contains("summary"):
                    r = 10;
                    break;
                case string _ when key.Contains("describe"):
                    r = 10;
                    break;
                case string _ when key.Contains("description"):
                    r = 10;
                    break;
                //case string i when key.Contains("note"): r = 10; break;
                case string _ when key.Contains("content"):
                    r = 11;
                    break;
                case string _ when key.Contains("email"):
                    r = 12;
                    break;
            }
            return r;
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
