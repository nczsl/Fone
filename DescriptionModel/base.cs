using System;

namespace DescriptionModel.basex {
    public class Person {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Pwd { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string IdentityCode { get; set; }
        public byte? Sex { get; set; }
        public string Address { get; set; }
    }
    public struct AreaInfo {
        public string Province => nameof(this.Province);
        public string City => nameof(this.City);
        public string District => nameof(this.District);
    }
    public class Area {
        public int Id { get; set; }
        public int? Pid { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 省，市，县，区
        /// </summary>
        public string Type { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        /// <summary>地区代码</summary>
        public string DistrictCode { get; set; }
    }
    public class Animal {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Nickname { get; set; }
        public int? TypeId { get; set; }
        public string Summary { get; set; }
        public string PhotoUri1 { get; set; }
        public string PhotoUri2 { get; set; }
    }
}
