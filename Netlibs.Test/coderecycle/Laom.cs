using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Util.Mathematics.Basic;

namespace Util.Mathematics.LinearAlgebra2 {
    //todo:行列式定义
    //todo:矩阵定义
    //todo:向量定义
    public class Vector {
        virtual protected internal List<M> Elements { get; set; }
        public IEnumerable<M> Items => Elements;
        //public Vector(params M[] vs) => Elements = new List<M>(vs);
        public Vector(params double[] eles) {
            Elements = new List<M>();
            for (var i = 0; i < eles.Length; i++) {
                Elements.Add(eles[i]);
            }
        }
        public M this[int idx] {
            get {
                return Elements[idx];
            }
            set {
                Elements[idx] = value;
            }
        }
        public int Count => Elements.Count;
        public void Add(M n) => Elements.Add(n);
        public void AddRange(M[] ns) => Elements.AddRange(ns);
        static public Vector operator +(Vector a, Vector b) {
            if (a.Count != b.Count) throw new Exception("两个向量维度不等");
            var c = new Vector();
            for (var i = 0; i < a.Count; i++) {
                c[i] = a[i] + b[i];
            }
            return c;
        }
        static public Vector operator -(Vector a, Vector b) {
            if (a.Count != b.Count) throw new Exception("两个向量维度不等");
            var c = new Vector();
            for (var i = 0; i < a.Count; i++) {
                c[i] = a[i] - b[i];
            }
            return c;
        }
        /// <summary>
        /// 内积 点积
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        static public double operator *(Vector a, Vector b) {
            if (a.Count != b.Count) throw new Exception("两个向量维度不等");
            var c = 0d;
            for (var i = 0; i < a.Count; i++) {
                c += a[i] * b[i];
            }
            return c;
        }
        static public Vector operator *(Vector a, double b) {
            for (var i = 0; i < a.Count; i++) {
                a[i] *= b;
            }
            return a;
        }
        /// <summary>
        /// 外积 叉积 只能算2维向量叉积
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        static public Vector operator ^(Vector a, Vector b) {
            if (a.Count != b.Count && (a.Count == 3)) throw new Exception("两个向量维度不等");
            var c = new Vector();
            c[0] = a[1] * b[2] - a[2] * b[1];
            c[1] = -(a[0] * b[2] - a[2] * b[0]);
            c[2] = a[0] * b[1] - a[1] * b[0];
            return c;
        }
        public void GetValues(out double[] vs) {
            vs = (from i in Elements select i.value).ToArray();
        }
        public override string ToString() {
            GetValues(out var temp);
            return string.Join(",", temp);
        }
    }
    public class Matrix {
        public Matrix(double[,] data) {
            Rows = data.GetLength(0);
            Cols = data.GetLength(1);
            rows = new List<Vector>();
            cols = new List<Vector>();
            for (var i = 0; i < Rows; i++) {
                var r = new Vector();
                for (var j = 0; j < Cols; j++) {
                    r.Elements.Add(data[i, j]);
                }
                rows.Add(r);
            }
            //for (int i = 0; i < Cols; i++) {
            //    var v = new Vector();
            //    for (int j = 0; j < Rows; j++) {
            //        v.elements.Add(rows[j].elements[i]);
            //    }
            //    cols.Add(v);
            //}
        }
        public Matrix(M[,] data) {
            Rows = data.GetLength(0);
            Cols = data.GetLength(1);
            rows = new List<Vector>();
            cols = new List<Vector>();
            for (var i = 0; i < Rows; i++) {
                var r = new Vector();
                for (var j = 0; j < Rows; j++) {
                    r.Elements.Add(data[i, j]);
                }
                rows.Add(r);
            }
            for (var i = 0; i < Cols; i++) {
                var v = new Vector();
                for (var j = 0; j < Cols; j++) {
                    v.Elements.Add(rows[j].Elements[i]);
                }
                cols.Add(v);
            }
        }
        /// <summary>
        /// 根据向量组构造
        /// </summary>
        /// <param name="vectors">向量组</param>
        /// <param name="rowOrCol">true:行向量，false:列向量</param>
        public Matrix(bool rowOrCol = false, params Vector[] vectors) {
            if (rowOrCol) {
                rows = vectors.ToList();
                Rows = rows.Count;
                Cols = rows[0].Elements.Count;
            } else {
                cols = vectors.ToList();
                Rows = cols[0].Elements.Count;
                Cols = cols.Count;
            }
        }

        readonly List<Vector> rows;
        readonly List<Vector> cols;
        public IEnumerable<M> GetRow(int no) {
            if (rows != null && rows.Count > 0) {
                var temp = rows[no];
                foreach (var item in temp.Elements) {
                    yield return item.data.value;
                }
            } else if (cols != null && cols.Count > 0) {
                foreach (var item in cols) {
                    yield return item.Elements[no].data.value;
                }
            }
        }
        public IEnumerable<M> GetColumn(int no) {
            if (cols != null && cols.Count > 0) {
                var temp = cols[no];
                foreach (var item in temp.Elements) {
                    yield return item.data.value;
                }
            } else if (rows != null && rows.Count > 0) {
                foreach (var item in rows) {
                    yield return item.Elements[no].data.value;
                }
            }
        }
        private Matrix() {

        }
        public int Rows { get; set; }
        public int Cols { get; set; }
        static public Matrix operator *(Matrix left, Matrix right) {
            //判断相乘条件是否满足？
            if (!(left.cols != null && right.rows != null && left.cols.Count == right.rows.Count))
                throw new Exception("不满足矩阵乘法条件");
            var temps = new double[left.Rows, right.Cols];
            for (var i = 0; i < left.Rows; i++) {
                for (var j = 0; j < right.Cols; j++) {
                    var x1 = left.GetRow(i).ToArray();
                    var x2 = right.GetColumn(j).ToArray();
                    var temp = default(double);
                    for (var k = 0; k < left.Cols; k++) {
                        temp += x1[k] * x2[k];
                    }
                    temps[i, j] = temp;
                }
            }
            var r = new Matrix(temps);
            return r;
        }
        /// <summary>
        /// 矩阵求逆
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        static public Matrix operator -(Matrix m) {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 展开
        /// </summary>
        /// <returns></returns>
        public M[,] Unfold() {
            var r = default(M[,]);
            if (rows != null) {
                r = new M[rows.Count, rows[0].Elements.Count];
                for (var i = 0; i < rows.Count; i++) {
                    for (var j = 0; j < rows[i].Elements.Count; j++) {
                        r[i, j] = rows[i].Elements[j].data.value;
                    }
                }
            } else if (cols != null) {
                r = new M[cols.Count, cols[0].Elements.Count];
                for (var i = 0; i < cols.Count; i++) {
                    for (var j = 0; j < cols[i].Elements.Count; j++) {
                        r[i, j] = cols[i].Elements[j].data.value;
                    }
                }
            } else {
                throw new Exception("no datas");
            }
            return r;
        }
        /// <summary>
        /// 求逆序数
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public int GetUnsortNumber(string n) {
            if (!Regex.IsMatch(n, @"\d+")) {
                throw new ArgumentException("求解逆序数需要传入数字化字符串");
            }
            var ar = new List<int>();
            foreach (var item in n) {
                ar.Add(int.Parse(item.ToString()));
            }
            var r = 0;
            for (var i = 0; i < ar.Count; i++) {
                for (var j = i + 1; j < ar.Count; j++) {
                    if (ar[i] > ar[j]) {
                        r++;
                    }
                }
            }
            return r;
        }
    }
}
