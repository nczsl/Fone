// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
//usingUtil.Mathematics.Discrete;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Util;
using Util.Generator;
using Util.TestDoc;

namespace Util.Ex {
    public static class Acex {
        //
        #region sort
        /// <summary>
        /// 对两个有序集合进行合并
        /// </summary>
        /// <typeparam name="T">集合类型</typeparam>
        /// <param name="x">主集合</param>
        /// <param name="y">目标集合</param>
        public static T[] SortMerge<T>(this T[] x, T[] y) where T : IComparable<T> {
            var xlen = x.Length; var ylen = y.Length;
            var rlen = xlen + ylen - 1;
            var r = new T[rlen + 1];
            int xidx,
            yidx; xidx = yidx = 0; var idx = 0;
            while (xidx < xlen && yidx < ylen) {
                if (x[xidx].CompareTo(y[yidx]) > 0) r[idx++] = y[yidx++];
                else r[idx++] = x[xidx++];
            }
            if (xidx >= xlen)
                for (int i = yidx; i < ylen; i++) r[idx++] = y[i];
            else
                for (int i = xidx; i < xlen; i++) r[idx++] = x[i];
            return r;
        }
        /*二叉堆排序*/
        public static void SortHeap<T>(this T[] data)
        where T : IComparable<T> {
            var len = data.Length; var len1 = len + 2;
            var temps = new T[len1];
            for (int i = 0; i < len; i++) {
                Enheap(temps, data[i], i);
            }
            for (int i = 0; i < len; i++) {
                data[i] = Deheap(temps, len - i);
            }
        }
        static void Enheap<T>(T[] temps, T item, int p)
        where T : IComparable<T> {
            if (p == 0) {
                temps[1] = item;
            } else {
                var len = p + 1;
                var poss = len.GetLengthHigh().ToArray();
                var len2 = poss.Length;
                var mark = 0;
                for (int i = 1; i < len2; i++) {
                    mark = i - 1;
                    if (item.CompareTo(temps[poss[i]]) < 0) { temps[poss[mark]] = temps[poss[i]]; continue; }
                    break;
                }
                if (mark == len2 - 2 && temps[poss[mark]].CompareTo(temps[poss[mark + 1]]) == 0) {
                    temps[1] = item; //换根节点
                } else {
                    temps[poss[mark]] = item;
                }
            }
        }
        /// <summary>
        /// Remove the top item from the heap
        /// </summary>
        static T Deheap<T>(T[] temps, int len) where T : IComparable<T> {
            var p = 1;
            var temp = temps[p];
            while (2 * p < len) {
                if (2 * p <= len && 2 * p + 1 <= len) {
                    if (temps[2 * p].CompareTo(temps[2 * p + 1]) > 0) {
                        var temp2 = temps[2 * p + 1];
                        if (temps[len].CompareTo(temp2) < 0) {
                            temps[p] = temps[len];
                            goto step2;
                        } else {
                            temps[p] = temp2;
                        }
                        p = 2 * p + 1;
                    } else {
                        var temp2 = temps[2 * p];
                        if (temps[len].CompareTo(temp2) < 0) {
                            temps[p] = temps[len];
                            goto step2;
                        } else {
                            temps[p] = temp2;
                        }
                        p *= 2;
                    }
                } else if (2 * p <= len) {
                    var temp2 = temps[2 * p];
                    if (temps[len].CompareTo(temp2) < 0) {
                        temps[p] = temps[len];
                        goto step2;
                    } else {
                        temps[p] = temp2;
                    }
                    p *= 2;
                }
            }
            temps[p] = temps[len];
step2: return temp;
        }
        //
        public static void SortSelect(this int[] data) {
            var count = data.Length;
            var min = 0;
            var x = data[0];
            for (int i = 0; i < count - 1; i++) {
                min = i;
                for (int j = i + 1; j < count; j++) {
                    if (data[j] < data[min]) {
                        min = j;
                    }
                }
                var temp = data[min];
                data[min] = data[i];
                data[i] = temp;
            }
        }
        public static void SortSelectD(this int[] data) {
            var count = data.Length - 1;
            var min = 0;
            var max = count;
            var len = (count + 1) / 2;
            for (int i = 0; i < len; i++) {
                min = i;
                max = count - i;
                if (data[min] > data[max]) {
                    min = min ^ max;
                    max = min ^ max;
                    min = min ^ max;
                }
                for (int j = i + 1; j < count - i; j++) {
                    if (data[j] < data[min]) {
                        min = j;
                    } else if (data[j] > data[max]) {
                        max = j;
                    }
                }
                if (min != i) {
                    var temp = data[min];
                    data[min] = data[i];
                    data[i] = temp;
                    if (max == i) max = min;
                }
                if (max != count - i) {
                    var tempmax = data[max];
                    data[max] = data[count - i];
                    data[count - i] = tempmax;
                }
            }
        }
        public static void SortBubble(this int[] data) {
            var len = data.Length;
            for (int j = 0; j < len; j++) {
                for (int i = j; i < len; i++) {
                    if (data[j] > data[i]) {
                        var temp = data[j];
                        data[j] = data[i];
                        data[i] = temp;
                    }
                }
            }
        }
        public static void SortInsert(this int[] data) {
            var count = data.Length;
            for (int i = 1; i < count; i++) {
                var t = data[i];
                var j = i;
                //通过这种循环copy的方式来移动和扩张已排序列
                while (j > 0 && data[j - 1] > t) {
                    data[j] = data[--j];
                }
                data[j] = t;
            }
        }
        //
        public static void SortQuick(this int[] data) {
            QuickSort(data, 0, data.Length - 1);
        }
        private static void QuickSort(int[] numbers, int left, int right) {
            if (left < right) {
                int middle = numbers[(left + right) / 2];
                int i = left - 1;
                int j = right + 1;
                while (true) {
                    while (numbers[++i] < middle) ;
                    while (numbers[--j] > middle) ;
                    if (i >= j) break;
                    Swap(numbers, i, j);
                }
                QuickSort(numbers, left, i - 1);
                QuickSort(numbers, j + 1, right);
            }
        }
        private static void Swap(int[] numbers, int i, int j) {
            int number = numbers[i];
            numbers[i] = numbers[j];
            numbers[j] = number;
        }
        //
        /// <summary>
        /// 奇偶排序
        /// </summary>
        /// <param name="data"></param>
        public static void SortOddEven(this int[] data) {
            var sorted = false;
            var count = data.Length;
            while (!sorted) {
                sorted = true;
                for (var i = 1; i < count - 1; i += 2) {
                    if (data[i] > data[i + 1]) {
                        Swap(data, i, i + 1);
                        sorted = false;
                    }
                }
                for (var i = 0; i < count - 1; i += 2) {
                    if (data[i] > data[i + 1]) {
                        Swap(data, i, i + 1);
                        sorted = false;
                    }
                }
            }
        }
        /// <summary>
        /// 鸡尾酒排序
        /// </summary>
        /// <param name="data"></param>
        public static void SortCocktail(this int[] data) { // the first element of list has index 0
            var bottom = 0;
            var top = data.Length - 1;
            var swapped = true;
            while (swapped == true) {
                // if no elements have been swapped, then the list is sorted
                swapped = false;
                for (int i = bottom; i < top; i = i + 1) {
                    if (data[i] > data[i + 1]) {
                        // test whether the two elements are in the correct order
                        Swap(data, i, i + 1); // let the two elements change places
                        swapped = true;
                    }
                }
                // decreases top the because the element with the largest value in the unsorted
                // part of the list is now on the position top
                top = top - 1;
                for (int i = top; i > bottom; i = i - 1) {
                    if (data[i] < data[i - 1]) {
                        Swap(data, i, i - 1);
                        swapped = true;
                    }
                }
                // increases bottom because the element with the smallest value in the unsorted
                // part of the list is now on the position bottom
                bottom = bottom + 1;
            }
        }
        //
        public static void SortSelect(this float[] data) {
            var count = data.Length;
            var min = 0;
            for (int i = 0; i < count - 1; i++) {
                min = i;
                for (int j = i + 1; j < count; j++) {
                    if (data[j] < data[min]) {
                        min = j;
                    }
                }
                var temp = data[min];
                data[min] = data[i];
                data[i] = temp;
            }
        }
        public static void SortBubble(this float[] data) {
            var len = data.Length;
            for (int j = 0; j < len; j++) {
                for (int i = j; i < len; i++) {
                    if (data[j] > data[i]) {
                        var temp = data[j];
                        data[j] = data[i];
                        data[i] = temp;
                    }
                }
            }
        }
        public static void SortInsert(this float[] data) {
            var count = data.Length;
            for (int i = 1; i < count; i++) {
                var t = data[i];
                var j = i;
                while (j > 0 && data[j - 1] > t) {
                    data[j] = data[--j];
                }
                data[j] = t;
            }
        }
        //
        public static void SortQuick(this float[] data) {
            QuickSort(data, 0, data.Length - 1);
        }
        private static void QuickSort(float[] data, int left, int right) {
            if (left < right) {
                var middle = data[(left + right) / 2];
                int i = left - 1;
                int j = right + 1;
                while (true) {
                    while (data[++i] < middle) ;
                    while (data[--j] > middle) ;
                    if (i >= j) break;
                    Swap(data, i, j);
                }
                QuickSort(data, left, i - 1);
                QuickSort(data, j + 1, right);
            }
        }
        private static void Swap(float[] numbers, int i, int j) {
            var number = numbers[i];
            numbers[i] = numbers[j];
            numbers[j] = number;
        }
        /// <summary>
        /// 奇偶排序
        /// </summary>
        /// <param name="data"></param>
        public static void SortOddEven(this float[] data) {
            var sorted = false;
            var count = data.Length;
            while (!sorted) {
                sorted = true;
                for (var i = 1; i < count - 1; i += 2) {
                    if (data[i] > data[i + 1]) {
                        Swap(data, i, i + 1);
                        sorted = false;
                    }
                }
                for (var i = 0; i < count - 1; i += 2) {
                    if (data[i] > data[i + 1]) {
                        Swap(data, i, i + 1);
                        sorted = false;
                    }
                }
            }
        }
        /// <summary>
        /// 鸡尾酒排序
        /// </summary>
        /// <param name="list"></param>
        public static void SortCocktail(this float[] list) { // the first element of list has index 0
            var bottom = 0;
            var top = list.Length - 1;
            var swapped = true;
            while (swapped == true) {
                // if no elements have been swapped, then the list is sorted
                swapped = false;
                for (int i = bottom; i < top; i = i + 1) {
                    if (list[i] > list[i + 1]) {
                        // test whether the two elements are in the correct order
                        Swap(list, i, i + 1); // let the two elements change places
                        swapped = true;
                    }
                }
                // decreases top the because the element with the largest value in the unsorted
                // part of the list is now on the position top
                top = top - 1;
                for (int i = top; i > bottom; i = i - 1) {
                    if (list[i] < list[i - 1]) {
                        Swap(list, i, i - 1);
                        swapped = true;
                    }
                }
                // increases bottom because the element with the smallest value in the unsorted
                // part of the list is now on the position bottom
                bottom = bottom + 1;
            }
        }
        public static void SortSelect(this double[] data) {
            var count = data.Length;
            var min = 0;
            for (int i = 0; i < count - 1; i++) {
                min = i;
                for (int j = i + 1; j < count; j++) {
                    if (data[j] < data[min]) {
                        min = j;
                    }
                }
                var temp = data[min];
                data[min] = data[i];
                data[i] = temp;
            }
        }
        public static void SortBubble(this double[] data) {
            var len = data.Length;
            for (int j = 0; j < len; j++) {
                for (int i = j; i < len; i++) {
                    if (data[j] > data[i]) {
                        var temp = data[j];
                        data[j] = data[i];
                        data[i] = temp;
                    }
                }
            }
        }
        public static void SortInsert(this double[] data) {
            var count = data.Length;
            for (int i = 1; i < count; i++) {
                var t = data[i];
                var j = i;
                while (j > 0 && data[j - 1] > t) {
                    data[j] = data[--j];
                }
                data[j] = t;
            }
        }
        //
        public static void SortQuick(this double[] data) {
            SortQuick(data, 0, data.Length - 1);
        }
        private static void SortQuick(double[] data, int left, int right) {
            if (left < right) {
                var middle = data[(left + right) / 2];
                int i = left - 1;
                int j = right + 1;
                while (true) {
                    while (data[++i] < middle) ;
                    while (data[--j] > middle) ;
                    if (i >= j) break;
                    Swap(data, i, j);
                }
                SortQuick(data, left, i - 1);
                SortQuick(data, j + 1, right);
            }
        }
        private static void Swap(double[] numbers, int i, int j) {
            var number = numbers[i];
            numbers[i] = numbers[j];
            numbers[j] = number;
        }
        //
        /// <summary>
        /// 奇偶排序
        /// </summary>
        /// <param name="data"></param>
        public static void SortOddEven(this double[] data) {
            var sorted = false;
            var count = data.Length;
            while (!sorted) {
                sorted = true;
                for (var i = 1; i < count - 1; i += 2) {
                    if (data[i] > data[i + 1]) {
                        Swap(data, i, i + 1);
                        sorted = false;
                    }
                }
                for (var i = 0; i < count - 1; i += 2) {
                    if (data[i] > data[i + 1]) {
                        Swap(data, i, i + 1);
                        sorted = false;
                    }
                }
            }
        }
        /// <summary>
        /// 鸡尾酒排序
        /// </summary>
        /// <param name="list"></param>
        public static void SortCocktail(this double[] list) { // the first element of list has index 0
            var bottom = 0;
            var top = list.Length - 1;
            var swapped = true;
            while (swapped == true) {
                // if no elements have been swapped, then the list is sorted
                swapped = false;
                for (int i = bottom; i < top; i = i + 1) {
                    if (list[i] > list[i + 1]) {
                        // test whether the two elements are in the correct order
                        Swap(list, i, i + 1); // let the two elements change places
                        swapped = true;
                    }
                }
                // decreases top the because the element with the largest value in the unsorted
                // part of the list is now on the position top
                top = top - 1;
                for (int i = top; i > bottom; i = i - 1) {
                    if (list[i] < list[i - 1]) {
                        Swap(list, i, i - 1);
                        swapped = true;
                    }
                }
                // increases bottom because the element with the smallest value in the unsorted
                // part of the list is now on the position bottom
                bottom = bottom + 1;
            }
        }
        #endregion
        //
        #region date time
        /// <summary>
        /// 返回一个以星期几为key的 7组 日期集合，一般每组有4天或5天
        /// </summary>
        /// <param name="month"></param>
        /// <returns></returns>
        public static Dictionary<DayOfWeek, List<(bool, DateTime)>> GetCanlenderMonthDays(this DateTime month) {
            var start = month.AddDays(1 - month.Day);
            var current = start;
            var end = month.AddDays(1 - month.Day).AddMonths(1).AddDays(-1);
            var ds = new List<DateTime>();
            while (current < end) {
                ds.Add(current);
                current = current.AddDays(1);
            }
            ds.Add(end);
            var dds = new Dictionary<DayOfWeek,
                List<(bool, DateTime)>>();
            var weekgroup = from item in ds group item by item.DayOfWeek;
            var wgo = from item in weekgroup orderby item.ElementAt(0).Day select item;
            var markg = wgo.ElementAt(0).Key;
            var wgog = from item in wgo
                       let key = item.Key >= markg
                       group item by key;
            foreach (var item in wgog) {
                if (item.Key) {
                    if (markg == DayOfWeek.Sunday) {
                        foreach (var item2 in item) {
                            var x = (from item3 in item2 select (true, item3)).ToList();
                            if (item2.Count() == 5) {
                                x.Insert(0, (false, x[0].item3.AddDays(-7)));
                            } else {
                                x.Insert(0, (false, x[0].item3.AddDays(-7)));
                                x.Add((false, x[x.Count - 1].item3.AddDays(7)));
                            }
                            dds.Add(item2.Key, x);
                        }
                    } else {
                        foreach (var item2 in item) {
                            var x = (from item3 in item2 select (true, item3)).ToList();
                            if (item2.Count() == 5) {
                                x.Add((false, x[x.Count - 1].item3.AddDays(7)));
                            } else {
                                x.Add((false, x[x.Count - 1].item3.AddDays(7)));
                                x.Add((false, x[x.Count - 1].item3.AddDays(7)));
                            }
                            dds.Add(item2.Key, x);
                        }
                    }
                } else {
                    foreach (var item2 in item) {
                        var x = (from item3 in item2 select (true, item3)).ToList();
                        if (item2.Count() == 5) {
                            x.Insert(0, (false, x[0].item3.AddDays(-7)));
                        } else {
                            x.Insert(0, (false, x[0].item3.AddDays(-7)));
                            x.Add((false, x[x.Count - 1].item3.AddDays(7)));
                        }
                        dds.Add(item2.Key, x);
                    }
                }
            }
            return dds;
        }
        #endregion
        //
        #region string
        public static string PunctuationSet(this char punctuation, int times) {
            var list = new List<char>();
            for (int i = 0; i < times; i++) {
                list.Add(punctuation);
            }
            return new string(list.ToArray());
        }
        /// <summary>
        /// 计算字符串中子串出现的次数
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="substring">子串</param>
        /// <returns>出现的次数</returns>
        public static int SubstringCount(this string str, string substring) {
            if (str.Contains(substring)) {
                string strReplaced = str.Replace(substring, "");
                return (str.Length - strReplaced.Length) / substring.Length;
            }
            return 0;
        }
        public static string LowFirst(this string s) => s.Substring(0, 1).ToLower() + s.Substring(1);
        public static string UpFirst(this string s) => s.Substring(0, 1).ToUpper() + s.Substring(1);
        //static public void SeveTo(this string path) {
        //    File.Open(path, FileMode.OpenOrCreate);
        //}
        /// <summary>
        /// 计算相同长度两个字符串中是否包含相同的char集组合,要求char集不重复
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static bool CheckMatch(this string s1, string s2) {
            if ((string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) || (s1.Length != s2.Length)) return false;
            int[] check = new int[128];
            var len = s1.Length;
            foreach (var c in s1) {
                check[c]++;
            }
            foreach (var c in s2) {
                check[c]--;
            }
            return check.Max() == 0 && check.Min() == 0;
        }
        /// <summary>
        /// 求一个字符串的回溯函数。
        /// 约定序列下标从0开始。
        /// 回溯函数是整数集[0,n-1]到N的映射，n为字符串的长度。
        /// 回溯函数的定义：
        /// 设存在非空序列L，i为其合法下标；
        /// L[i]的前置序列集为：{空集,L中所有以i-1为最后一个元素下标的子序列}
        /// L的前置序列集为：{空集,L中所有以0为第一个元素下标的子序列}
        /// 下标i的回溯函数值的定义为：
        /// 如果i=0,回溯函数值为-1
        /// 否则i的回溯函数值为i的前置序列集和L的前置序列集中相等元素的最大长度,但是相等的两个元素不能是L中的同一个子串，例如[0-i,1]~[i-1,0]reversed
        /// 换句话说是，设集合V={x,x属于i的前置序列集,并且x属于L的前置序列集，并且x的长度小于i}，回溯函数值为max(V).length
        /// 当i=0时并不存在这样的一个x，所以约定此时的回溯函数值为-1
        /// 回溯函数的意义：
        /// 如果子串中标号为j的字符同主串失配，那么将子串回溯到next[j]继续与主串匹配，如果next[j]=-1,则主串的匹配点后移一位，同子串的第一个元素开始匹配。
        /// 同一般的模式匹配算法相比，kmp通过回溯函数在失配的情况下跳过了若干轮匹配(向右滑动距离可能大于1)
        /// kmp算法保证跳过去的这些轮匹配一定是失配的，这一点可以证明
        /// </summary>
        /// <param name="pattern">模式串，上面的注释里将其称为子串</param>
        /// <returns>回溯函数是kmp算法的核心，本函数依照其定义求出回溯函数，KMP函数依照其意义使用回溯函数。</returns>
        static int[] Next(string pattern) {
            int[] next = new int[pattern.Length];
            next[0] = -1;
            if (pattern.Length < 2) //如果只有1个元素不用kmp效率会好一些
            {
                return next;
            }
            next[1] = 0; //第二个元素的回溯函数值必然是0，可以证明：
            //1的前置序列集为{空集,L[0]}，L[0]的长度不小于1，所以淘汰，空集的长度为0，故回溯函数值为0
            int i = 2; //正被计算next值的字符的索引
            int j = 0; //计算next值所需要的中间变量，每一轮迭代初始时j总为next[i-1]
            while (i < pattern.Length) //很明显当i==pattern.Length时所有字符的next值都已计算完毕，任务已经完成
            { //状态点
                if (pattern[i - 1] == pattern[j]) //首先必须记住在本函数实现中，迭代计算next值是从第三个元素开始的
                { //如果L[i-1]等于L[j]，那么next[i] = j + 1
                    next[i++] = ++j;
                } else { //如果不相等则检查next[i]的下一个可能值----next[j]
                    j = next[j];
                    if (j == -1) //如果j == -1则表示next[i]的值是1
                    { //可以把这一部分提取出来与外层判断合并
                        //书上的kmp代码很难理解的一个原因就是已经被优化，从而遮蔽了其实际逻辑
                        next[i++] = ++j;
                    }
                }
            }
            return next;
        }
        /// <summary>
        /// KMP函数同普通的模式匹配函数的差别在于使用了next函数来使模式串一次向右滑动多位称为可能
        /// next函数的本质是提取重复的计算
        /// </summary>
        /// <param name="source">主串</param>
        /// <param name="pattern">用于查找主串中一个位置的模式串</param>
        /// <returns>-1表示没有匹配，否则返回匹配的标号</returns>
        public static int ExecuteKMP(this string source, string pattern) {
            int[] next = Next(pattern);
            int i = 0; //主串指针
            int j = 0; //模式串指针
            //如果子串没有匹配完毕并且主串没有搜索完成
            while (j < pattern.Length && i < source.Length) {
                if (source[i] == pattern[j]) //i和j的逻辑意义体现于此，用于指示本轮迭代中要判断是否相等的主串字符和模式串字符
                {
                    i++;
                    j++;
                } else {
                    j = next[j]; //依照指示迭代回溯
                    if (j == -1) //回溯有情况，这是第二种
                    {
                        i++;
                        j++;
                    }
                }
            }
            //如果j==pattern.Length则表示循环的退出是由于子串已经匹配完毕而不是主串用尽
            return j < pattern.Length ? -1 : i - j;
        }
        //
        public static int Kmp(char[] a, char[] b, int pos) {
            int i = pos, j = 0;
            int lena = a.Length, lenb = b.Length;
            var count = 0;
            while ((i <= lena - 1) && (j <= lenb - 1)) {
                if (a[i] == b[j]) { ++i; ++j; } else { i = i - j + 1; j = 0; }
                count++;
            }
            if (j > lenb - 1)
                return i - lenb;
            else
                return 0;
        }
        /// <summary>
        /// Compute the distance between two strings.(edit distance)
        /// </summary>
        public static int Levenshtein(this string s, string t) {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];
            // Step 1
            if (n == 0) {
                return m;
            }
            if (m == 0) {
                return n;
            }
            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }
            // Step 3
            for (int i = 1; i <= n; i++) {
                //Step 4
                for (int j = 1; j <= m; j++) {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }
        public static string Subtract(this string a, string b) {
            if (a.Length > b.Length) {
                var len = a.Length - b.Length;
                if (a.Substring(len) == b) {
                    return a.Remove(len, b.Length);
                }
            }
            throw new Exception("the two string must match");
        }
        /// <summary>
        /// 根据string 源返回 特定的 结构组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="patterns"></param>
        /// <returns></returns>
        public static T[] ToTypeValues<T>(this string source, params char[] patterns) where T : struct {
            var ss = source.Split(patterns, StringSplitOptions.RemoveEmptyEntries);
            var ts = new List<T>();
            switch (typeof(T).Name) {
                case "Int32":
                foreach (var item in ss) { ts.Add((T)(object)Convert.ToInt32(item)); }
                break;
                case "Int64":
                foreach (var item in ss) { ts.Add((T)(object)Convert.ToInt64(item)); }
                break;
                case "Int16":
                foreach (var item in ss) { ts.Add((T)(object)Convert.ToInt16(item)); }
                break;
                case "Byte":
                foreach (var item in ss) { ts.Add((T)(object)Convert.ToByte(item)); }
                break;
                case "Double":
                foreach (var item in ss) { ts.Add((T)(object)Convert.ToDouble(item)); }
                break;
                case "Single":
                foreach (var item in ss) { ts.Add((T)(object)Convert.ToSingle(item)); }
                break;
                case "Boolean":
                foreach (var item in ss) { ts.Add((T)(object)Convert.ToBoolean(item)); }
                break;
                case "DateTime":
                foreach (var item in ss) { ts.Add((T)(object)Convert.ToDateTime(item)); }
                break;
            }
            return ts.ToArray();
        }
        public static string MaxSubstr(this string s1, string s2) {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return null;
            else if (s1 == s2) return s1;
            int max = 0;
            int end = 0;
            int[,] a = new int[s1.Length, s2.Length];
            for (int i = 0; i < s1.Length; i++) {
                for (int j = 0; j < s2.Length; j++) {
                    int n = (i - 1 >= 0 && j - 1 >= 0) ? a[i - 1, j - 1] : 0;
                    a[i, j] = s1[i] == s2[j] ? 1 + n : 0;
                    if (a[i, j] > max) { max = a[i, j]; end = i; }
                }
            }
            return s1.Substring(end - max + 1, max);
        }
        public static string Replace(this string the, params string[] subs) {
            var result = the;
            for (int i = 0; i < subs.Length / 2; i++) {
                var j = i * 2;
                result = result.Replace(subs[j], subs[j + 1]);
            }
            return result;
        }
        static string SqlTypeName2XsdType(this string sqltypename) {
            if (string.IsNullOrEmpty(sqltypename)) return string.Empty;
            string[] SqlTypeNames = new string[] { "int", "varchar", "bit", "datetime", "date", "decimal", "float", "image", "money", "ntext", "nvarchar", "smalldatetime", "smallint", "text", "bigint", "binary", "char", "nchar", "numeric", "real", "smallmoney", "sql_variant", "timestamp", "tinyint", "uniqueidentifier", "varbinary", "xml" };
            string[] XsdTypes = new string[] { "int", "string", "boolean", "dateTime", "date", "decimal", "float", "base64Binary", "float", "string", "string", "dateTime", "short", "string", "long", "hexBinary", "string", "string", "decimal", "float", "float", "anytype", "hexBinary", "byte", "token", "base64Binary", "string" };
            var mark = sqltypename.ToLower().Trim();
            int i = Array.IndexOf(SqlTypeNames, mark);
            return XsdTypes[i];
        }
        #endregion
        //
        #region double[,]
        public static bool Equal(this double[,] owner, double[,] target) {
            var r = false;
            var len0 = owner.GetLength(0);
            var len1 = owner.GetLength(1);
            if (owner.Length == target.Length && len0 == target.GetLength(0) && len1 == target.GetLength(1)) {
                return false;
            }
            for (int i = 0; i < len0; i++) {
                for (int j = 0; j < len1; j++) {
                    if (owner[i, j] != target[i, j]) {
                        r = false;
                        break;
                    }
                }
            }
            return r;
        }
        /// <summary>
        /// 矩阵乘
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static double[,] Mul(this double[,] left, double[,] right) {
            //判断行列是否匹配
            var len1 = left.GetLength(0);
            var len2 = right.GetLength(1);
            var len3 = left.GetLength(1);
            var len4 = right.GetLength(0);
            if (len3 != len4)
                throw new Exception("乘法条件不匹配，请确保前行后列长度相等");
            var x1 = new double[len1, len1];
            for (int k = 0; k < len2; k++) {
                for (int i = 0; i < len1; i++) {
                    for (int j = 0; j < len4; j++) {
                        x1[i, k] += left[i, j] * right[j, k];
                    }
                }
            }
            return x1;
        }
        //
        /// <summary>
        /// 矩阵的逆
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static double[,] Inverse(this double[,] target) {
            if (target.GetLength(0) != target.GetLength(1)) {
                throw new Exception("只有方阵才有逆");
            }
            var level = target.GetLength(0);
            double dMatrixValue = MatrixValue(target, level);
            if (dMatrixValue == 0) return null;
            double[,] dReverseMatrix = new double[level, 2 * level];
            double x, c;
            // Init Reverse matrix
            for (int i = 0; i < level; i++) {
                for (int j = 0; j < 2 * level; j++) {
                    if (j < level)
                        dReverseMatrix[i, j] = target[i, j];
                    else
                        dReverseMatrix[i, j] = 0;
                }
                dReverseMatrix[i, level + i] = 1;
            }
            for (int i = 0, j = 0; i < level && j < level; i++, j++) {
                if (dReverseMatrix[i, j] == 0) {
                    int m = i;
                    for (; target[m, j] == 0; m++) ;
                    if (m == level)
                        return null;
                    else {
                        // Add i-row with m-row
                        for (int n = j; n < 2 * level; n++)
                            dReverseMatrix[i, n] += dReverseMatrix[m, n];
                    }
                }
                // Format the i-row with "1" start
                x = dReverseMatrix[i, j];
                if (x != 1) {
                    for (int n = j; n < 2 * level; n++)
                        if (dReverseMatrix[i, n] != 0)
                            dReverseMatrix[i, n] /= x;
                }
                // Set 0 to the current column in the rows after current row
                for (int s = level - 1; s > i; s--) {
                    x = dReverseMatrix[s, j];
                    for (int t = j; t < 2 * level; t++)
                        dReverseMatrix[s, t] -= (dReverseMatrix[i, t] * x);
                }
            }
            // Format the first matrix into unit-matrix
            for (int i = level - 2; i >= 0; i--) {
                for (int j = i + 1; j < level; j++)
                    if (dReverseMatrix[i, j] != 0) {
                        c = dReverseMatrix[i, j];
                        for (int n = j; n < 2 * level; n++)
                            dReverseMatrix[i, n] -= (c * dReverseMatrix[j, n]);
                    }
            }
            double[,] dReturn = new double[level, level];
            for (int i = 0; i < level; i++)
                for (int j = 0; j < level; j++)
                    dReturn[i, j] = dReverseMatrix[i, j + level];
            return dReturn;
        }
        static double MatrixValue(double[,] matrixList, int level) {
            double[,] dMatrix = new double[level, level];
            for (int i = 0; i < level; i++)
                for (int j = 0; j < level; j++)
                    dMatrix[i, j] = matrixList[i, j];
            double c, x;
            int k = 1;
            for (int i = 0, j = 0; i < level && j < level; i++, j++) {
                if (dMatrix[i, j] == 0) {
                    int m = i;
                    for (; dMatrix[m, j] == 0; m++) ;
                    if (m == level)
                        return 0;
                    else {
                        // Row change between i-row and m-row
                        for (int n = j; n < level; n++) {
                            c = dMatrix[i, n];
                            dMatrix[i, n] = dMatrix[m, n];
                            dMatrix[m, n] = c;
                        }
                        // Change value pre-value
                        k *= (-1);
                    }
                }
                // Set 0 to the current column in the rows after current row
                for (int s = level - 1; s > i; s--) {
                    x = dMatrix[s, j];
                    for (int t = j; t < level; t++)
                        dMatrix[s, t] -= dMatrix[i, t] * (x / dMatrix[i, j]);
                }
            }
            double sn = 1;
            for (int i = 0; i < level; i++) {
                if (dMatrix[i, i] != 0)
                    sn *= dMatrix[i, i];
                else
                    return 0;
            }
            return k * sn;
        }
        /// <summary>
        /// 使用Gauss 消解法求方阵的det值
        /// </summary>
        /// <returns></returns>
        public static double Det(this double[,] m) {
            int i, j, k, nis = 0, js = 0, l, u, v;
            double f, det, q, d;
            // 初值
            f = 1.0;
            det = 1.0;
            // 消元
            var cols = m.GetLength(1);
            var rows = m.GetLength(0);
            if (cols != rows) throw new Exception("nor square,cols and rows must be equal.");
            var ele = new List<double>();
            for (int ii = 0; ii < rows; ii++) {
                for (int jj = 0; jj < cols; jj++) {
                    ele.Add(m[ii, jj]);
                }
            }
            var elements = new double[m.Length];
            ele.CopyTo(elements);
            ele = null;
            for (k = 0; k <= cols - 2; k++) {
                q = 0.0;
                for (i = k; i <= cols - 1; i++) {
                    for (j = k; j <= cols - 1; j++) {
                        l = i * cols + j;
                        d = Math.Abs(elements[l]);
                        if (d > q) {
                            q = d;
                            nis = i;
                            js = j;
                        }
                    }
                }
                if (q == 0.0) {
                    det = 0.0;
                    return (det);
                }
                if (nis != k) {
                    f = -f;
                    for (j = k; j <= cols - 1; j++) {
                        u = k * cols + j;
                        v = nis * cols + j;
                        d = elements[u];
                        elements[u] = elements[v];
                        elements[v] = d;
                    }
                }
                if (js != k) {
                    f = -f;
                    for (i = k; i <= cols - 1; i++) {
                        u = i * cols + js;
                        v = i * cols + k;
                        d = elements[u];
                        elements[u] = elements[v];
                        elements[v] = d;
                    }
                }
                l = k * cols + k;
                det = det * elements[l];
                for (i = k + 1; i <= cols - 1; i++) {
                    d = elements[i * cols + k] / elements[l];
                    for (j = k + 1; j <= cols - 1; j++) {
                        u = i * cols + j;
                        elements[u] = elements[u] - d * elements[k * cols + j];
                    }
                }
            }
            // 求值
            det = f * det * elements[cols * cols - 1];
            return det;
        }
        #endregion
        //
        #region serializer
        //json
        public static T JsonDeserialise<T>(this string json) {
            T obj = default(T);
            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json))) {
                var serializer = new DataContractJsonSerializer(typeof(T));
                obj = (T)serializer.ReadObject(ms);
                return obj;
            }
        }
        public static string JsonSerialize<T>(this T obj) {
            var serializer = new DataContractJsonSerializer(obj.GetType());
            using (var ms = new MemoryStream()) {
                serializer.WriteObject(ms, obj);
                var json = ms.ToArray();
                return Encoding.UTF8.GetString(json, 0, json.Length);
            }
        }
        //xml
        public static T XmlDeserialize<T>(this XNode xn) {
            T t = default(T);
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(T));
            var xr = xn.CreateReader();
            t = (T)xs.Deserialize(xr);
            return t;
        }
        public static string XmlSerialize<T>(this T t) {
            var xdoc = new StringBuilder();
            var xw = XmlWriter.Create(xdoc, new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true, IndentChars = "    ", NewLineChars = "\n" });
            xw.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
            var xsn = new XmlSerializerNamespaces();
            xsn.Add("", "");
            var serialize = new System.Xml.Serialization.XmlSerializer(typeof(T));
            serialize.Serialize(xw, t, xsn);
            return xdoc.ToString();
        }
        //byte[]
        public static T BytesDeserialize<T>(this byte[] bs) {
            var dsc = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
            var mem = new MemoryStream(bs);
            return (T)dsc.ReadObject(mem);
        }
        public static byte[] BytesSerialize<T>(this T obj) {
            var ms = new MemoryStream();
            var ser = new DataContractSerializer(typeof(T));
            ser.WriteObject(ms, obj);
            return ms.ToArray();
        }
        #endregion
        //
        #region xml contract serializer
        public static T XmlContractDeserialize<T>(this XNode xn) {
            T t = default(T);
            var xs = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
            var xr = XmlDictionaryReader.CreateDictionaryReader(xn.CreateReader());
            t = (T)xs.ReadObject(xr);
            return t;
        }
        public static string XmlContractSerialize<T>(this T t) {
            var xdoc = new StringBuilder();
            var xw = XmlWriter.Create(xdoc, new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true, IndentChars = "    ", NewLineChars = "\n" });
            xw.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
            var xdw = XmlDictionaryWriter.CreateDictionaryWriter(xw);
            var ser = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
            ser.WriteObject(xdw, t);
            xw.Dispose();
            return xdoc.ToString();
        }
        #endregion
        //
        #region collection
        public static HashSet<T> Intersect<T>(this HashSet<T> a, HashSet<T> b) {
            T[] temp = new T[a.Count];
            a.CopyTo(temp);
            var r = new HashSet<T>(temp);
            r.IntersectWith(b);
            return r;
        }
        public static HashSet<T> Except<T>(this HashSet<T> a, HashSet<T> b) {
            T[] temp = new T[a.Count];
            a.CopyTo(temp);
            var r = new HashSet<T>(temp);
            r.ExceptWith(b);
            return r;
        }
        public static HashSet<T> SymmetricExcept<T>(this HashSet<T> a, HashSet<T> b) {
            T[] temp = new T[a.Count];
            a.CopyTo(temp);
            var r = new HashSet<T>(temp);
            r.SymmetricExceptWith(b);
            return r;
        }
        public static HashSet<T> Union<T>(this HashSet<T> a, HashSet<T> b) {
            T[] temp = new T[a.Count];
            a.CopyTo(temp);
            var r = new HashSet<T>(temp);
            r.UnionWith(b);
            return r;
        }
        #endregion
        //
        #region color
        /// <summary>
        /// 把4个0-255的值 转成一个int代表颜色
        /// 从左到右按：
        /// A,R,G,B的顺序 按位组合而成,若要取出其中的byte[]那么请使用
        /// BitConverter.GetBytes(...) 方法
        /// </summary>
        /// <param name="a">alpha channel</param>
        /// <param name="r">red</param>
        /// <param name="g">green</param>
        /// <param name="b">blue</param>
        /// <returns></returns>
        public static uint GetColor(byte a, byte r, byte g, byte b) {
            uint color = 0;
            var bytelen = 8;
            color += (uint)a << bytelen * 0;
            color += (uint)r << bytelen * 1;
            color += (uint)g << bytelen * 2;
            color += (uint)b << bytelen * 3;
            return color;
        }
        public static byte[] GetArgb(uint value) {
            //return BitConverter.GetBytes(value).Reverse().ToArray();
            var x = new byte[4];
            var bytelen = 8;
            x[0] = (byte)(value >> bytelen * 0);
            x[1] = (byte)(value >> bytelen * 1);
            x[2] = (byte)(value >> bytelen * 2);
            x[3] = (byte)(value >> bytelen * 3);
            return x;
        }
        #endregion
        //
        #region search
        public static int BinSearch(this int[] arr, int key) {
            int left, right;
            int mid;
            left = 0;
            right = arr.Length;
            while (left <= right) {
                mid = (left + right) / 2;
                if (key < arr[mid]) {
                    right = mid - 1;
                } else if (key > arr[mid]) {
                    left = mid + 1;
                } else
                    return mid;
            }
            return -1;
        }
        public static int BinSearch(this float[] arr, int key) {
            int left, right;
            int mid;
            left = 0;
            right = arr.Length;
            while (left <= right) {
                mid = (left + right) / 2;
                if (key < arr[mid]) {
                    right = mid - 1;
                } else if (key > arr[mid]) {
                    left = mid + 1;
                } else
                    return mid;
            }
            return -1;
        }
        public static int BinSearch(this double[] arr, int key) {
            int left, right;
            int mid;
            left = 0;
            right = arr.Length;
            while (left <= right) {
                mid = (left + right) / 2;
                if (key < arr[mid]) {
                    right = mid - 1;
                } else if (key > arr[mid]) {
                    left = mid + 1;
                } else
                    return mid;
            }
            return -1;
        }
        #endregion
        //        
        //
        #region iterator        
        /// <summary>
        /// 从目标集合中切取指定索引开始的指定个数的子集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IEnumerable<T> CutOut<T>(this IEnumerable<T> array, int start, int count) {
            var index = 0;
            foreach (var item in array) {
                if (index >= start && count > 0) {
                    yield return item;
                    count--;
                }
                index++;
            }
        }
        /// <summary>
        /// 集合的长度对割序列
        /// </summary>
        /// <param name="len"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static IEnumerable<int> GetLengthHigh(this int len, int p = 2) {
            if (p < 2) p = 2;
            var x = len;
            do {
                yield return x;
                x /= 2;
            } while (x >= 1);
        }
        public static T[] GetItems<T>(params T[] ps) => ps;
        /// <summary>
        /// 洗牌算法
        /// </summary>
        /// <param name="data"></param>
        public static IEnumerable<T> Shuffle<T>(this T[] dataArray) {
            var ran = new Random();
            var count = dataArray.Count();
            var indexSet = count.UnorderFullIndex();
            foreach (var item in indexSet) {
                yield return dataArray[item];
            }
        }
        /// <summary>
        /// 获取指定范围的随机不重复整数
        /// </summary>
        /// <typeparam name="T">short or byte</typeparam>
        /// <param name="num">随机数组长度</param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public static IEnumerable<int> UnorderFullIndex(this int num) {
            var x = Enumerable.Range(0, num);
            var x2 = x.ToList();
            var ran = new Random();
            var max = num;
            var min = 0;
            var range = max - min;
            for (int i = 0; i < num; i++) {
                var x3 = ran.Next(range - i);
                yield return x2[x3];
                x2.Remove(x2[x3]); //利用移除后元素自动连续性重振不留的空位的特性，保持此算法为高效成立的算法。
            }
        }
        /// <summary>
        /// 2次方集合
        /// </summary>
        /// <param name="radix"></param>
        /// <param name="exp"></param>
        /// <returns></returns>
        public static IEnumerable<int> PowerSet(this byte radix, byte exp) {
            if (radix < 0 || exp < 0)
                throw new Exception("input argument is less than zero");
            if (exp > 32) {
                throw new Exception("the argument 'exp' out off the range");
            }
            var result = 1;
            for (int i = 0; i < exp; i++) {
                yield return result *= radix;
            }
        }
        public static int Fibonacci(this int n) {
            if (n < 0)
                throw new Exception("input argument is less than zero");
            if (n == 1 || n == 2) {
                return 1;
            }
            return (Fibonacci(n - 1) + Fibonacci(n - 2));
        }
        public static IEnumerable<int> FibonacciSet(this int n) {
            if (n < 0)
                throw new Exception("input argument is less than zero");
            for (int i = 1; i < n + 1; i++) {
                yield return Fibonacci(i);
            }
        }
        /// <summary>
        /// 对一维集合进行分段重组
        /// !! 需要在两个foreach的外部调用时才可以正确运行，否则可能
        /// 出现死循环错误。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="getNext">步进因为不是数组不能依靠下标++来实现while的步进所以要单独写上这个逻辑,这里需要返回下一个迭代对象</param>
        /// <param name="splitCondition">分段条件</param>
        /// <param name="filter">元素过滤条件</param>
        /// <returns>分组集合</returns>
        /// <example>
        /// var data = new[] { 1, 3, 8, 7, 5, 22, 5, 6, 4, 9, 82 };
        /// var x = data.SplitGroup(
        ///     i => i.idx < data.Length
        ///     , i => data[i.idx>=data.Length?i.idx-1:i.idx]
        ///     , i => i.item % 2 != 0
        ///     , i => true
        /// );
        /// </example>
        public static IEnumerable<IEnumerable<T>> SplitGroup<T>(this IEnumerable<T> data, Func<(int idx, T item), T> getNext, Predicate<(int idx, T item)> splitCondition, Predicate<T> filter = null) {
            filter ??= i => true;
            var count = data.Count();
            var pointer = data.First();
            var swit = default(bool);
            var isIdxOutOf = default(bool);
            var idx = 0;
            while (!isIdxOutOf) {
                swit = true;
                yield return IteratesElement();
            }
            /*
            重要的是条件上多个函数互相配合共同完成一个遍历逻辑
             */
            IEnumerable<T> IteratesElement() {
                while (!isIdxOutOf) {
                    if (splitCondition.Invoke((idx, pointer)) && swit) {
                        yield return pointer;

                        swit = !swit;
                    } else if (splitCondition.Invoke((idx, pointer)) && !swit) {
                        break;
                    } else if (!splitCondition.Invoke((idx, pointer)) && filter.Invoke(pointer)) {
                        yield return pointer;
                    }
                    ++idx;
                    if (idx >= count) {
                        idx -= 1;
                        isIdxOutOf = true;
                    };
                    pointer = getNext.Invoke((idx, pointer));

                    if (isIdxOutOf) {
                        idx += 1;
                    };
                }
            }
        }
        #endregion
        //
        #region least squares fit
        /// <summary>
        /// 通过多项式最小二乘计算插入x坐标值的函数值y
        /// </summary>
        /// <param name="equationData">多项式方程系数阵</param>
        /// <param name="x">要插入的x值</param>
        /// <returns>函数值y</returns>
        public static double GetMCValue(this double[] equationData, double x) {
            var r = equationData[0];
            for (int i = 1; i < equationData.Length; i++) {
                r += Math.Pow(x, i) * equationData[i];
            }
            return r;
        }
        public static string ToEquationString(this double[] equationData) {
            var r = string.Empty;
            for (int i = 0; i < equationData.Length; i++) {
                r += string.Format("{0}*x^{1}+", equationData[i].ToString(), i);
            }
            r = r.Substring(0, r.Length - 1);
            r = r.Replace("*x^0", "");
            r = "f(x)=" + r;
            return r;
        }
        /*
         * http://blog.sina.com.cn/s/blog_6e51df7f0100thie.html
         * 星星分享的博客
         */
        ///<summary>
        ///用最小二乘法拟合一元多次曲线
        ///例如：y=a0+a1*x 返回值则为a0 a1
        ///例如：y=a0+a1*x+a2*x*x 返回值则为a0 a1 a2
        ///</summary>
        ///<param name="ps">一组待考查2d坐标点数据集合</param>
        ///<param name="dimension">方程的最高次数</param>
        public static double[] MultiCurveEquation(this (double, double)[] ps, int dimension) { //二元多次线性方程拟合曲线
            var length = ps.Length;
            var arrX = (from i in ps select i.Item1).ToArray();
            var arrY = (from i in ps select i.Item2).ToArray();
            int n = dimension + 1; //dimension次方程需要求 dimension+1个 系数
            double[,] Guass = new double[n, n + 1]; //高斯矩阵 例如：y=a0+a1*x+a2*x*x
            for (int i = 0; i < n; i++) {
                int j;
                for (j = 0; j < n; j++) {
                    Guass[i, j] = SumArr(arrX, j + i, length);
                }
                Guass[i, j] = SumArr(arrX, i, arrY, 1, length);
            }
            return ComputGauss(Guass, n);
        }
        static double SumArr(double[] arr, int n, int length) { //求数组的元素的n次方的和
            double s = 0;
            for (int i = 0; i < length; i++) {
                if (arr[i] != 0 || n != 0)
                    s = s + Math.Pow(arr[i], n);
                else
                    s = s + 1;
            }
            return s;
        }
        static double SumArr(double[] arr1, int n1, double[] arr2, int n2, int length) {
            double s = 0;
            for (int i = 0; i < length; i++) {
                if ((arr1[i] != 0 || n1 != 0) && (arr2[i] != 0 || n2 != 0))
                    s = s + Math.Pow(arr1[i], n1) * Math.Pow(arr2[i], n2);
                else
                    s = s + 1;
            }
            return s;
        }
        static double[] ComputGauss(double[,] Guass, int n) {
            int i, j;
            int k, m;
            double temp;
            double max;
            double s;
            double[] x = new double[n];
            for (i = 0; i < n; i++) x[i] = 0.0; //初始化
            for (j = 0; j < n; j++) {
                max = 0;
                k = j;
                for (i = j; i < n; i++) {
                    if (Math.Abs(Guass[i, j]) > max) {
                        max = Guass[i, j];
                        k = i;
                    }
                }
                if (k != j) {
                    for (m = j; m < n + 1; m++) {
                        temp = Guass[j, m];
                        Guass[j, m] = Guass[k, m];
                        Guass[k, m] = temp;
                    }
                }
                if (0 == max) {
                    // "此线性方程为奇异线性方程"
                    return x;
                }
                for (i = j + 1; i < n; i++) {
                    s = Guass[i, j];
                    for (m = j; m < n + 1; m++) {
                        Guass[i, m] = Guass[i, m] - Guass[j, m] * s / (Guass[j, j]);
                    }
                }
            } //结束for (j=0;j<n;j++)
            for (i = n - 1; i >= 0; i--) {
                s = 0;
                for (j = i + 1; j < n; j++) {
                    s = s + Guass[i, j] * x[j];
                }
                x[i] = (Guass[i, n] - s) / Guass[i, i];
            }
            return x;
        } //返回值是函数的系数
        #endregion
        //
        #region io
        /// <summary>
        /// copy stream to memory stream then get it byte[]
        /// </summary>
        /// <param name="s"></param>
        /// <param name="target">usually it is a memory stream</param>
        public static T CopyStreamTo<T>(this Stream s, T target) where T : Stream {
            var reader = new BinaryReader(s);
            var writer = new BinaryWriter(target);
            writer.Seek((int)target.Length, SeekOrigin.Begin);
            byte[] buffer = null;
            do {
                buffer = reader.ReadBytes(256);
                writer.Write(buffer);
            } while (buffer.Length > 0);
            writer.Flush();
            target.Position = 0;
            return target;
        }
        public static void SaveTo(this string content, string path) => File.WriteAllText(path, content);
        #endregion
        //
        #region type
        static IEnumerable<string> GetStandardTypeName_Help(Type t) {
            var x = t.GetGenericArguments();
            if (x.Length > 0) {
                yield return t.Name.Split('`')[0];
                yield return "<";
                foreach (var item in x) {
                    foreach (var item2 in GetStandardTypeName_Help(item)) { yield return item2; }
                }
                yield return ">";
            } else {
                yield return t.Name;
            }
        }
        /// <summary>
        /// 获取类型名，主要用于泛型名的正确显示，例如：
        /// Dictionary<string,object> 如果不使用本方法，那么将可能 
        /// 显示为Dictionary`2
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string GetStandardTypeName(this Type t) {
            if (!t.IsGenericType) {
                return t.Name;
            }
            var x = GetStandardTypeName_Help(t);
            var x2 = x.ToArray();
            var x3 = string.Join(",", x2);
            var x4 = x3.Replace(",<,", "<");
            return x4.Replace(",>", ">");
        }
        public static bool IsPrimaryType(this Type t) {
            var r = false;
            if (t.Name == "Nullable`1")
                t = t.GetGenericArguments()[0];
            var x1 = t.IsPrimitive;
            var x2 = t == typeof(DateTime);
            var x3 = t == typeof(TimeSpan);
            var x4 = t == typeof(string);
            var x5 = t == typeof(decimal);
            r = x1 || x2 || x3 || x4 || x5;
            return r;
        }
        public static bool CanSqlType(this Type t) {
            var r = false;
            if (t.Name == "Nullable`1")
                t = t.GenericTypeArguments[0];
            var dotNetTypes = new string[] { "Int32", "String", "Boolen", "DateTime", "Decimal", "Double", "Byte[]", "Single", "String", "String", "DateTime", "Int16", "String", "Int64", "Byte[]", "String", "String", "Decimal", "Single", "Single", "Object", "Byte[]", "Byte", "Guid", "Byte[]", "String" };
            if ((from it in dotNetTypes.Distinct() where it == t.Name select it).Count() == 1) r = true;
            return r;
        }
        public static string GetGenericBaseName(this Type t) {
            if (!t.IsGenericType) {
                return t.Name;
            }
            return t.GetGenericTypeDefinition().Name.Split('`').First();
        }
        public static bool IsCollection(this Type t) {
            return
            typeof(IEnumerable).IsAssignableFrom(t)
            || t.IsAssignableTo(typeof(IEnumerable));
        }
        #endregion
        //
        #region ef core generator
        /// <summary>
        /// 根据指定名称空间中包含的实体，构造其ef  DbContext对象
        /// </summary>
        /// <param name="assembly">限于被引用过的dll</param>
        /// <param name="namesps"></param>
        /// <param name="path"></param>
        public static void GenDbcByNsps(this Assembly assembly, string namesps, string path, string dbcon = "") {
            var types = assembly.GetTypes();
            var oa = from i in types where i.Namespace == namesps && i.IsClass select i;
            var nps = new CsNamespace();
            nps.Using($".,.Linq,Microsoft.EntityFrameworkCore,{namesps}");
            var ctx = nps.StartClass("OaContext", inhlist: "DbContext");
            var ctor = ctx.StartConstructor("");
            var ctxconfig = ctx.StartMethod("OnConfiguring", "DbContextOptionsBuilder op", "void", "protected override");
            ctxconfig.Sentence($"//op.UseNpgsql(\"{dbcon}\")");
            ctxconfig.Sentence($"//op.UseSqlServer(\"{dbcon}\")");
            ctxconfig.Sentence($"//op.UseSqlite(\"{dbcon}\")");
            ctxconfig.Sentence($"//op.UseMySQL(\"{dbcon}\")");
            ctxconfig.Sentence($"op.UseInMemoryDatabase(\"testdb\")");
            var ctxms = ctx.StartMethod("OnModelCreating", "ModelBuilder mb", "void", "protected override");
            foreach (var item in oa) {
                ctx.Property(item.Name + "s", $"DbSet<{item.Name}>");
                var temp = $"{item.Name.ToLower()}Config";
                ctxms.Sentence($"var {temp} = mb.Entity<{item.Name}>()");
                foreach (var it in item.GetProperties()) {
                    if (it.Name.Trim() == "Id") {
                        ctxms.Sentence($"{temp}.HasKey(x=>x.Id)");
                    } else {
                        if (it.PropertyType == typeof(string)) {
                            ctxms.Sentence($"{temp}.Property(x=>x.{it.Name}).HasColumnName(\"{it.Name}\").HasColumnType(\"{CalculateLengthByName(it.Name)}\").IsRequired(false)");
                        } else {
                            ctxms.Sentence($"{temp}.Property(x=>x.{it.Name}).HasColumnName(\"{it.Name}\").HasColumnType(\"{FactoryDbCode.NetType2SqlType(it.PropertyType)}\").IsRequired(false)");
                        }
                    }
                }
            }
            nps.ToString(namesps).SaveTo(path);
        }
        static string CalculateLengthByName(string name) {
            var len = string.Empty;
            var mark = name.ToLower().Trim();
            switch (mark) {
                case "name":
                len = "nvarchar(20)";
                break;
                case "pwd":
                case "password":
                len = "varchar(100)";
                break;
                case "summary":
                len = "nvarchar(200)";
                break;
                case "phone":
                len = "varchar(20)";
                break;
                case "email":
                len = "varchar(50)";
                break;
                case "address":
                len = "nvarchar(100)";
                break;
                case string i when i.Contains("_xml"):
                len = "xml";
                break;
                case string i when i.Contains("xml_"):
                len = "xml";
                break;
                case string i when i.Contains("jsonb_"):
                len = "jsonb";
                break; //postgresql
                case string i when i.Contains("_jsonb"):
                len = "jsonb";
                break; //postgresql
                case string i when i.Contains("_json"):
                len = "json";
                break; //postgresql
                case string i when i.Contains("json_"):
                len = "json";
                break; //postgresql
                case string i when i.Contains("name"):
                len = "nvarchar(80)";
                break;
                case string i when i.Contains("code"):
                len = "varchar(50)";
                break;
                case string i when i.Contains("phone"):
                len = "varchar(20)";
                break;
                case string i when i.Contains("remark"):
                goto default;
                case string i when i.Contains("mark"):
                len = "nvarchar(10)";
                break;
                case string i when i.Contains("node"):
                goto default;
                case string i when i.Contains("title"):
                len = "nvarchar(50)";
                break;
                case string i when i.Contains("label"):
                len = "nvarchar(80)";
                break;
                case string i when i.Contains("content"):
                len = "varchar(max)";
                break;
                case string i when i.Contains("descri"):
                len = "nvarchar(200)";
                break;
                case string i when i.Contains("explain"):
                len = "nvarchar(1000)";
                break;
                case string i when i.Contains("article"):
                len = "text";
                break;
                case string i when i.Contains("blog"):
                len = "text";
                break;
                case string i when i.Contains("addr"):
                len = "nvarchar(500)";
                break; //网络地址有些比较长
                case string i when i.Contains("email"):
                len = "varchar(50)";
                break; //网络地址有些比较长
                case string i when i.Contains("pwd"):
                len = "varchar(100)";
                break; //网络地址有些比较长
                case string i when i.Contains("no"):
                len = "varchar(10)";
                break;
                default:
                len = "nvarchar(50)";
                break;
            }
            return len;
        }
        /// <summary>
        /// 设计为返回任意结果集，在实例化泛型的时候使用ValueTuple去做
        /// </summary>
        /// <typeparam name="T">型如:(int id,string col1,string col2) 这样的ValueTuple去做为T</typeparam>
        /// <param name="dbcon">连接对象</param>
        /// <param name="sql">要执行的sql语句，任意可能是 exec(sp_xxx @a,@b) 等</param>
        /// <returns></returns>
        public static IEnumerable<T> DbQuery<T>(this DbConnection dbcon, string sql)
        where T : new() {
            try {
                dbcon.Open();
                var cmd = dbcon.CreateCommand();
                cmd.CommandText = sql;
                var reader = cmd.ExecuteReader();
                var columnSchema = reader.GetColumnSchema();
                var tt = typeof(T);
                if (tt.GetProperties().Length == 0 && tt.GetFields().Length > 0) {
                    var pis = (from i in tt.GetFields() orderby i.Name select i).ToDictionary(i => i.Name);
                    while (reader.Read()) {
                        var temp = new T();
                        foreach (var item in columnSchema) {
                            var name = item.ColumnName;
                            pis[name].SetValue(temp, reader[name]);
                        }
                        yield return temp;
                    }
                } else {
                    var pis = (from i in tt.GetProperties() select i).ToDictionary(i => i.Name);
                    while (reader.Read()) {
                        var temp = new T();
                        foreach (var item in columnSchema) {
                            var name = item.ColumnName;
                            pis[name].SetValue(temp, reader[name]);
                        }
                        yield return temp;
                    }
                }
            } finally {
                dbcon.Close();
            }
        }
        #endregion
        //
        #region Expression
        public static Expression Foreach<T>(this ParameterExpression source, Func<ParameterExpression, Expression> loopContent) {
            var elementType = typeof(T);
            var item = Expression.Variable(elementType, "item");
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(elementType);
            var enumeratorVar = Expression.Variable(enumeratorType, "enumerator");
            var getEnumeratorCall = Expression.Call(source, enumerableType.GetMethod("GetEnumerator"));
            var enumeratorAssign = Expression.Assign(enumeratorVar, getEnumeratorCall);
            var moveNextCall = Expression.Call(enumeratorVar, typeof(System.Collections.IEnumerator).GetMethod("MoveNext"));
            var breakLabel = Expression.Label("LoopBreak");
            var ifThenElseExpr = Expression.IfThenElse(
                Expression.Equal(moveNextCall, Expression.Constant(true)),
                Expression.Block(new[] { item },
                    Expression.Assign(item, Expression.Property(enumeratorVar, "Current")),
                    loopContent.Invoke(item)
                ),
                Expression.Break(breakLabel)
            );
            var loop = Expression.Loop(ifThenElseExpr, breakLabel);
            var block = Expression.Block(new[] { enumeratorVar },
                enumeratorAssign,
                loop
            );
            return block;
        }
        #endregion
        //
        #region TestDocResult
        /// <summary>
        /// 将测试文档构造为html文档
        /// </summary>
        /// <param name="results"></param>
        public static string TestDocToHtml<T>(this string title, params TestDocContext<T>[] results) {
            var idx = 0;
            var result =
                "<!DOCTYPE html>\n" +
                "<html lang = 'en'>\n" +
                "<head>\n" +
                "<meta charset = 'UTF-8'>\n" +
                $"<title>test report</title>\n" +
                "<head>\n" +
                "   <style>\n" +
                "   body{" +
                "       margin:0;\n" +
                "       padding:0;\n" +
                "   }\n" +
                "   header{\n" +
                "       height:80px;\n" +
                "       background-color:silver;\n" +
                "       padding:0;\n" +
                "       z-index:1;\n" +
                "       position:fixed;\n" +
                "       width:100%;\n" +
                "   }\n" +
                "   header::before{\n" +
                "       content: '';\n" +
                "       display: table;\n" +
                "   }\n" +
                "   header>h2{\n" +
                "       margin-left: 20px;\n" +
                "       color: #686868;\n" +
                "   }\n" +
                "   main{\n" +
                "       position:absolute;\n" +
                "       left:10%;\n" +
                "       width:80%;\n" +
                "       top:85px;\n" +
                "   }\n" +
                "   main>aside{\n" +
                "       width:25%;\n" +
                "       position:absolute;\n" +
                "       left:0;\n" +
                "       border-right: 1px solid #888;\n" +
                "   }\n" +
                "   main>article{\n" +
                "       width:74%;\n" +
                "       position:absolute;\n" +
                "       right:0;\n" +
                "       left: 26%;\n" +
                "   }\n" +
                ".table {\n" +
                "border: 1px solid #676;\n" +
                "border-collapse: collapse;\n" +
                "}\n" +

                ".table th, .table td {\n" +
                "color: #575757;\n" +
                "border: 1px solid #686;\n" +
                "padding: 5px 10px;\n" +
                "}\n" +

                ".table th {\n" +
                "background-color: #2c3ba0;\n" +
                "color: skyblue;\n" +
                "}\n" +

                ".table>tbody>tr:nth-child(even) {\n" +
                "background-color: #e0e8ef;\n" +
                "}\n" +

                "   </style>\n" +
                "<script  type='text/javascript'>\n" +
                "       function detail(idx){\n" +
                "           var sumid='sum'+idx;\n" +
                "           var rid='r'+idx;\n" +
                "           var sumtd=document.getElementById(sumid);\n" +
                "           var rtd=document.getElementById(rid);\n" +
                "           var sump=document.querySelector('body>main>aside>p:nth-of-type(1)');\n" +
                "           var rp=document.querySelector('body>main>aside>p:nth-of-type(2)');\n" +
                "           sump.innerText=sumtd.innerText;\n" +
                "           rp.innerText=rtd.innerText;\n" +
                "       }\n" +
                "   window.onload=()=>{\n" +
                "       function getPositionTop(node) \n{" +
                "           var top = node.offsetTop;\n" +
                "           \n" +
                "           var parent = node.offsetParent;\n" +
                "           while (parent != null) {\n" +
                "               top += parent.offsetTop;\n" +
                "               parent = parent.offsetParent;\n" +
                "           }\n" +
                "           return top;\n" +
                "       }\n" +
                "       var aside = document.querySelector('body>main>aside');\n" +
                "       //需要固定的元素距离顶部的距离\n" +
                "       var offsetTop=85;\n" +
                "       var asideTop = getPositionTop(aside)+aside.offsetHeight-offsetTop;//offsetTop是 对header的偏移值，因为header为 fixed所以不被计算在内可能是这样\n" +
                "       var deltaTop = 0;\n" +
                "       window.onscroll = function(){\n" +
                "           var lostHeight = document.documentElement.scrollTop || document.body.scrollsToTop;\n" +
                "           if(lostHeight>asideTop){\n" +
                "               deltaTop=lostHeight-asideTop;\n" +
                "               aside.style.top=deltaTop+offsetTop+'px';\n" +
                "           }else{\n" +
                "               aside.style='';\n" +
                "           }\n" +
                "       }" +
                "   }\n" +
                "</script>\n" +
                "</head>\n" +
                "<body>\n" +
                "   <header>\n" +
                $"       <h2>{title}</h2>\n" +
                "   </header>\n" +
                "   <main>\n" +
                "      <aside>\n" +
                "       <h3>详细描述:</h3>\n" +
                "       <p>\n" +
                "       <h3>测试结果:</h3>\n" +
                "       <p>\n" +
                "      </aside>\n" +
                "      <article>\n" +
                "         <table class='table'>\n" +
                "             <thead>\n" +
                "                 <tr>\n" +
                "                     <th>序号</th>\n" +
                "                     <th>概述</th>\n" +
                "                     <th>测试结果</th>\n" +
                "                     <th>时间</th>\n" +
                "                     <th>是否成功</th>\n" +
                "                     <th>详细</th>\n" +
                "                 </tr>\n" +
                "             </thead>\n" +
                $"               <tbody>{string.Join(Environment.NewLine, from i in results let j = idx++ select i.ToHtmlTbody(j))}</tbody>\n" +
                "         </table>\n" +
                "      </article>\n" +
                "   </main>\n" +
                "</body>\n" +
                "</html>";
            return result;
        }
        #endregion
        #region log and exception
        static public Exception Throw(this string title, params string[] details) {
            var sb = new StringBuilder();
            sb.AppendLine(title);
            foreach (var i in details) {
                sb.AppendLine(i);
            }
            return new Exception(sb.ToString());
        }
        #endregion
        #region debug
        static public void Debug(Action work){
            
        }
        #endregion
    }
}