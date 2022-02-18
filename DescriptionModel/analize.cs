using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DescriptionModel {
    class analyze {
    }
    public class DataMapping {
        public double[] x;
        public double[] y;
        // [JsonIgnore]
        public Func<double, double> function;
        // [JsonIgnore]
        Random ran;
        public DataMapping() {
            ran = new Random();
        }
        public void Build(int count = 100, int? range = null) {
            var ran1 = 0;
            var ran2 = 0;
            if (range == null) {
                ran1 = short.MinValue;
                ran2 = short.MaxValue;
            } else {
                ran1 = range.Value;
                ran2 = ran.Next(100) > 50 ? 0 : -range.Value;
            }
            x = new double[count];
            y = new double[count];
            for (int i = 0; i < count; i++) {
                x[i] = ran.Next(ran1, ran2) + ran.NextDouble();
                y[i] = function(x[i]);
            }
        }
    }
}
