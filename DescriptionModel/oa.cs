using DescriptionModel.basex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DescriptionModel.oa {
    public struct EmployeeDesc{
        public string RoleDescribtion_Manager => nameof(RoleDescribtion_Manager).Split('_').Last();
        public string RoleDescribtion_CorporateRepresentative => nameof(RoleDescribtion_CorporateRepresentative).Split('_').Last();
    }
    public class Employee:Person {
        public string RoleTitle { get; set; }
        public string RoleDescribtion { get; set; }
        public Employee() {}
        public Employee(Person p) {
            this.Address = p.Address;
            base.Email = p.Email;
            base.Id = p.Id;
            base.IdentityCode = p.IdentityCode;
            base.Name = p.Name;
            base.Phone = p.Phone;
            base.Pwd = p.Pwd;
            base.Sex = p.Sex;
        }
        public int? OwnerCompany { get; set; }
        public int? OwnerDepartment { get; set; }
    }
    public class Department {
        public int Id { get; set; }
        public int? Pid { get; set; }
        public string Summary { get; set; }
        public string Name { get; set; }
    }
    public class R_ComanyDepartment {
        public int Id { get; set; }
        public int? Company { get; set; }
        public int? Demaprtment { get; set; }
    }
    public class Product {
        public int Id { get; set; }
        public int? Pid { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
        public string Code { get; set; }
        public decimal? Price { get; set; }
        public int? OwnerCompany { get; set; }
    }
    public class Company {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string OfficialWebsite { get; set; }
        public double? RegisteredCapital { get; set; }
        public string Industry { get; set; }
        public string Telphone { get; set; }
    }
}
