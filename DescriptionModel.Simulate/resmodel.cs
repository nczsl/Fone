using System;
using System.Collections.Generic;
using System.Text;

namespace DescriptionModel.Simulate.JsonSource {

    public class bmodel {
        public Name name { get; set; }
        public Phone phone { get; set; }
        public string[] wildlife { get; set; }
        public string[] occupation { get; set; }
        public string[] company { get; set; }
        public string[] software { get; set; }
        public string[] foodmenu { get; set; }
        public string[] fruit { get; set; }
        public string[] attractions { get; set; }
        public string[] companysuffix { get; set; }
        public string[] industry { get; set; }
        public string[] companypostsrank { get; set; }
        public string[] militaryrank { get; set; }
        public Letter letter { get; set; }
        public Country country { get; set; }
        public Position position { get; set; }
        public string[] department { get; set; }
        public Product product { get; set; }
        public Bank[] banks { get; set; }
    }

    public class Name {
        public string[] first { get; set; }
        public string[] last { get; set; }
        public string[] lastman { get; set; }
        public string[] lastwoman { get; set; }
        public string[] lastneutral { get; set; }
        public string[] enname { get; set; }
    }

    public class Phone {
        public int[] profix { get; set; }
    }

    public class Letter {
        public string[] en_bigchar { get; set; }
        public string[] en_smallchar { get; set; }
        public string[] num { get; set; }
        public string[] symbol { get; set; }
        public string[] xinasymbol { get; set; }
        public string[] xinaspell { get; set; }
        public string[] nomasymbol { get; set; }
        public string[] japansymbol { get; set; }
    }

    public class Country {
        public string[] encountry { get; set; }
        public string[] cncountry { get; set; }
    }

    public class Position {
        public string[] province { get; set; }
        public string[] cncaption { get; set; }
        public Pc[] pcs { get; set; }
    }

    public class Pc {
        public string pn { get; set; }
        public string[] city { get; set; }
    }

    public class Product {
        public string[] productsuffix { get; set; }
        public string[] productfuncpart { get; set; }
        public string[] productsummary { get; set; }
    }

    public class Bank {
        public string name { get; set; }
        public string abbreviation { get; set; }
    }
}
