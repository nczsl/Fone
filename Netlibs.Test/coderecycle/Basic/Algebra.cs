using System;
using System.Collections.Generic;
using System.Text;

namespace Util.Mathematics.Basic {

    public class Algebra {
        public List<Algebra> factors;
        public List<Algebra> items;
        public char? markPartMajor;
        public int? markPartMinor;
        /// <summary>
        /// 次数
        /// </summary>
        public double times;
        /// <summary>
        /// 系数
        /// </summary>
        public double coefficient;
        public bool IsComplex => markPartMajor == null && markPartMinor == null;
        Algebra(int partMinor, char name = 'a') {
            this.markPartMajor = name;
            this.markPartMinor = partMinor;
        }
        Algebra() {
            factors = new List<Algebra>();
            items = new List<Algebra>();
        }
        static int no;
        static public Algebra BuildBasic(char name = 'a') {
            return new Algebra(no++, name);
        }
        //static public Algebra operator *(Algebra a, Algebra b) {
        //    var x = new Algebra();

        //}
    }
}
