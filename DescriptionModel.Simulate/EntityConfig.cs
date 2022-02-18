using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using DescriptionModel.Simulate.JsonSource;
using System.ComponentModel;

namespace DescriptionModel.Simulate {
    /// <summary>
    /// 配置实体的仿真清单
    /// </summary>
    public class EntityConfig<Poco> where Poco : new() {
        public Random ran;
        private bmodel bm;
        private int firstlen;
        private int lastlen;
        private int phonelen;
        private int lastmanlen;
        private int lastwomanlen;
        private int lastneutrallen;
        private int lastnelen;
        private string[] keyboard;
        private int kblen;
        private int productfuncpartLen;
        private int productsuffixLen;
        private int departmentLen;
        private int companypostsrankLen;
        DateTime now;
        public EntityConfig() {
            this.configs = new List<PropConfig>();
            this.ran = new Random();
            //
            bm = JsonConvert.DeserializeObject<bmodel>(Properties.Resources.basedata);
            firstlen = this.bm.name.first.Length;
            lastlen = this.bm.name.last.Length;
            phonelen = this.bm.phone.profix.Length;
            lastmanlen = bm.name.lastman.Length;
            lastwomanlen = bm.name.lastwoman.Length;
            lastneutrallen = bm.name.lastneutral.Length;
            lastnelen = bm.name.lastneutral.Length;
            keyboard = bm.letter.en_bigchar.Union(bm.letter.en_smallchar.Union(bm.letter.num.Union(bm.letter.symbol))).ToArray();
            kblen = this.keyboard.Length;
            productfuncpartLen = this.bm.product.productfuncpart.Length;
            productsuffixLen = this.bm.product.productsuffix.Length;
            departmentLen = bm.department.Length;
            companypostsrankLen = bm.companypostsrank.Length;
            companylen = bm.company.Length;
            industrylen = bm.industry.Length;
            now = DateTime.Now;
        }
        List<PropConfig> configs;
        private int companylen;
        int industrylen;
        /// <summary>
        /// 设置仿真字段
        /// </summary>
        /// <param name="propName">属性名称</param>
        /// <param name="type">仿真类型</param>
        /// <returns>仿真配置</returns>
        public PropConfig Set(string propName, FieldType type) {
            var pcon = propName.GetConfig(type);
            this.configs.Add(pcon);
            return pcon;
        }
        /// <summary>
        /// 设置仿真字段
        /// </summary>
        /// <param name="propName">属性名称</param>
        /// <param name="dependency">仿真值逻辑</param>
        /// <param name="dependencyObj">仿真值依赖的其它属性值</param>
        /// <param name="type">仿真类型</param>
        /// <returns>仿真配置</returns>
        public PropConfig Set(string propName, Func<dynamic> dependency, PropConfig dependencyObj = null) {
            var pcon = propName.GetConfig(FieldType.none);
            pcon.dependency = dependency;
            pcon.dependencyObj = dependencyObj;
            this.configs.Add(pcon);
            return pcon;
        }
        /// <summary>
        /// 设置仿真字段
        /// </summary>
        /// <param name="propName">属性名称</param>
        /// <param name="type">仿真类型</param>
        /// <returns>仿真配置</returns>
        public PropConfig Set(string propName, int min, int max, bool isdigital) {
            var pcon = propName.GetConfig(min, max, isdigital);
            this.configs.Add(pcon);
            return pcon;
        }
        //
        string CreateDeviceName() {
            return $"{bm.product.productfuncpart[productfuncpartLen]}{bm.product.productsuffix[productsuffixLen]}";
        }
        string CreateCode() {
            var enname = string.Empty;
            var len = ran.Next(3, 12);
            var source = string.Concat(bm.letter.en_bigchar, bm.letter.en_smallchar, bm.letter.num);
            var list = new List<char>();
            var len3 = source.Length;
            for (int i = 0; i < len; i++) {
                var point = ran.Next(len3);
                list.Add(source[point]);
            }
            var split = ran.Next(1, list.Count);
            var pp = ran.Next(100) % 10;
            switch (pp) {
                case int i when i < 3: list.Insert(split, '_'); break;
                case int i when i >= 3 && i < 6: list.Insert(split, '-'); break;
                case int i when i >= 6: break;
            }
            enname = new string(list.ToArray());
            return enname;
        }
        public string CreateWebsiteAddress() {
            var protocal = "";
            var domain = "{0}";
            var path = "";
            var probability_protocal = ran.Next(100) % 10 == 0;
            if (probability_protocal)
                protocal = "https";
            else
                protocal = "http";
            var probability_domain1 = ran.Next(100) % 10 == 0;
            if (!probability_domain1)
                domain = "www.{0}.com";
            domain = string.Format(domain, CreateEnName());
            var switch_path = ran.Next(5);
            switch (switch_path) {
                case 0: path = "index.html"; break;
                case 1: path = "home"; break;
                case 2: path = ""; break;
                case 3: path = "default"; break;
                case 4: path = ""; break;
            }
            return $"{protocal}://{domain}/{path}";
        }
        string CreateEnName() {
            var enname = string.Empty;
            var len = ran.Next(3, 12);
            var list = new List<string>();
            var len3 = bm.letter.en_smallchar.Length;
            for (int i = 0; i < len; i++) {
                var point = ran.Next(len3);
                list.Add(bm.letter.en_smallchar[point]);
            }
            enname = string.Concat(list);
            return enname;
        }
        public string Telphone => $"{ran.Next(900).ToString("D3")}-{ran.Next(100000000).ToString("D8")}";
        int idx;
        public Poco[] Generate(int count) {
            var list = new List<Poco>();
            for (int k = 0; k < count; k++) {
                var temp = new Poco();
                foreach (var item in from i in configs join j in typeof(Poco).GetProperties() on i.propName equals j.Name select new { i, j }) {
                    var pv = default(dynamic);
                    if (item.i.dependency != null) {
                        pv = item.i.dependency();
                    } else {
                        switch (item.i.type) {
                            case FieldType.none:
                                break;
                            case FieldType.product:
                                var p1 = bm.product.productfuncpart[ran.Next(this.productfuncpartLen)];
                                var p2 = bm.product.productsuffix[ran.Next(this.productsuffixLen)];
                                var p3 = CreateCode();
                                pv = $"{p1}{p2}{p3}";
                                break;
                            case FieldType.departmentName:
                                pv = this.bm.department[ran.Next(this.departmentLen)].Split(':', StringSplitOptions.RemoveEmptyEntries)[0];
                                break;
                            case FieldType.departmentSummary:
                                pv = this.bm.department[ran.Next(this.departmentLen)].Split(':', StringSplitOptions.RemoveEmptyEntries)[1];
                                break;
                            case FieldType.industry:
                                pv = bm.industry[ran.Next(industrylen)];
                                break;
                            case FieldType.attractions:
                                pv = bm.attractions[ran.Next(bm.attractions.Length)];
                                break;
                            case FieldType.fruit:
                                pv = bm.fruit[ran.Next(bm.fruit.Length)];
                                break;
                            case FieldType.foodmenu:
                                pv = bm.foodmenu[ran.Next(bm.foodmenu.Length)];
                                break;
                            case FieldType.software:
                                pv = bm.software[ran.Next(bm.software.Length)];
                                break;
                            case FieldType.wildlife:
                                pv = bm.wildlife[ran.Next(bm.wildlife.Length)];
                                break;
                            case FieldType.occupation:
                                pv = bm.occupation[ran.Next(bm.occupation.Length)];
                                break;
                            case FieldType.phone:
                                pv = $"{this.bm.phone.profix[this.ran.Next(phonelen)]}{this.ran.Next(1000, 10000)}{this.ran.Next(1000, 10000)}";
                                break;
                            case FieldType.telphone:
                                pv = Telphone;
                                break;
                            case FieldType.identityCode:
                                var verifycodestr = ran.Next(11);
                                var verifycode = verifycodestr % 11 == 10 ? "X" : verifycodestr.ToString();
                                pv = $"{ran.Next(110000, 659001)}{ran.Next(DateTime.Now.Year - 90, DateTime.Now.Year)}{ran.Next(1, 12).ToString("D2")}{ran.Next(1, 31).ToString("D2")}{ran.Next(1000).ToString("D3")}{verifycode}";
                                break;
                            case FieldType.longitude:
                                // 经度0°——180°（东行,标注E）0°——180°（西行,标注W）
                                //纬度0°——90°N、0°——90°S
                                pv = (double)ran.Next(-180, 179) + ran.NextDouble();
                                break;
                            case FieldType.latitude:
                                pv = (double)ran.Next(-90, 89) + ran.NextDouble();
                                break;
                            case FieldType.city:
                                var _cityidx = ran.Next(bm.position.pcs.Length);
                                pv = bm.position.pcs[_cityidx].city[ran.Next(bm.position.pcs[_cityidx].city.Length)];
                                break;
                            case FieldType.province:
                                pv = bm.position.province[ran.Next(bm.position.province.Length)];
                                break;
                            case FieldType.id:
                                pv=idx++;
                                break;
                            case FieldType.sex:
                                pv = ran.Next(100) % 2 == 0 ? true : false;
                                break;
                            case FieldType.manHeight:
                                switch (ran.Next(15)) {
                                    case 0:
                                    case 1:
                                        pv = ran.Next(160, 165);
                                        break;
                                    case 2:
                                    case 3:
                                        pv = ran.Next(165, 170);
                                        break;
                                    case 4:
                                    case 5:
                                    case 6:
                                    case 7:
                                        pv = ran.Next(170, 175);
                                        break;
                                    case 11:
                                    case 8:
                                    case 9:
                                    case 10:
                                        pv = ran.Next(175, 180);
                                        break;
                                    case 12:
                                    case 13:
                                        pv = ran.Next(180, 185);
                                        break;
                                    case 14:
                                        pv = ran.Next(185, 195);
                                        break;
                                }
                                break;
                            case FieldType.manWeight:
                                break;
                            case FieldType.womanHeight:
                                switch (ran.Next(15)) {
                                    case 0:
                                    case 1:
                                        pv = ran.Next(150, 155);
                                        break;
                                    case 2:
                                    case 3:
                                        pv = ran.Next(155, 160);
                                        break;
                                    case 4:
                                    case 5:
                                    case 6:
                                    case 7:
                                        pv = ran.Next(160, 165);
                                        break;
                                    case 11:
                                    case 8:
                                    case 9:
                                    case 10:
                                        pv = ran.Next(165, 170);
                                        break;
                                    case 12:
                                    case 13:
                                        pv = ran.Next(170, 175);
                                        break;
                                    case 14:
                                        pv = ran.Next(175, 185);
                                        break;
                                }
                                break;
                            case FieldType.womanWeight:
                                break;
                            case FieldType.personName:
                                if (ran.Next(20) % 2 == 0) {
                                    item.i.state = 0;
                                    if (ran.Next(100) % 6 == 0)
                                        pv = $"{bm.name.first[ran.Next(firstlen)]}{bm.name.lastneutral[ran.Next(lastneutrallen)]}";
                                    else
                                        pv = $"{bm.name.first[ran.Next(firstlen)]}{bm.name.lastman[ran.Next(lastmanlen)]}";

                                } else {
                                    item.i.state = 1;
                                    if (ran.Next(100) % 7 == 0)
                                        pv = $"{bm.name.first[ran.Next(firstlen)]}{bm.name.lastneutral[ran.Next(lastneutrallen)]}";
                                    else
                                        pv = $"{bm.name.first[ran.Next(firstlen)]}{bm.name.lastwoman[ran.Next(lastmanlen)]}";
                                }
                                break;
                            case FieldType.companyName:
                                pv = bm.company[ran.Next(companylen)];
                                break;
                            case FieldType.deviceNo:
                                var _deviceno = ran.Next(1000);
                                var _deviceheader = ran.Next(100);
                                pv = $"{_deviceheader}_{_deviceno}";
                                break;
                            case FieldType.deviceName:
                                pv = CreateDeviceName();
                                break;
                            case FieldType.deviceCode:
                                pv=GetDeviceCode();
                                break;
                            case FieldType.guid:
                                pv = Guid.NewGuid();
                                break;
                            case FieldType.selfnum:
                                if (item.i.isDigital) {
                                    pv = ran.Next(item.i.min, item.i.max);
                                } else {
                                    pv = ran.Next(item.i.min, item.i.max) + ran.NextDouble();
                                }
                                break;
                            case FieldType.email:
                                pv = $"{CreateEnName()}@{new[] { "qq.com", "163.com", "outlook.com", "gmail.com", "hotmail.com", "foxmail.com" }[ran.Next(6)]}";
                                break;
                            case FieldType.ondate:
                                pv = now.AddDays(ran.Next(-3000,0)).Date;
                                break;
                            case FieldType.ontime:
                                pv = now.AddMinutes(ran.Next(-3000,0)).TimeOfDay;
                                break;
                            case FieldType.ondatetime:
                                pv = now.AddDays(ran.Next(-3000,0)).AddHours(ran.Next(-24,0)).AddMinutes(60).AddSeconds(60);
                                break;
                            case FieldType.bank:
                                pv = bm.banks[ran.Next(bm.banks.Length)].name;
                                break;
                                case FieldType.bit:
                                if (ran.Next(100) > 50) {
                                    pv=1;
                                } else {
                                    pv=0;
                                }
                                break;
                        }
                    }
                    item.i.temp = pv;
                    if (item.j.PropertyType.IsGenericType && item.j.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                        var nulla = new NullableConverter(item.j.PropertyType);
                        item.j.SetValue(temp, Convert.ChangeType(pv, nulla.UnderlyingType));
                    } else {
                        item.j.SetValue(temp, Convert.ChangeType(pv, item.j.PropertyType));
                    }
                }
                list.Add(temp);
            }
            return list.ToArray();
        }
        public string BuildEnCode() {
            //var serial=ran.Next(10,100000000).ToString();
            var len=ran.Next(1,18);
            var list=new char[len]    ;
            for (int i = 0; i < len; i++) {
                list[i]=bm.letter.en_smallchar[ran.Next(26)][0];
            }
            var r=new string(list);
            var swch=ran.Next(100);
            if (swch < 20) {
                return r.ToUpper();
            }
            return r;
        }
        public string GetDeviceCode() {
            var mark = ran.Next(100);
            var dcode = string.Empty;
            var segment1=BuildEnCode();
            var segment2= ran.Next(1,7);
            var segment3 = bm.letter.nomasymbol[ran.Next(10)];
            var segment4 = bm.letter.xinasymbol[ran.Next(24)];
            var segment5 = ran.Next(3, 10);

            switch (mark) {
                case int i when i < 20:
                    dcode = $"{segment1}-{segment2}";
                    break;
                case int i when i < 40:
                    dcode = $"{segment1}{segment2}";
                    break;
                case int i when i < 50:
                    dcode = $"{segment1} {segment2}";
                    break;
                case int i when i < 60:
                    dcode = $"{segment1}{segment3}";
                    break;
                case int i when i < 70:
                    dcode = $"{segment1}{segment4}";
                    break;
                case int i when i < 80:
                    dcode = $"{segment1}-{segment4}";
                    break;
                case int i when i < 90:
                    dcode = $"{segment1}-{segment3}";
                    break;
                case int i when i < 100:
                    dcode = $"{segment1}-{segment5}-{segment2}";
                    break;
            }
            return dcode;
        }
        public string GetBankAbbreviation(string bname) => bm.banks.Single(i => i.name == bname).abbreviation;
        public object GetManName() {
            var pv = default(object);
            if (ran.Next(100) % 7 == 0)
                pv = $"{bm.name.first[ran.Next(firstlen)]}{bm.name.lastneutral[ran.Next(lastneutrallen)]}";
            else
                pv = $"{bm.name.first[ran.Next(firstlen)]}{bm.name.lastman[ran.Next(lastmanlen)]}";
            return pv;
        }
        public object GetWomanName() {
            var pv = default(object);
            if (ran.Next(100) % 6 == 0)
                pv = $"{bm.name.first[ran.Next(firstlen)]}{bm.name.lastneutral[ran.Next(lastneutrallen)]}";
            else
                pv = $"{bm.name.first[ran.Next(firstlen)]}{bm.name.lastwoman[ran.Next(lastmanlen)]}";
            return pv;
        }
        public object PersonName => $"{bm.name.first[ran.Next(firstlen)]}{bm.name.last[ran.Next(lastmanlen)]}";
    }
    public enum FieldType {
        none, product, departmentName, departmentSummary, industry, attractions, fruit, foodmenu, software,
        wildlife, occupation, phone, telphone, identityCode, id, sex,
        manHeight, manWeight, personName,
        womanHeight, womanWeight, companyName, deviceName, deviceNo,deviceCode, guid, longitude, latitude, city,
        province, selfnum, email, ondate, ontime, ondatetime, bank,bit
    }
    public class PropConfig {
        public FieldType type;
        public string propName;
        public PropConfig(string propName, FieldType type) {
            this.type = type; this.propName = propName;
        }
        public PropConfig(string propName, int min, int max, bool isdigital) {
            this.propName = propName;
            this.min = min;
            this.max = max;
            this.isDigital = isdigital;
        }
        public int min, max, state;
        public bool isDigital;
        public object temp;
        public PropConfig dependencyObj;
        public Func<dynamic> dependency;
    }
    static public class Ex {
        static public PropConfig GetConfig(this string prop, FieldType type) => new PropConfig(prop, type);
        static public PropConfig GetConfig(this string prop, int min, int max, bool isdigital) => new PropConfig(prop, min, max, isdigital);
    }
}
