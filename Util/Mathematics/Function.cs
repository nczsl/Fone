// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using Util.Ex;
/* 设计思想：
概念：单链
包括：基单链，复合单链，组单链
在计算两个FuncRadical merge的时候，其中一个FuncRadical必须是单链
以单链的token做为查询对象对另一个FuncRadical 进行查询,而
这个被查询的FuncRadical则是该FuncRadical所同level的一个
集合。对于+，-，*，/ 四则运算来说，这个集合FuncRadical就是
merge函数的左参数，而对于^,&,|来说，则是它的右参数
然后依据运算pattern决定查询到相同token的变换方式。

概念：token注册表
对每个FuncRadical 进行注册，注册为一个计数id此id是一个
递增的整数，用于构造token，token是一个由整数排序构造的
字符串，通过这种方式，对计算单元进行编码，降噪的处理，然后
就可以进行代数运算了
*/
namespace Util.Mathematics.Function {

    public enum PatternType {
        Add, Subtract, Multiply
            , Divide, Pow, Log, Root
            , Sin, Cos, Tan, Cot, Sec, Csc
            , Cross,
    }
    public enum ValueType {
        None, Scalar, Vector, Matrix, Sign,
    }
    public class Radical {
        private string sign;
        private double scalar;
        private Map<double> matrix;
        private PatternType pattern;
        private ValueType type;
        public bool IsNone => Type == ValueType.None;

        public string Sign {
            get => sign; set {
                sign = value;
                type = ValueType.Sign;
            }
        }
        public double Scalar {
            get => scalar; set {
                scalar = value;
                type = ValueType.Scalar;
            }
        }
        public ValueType Type { get => type; set => type = value; }
        public PatternType Pattern { get => pattern; set => pattern = value; }

