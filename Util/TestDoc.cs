// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Util.TestDoc {
    /// <summary>
    /// 在测试项目里使用，理念为测试文档做为重要的开发
    /// 和构思的驱动要素，为整个设计，思想，探索提供，反馈信息
    /// 在单元测试项目，创建一个类，类里使用公共静态方法进行测试代码
    /// 的录入，然后此方法返回本对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TestDocContext<T> {
        public string Summary;
        /// <summary>
        /// 事先用于验证的答案字符串如果有
        /// </summary>
        public string Expect = "...";
        /// <summary>
        /// 测试结果字符串，可以和Answer进行比较
        /// </summary>
        public string Result;
        /// <summary>
        /// 测试后选择的一个片断用于验证测试是否成功
        /// </summary>
        public string Verify;
        public bool IsCorrect => Expect == Verify;

        public DateTimeOffset TimeStamp { get => timeStamp; }

        public T Status;
        private DateTimeOffset timeStamp;
        public string ToHtmlTbody(int idx = 0) {
            var v = Result?.Length >= 20 ? Result.Substring(0, 20) + "..." : Verify;
            var sum = Summary?.Length >= 20 ? Summary.Substring(0, 20) + "..." : Summary;
            timeStamp = DateTimeOffset.Now;
            return
                "<tr>" +
                $"   <td>{idx}</td>" +
                $"   <td>{sum}</td>" +
                $"   <td>{v}</td>" +
                $"   <td>{timeStamp}</td>" +
                $"   <td>{IsCorrect}</td>" +
                $"   <td id='sum{idx}' style='display:none;'>{Summary}</td>" +
                $"   <td id='r{idx}' style='display:none;'>{Result}</td>" +
                $"   <td><a href='javascript:;' onclick='detail({idx})'>goto</a></td>" +
                "</tr>";
        }
    }
}
