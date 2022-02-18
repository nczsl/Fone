using System;
using System.Collections.Generic;
using System.Text;

namespace DescriptionModel.e_commerce {
    public class Order{
        public int Id { get; set; }
    }
    public class Commodity {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
        public float Width { get; set; }
        public float Hight { get; set; }
        public float Long { get; set; }
        public string PhotoSize { get; set; }
    }
}
