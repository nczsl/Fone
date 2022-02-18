using Util.Mathematics.Basic;
using System;
using System.Collections.Generic;
using System.Text;

namespace Util.Mathematics.Calculus {

    /// <summary>
    /// 微分 :变量相除
    /// </summary>
    public class Differential {

    }
    /// <summary>
    /// 积分：变量相乘
    /// </summary>
    public class Integral {

    }
    /*
     * 所有函数都是变量，一切变量都基于自变量，自变量
     * 就是一个自发的模式，叫数学变量自然模式，模式为
     * x(next)=x+1;这个东西为一切变量的基础
     */
    public interface IFunc<T> {
        T[] Inputs { get; }
        T Pattern();
    }
    /// <summary>
    /// 自然 变量    
    /// </summary>
    public class NaturalVariable : IFunc<int> {
        public NaturalVariable() {
            Inputs = new int[1] { 0 };
        }
        public int[] Inputs { get; }
        public int Pattern() => Inputs[0]++;
    }
}
