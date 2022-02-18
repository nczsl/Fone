using System;
using System.Collections.Generic;
using System.Text;

namespace Util.Mathematics.Basic {
    public class Ex{
        
        static public Map<U> AntisymmetricIdentityOperator<U>(this int dimension) where U : struct {
            //首先构造上三角阵
            //Map<U> trangler;
            Map<U> result;
            //trangler = new();
            //trangler.ReserveSpace(dimension, dimension);
            result = new();
            var convert=Map<U>.NumberConvert<int>();
            // var convert2=Map<U>.NumberConvert<U>();
            result.ReserveSpace(dimension, dimension);
            var p = 0;
            var rowtemp = new U[dimension];
            for (var column = 0; column < dimension; column++) {
                rowtemp[column] = Math.Pow(-1, p++).ToString().NumberConvert<U>();
                // rowtemp[column] = convert((int)(Math.Pow(-1, p++)));
            }
            for (var column = 0; column < dimension; column++) {
                var rp = 1;
                result[0, column] = rowtemp[column];
                while (rp < dimension) {
                    result[rp, column] = (-1 * int.Parse(result[rp - 1, column].ToString())).ToString().NumberConvert<U>();
                    // result[rp, column] = convert((-1 * int.Parse(result[rp - 1, column].ToString())));
                    rp++;
                }
            }
            var diagonallist = new U[dimension];
            for (var i = 0; i < dimension; i++) {
                diagonallist[i] = 0.ToString().NumberConvert<U>();
            }
            result.UpdateDiagonal(0, true, true, diagonallist);
            return result;
        }
    }
    public class Vairable : Oper {
        public Vairable(FoundationConnect fc) : base(fc) {
            this.Step = 1;
            Domain=(0d,1000d);
        }
        public string id;
        public double value;
        public IEnumerable<double> Items {
            get {
                this.value=Domain.start;
                while (value<Domain.end) {
                    yield return value;
                    value+=Step;
                }
            }
        }
        public (double start,double end) Domain{get;set; }
        /// <summary>
        /// 采样次数
        /// </summary>
        public int Time { get=>(int)((Domain.end-Domain.start)/Step); }
        /// <summary>
        /// 步长
        /// </summary>
        public double Step { get; set; }        
        static public Vairable operator +(Vairable a,Vairable b) 
            =>new Vairable(FoundationConnect.Add){Left=a,Right=b};
        static public Vairable operator +(Vairable a, double b)
            => new Vairable(FoundationConnect.Add) { Left = a, Right = (Constant)b };
        static public Vairable operator +(double a, Vairable b)
            => new Vairable(FoundationConnect.Add) { Left = (Constant)a, Right = b };
        static public Vairable operator -(Vairable a, Vairable b)
            => new Vairable(FoundationConnect.Subtract) { Left = a, Right = b };
        static public Vairable operator -(Vairable a, double b)
            => new Vairable(FoundationConnect.Subtract) { Left = a, Right = (Constant)b };
        static public Vairable operator -(double a, Vairable b)
            => new Vairable(FoundationConnect.Subtract) { Left = (Constant)a, Right = b };
        static public Vairable operator *(Vairable a, Vairable b)
            => new Vairable(FoundationConnect.Multiply) { Left = a, Right = b };
        static public Vairable operator *(Vairable a, double b)
            => new Vairable(FoundationConnect.Multiply) { Left = a, Right = (Constant)b };
        static public Vairable operator *(double a, Vairable b)
            => new Vairable(FoundationConnect.Multiply) { Left = (Constant)a, Right = b };
        static public Vairable operator /(Vairable a, Vairable b)
            => new Vairable(FoundationConnect.Divide) { Left = a, Right = b };
        static public Vairable operator /(Vairable a, double b)
            => new Vairable(FoundationConnect.Divide) { Left = a, Right = (Constant)b };
        static public Vairable operator /(double a, Vairable b)
            => new Vairable(FoundationConnect.Divide) { Left = (Constant)a, Right = b };
        static public Vairable operator ^(Vairable a, Vairable b)
            => new Vairable(FoundationConnect.Pow) { Left = a, Right = b };
        static public Vairable operator ^(Vairable a, double b)
            => new Vairable(FoundationConnect.Pow) { Left = a, Right = (Constant)b };
        static public Vairable operator ^(double a, Vairable b)
            => new Vairable(FoundationConnect.Pow) { Left = (Constant)a, Right = b };
        static public Vairable operator &(Vairable a, Vairable b)
            => new Vairable(FoundationConnect.Log) { Left = a, Right = b };
        static public Vairable operator &(Vairable a, double b)
            => new Vairable(FoundationConnect.Log) { Left = a, Right = (Constant)b };
        static public Vairable operator &(double a, Vairable b)
            => new Vairable(FoundationConnect.Log) { Left = (Constant)a, Right = b };
    }
}

