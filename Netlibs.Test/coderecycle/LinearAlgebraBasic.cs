using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Util.Mathematics.LinearAlgebra3 {
    public sealed class Vector : IEnumerable<double> {
        readonly double[] content;
        internal Vector(double[] content, int row, int col) {
            this.content = content;
            this.row = row;
            this.col = col;
        }
        internal int row, col;
        /// <summary>
        /// 此index为整体index内部使用，标明的是Vector基于整体的
        /// 索引或偏移的信息而不是其内元素的索引 
        /// </summary>
        internal int index;
        /// <summary>
        /// 列或行，默认(false)为行
        /// </summary>
        internal bool colOrRow;
        internal int offset => colOrRow ? index : index * col;
        public IEnumerator<double> GetEnumerator() {
            if (colOrRow) {
                for (int i = 0; i < row * col; i += row + 1) {
                    yield return content[offset + i];
                }
            } else {
                for (int i = 0; i < col; i++) {
                    yield return content[offset + i];
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Length => colOrRow ? col : row;
        public ref double this[int i] {
            get {
                if (colOrRow) {
                    return ref content[offset + i * col];
                } else {
                    return ref content[offset + i];
                }
            }
        }
        static public Vector operator *(Vector x, double v) {

            return x;
        }
        static public Vector operator *(Vector x, Vector v) {

        }
        static public Vector operator +(Vector x, double v) {

        }
        static public Vector operator +(Vector x, Vector v) {

        }
        static public Vector operator /(Vector x, double v) {

        }
        static public Vector operator /(Vector x, Vector v) {

        }
        static public Vector operator -(Vector x, double v) {

        }
        static public Vector operator -(Vector x, Vector v) {

        }
    }
    public class Matrix {
        readonly double[] content;
        public Matrix(int rank) {
            content = new double[rank * rank];
            Row = Col = rank;
        }
        public Matrix(int row, int col) {
            content = new double[row * col];
            Row = row; Col = col;
        }
        public int Row { get; private set; }
        public int Col { get; private set; }
        //public Span<double> GetRow(int row) {
        //    var span = content.AsSpan();
        //    return span.Slice(row * Col, Col);
        //}
        //public Span<double> GetCol(int col) {
        //    throw new NotImplementedException();
        //}
        public Vector GetRow(int row) {
            var x = new Vector(content, Row, Col);
            x.index = row;
            x.colOrRow = false;
            return x;
        }
        public Vector GetCol(int col) {
            var x = new Vector(content, Row, Col);
            x.index = col;
            x.colOrRow = true;
            return x;
        }
        public ref double this[int i, int j] => ref content[i * Col + j];
        public override string ToString() {
            var sb = new StringBuilder();
            var temp = new List<double>();
            for (var i = 0; i < Row; i++) {
                for (var j = 0; j < Col; j++) {
                    //var offset = i * Col;
                    temp.Add(this[i, j]);
                }
                sb.AppendLine(string.Join(",", temp));
                temp.Clear();
            }
            return sb.ToString();
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
    }
}
