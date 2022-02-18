using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Util.Mathematics.LinearAlgebra {
    public sealed class Vector : IEnumerable<double> {
        readonly Matrix owner;
        internal Vector(Matrix owner) {
            this.owner = owner;
        }
        /// <summary>
        /// 此index为整体index内部使用，标明的是Vector基于整体的
        /// 索引或偏移的信息而不是其内元素的索引 
        /// </summary>
        internal int index;
        /// <summary>
        /// 列或行，默认(false)为行
        /// </summary>
        internal bool colOrRow;
        public static Vector Build(int rank) {
            var x = new Vector(new Matrix(rank, 0));//节约存储空间
            x.index = 0;
            return x;
        }
        public static Vector Build(params double[] values) {
            var x = new Vector(new Matrix(values.Length));
            x.index = 0;
            for (int i = 0; i < values.Length; i++) {
                x[i] = values[i];
            }
            return x;
        }
        /// <summary>
        /// 转置
        /// </summary>
        public void Transposition() {
            colOrRow = !colOrRow;
        }
        /// <summary>
        /// 向量的模
        /// </summary>
        public double Norm {
            get {
                var temp = default(double);
                for (var i = 0; i < this.Length; i++) {
                    temp += Math.Pow(this[i], 2);
                }
                return Math.Sqrt(temp);
            }
        }
        public IEnumerator<double> GetEnumerator() {
            if (colOrRow) {
                for (var i = 0; i < owner.Row; i++) {
                    yield return owner[index, i];
                }
            } else {
                for (var i = 0; i < owner.Col; i++) {
                    yield return owner[i, index];
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Length => colOrRow ? owner.Col : owner.Row;
        public ref double this[int i] {
            get {
                if (colOrRow) {
                    return ref owner[index, i];
                } else {
                    return ref owner[i, index];
                }
            }
        }
        static public Vector operator *(Vector x, double v) {
            for (var i = 0; i < x.Length; i++) {
                x[i] *= v;
            }
            return x;
        }
        /// <summary>
        /// 向量点积 DOT
        /// </summary>
        /// <param name="x"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        static public double operator *(Vector x, Vector v) {
            if (x.Length != v.Length) {
                throw new Exception("向量长度不相等不能相乘");
            }
            var temp = 0d;
            for (var i = 0; i < x.Length; i++) {
                temp += x[i] * v[i];
            }
            return temp;
        }
        /// <summary>
        /// 要求是同源向量，即来源于同一个矩阵的向量
        /// </summary>
        /// <param name="x">本体向量</param>
        /// <param name="v">影子向量</param>
        /// <returns></returns>
        static public Vector operator +(Vector x, Vector v) {
            if (x.Length != v.Length) throw new Exception("向量相加需要长度相等");
            for (var i = 0; i < x.Length; i++) {
                x[i] += v[i];
            }
            return x;
        }
        static public Vector operator /(Vector x, double v) {
            for (var i = 0; i < x.Length; i++) {
                x[i] /= v;
            }
            return x;
        }
        //static public Vector operator /(Vector x, Vector v) {

        //}
        //static public Vector operator -(Vector x, double v) {

        //}
        static public Vector operator -(Vector x, Vector v) {
            if (x.owner != v.owner) throw new Exception("需要同源向量");
            for (var i = 0; i < x.Length; i++) {
                x[i] -= v[i];
            }
            return x;
        }
        public override string ToString() => string.Join(",", this);
    }
    public enum MatrixTraversal {
        Row, Col
    }
    public class Matrix : IEnumerable<Vector> {
        public MatrixTraversal matrixTraversal;
        double[,] content;
        public int Rank { get; }
        public Matrix(int rank) {
            content = new double[rank, rank];
            matrixTraversal = MatrixTraversal.Col;
            Rank = rank;
        }
        public Matrix(int row, int col) {
            content = new double[row, col];
            matrixTraversal = MatrixTraversal.Col;
            if (Row == Col) {
                Rank = Row;
            }
        }
        public Matrix(double[,] content) {
            this.content = content;
            matrixTraversal = MatrixTraversal.Col;
            if (Row == Col) {
                Rank = Row;
            }
        }
        public int Row => content.GetLength(0);
        public int Col => content.GetLength(1);
        public Vector GetRow(int row) {
            var x = new Vector(this);
            x.index = row;
            x.colOrRow = false;
            return x;
        }
        public void SetRow(int row, Vector value) {
            if (Col != value.Length) {
                throw new Exception("传入Vector的长度必须与矩阵行向量长度相等");
            }
            for (var i = 0; i < Col; i++) {
                content[row, i] = value[i];
            }
        }
        public void SetRow(int row, params double[] value) {
            if (Col != value.Length) {
                throw new Exception("传入Vector的长度必须与矩阵行向量长度相等");
            }
            for (var i = 0; i < Col; i++) {
                content[row, i] = value[i];
            }
        }
        public Vector GetCol(int col) {
            var x = new Vector(this);
            x.index = col;
            x.colOrRow = true;
            return x;
        }
        public void SetCol(int col, Vector value) {
            if (Row != value.Length) {
                throw new Exception("传入Vector的长度必须与矩阵行向量长度相等");
            }
            for (var i = 0; i < Row; i++) {
                content[i, col] = value[i];
            }
        }
        public void SetCol(int col, params double[] value) {
            if (Row != value.Length) {
                throw new Exception("传入Vector的长度必须与矩阵行向量长度相等");
            }
            for (var i = 0; i < Row; i++) {
                content[i, col] = value[i];
            }
        }
        public ref double this[int i, int j] => ref content[i, j];
        public override string ToString() {
            var temp = this.matrixTraversal;
            this.matrixTraversal = MatrixTraversal.Row;
            var outstr = string.Join(Environment.NewLine, this);
            this.matrixTraversal = temp;
            return outstr;
        }
        /// <summary>
        /// 矩阵的行列式
        /// </summary>
        public double Determinant {
            get {
                if (Row != Col) {
                    throw new Exception("只有方阵才有行列式");
                }
                var temp = 0d;
                //按第一行展开
                for (int i = 0; i < Rank; i++) {
                    temp += ComplementMinor(0, i, new List<int>(), new List<int>());
                }
                return temp;
            }
        }
        IEnumerable<int> GetRanks(List<int> otherwise) {
            for (int i = 0; i < Rank; i++) {
                if (!otherwise.Contains(i)) yield return i;
            }
        }
        /// <summary>
        /// 余子式求值
        /// </summary>
        /// <remarks>
        /// 算法设计主要思路是，采用动态坐标法，来切取余子式的值，
        /// 余子式按一般思路本应该返回一个子行列式，又是一个维方阵数列
        /// 占用空间大，而且临时使用cg压力大，可想对cpu消耗会很大的，
        /// 而此方式，始终将余子式的二维方阵数列看成是一个计算遍历的时间
        /// 结构 ，所以这个算法构思的精髓在于华空间复杂结构为时间复杂结构
        /// 采用动态坐标加以解决切取的问题，找出它的递归基，是成功的关键
        /// 为此费了不少尝试和思考，终于发现此结构，即简单又有不错的运算
        /// 效率的预期
        /// </remarks>
        /// <returns></returns>
        double ComplementMinor(int row, int col, List<int> rows, List<int> cols) {
            var _rows = GetRanks(rows).ToArray();
            var _cols = GetRanks(cols).ToArray();
            if (_rows.Length == 2 && _cols.Length == 2) {
                return content[_rows[0], _cols[0]] * content[_rows[1], _cols[1]] - content[_rows[0], _cols[1]] * content[_rows[1], _cols[0]];
            } else {
                rows.Add(row);
                cols.Add(col);
                var temp = 0d;
                foreach (var i in GetRanks(rows)) {
                    foreach (var j in GetRanks(cols)) {
                        temp = content[row, col] * ComplementMinor(i, j, rows, cols);
                    }
                }
                return temp;
            }
            //throw new NotImplementedException();
        }
        static public Matrix BuildOnes(int rank) {
            var m = new Matrix(rank);
            for (var i = 0; i < rank; i++) {
                for (var j = 0; j < rank; j++) {
                    m[i, j] = 1;
                }
            }
            return m;
        }
        static public Matrix BuildIdentity(int rank) {
            var m = new Matrix(rank);
            for (var i = 0; i < rank; i++) {
                m[i, i] = 1;
            }
            return m;
        }
        static public Matrix BuildByRows(params Vector[] vectors) {
            var len = vectors[0].Length;
            foreach (var item in vectors) {
                if (len != item.Length) throw new Exception("所以合并的向量长度必须一致");
            }
            var m = new Matrix(vectors.Length, len);
            for (int i = 0; i < m.Row; i++) {
                for (int j = 0; j < m.Col; j++) {
                    m[i, j] = vectors[i][j];
                }
            }
            return m;
        }
        static public Matrix BuildByCols(params Vector[] vectors) {
            var len = vectors[0].Length;
            foreach (var item in vectors) {
                if (len != item.Length) throw new Exception("所以合并的向量长度必须一致");
            }
            var m = new Matrix(len, vectors.Length);
            for (int i = 0; i < m.Row; i++) {
                for (int j = 0; j < m.Col; j++) {
                    m[i, j] = vectors[j][i];
                }
            }
            return m;
        }
        /// <summary>
        /// 合并,通过向量的方式扩展一个新Matrix，这个
        /// Matrix的值 是现有Matrix的值的基础之上以行向量优先匹配的
        /// 规则 进行的扩展，扩展后的Matrix的行或列会增加1
        /// 因为Expend导致完全遍历内部二维数组所以可能性能开销还是比较大的
        /// </summary>
        /// <param name="other"></param>
        /// <param name="colOrRow">默认按列合并</param>
        public void Expend(Vector v) {
            switch (v.Length) {
                case int len when len == Row:
                    var m = new double[Row + 1, Col];
                    for (var i = 0; i < Row; i++) {
                        for (var j = 0; j < Col; j++) {
                            m[i, j] = content[i, j];
                        }
                    }
                    for (var i = 0; i < Col; i++) {
                        m[Row, i] = v[i];
                    }
                    content = m;
                    break;
                case int len when len == Col:
                    var m2 = new double[Row, Col + 1];
                    for (var i = 0; i < Row; i++) {
                        for (var j = 0; j < Col; j++) {
                            m2[i, j] = content[i, j];
                        }
                    }
                    for (var i = 0; i < Row; i++) {
                        m2[i, Col] = v[i];
                    }
                    content = m2;
                    break;
                default:
                    throw new Exception("长度不同不能合并");
            }
        }
        static public Matrix operator *(Matrix a, Matrix b) {
            if (a.Col != b.Row) {
                throw new ArgumentException("左乘矩阵取行向量，右乘矩阵取列向量，此二向量长度要相等才能进相乘");
            }
            var m = new Matrix(a.Row, b.Col);
            for (int i = 0; i < m.Row; i++) {
                for (int j = 0; j < m.Col; j++) {
                    m[i, j] = a.GetRow(i) * b.GetCol(j);
                }
            }
            return m;
        }
        public IEnumerator<Vector> GetEnumerator() {
            switch (matrixTraversal) {
                case MatrixTraversal.Row:
                    for (var i = 0; i < Row; i++) {
                        yield return GetRow(i);
                    }
                    break;
                case MatrixTraversal.Col:
                    for (var i = 0; i < Col; i++) {
                        yield return GetCol(i);
                    }
                    break;
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