        public static explicit operator double(Radical r) {
            if (r.Type == ValueType.Scalar) {
                return r.Scalar;
            }
            throw new Exception("非实数 类型不能转换为实数");
        }
        public static implicit operator string(Radical r) => r.Type switch {
            ValueType.Sign => r.Sign,
            ValueType.Scalar => r.Scalar.ToString(),
            _ => string.Empty
        };
        public override string ToString() => (string)this;
        public static explicit operator Map<double>(Radical r) {
            if (r.Type == ValueType.Vector) {
                return r.matrix;
            }
            throw new Exception("非向量 类型不能转换为向量");
        }
        public Radical(PatternType pt = PatternType.Add) {
            Pattern = pt;
            Type = ValueType.None;
        }
        public Radical(double v, PatternType pt = PatternType.Add) {
            Scalar = v;
            Pattern = pt;
            Type = ValueType.Scalar;
        }
        public Radical(string v, PatternType pt = PatternType.Add) {
            Sign = v;
            Pattern = pt;
            Type = ValueType.Sign;
        }
        public Radical(Map<double> v, PatternType pt = PatternType.Add) {
            matrix = v;
            Pattern = pt;
            Type = ValueType.Matrix;
        }
    }
    public class FuncRadical {
        public int Id { get; set; }
        public int Group { get; set; }
        public int Level { get; set; }
        public Radical Radical { get; set; }
        internal FuncRadical left, right;
        public FuncRadical(Radical content, int level = 0) {
            this.Radical = content;
            this.Level = level;
        }
        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append($"{nameof(Id)}:{Id},");
            sb.Append($"{nameof(Group)}:{Group},");
            sb.Append($"{nameof(Level)}:{Level},");
            sb.Append($"{nameof(Radical)}:{Radical};");
            return sb.ToString();
        }
    }
    public class FuncRadicalChain {
        private FuncRadicalChain() {
            ResetHash();
            currentDic = new Dictionary<int, FuncRadical>();
        }
        void ResetHash() {
            this.signhash = new HashSet<string>();
            this.signhash.Add(string.Empty);
            this.signhash.Add("__num");
        }
        internal static FuncRadicalChain GetInstance() => new FuncRadicalChain();
        FuncRadicalChain instance;
        public FuncRadicalChain Instance {
            get {
                if (instance == null) instance = new FuncRadicalChain();
                return instance;
            }
        }
        int idline;
        public FuncRadical root, current;
        public Dictionary<int, FuncRadical> currentDic;
        public void ReLoad(FuncRadical fr) {
            this.current = this.root = fr.GetRoot();
            this.ResetHash();
            this.currentDic.Clear();
            foreach (var i in this.Query()) {
                signhash.Add(i.GetSignCode());
                currentDic.Add(i.Id, i);
            }
            idline = currentDic.Count;
        }
        public FuncRadical FindId(int id) => currentDic[id];
        public void Registry(Radical r, int level = 0) {
            var fr = new FuncRadical(r, level);
            if (root == null) {
                current = root = new FuncRadical(r, level);
            } else {
                fr.left = current;
                current.right = fr;
                current = current.right;
            }
            current.Id = idline;
            current.Group = current.Id;
            signhash.Add(current.GetSignCode());
            currentDic.Add(current.Id, current);
            idline++;
        }
        public HashSet<string> signhash;

        public IEnumerable<FuncRadical> Query(Predicate<FuncRadical> filter = null) {
            if (filter == null) filter = i => true;
            this.current = this.root;
            while (this.current != null) {
                if (filter(this.current)) {
                    yield return this.current;
                }
                this.current = this.current.right;
            }
        }
    }
    public class FuncRadicalHandler {
        public FuncRadicalChain chain;
        public FuncRadicalHandler() {
            chain = FuncRadicalChain.GetInstance();
        }
        public void Build(params (Radical radical, int level)[] rs) {
            foreach (var item in rs) {
                chain.Registry(item.radical, item.level);
            }
            if (chain.root.Level > 0) {
                var fr = new FuncRadical(new Radical());
                fr.right = chain.root;
                chain.root.left = fr;
                chain.ReLoad(fr);
            }
            Target = chain.root;
        }
        public string Name { get; set; }
        public FuncRadical Target { get; set; }
        sbyte patternchannel;
        public (int leftId, int rightId, int leftidx, int rightidx) FindHot(Map<int> coding, List<int> groupColumnIdxs) {
            var result = default((int leftId, int rightId, int leftidx, int rightidx));
            // coding.GetColumn()
            var maxlevel = coding.GetRow(1, groupColumnIdxs).Max();
            Debug.WriteLine(maxlevel, nameof(maxlevel));
            Debug.WriteLine(string.Join(",", groupColumnIdxs), nameof(groupColumnIdxs));
            var maxordersOfmaxlevel = from i in groupColumnIdxs
                                      where coding[1, i] == maxlevel
                                      select i;
            var maxorderOfmaxlevel = (from i in maxordersOfmaxlevel
                                      select coding[5, i]).Max();
            var hot = -1;
            Debug.WriteLine(maxorderOfmaxlevel, nameof(maxorderOfmaxlevel));
            if (maxorderOfmaxlevel == 2) {
                hot = (from i in maxordersOfmaxlevel
                       where coding[5, i] == maxorderOfmaxlevel
                       select i).Last();
            } else {
                hot = (from i in maxordersOfmaxlevel
                       where coding[5, i] == maxorderOfmaxlevel
                       select i).First();
            }
            var hotidx = groupColumnIdxs.IndexOf(hot);
            Debug.WriteLine(hotidx, nameof(hotidx));
            // hot matching
            var pattern1 = (hotidx - 1 < 0, hotidx + 1 >= groupColumnIdxs.Count);
            var pattern2 = (0, 0, false, false, false, 0, 0, 0);
            int getLevel(int left, int right) {
                int r = 0;
                switch ((left, right)) {
                    case (int, int) k when k.left > k.right: r = 1; break;
                    case (int, int) k when k.left < k.right: r = 2; break;
                    case (int, int) k when k.left == k.right: r = 0; break;
                }
                return r;
            }
            Debug.WriteLine(pattern1, nameof(pattern1));
            switch (pattern1) {
                case (true, true):
                // result = (hot, null);
                throw new Exception("only one radical but need two");
                case (true, false):
                pattern2 = (-1, getLevel(coding[1, groupColumnIdxs[hotidx]], coding[1, groupColumnIdxs[hotidx + 1]]), false, coding[4, groupColumnIdxs[hotidx]] == 0, coding[4, groupColumnIdxs[hotidx + 1]] == 0, -1, coding[5, groupColumnIdxs[hotidx]], coding[5, groupColumnIdxs[hotidx + 1]]);
                Debug.WriteLine(pattern2, nameof(pattern2));
                switch (pattern2) {
                    case (_, 0, _, _, _, _, 0, 0): patternchannel = 0; result = (groupColumnIdxs[hotidx], groupColumnIdxs[hotidx + 1], hotidx, hotidx + 1); break;
                    case (_, 2, _, true, _, _, 3, 0): patternchannel = 1; result = (groupColumnIdxs[hotidx], groupColumnIdxs[hotidx + 1], hotidx, hotidx + 1); break;
                    case (_): throw "Unexpected function construction".Throw($"pattern 1:{pattern1}", $"pattern 2:{pattern2}");
                }
                break;
                case (false, true):
                pattern2 = (getLevel(coding[1, groupColumnIdxs[hotidx - 1]], coding[1, groupColumnIdxs[hotidx]]), -1, false, coding[4, groupColumnIdxs[hotidx]] == 0, false, coding[5, groupColumnIdxs[hotidx - 1]], coding[5, groupColumnIdxs[hotidx]], -1);
                Debug.WriteLine(pattern2, nameof(pattern2));
                switch (pattern2) {
                    case (0, _, false, false, _, 0, 1, _): patternchannel = 2; result = (groupColumnIdxs[hotidx - 1], groupColumnIdxs[hotidx], hotidx - 1, hotidx); break;
                    case (0, _, false, false, _, 0, 2, _): patternchannel = 3; result = (groupColumnIdxs[hotidx - 1], groupColumnIdxs[hotidx], hotidx - 1, hotidx); break;
                    case (0, _, false, false, _, 1, 2, _): patternchannel = 4; result = (groupColumnIdxs[hotidx - 1], groupColumnIdxs[hotidx], hotidx - 1, hotidx); break;
                    case (0, _, false, false, _, 2, 2, _): patternchannel = 5; result = (groupColumnIdxs[hotidx - 1], groupColumnIdxs[hotidx], hotidx - 1, hotidx); break;
                    case (0, _, true, _, _, _, 3, _): patternchannel = 6; result = (groupColumnIdxs[hotidx - 1], groupColumnIdxs[hotidx], hotidx - 1, hotidx); break;
                    case (2, _, true, _, _, _, 3, _): patternchannel = 7; result = (groupColumnIdxs[hotidx - 1], groupColumnIdxs[hotidx], hotidx - 1, hotidx); break;
                    case (_): throw "Unexpected function construction".Throw($"pattern 1:{pattern1}", $"pattern 2:{pattern2}");
                }
                break;
                case (false, false):
                pattern2 = (getLevel(coding[1, groupColumnIdxs[hotidx - 1]], coding[1, groupColumnIdxs[hotidx]]), getLevel(coding[1, groupColumnIdxs[hotidx]], coding[1, groupColumnIdxs[hotidx + 1]]), coding[4, groupColumnIdxs[hotidx - 1]] == 0, coding[4, groupColumnIdxs[hotidx]] == 0, coding[4, groupColumnIdxs[hotidx + 1]] == 0, coding[5, groupColumnIdxs[hotidx - 1]], coding[5, groupColumnIdxs[hotidx]], coding[5, groupColumnIdxs[hotidx + 1]]);
                Debug.WriteLine(pattern2, nameof(pattern2));
                switch (pattern2) {
                    case (2, 1, true, _, _, _, 0, _): patternchannel = 8; result = (groupColumnIdxs[hotidx - 1], groupColumnIdxs[hotidx], hotidx - 1, hotidx); break;
                    case (2, 0, true, _, _, _, 0, 0): patternchannel = 9; result = (groupColumnIdxs[hotidx], groupColumnIdxs[hotidx + 1], hotidx, hotidx + 1); break;
                    case (0, _, _, _, _, 0, 1, _): patternchannel = 10; result = (groupColumnIdxs[hotidx - 1], groupColumnIdxs[hotidx], hotidx - 1, hotidx); break;
                    case (0, _, _, _, _, 0, 2, _): patternchannel = 11; result = (groupColumnIdxs[hotidx - 1], groupColumnIdxs[hotidx], hotidx - 1, hotidx); break;
                    case (0, _, _, _, _, 1, 2, _): patternchannel = 12; result = (groupColumnIdxs[hotidx - 1], groupColumnIdxs[hotidx], hotidx - 1, hotidx); break;
                    case (0, _, _, _, _, 2, 2, _): patternchannel = 13; result = (groupColumnIdxs[hotidx - 1], groupColumnIdxs[hotidx], hotidx - 1, hotidx); break;
                    case (0, _, true, _, _, _, 3, _): patternchannel = 14; result = (groupColumnIdxs[hotidx - 1], groupColumnIdxs[hotidx], hotidx - 1, hotidx); break;
                    case (2, _, true, _, _, _, 3, _): patternchannel = 15; result = (groupColumnIdxs[hotidx - 1], groupColumnIdxs[hotidx], hotidx - 1, hotidx); break;

                    case (_): throw "Unexpected function construction".Throw($"pattern 1:{pattern1}", $"pattern 2:{pattern2}");
                }
                break;
            }
            Debug.WriteLine(patternchannel, nameof(patternchannel));
            Debug.WriteLine(result, nameof(result));
            return result;
        }
        void Calculate(Map<int> coding, List<int> groupColumnIdxs, int leftColNo, int rightColNo, int leftIdx, int rightIdx) {
            var left = chain.currentDic[coding[0, leftColNo]];
            var right = chain.currentDic[coding[0, rightColNo]];
            Debug.WriteLine(left, nameof(left));
            Debug.WriteLine(right, nameof(right));
            Debug.WriteLine("---------------------");
            var lastRidx = rightIdx + 1 < groupColumnIdxs.Count ? rightIdx + 1 : groupColumnIdxs.Count - 1;
            var leftCols = Enumerable.Range(groupColumnIdxs[leftIdx], groupColumnIdxs[rightIdx] - groupColumnIdxs[leftIdx]);
            var rightCols = Enumerable.Range(groupColumnIdxs[rightIdx], lastRidx == rightIdx ? 1 : groupColumnIdxs[lastRidx] - groupColumnIdxs[rightIdx]);
            var currentLevel = coding[1, rightColNo];
            var leftNumbers = coding.GetRow(6, leftCols);
            var rightNumbers = coding.GetRow(6, rightCols);
            var standardSign = "";
            var leftGroupSign = string.Concat(leftNumbers.OrderBy(i => i));
            var rightGroupSign = string.Concat(rightNumbers.OrderBy(i => i));
            Debug.WriteLine(string.Join(",", leftCols), nameof(leftCols));
            Debug.WriteLine(string.Join(",", rightCols), nameof(rightCols));
            Debug.WriteLine(currentLevel, nameof(currentLevel));
            Debug.WriteLine(string.Join(",", leftNumbers), nameof(leftNumbers));
            Debug.WriteLine(string.Join(",", rightNumbers), nameof(rightNumbers));
            Debug.WriteLine(leftGroupSign, nameof(leftGroupSign));
            Debug.WriteLine(rightGroupSign, nameof(rightGroupSign));
            if ((leftGroupSign == "1" && rightGroupSign == "1") || (leftGroupSign == "0" && rightGroupSign == "1")) {
                left.UpdateScalar(right);
            }
            if (coding[6, leftColNo] == 0) {
                MergeInLeft(left, right);
            } else {
                if (coding[5, rightColNo] == 2) {
                    standardSign = leftGroupSign;
                } else {
                    standardSign = rightGroupSign;
                }
            }
            static void MergeInLeft(FuncRadical left, FuncRadical right) {
                foreach (var i in right.GetGroup()) {
                    i.Group = left.Group;
                    Debug.WriteLine(i, "right group");
                }
            }
        }
        ///<summary>
        /// 基本计算过程，数值和符号混合型
        ///</summary>
        public void Procedrue() {
            var coding = this.chain.Encoding();
            Debug.WriteLine(Environment.NewLine + coding.Print(), nameof(coding));
            if (new HashSet<int>(coding.GetRow(2)).Count <= 1) {
                return;
            }
            var groupColumnIdxs = coding.GetGroupColumnNo().ToList();
            patternchannel = -1;
            var hot = FindHot(coding, groupColumnIdxs);
            Calculate(coding, groupColumnIdxs, hot.leftId, hot.rightId, hot.leftidx, hot.rightidx);
            Debug.WriteLine(hot.leftId, nameof(hot.leftId));
            Debug.WriteLine(hot.rightId, nameof(hot.rightId));
            Procedrue();
        }
    }
    public interface IParser {
        FuncRadical Parse(string exp);
    }
    public class FunctionParser : IParser {
        public FuncRadical Parse(string exp) {
            throw new NotImplementedException();
        }
    }
    public class FunctionContext {
        readonly IParser parser;
        readonly Dictionary<string, FuncRadical> funcs;
        readonly FuncRadicalHandler frHandler;
    }
    public static class FuncEx {
        static public void Insert(this FuncRadical fr,FuncRadical newFr){
            
        }
        static public (FuncRadical left, FuncRadical right) GetGroupEndpoint(this FuncRadical fr) {
            var result = default((FuncRadical left, FuncRadical right));
            var group = fr.Group;
            var temp = fr;
            while (temp.left != null && temp.left.Group == group) {
                temp = temp.left;
            }
            result.left = temp;
            temp = fr;
            while (temp.right != null && temp.right.Group == group) {
                temp = temp.right;
            }
            result.right = temp;
            return result;
        }
        static public void DeleteGroup(this FuncRadical fr) {
            var group = fr.Group;
            var temp = fr;
            while (temp.right != null && temp.right.Group == group) {
                temp = temp.right;
            }
            while (temp.left != null && temp.left.Group == group) {
                temp.right = null;
                temp = temp.left;
            }
            fr = null;
        }
        static public void DeleteGroup(this (FuncRadical left, FuncRadical right) fr) {
            var temp = fr.right;
            while (temp != fr.left) {
                temp.right = null;
                temp = temp.left;
            }
            fr.left = null;
        }
        static public void RemoveGroup(this (FuncRadical left, FuncRadical right) fr) {
            var _left = fr.left.left;
            var _right = fr.right.right;
            DeleteGroup(fr);
            switch ((_left, _right)) {
                case (null, null): break;
                case (FuncRadical, null):
                _left.right = null;
                break;
                case (null, FuncRadical):
                _right.left = null;
                break;
                case (FuncRadical, FuncRadical):
                _left.right = _right; _right.left = _left;
                break;
            }
        }
        static public void RemoveGroup(this FuncRadical fr) {
            if (fr == null) throw new Exception("main pointer must don't null");
            var (left, right) = fr.GetGroupEndpoint();
            var _left = left.left;
            var _right = right.right;
            DeleteGroup(fr);
            switch ((_left, _right)) {
                case (null, null): break;
                case (FuncRadical, null):
                _left.right = null;
                break;
                case (null, FuncRadical):
                _right.left = null;
                break;
                case (FuncRadical, FuncRadical):
                _left.right = _right; _right.left = _left;
                break;
            }
        }
        static public FuncRadical ReplaceGroup(this (FuncRadical left, FuncRadical right) fr, (FuncRadical left, FuncRadical right) newFr) {
            var _left = fr.left.left;
            var _right = fr.right.right;
            DeleteGroup(fr);
            switch ((_left, _right)) {
                case (null, null): break;
                case (FuncRadical, null):
                _left.right = newFr.left;
                newFr.left.left = _left;
                break;
                case (null, FuncRadical):
                _right.left = newFr.right;
                newFr.right.right = _right;
                break;
                case (FuncRadical, FuncRadical):
                _left.right = newFr.left;
                newFr.left.left = _left;
                _right.left = newFr.right;
                newFr.right.right = _right;
                break;
            }
            return newFr.left;
        }
        static public FuncRadical ReplaceGroup(this FuncRadical fr, FuncRadical newFr) {
            if (fr == null) throw new Exception("main pointer must don't null");
            var (left, right) = fr.GetGroupEndpoint();
            var _left = left.left;
            var _right = right.right;
            var (newLeft, newRight) = newFr.GetGroupEndpoint();
            DeleteGroup(fr);
            switch ((_left, _right)) {
                case (null, null): break;
                case (FuncRadical, null):
                _left.right = newLeft;
                newLeft.left = _left;
                break;
                case (null, FuncRadical):
                _right.left = newRight;
                newRight.right = _right;
                break;
                case (FuncRadical, FuncRadical):
                _left.right = newLeft;
                newLeft.left = _left;
                _right.left = newRight;
                newRight.right = _right;
                break;
            }
            return newFr;
        }
        static public void UpdateScalar(this FuncRadical left, FuncRadical right) {
            switch ((left.Radical.Type, right.Radical.Type)) {
                case (ValueType.Scalar, ValueType.Scalar):
                switch (right.Radical.Pattern) {
                    case PatternType.Add:
                    if(left.Radical.Pattern==PatternType.Subtract){
                        left.Radical.Scalar*=-1;
                        left.Radical.Pattern=PatternType.Add;
                    }
                    left.Radical.Scalar += right.Radical.Scalar;
                    break;
                    case PatternType.Subtract:
                    if(left.Radical.Pattern==PatternType.Subtract){
                        left.Radical.Scalar*=-1;
                        left.Radical.Pattern=PatternType.Add;
                    }
                    left.Radical.Scalar -= right.Radical.Scalar;
                    break;
                    case PatternType.Multiply: left.Radical.Scalar *= right.Radical.Scalar; break;
                    case PatternType.Divide: left.Radical.Scalar /= right.Radical.Scalar; break;
                    case PatternType.Pow: left.Radical.Scalar = Math.Pow(left.Radical.Scalar, right.Radical.Scalar); break;
                    case PatternType.Log: left.Radical.Scalar = Math.Log(left.Radical.Scalar, right.Radical.Scalar); break;
                    case PatternType.Root: left.Radical.Scalar = Math.Pow(left.Radical.Scalar, 1d / right.Radical.Scalar); break;
                }
                break;
                case (ValueType.None, ValueType.Scalar):
                left.Radical.Type = ValueType.Scalar;
                if (right.Radical.Pattern == PatternType.Subtract) {
                    right.Radical.Scalar *= -1;
                }
                left.Radical.Scalar = right.Radical.Scalar;
                break;
            }
            right.RemoveGroup();
        }
        static public IEnumerable<T> GetRow<T>(this Map<T> m, int rowNo, IEnumerable<int> columnNos) where T : struct {
            foreach (var i in columnNos) {
                yield return m[rowNo, i];
            }
        }
        static public IEnumerable<T> GetColumn<T>(this Map<T> m, int columnNo, IEnumerable<int> rowNos) where T : struct {
            foreach (var i in rowNos) {
                yield return m[columnNo, i];
            }
        }
        static public IEnumerable<FuncRadical> GetGroup(this FuncRadical fr) {
            var group = fr.Group;
            var temp = fr;
            while (temp.left != null && temp.left.Group == group) {
                temp = temp.left;
            }
            while (temp != null && temp.Group == group) {
                yield return temp;
                temp = temp.right;
            }
        }
        static public IEnumerable<int> GetGroupColumnNo(this Map<int> coding) {
            var marks = new HashSet<int>();
            for (var i = 0; i < coding.Column; i++) {
                if (!marks.Contains(coding[2, i])) {
                    yield return i;
                }
                marks.Add(coding[2, i]);
            }
        }
        static public Map<int> Encoding(this FuncRadicalChain frc) {
            var r = new Map<int>();
            r.ReserveSpace(7, frc.Query().Count());
            foreach (var i in frc.Query()) {
                r.AppendColumn(i.Id, i.Level, i.Group, (int)i.Radical.Pattern, (int)i.Radical.Type, i.Radical.Pattern.GetOrder(), frc.GetSignCode(i));
            }
            return r;
        }
        static public string GetSignCode(this FuncRadical fr) {
            var sign = string.Empty;
            sign = fr.Radical.Type switch {
                ValueType.Sign => fr.Radical.Sign,
                ValueType.Scalar => "__num",
                _ => string.Empty
            };
            return sign;
        }
        static public int GetSignCode(this FuncRadicalChain frc, FuncRadical fr) {
            var code = 0;
            switch (fr.Radical.Type) {
                case ValueType.None:
                break;
                case ValueType.Scalar:
                code = frc.signhash.ToList().IndexOf("__num");
                break;
                case ValueType.Sign:
                code = frc.signhash.ToList().IndexOf(fr.Radical.Sign);
                break;
            }
            return code;
        }
        public static FuncRadical GetRightGroup(this FuncRadical currentGroup) {
            var result = currentGroup;
            while (result != null && result.Group == currentGroup.Group) {
                result = result.right;
            }
            return result;
        }
        public static FuncRadical GetLeftGroup(this FuncRadical currentGroup) {
            var result = currentGroup;
            while (result != null && result.Group == currentGroup.Group) {
                result = result.left;
            }
            return result;
        }
        public static IEnumerable<FuncRadical> GetElement(this FuncRadical currentGroup) {
            var groupNo = currentGroup.Group;
            var pointer = currentGroup;
            while (pointer != null && pointer.Group == groupNo) {
                yield return pointer;
                pointer = pointer.right;
            }
        }
        public static IEnumerable<IEnumerable<FuncRadical>> GetItems(this IEnumerable<FuncRadical> frs, int level = 0) {
            var x = frs.SplitGroup(
                i => i.item.right
                , i => i.item.Level == level && i.item.Radical.Pattern.GetOrder() == 0
            );
            foreach (var it in x) {
                var list = new List<FuncRadical>();
                foreach (var iu in it) {
                    list.Add(iu);
                }
                yield return list;
            }
        }
        public static IEnumerable<IEnumerable<FuncRadical>> GetFactors(this IEnumerable<FuncRadical> frs, int level = 0) {
            var x = frs.SplitGroup(
                i => i.item.right
                , i => i.item.Level == level && i.item.Radical.Pattern.GetOrder() <= 1
            );
            foreach (var it in x) {
                var list = new List<FuncRadical>();
                foreach (var iu in it) {
                    list.Add(iu);
                }
                yield return list;
            }

        }
        public static IEnumerable<FuncRadical> GetIndeies(this FuncRadical currentGroup, int level) {
            return from i in currentGroup.GetElement()
                   where i.Radical.Pattern.GetOrder() == 2 && i.Level == level
                   select i;
        }
        ///<summary>
        /// 对给定基元取其唯一标识（token）
        ///</summary>
        // public static string GetToken(this FuncRadical ele) {
        //     throw new NotImplementedException();
        // }
        public static FuncRadical GetRoot(this FuncRadical fr) {
            var temp = fr;
            while (temp.left != null) {
                temp = temp.left;
            }
            return temp;
        }
        static public IEnumerable<FuncRadical> Query(this FuncRadical fr) {
            var temp = fr;
            while (temp != null) {
                yield return temp;
                temp = temp.right;
            }
        }
        static string PrintNormal(this Radical radical) {
            var r = string.Empty;
            switch (radical.Type) {
                case ValueType.Scalar:
                r = $"{radical.Pattern.ToSign()} {radical.Scalar}";
                break;
                case ValueType.Sign:
                r = $"{radical.Pattern.ToSign()} {radical.Sign}";
                break;
                case ValueType.None:
                r = $"{radical.Pattern.ToSign()}";
                break;
            }
            return r;
        }
        public static string Print(this IEnumerable<FuncRadical> nodes) {
            void PrintRadical(Radical radical, int bracketPointer, bool isEnd, List<string> list) {
                var x3 = "";
                var x5 = PrintNormal(radical);
                var x4 = "";
                var absbp = Math.Abs(bracketPointer);
                if (bracketPointer > 0) {
                    for (int i = 0; i < absbp; i++) {
                        x4 += " ( ";
                    }
                    x3 = x4 + x5;
                } else if (bracketPointer < 0) {
                    for (int i = 0; i < absbp; i++) {
                        x4 += " ) ";
                    }
                    if (isEnd) {
                        x3 = x4;
                    } else {
                        x3 = x4 + x5;
                    }
                } else {
                    x3 = x5;
                }
                list.Add(x3);
            }
            var result = string.Empty;
            var list = new List<string>();
            int? bracketPointer = 0;
            foreach (var item in nodes) {
                if (item.left == null) {
                    bracketPointer = item.Level - 0;
                } else {
                    bracketPointer = item.Level - item.left.Level;
                }
                PrintRadical(item.Radical, bracketPointer.Value, false, list);
            }
            bracketPointer = 0 - nodes?.Last().Level;
            //bracketPointer = temp.Priority - temp.left.Priority;
            if (bracketPointer < 0) {
                PrintRadical(nodes?.Last().Radical, bracketPointer.Value, true, list);
            }
            var x = string.Join(" ", list);
            result = Regex.Replace(x, @"\(\s*\+", @"(");
            return result;
        }
        public static int GetOrder(this PatternType pattern) => pattern switch {
            PatternType.Add => 0,
            PatternType.Subtract => 0,
            PatternType.Multiply => 1,
            PatternType.Divide => 1,
            PatternType.Pow => 2,
            PatternType.Log => 2,
            PatternType.Root => 2,
            PatternType.Sin => 3,
            PatternType.Cos => 3,
            PatternType.Tan => 3,
            PatternType.Cot => 3,
            PatternType.Sec => 3,
            PatternType.Csc => 3,
            _ => -1
        };
        public static string ToSign(this PatternType pattern) => pattern switch {
            PatternType.Add => "+",
            PatternType.Subtract => "-",
            PatternType.Multiply => "*",
            PatternType.Divide => "/",
            PatternType.Pow => "^",
            PatternType.Log => "&",
            PatternType.Root => "|",
            PatternType.Sin => "sin",
            PatternType.Cos => "cos",
            PatternType.Tan => "tan",
            PatternType.Cot => "cot",
            PatternType.Sec => "sec",
            PatternType.Csc => "csc",
        };
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
        static double Cofactor(this double[,] self, int row, int col) {
            throw new NotImplementedException();
        }
        public static double Dot(this double[] self, double[] other) {
            if (self.Length != other.Length) {
                throw new Exception("向量长度不相等不能相乘");
            }
            var result = 0d;
            for (var i = 0; i < self.Length; i++) {
                result += self[i] * other[i];
            }
            return result;
        }
        public static double Corss(this double[] self, double[] other) {
            throw new NotImplementedException();
        }
        public static double Norm(this double[] self) {
            var temp = default(double);
            for (var i = 0; i < self.Length; i++) {
                temp += Math.Pow(self[i], 2);
            }
            return Math.Sqrt(temp);
        }
        static public Map<T> Transpose<T>(this Map<T> self) where T : struct {
            var result = new Map<T>();
            result.ReserveSpace(self.Column, self.Row);
            for (var i = 0; i < self.Row; i++) {
                result.AppendColumn(self.GetRow(i).ToArray());
            }
            return result;
        }
        static public Map<T> Symmetric<T>(this Map<T> self) where T : struct {
            if (self.Row < 2 || self.Column < 2) throw new ArgumentException("dimension must greater than 2");
            if (self.Row != self.Column) throw new ArgumentException("row must equal column");
            var result = new Map<T>();
            result.ReserveSpace(self.Row, self.Column);
            var tself = self.Transpose();
            result = self + tself;
            for (var i = 0; i < result.Length; i++) {
                var temp = double.Parse(result[i].ToString());
                result[i] = (1d / 2d * temp).ToString().NumberConvert<T>();
            }
            return result;
        }
        static public Map<T> Antisymmetric<T>(this Map<T> self) where T : struct {
            if (self.Row < 2 || self.Column < 2) throw new ArgumentException("dimension must greater than 2");
            if (self.Row != self.Column) throw new ArgumentException("row must equal column");
            var result = new Map<T>();
            result.ReserveSpace(self.Row, self.Column);
            var tself = self.Transpose();
            result = self - tself;
            for (var i = 0; i < result.Length; i++) {
                var temp = double.Parse(result[i].ToString());

                result[i] = (1d / 2d * temp).ToString().NumberConvert<T>();
            }
            return result;
        }
        static public U NumberConvert<U>(this string numberStr)
        where U : struct {
            var result = default(U);
            switch (Type.GetTypeCode(typeof(U))) {
                case TypeCode.Int16: result = (U)(object)(Int16)Int16.Parse(numberStr); break;
                case TypeCode.Int32: result = (U)(object)(Int32)Int32.Parse(numberStr); break;
                case TypeCode.Int64: result = (U)(object)(Int64)Int64.Parse(numberStr); break;
                case TypeCode.Single: result = (U)(object)(Single)Single.Parse(numberStr); break;
                case TypeCode.Double: result = (U)(object)(Double)Double.Parse(numberStr); break;
                case TypeCode.Decimal: result = (U)(object)(Decimal)Decimal.Parse(numberStr); break;
                case TypeCode.Byte: result = (U)(object)(Byte)Byte.Parse(numberStr); break;
                case TypeCode.SByte: result = (U)(object)(SByte)SByte.Parse(numberStr); break;
                case TypeCode.UInt16: result = (U)(object)(UInt16)UInt16.Parse(numberStr); break;
                case TypeCode.UInt32: result = (U)(object)(UInt32)UInt32.Parse(numberStr); break;
                case TypeCode.UInt64: result = (U)(object)(UInt64)UInt64.Parse(numberStr); break;
            }
            return result;
        }
        static public Func<T, U> NumberConvert<T, U>(this T number)
        where U : struct
        where T : struct {
            var result = default(Func<T, U>);
            var input = Expression.Parameter(typeof(T));
            var local = Expression.Variable(typeof(U));
            var assignment = Expression.Assign(local, input);
            switch ((Type.GetTypeCode(typeof(U)), Type.GetTypeCode(typeof(T)))) {
                case (TypeCode.Int16, TypeCode.Int16): break;
            }
            result = Expression.Lambda<Func<T, U>>(
                Expression.Block(new[] { local }, assignment)
                , new[] { input }).Compile();
            return result;
        }
        static public Map<U> AntisymmetricIdentityOperator<U>(this int dimension) where U : struct {
            //首先构造上三角阵
            Map<U> trangler;
            Map<U> result;
            trangler = new();
            trangler.ReserveSpace(dimension, dimension);
            result = new();
            result.ReserveSpace(dimension, dimension);
            var convert = Map<U>.NumberConvert<int>();
            // var convertf = Map<U>.NumberConvert<float>();
            // var convert2=Map<U>.NumberConvert<U>();
            var mark = true;
            for (var r = 0; r < dimension; r++) {
                for (var c = 0; c < dimension; c++) {
                    if (r < c) {
                        if (mark) {
                            trangler[r, c] = convert(2);
                        } else {
                            trangler[r, c] = convert(-2);
                        }
                    } else {
                        trangler[r, c] = convert(0);
                    }
                    mark = !mark;
                }
                if (r % 2 == 0) mark = false;
                else mark = true;
            }
            var trangler_t = trangler.Transpose();
            result = trangler - trangler_t;
            var mul = Map<U>.BuildBinaryExpression(PatternType.Multiply);
            for (var r = 0; r < dimension; r++) {
                for (var c = 0; c < dimension; c++) {
                    if (!result[r, c].Equals(default(U))) {
                        result[r, c] = (int.Parse(result[r, c].ToString()) * 1 / 2).ToString().NumberConvert<U>();
                    }
                }
            }
            return result;
        }
    }
    //
    public class Map<T> where T : struct {
        T[] data;
        int pointer;
        public int Row { get; set; }
        public int Column { get; set; }
        public int Length => this.data.Length;
        public Map() {
            pointer = 0;
        }
        public void Load(T[,] data) {
            this.Row = data.GetLength(0);
            this.Column = data.GetLength(1);
            this.data = new T[this.Row * this.Column];
            for (var i = 0; i < this.Row; i++) {
                for (var j = 0; j < this.Column; j++) {
                    this.data[this.pointer++] = data[i, j];
                }
            }
        }
        public void Load(T[] data, int row, int column) {
            if (data.Length != row * column) throw new Exception("number of row times column is not mapping the data length.");
            this.data = new T[data.Length];
            this.Row = row;
            this.Column = column;
            foreach (var i in data) {
                this.data[this.pointer++] = i;
            }
        }
        public void UpdateDiagonal(int no, bool isTop, bool isMain, IEnumerable<T> diagonal) {
            if (no < 0 || no > this.Row) throw new ArgumentException("parameter out of range.");
            var rp = 0; var cp = 0; var idx = 0;
            switch ((isTop, isMain)) {
                case (true, false):
                rp = 0; cp = no;
                while (rp >= 0 && rp < this.Row && cp >= 0 && cp < this.Column) {
                    this[rp++, cp--] = diagonal.ElementAt(idx++);
                }
                break;
                case (false, false):
                rp = this.Row - 1; cp = no;
                while (rp >= 0 && rp < this.Row && cp >= 0 && cp < this.Column) {
                    this[rp--, cp++] = diagonal.ElementAt(idx++);
                }
                break;
                case (true, true):
                rp = 0; cp = no;
                while (rp >= 0 && rp < this.Row && cp >= 0 && cp < this.Column) {
                    this[rp++, cp++] = diagonal.ElementAt(idx++);
                }
                break;
                case (false, true):
                rp = this.Row - 1; cp = no;
                while (rp >= 0 && rp < this.Row && cp >= 0 && cp < this.Column) {
                    this[rp--, cp++] = diagonal.ElementAt(idx++);
                }
                break;
            }
        }
        public IEnumerable<T> GetDiagonal(int no, bool isTop = true, bool isMain = false) {
            if (no < 0 || no > this.Row) throw new ArgumentException("parameter out of range.");
            var rp = 0; var cp = 0;
            switch ((isTop, isMain)) {
                case (true, false):
                rp = 0; cp = no;
                while (rp >= 0 && rp < this.Row && cp >= 0 && cp < this.Column) {
                    yield return this[rp++, cp--];
                }
                break;
                case (false, false):
                rp = this.Row - 1; cp = no;
                while (rp >= 0 && rp < this.Row && cp >= 0 && cp < this.Column) {
                    yield return this[rp--, cp++];
                }
                break;
                case (true, true):
                rp = 0; cp = no;
                while (rp >= 0 && rp < this.Row && cp >= 0 && cp < this.Column) {
                    yield return this[rp++, cp++];
                }
                break;
                case (false, true):
                rp = this.Row - 1; cp = no;
                while (rp >= 0 && rp < this.Row && cp >= 0 && cp < this.Column) {
                    yield return this[rp--, cp--];
                }
                break;
            }
        }
        ///<summary>
        /// .net 6 的预览功能INumber<T>的抽象静态方法正式版出来
        /// 之前的一个解决方案，使用表达式树技术替代多次泛型运算所
        /// 带来的频繁类型转换和box unbox。
        ///</summary>
        static public Func<T, T, T> BuildBinaryExpression(PatternType pt) {
            var result = default(Func<T, T, T>);
            var left = Expression.Parameter(typeof(T));
            var right = Expression.Parameter(typeof(T));
            switch (pt) {
                case PatternType.Add:
                result = Expression.Lambda<Func<T, T, T>>(
                    Expression.Add(left, right), new[] { left, right }
                ).Compile();
                break;
                case PatternType.Subtract:
                result = Expression.Lambda<Func<T, T, T>>(
                    Expression.Subtract(left, right), new[] { left, right }
                ).Compile();
                break;
                case PatternType.Multiply:
                result = Expression.Lambda<Func<T, T, T>>(
                    Expression.Multiply(left, right), new[] { left, right }
                ).Compile();
                break;
                case PatternType.Divide:
                result = Expression.Lambda<Func<T, T, T>>(
                    Expression.Divide(left, right), new[] { left, right }
                ).Compile();
                break;
                case PatternType.Pow:
                result = Expression.Lambda<Func<T, T, T>>(
                    Expression.Call(typeof(System.Math).GetMethod("Pow", new[] { typeof(T), typeof(T) }), left, right), new[] { left, right }
                ).Compile();
                break;
                case PatternType.Log:
                result = Expression.Lambda<Func<T, T, T>>(
                    Expression.Call(typeof(System.Math).GetMethod("Log", new[] { typeof(T), typeof(T) }), right, left), new[] { left, right }
                ).Compile();
                break;
                case PatternType.Root:
                result = Expression.Lambda<Func<T, T, T>>(
                    Expression.Call(typeof(System.Math).GetMethod("Pow", new[] { typeof(T), typeof(T) })
                    , left
                    , Expression.Divide(Expression.Constant(1d), right)), new[] { left, right }
                ).Compile();
                break;
            }
            return result;
        }
        ///<summary>
        /// .net 6 的预览功能INumber<T>的抽象静态方法正式版出来
        /// 之前的一个解决方案，使用表达式树技术替代多次泛型运算所
        /// 带来的频繁类型转换和box unbox。
        ///</summary>
        static public Func<T, T> BuildUnaryExpression(PatternType pt) {
            var result = default(Func<T, T>);
            var target = Expression.Parameter(typeof(T));
            switch (pt) {
                case PatternType.Sin:
                result = Expression.Lambda<Func<T, T>>(
                    Expression.Call(
                        typeof(System.Math).GetMethod("Sin"
                        , new[] { typeof(T) })
                        , target
                    )
                    , target
                ).Compile();
                break;
                case PatternType.Cos:
                result = Expression.Lambda<Func<T, T>>(
                    Expression.Call(
                        typeof(System.Math).GetMethod("Cos"
                        , new[] { typeof(T) })
                        , target
                    )
                    , target
                ).Compile();
                break;
                case PatternType.Tan:
                result = Expression.Lambda<Func<T, T>>(
                    Expression.Call(
                        typeof(System.Math).GetMethod("Tan"
                        , new[] { typeof(T) })
                        , target
                    )
                    , target
                ).Compile();
                break;
                case PatternType.Cot:
                result = Expression.Lambda<Func<T, T>>(
                    Expression.Divide(
                        Expression.Constant(1d),
                        Expression.Call(
                            typeof(System.Math).GetMethod("Tan"
                            , new[] { typeof(T) })
                            , target
                        )
                    )
                    , target
                ).Compile();
                break;
                case PatternType.Sec:
                result = Expression.Lambda<Func<T, T>>(
                    Expression.Divide(
                        Expression.Constant(1d),
                        Expression.Call(
                            typeof(System.Math).GetMethod("Cos"
                            , new[] { typeof(T) })
                            , target
                        )
                    )
                    , target
                ).Compile();
                break;
                case PatternType.Csc:
                result = Expression.Lambda<Func<T, T>>(
                    Expression.Divide(
                        Expression.Constant(1d),
                        Expression.Call(
                            typeof(System.Math).GetMethod("Sin"
                            , new[] { typeof(T) })
                            , target
                        )
                    )
                    , target
                ).Compile();
                break;
            }
            return result;
        }
        static public Func<U, T> NumberConvert<U>() where U : struct {
            var result = default(Func<U, T>);
            // var input = default(ParameterExpression);
            var input = Expression.Parameter(typeof(U));
            var local = Expression.Variable(typeof(T));

            var assignment = Expression.Assign(local, input);
            result = Expression.Lambda<Func<U, T>>(
                Expression.Block(new[] { local }, assignment)
                , new[] { input }).Compile();
            return result;
        }
        ///<summary>
        /// 矩阵乘法
        ///<summary>
        static public Map<T> operator *(Map<T> left, Map<T> right) {
            if (left.Column != right.Row)
                throw new Exception("multiply matrices do not match");
            var result = new Map<T>();
            result.ReserveSpace(left.Row, right.Column);
            var same = left.Column;
            var add = Map<T>.BuildBinaryExpression(PatternType.Add);
            var mul = Map<T>.BuildBinaryExpression(PatternType.Multiply);
            for (var row = 0; row < left.Row; row++) {
                for (var column = 0; column < right.Column; column++) {
                    for (var item = 0; item < same; item++) {
                        result[row, column] = add(result[row, column], mul(left[row, item], right[item, column]));
                    }
                }
            }
            return result;
        }
        static public Map<T> operator *(T left, Map<T> right) {
            // var add = Map<T>.BuildBinaryExpression(PatternType.Add);
            var mul = Map<T>.BuildBinaryExpression(PatternType.Multiply);
            var result = new Map<T>();
            result.ReserveSpace(right.Row, right.Column);
            for (var i = 0; i < right.Row; i++) {
                for (var j = 0; j < right.Column; j++) {
                    result[i, j] = mul(left, right[i, j]);
                }
            }
            return result;
        }
        static public Map<T> operator +(Map<T> left, Map<T> right) {
            if (left.Row != right.Row) throw new ArgumentException("add operator must have same Row");
            if (left.Column != right.Column) throw new ArgumentException("add operator must have same Column");
            var result = new Map<T>();
            var add = Map<T>.BuildBinaryExpression(PatternType.Add);
            result.ReserveSpace(left.Row, left.Column);
            for (var i = 0; i < left.Row; i++) {
                for (var j = 0; j < left.Column; j++) {
                    result[i, j] = add(left[i, j], right[i, j]);
                }
            }
            return result;
        }
        static public Map<T> operator -(Map<T> left, Map<T> right) {
            if (left.Row != right.Row) throw new ArgumentException("add operator must have same Row");
            if (left.Column != right.Column) throw new ArgumentException("add operator must have same Column");
            var result = new Map<T>();
            var subtract = Map<T>.BuildBinaryExpression(PatternType.Subtract);
            result.ReserveSpace(left.Row, left.Column);
            for (var i = 0; i < left.Row; i++) {
                for (var j = 0; j < left.Column; j++) {
                    result[i, j] = subtract(left[i, j], right[i, j]);
                }
            }
            return result;
        }
        static public Map<T> Identity(int size) {
            var identity = new Map<T>();
            identity.ReserveSpace(size, size);
            for (var i = 0; i < size; i++) {
                identity[i, i] = "1".NumberConvert<T>();
            }
            return identity;
        }
        /*
        * 求行列式值
        */
        public T Delta() {
            throw new NotImplementedException();
        }
        ///<summary>
        /// 预定数据空间
        ///</summary>
        public void ReserveSpace(int row, int column) {
            this.data = new T[row * column];
            this.Row = row; this.Column = column;
            this.rowoffset = 0;
            this.columnoffset = 0;
        }
        ///<summary>
        /// 预定数据空间
        ///</summary>
        public void ReserveSpace(int legth) {
            this.Row = 1; this.Column = legth;
            this.data = new T[legth];
            this.rowoffset = 0;
            this.columnoffset = 0;
        }
        ///<summary>
        /// 通过行和列重置空间大小
        ///</summary>
        public void ResetSapce(int row, int column) {
            var newlen = row * column;
            if (newlen != this.Length) {
                var newstore = new T[newlen];
                Array.Copy(this.data, newstore, this.Length);
                this.data = newstore;
                this.Row = row; this.Column = column;
            } else {
                this.Row = row; this.Column = column;
            }
        }
        static public Map<T> Mul(Map<T> left, Map<T> right) {
            var result = new Map<T>();
            if (left.Length != right.Length) throw new ArgumentException("left length and right length must eqaul.");
            if (left.Row != right.Row) throw new ArgumentException("left Row and right Row must eqaul.");
            if (left.Column != right.Column) throw new ArgumentException("left Column and right Column must eqaul.");
            var mul = Map<T>.BuildBinaryExpression(PatternType.Multiply);
            var samerow = left.Row;
            var samecolumn = right.Column;
            result.ReserveSpace(samerow, samecolumn);
            for (var i = 0; i < samerow; i++) {
                for (var j = 0; j < samecolumn; j++) {
                    result[i, j] = mul(left[i, j], right[i, j]);
                }
            }
            return result;
        }
        static public Map<T> Cross(T[] left, T[] right) {
            var result = new Map<T>();
            var _left = new Map<T>();
            var _right = new Map<T>();
            _left.Load(left, 1, left.Length);
            _right.Load(right, right.Length, 1);
            if (_left.Length != _right.Length) throw new ArgumentException("_left length and _right length must eqaul.");
            var same = _left.Length;
            var add = Map<T>.BuildBinaryExpression(PatternType.Add);
            var mul = Map<T>.BuildBinaryExpression(PatternType.Multiply);
            var unionVectorProduct = _right * _left;
            var ai = same.AntisymmetricIdentityOperator<T>();
            var _result = Map<T>.Mul(unionVectorProduct, ai);
            var _rarray = new List<T>();
            foreach (var i in new[] { 1, 0 }) {
                for (var j = same - 1; j >= i; j--) {
                    var temp = default(T);
                    foreach (var t in _result.GetDiagonal(j, i % 2 == 0)) {
                        temp = add(temp, t);
                    }
                    _rarray.Add(temp);
                }
            }
            result.ReserveSpace(1, _rarray.Count - 2);
            result.AppendRow(_rarray.ToArray()[1..^1]);
            // System.Console.WriteLine(unionVectorProduct.Print());
            // System.Console.WriteLine(ai.Print());
            // System.Console.WriteLine(_result.Print());
            return result;
        }
        int rowoffset, columnoffset;
        // public bool IsVector=>this.Row==1&&this.Column>0;
        public void AppendRow(params T[] row) {
            if (this.rowoffset < this.Row && row.Length <= this.Column) {
                var p = this.rowoffset * this.Column;
                foreach (var i in row) {
                    this.data[p++] = i;
                }
                this.rowoffset++;
            }
        }
        public void AppendColumn(params T[] column) {
            if (this.columnoffset < this.Column && column.Length <= this.Row) {
                var p = this.columnoffset;
                foreach (var i in column) {
                    this.data[p] = i;
                    p += this.Column;
                }
                this.columnoffset++;
            }
        }
        public IEnumerable<T> GetRow(int rowno) {
            return ReadVector(rowno, true);
        }
        public IEnumerable<T> GetColumn(int columnno) {
            return ReadVector(columnno, false);
        }
        public void UpdateRow(int rowno, IEnumerable<T> row) {
            if (row.Count() > this.Column) throw new ArgumentException("input row count is out of column range");
            var cp = 0;
            foreach (var r in row) {
                this[rowno, cp++] = r;
            }
        }
        public void UpdateColumn(int columnno, IEnumerable<T> column) {
            if (column.Count() > this.Row) throw new ArgumentException("input column count is out of row range");
            var rp = 0;
            foreach (var c in column) {
                this[rp++, columnno] = c;
            }
        }
        /**
        * 返回行或列向量
        * colorrow:column or row ,the true represent row,
        * false is column
        */
        IEnumerable<T> ReadVector(int no, bool roworcol) {
            if (roworcol) {
                for (var i = 0; i < this.Column; i++) {
                    yield return this[no, i];
                }
            } else {
                for (var i = 0; i < this.Row; i++) {
                    yield return this[i, no];
                }
            }
        }
        public IEnumerable<T> Query(int start, int step, int count) {
            var nostart = 0;
            var nostep = 0;
            var nocount = 0;
            foreach (var i in this) {
                nostart++;
                if (nostart < start) {
                    continue;
                }
                nostep++;
                if (nostep == step) {
                    yield return i;
                    nostep = 0;
                    nocount++;
                }
                if (nocount == count) {
                    break;
                }
            }
        }
        public byte Dimension => (this.Row, this.Column) switch { (1, >= 1) => 1, ( > 1, >= 1) => 2 };
        public T this[int row, int column] {
            get {
                var p = row * this.Column + column;
                if (p >= this.data.Length) throw new IndexOutOfRangeException("row times column must small than the data lenght.");
                return this.data[p];
            }
            set {
                var p = row * this.Column + column;
                if (p >= this.data.Length) throw new IndexOutOfRangeException("row times column must small than the data lenght.");
                this.data[p] = value;
            }
        }
        public T this[int idx] {
            get {
                var p = idx;
                if (p >= this.data.Length) throw new IndexOutOfRangeException("row times column must small than the data lenght.");
                return this.data[p];
            }
            set {
                var p = idx;
                if (p >= this.data.Length) throw new IndexOutOfRangeException("row times column must small than the data lenght.");
                this.data[p] = value;
            }
        }
        public string Print() {
            var sb = new StringBuilder();
            for (var i = 0; i < this.Row; i++) {
                sb.AppendLine(string.Join(" ", this.GetRow(i)));
            }
            return sb.ToString();
        }
        public Enumerator GetEnumerator() {
            this.pointer = -1;
            return new Enumerator(this);
        }

        public struct Enumerator {
            public Map<T> owner;
            public Enumerator(Map<T> owner) {
                this.owner = owner;
            }
            public ref T Current => ref owner.data[owner.pointer];
            public void Dispose() {

            }

            public bool MoveNext() {
                owner.pointer++;
                return owner.pointer < owner.data.Length;
            }

            public void Reset() {
                owner.pointer = -1;
            }
        }
    }
}