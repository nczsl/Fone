using Util.Mathematics.Basic;
using Util.Mathematics.LinearAlgebra2;
using System;
using System.Collections.Generic;
using System.Text;

namespace Util.IntelligenceNet {
    /// <summary>
    /// 感知机
    /// </summary>
    public class Perceptron {
        public Vector inputs; public Vector weights;
        public Random ran;
        public int count;
        public double offset;// 偏置值
        public Perceptron(double offset, int count) {
            ran = new Random();
            this.count = count;
            this.weights = new Vector();
            for (int i = 0; i < this.count; i++) {
                this.weights.Elements.Add(ran.NextDouble());
            }
            this.offset = offset;
            this.Activate = x => System.Math.Sign(x);

        }
        public Func<double, double> Activate;
        public double Action(Vector inputs) {
            if (inputs.Count != this.count) throw new Exception("count is error");
            var temp = 0.0d;
            this.inputs = inputs;
            for (int i = 0; i < this.count; i++) {
                temp += this.inputs[i].data.value * this.weights[i].data.value;
            }
            temp += offset;
            temp = this.Activate.Invoke(temp);
            return temp;
        }
    }
    public class AiCell {
        public double value;
    }
    //层
    public class Tier {
        public Vector inputs;
        public Vector outputs;
        //public Matrix weights;
        public Perceptron[] cells;
    }
}
