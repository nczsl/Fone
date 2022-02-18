using System;
using System.Collections.Generic;
using System.Linq;

namespace Util.Mathematics.Discrete {

    public class TopChain<T> {
        protected class Node {
            #region INode 成员
            public T Data { get; set; }
            public int Index { get; set; }
            public string Zone { get; set; }
            #endregion
            internal Node previous;
        }
        public TopChain() {
            Count = 0;
        }
        protected Node top, current;
        public int Count { get; private set; }
        public void Add(T data, string key = "") {
            var node = new Node { Data = data, Index = Count, Zone = key };
            node.previous = current;
            current = node;
            top = current;
            Count++;
        }
        protected void Clear(ref Node t) {
            if (t != null && t.previous != null) {
                Clear(ref t.previous);
            }
            if (t != null)
                Count--;
            t = null;
        }
        protected void Remove(Node t) {
            var x = top;
            if (x == t) {
                top = top.previous;
                Count--;
                return;
            }
            while (x != null && x.previous != t) {
                x = x.previous;
            }
            if (x != null) {
                x.previous = x.previous.previous;
                Count--;
            }
        }
        IEnumerable<Node> QueryNode() {
            current = top;
            while (current != null) {
                yield return current;
                current = current.previous;
            }
        }
        public IEnumerable<T> Query() {
            current = top;
            while (current != null) {
                yield return current.Data;
                current = current.previous;
            }
        }

        public IEnumerable<T> Query(Func<T, bool> filter) {
            return from item in QueryNode() where filter(item.Data) select item.Data;
        }
        public T this[int index] {
            get {
                return QueryNode().Single(item => item.Index == index).Data;
            }
        }
        public T this[string key] {
            get {
                return QueryNode().Single(item => item.Zone == key).Data;
            }
        }
    }
    public class BottomChain<T> {
        protected partial class Node {
            #region INode 成员
            public T Data { get; set; }
            public int Index { get; set; }
            public string Zone { get; set; }
            #endregion
            public Node next;
        }
        public BottomChain() {
            Count = 0;
        }
        protected Node root, current;
        public int Count { get; protected set; }
        public void Add(T data, string key = "") {
            var node = new Node { Data = data, Index = Count, Zone = key };
            if (root == null) {
                root = node;
                current = root;
            }
            current.next = node;
            current = node;
            Count++;
        }
        protected void Remove(Node t) {
            var x = root;
            if (x == t) {
                root = root.next;
                Count--;
                return;
            }
            while (x != null && x.next != t) {
                x = x.next;
            }
            if (x != null) {
                var xx = x.next.next;
                x.next = null;
                x.next = xx;
                xx = null;
                Count--;
            }
        }
        protected void Clear(ref Node t) {
            if (t != null && t.next != null) {
                Clear(ref t.next);
            }
            if (t != null)
                Count--;
            t = null;
        }
        protected IEnumerable<Node> QueryNode() {
            current = root;
            while (current != null) {
                yield return current;
                current = current.next;
            }
        }
        public IEnumerable<T> Query() {
            current = root;
            while (current != null) {
                yield return current.Data;
                current = current.next;
            }
        }

        public IEnumerable<T> Query(Func<T, bool> filter) {
            return from item in QueryNode() where filter(item.Data) select item.Data;
        }
        public T this[int index] {
            get {
                return QueryNode().Single(item => item.Index == index).Data;
            }
        }
        public T this[string key] {
            get {
                return QueryNode().Single(item => item.Zone == key).Data;
            }
        }
    }
    public enum TreeTraversalType {
        Pre, In, Un, Post
    }
    public class BinFinderTree<T> : IEnumerable<T> where T : IComparable<T> {
        public class Node {
            public Node left, right, parent;
            public Node(T data) {
                Data = data;
            }
            //大左小右
            public void LeftAdd(Node n) {
                if (left == null) {
                    n.parent = this;
                    left = n;
                } else if (left.Data.CompareTo(n.Data) >= 0) {
                    left.LeftAdd(n);
                } else {
                    left.RightAdd(n);
                }

            }
            public void RightAdd(Node n) {
                if (right == null) {
                    n.parent = this;
                    right = n;
                } else if (right.Data.CompareTo(n.Data) >= 0) {
                    right.LeftAdd(n);
                } else {
                    right.RightAdd(n);
                }
            }
            public T Data { get; set; }
            public int index { get; set; }
            public int depth;
            public int bf;
        }
        protected Node root;
        public BinFinderTree() {
            Count = 0;
        }
        /// <summary>
        /// 根据索引返回结点
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Node this[int index] {
            get {
                return (from item in Query2(root) where item.index == index select item).First();
            }
        }
        public string GetPath(Node n, bool isFromRoot = true) {
            string r = null;
            while (n != root) {
                var x = LeftOrLight(n.parent, n);
                if (x) {
                    r += "1";
                } else {
                    r += "0";
                }
                n = n.parent;
            }
            if (r != null && isFromRoot) {
                r = new string(r.ToCharArray().Reverse().ToArray());
            }
            return r;
        }
        public Node Root { get { return root; } }
        public T Max {
            get {
                var current = root.Data;
                foreach (var item in Query2(root)) {
                    if (current.CompareTo(item.Data) < 0) {
                        current = item.Data;
                    }
                }
                return current;
            }
        }
        public T Min {
            get {
                var current = root.Data;
                foreach (var item in Query2(root)) {
                    if (current.CompareTo(item.Data) >= 0) {
                        current = item.Data;
                    }
                }
                return current;
            }
        }
        //右大左小
        public void Add(T data) {
            var n = new BinFinderTree<T>.Node(data);
            n.index = Count;
            Count++;
            if (root == null) {
                root = n;
                return;
            }
            if (root.Data.CompareTo(n.Data) >= 0) {
                root.LeftAdd(n);
            } else {
                root.RightAdd(n);
            }
        }
        protected bool LeftOrLight(Node up, Node down) {
            var r = false;
            if (up.left != null && up.left.Equals(down)) {
                r = true;
            }
            return r;
        }
        public void Remove(Node n) {
            if (n.parent == null) {
                var left = n.left;
                var right = n.right;
                n = null;
                left.parent = right.parent = null;
                right.LeftAdd(left);
            } else if (n.right == null && n.left == null) {
                n = null;
            } else if (n.left != null && n.right != null) {
                var left = n.left;
                var right = n.right;
                var p = n.parent;
                var lor = LeftOrLight(p, n);
                n = null;
                if (p.left == null && p.right == null) {
                    if (p.parent != null) {
                        var p2 = p.parent;
                        var lor2 = LeftOrLight(p2, p);
                        if (lor) {
                            if (lor2) {
                                p2.left = right;
                            } else {
                                p2.right = right;
                            }
                            right.parent = p2;
                            right.RightAdd(p);
                            right.LeftAdd(left);
                        } else {
                            if (lor2) {
                                p2.left = left;
                            } else {
                                p2.right = left;
                            }
                            left.parent = p2;
                            left.RightAdd(right);
                            left.LeftAdd(p);
                        }
                    }
                } else {
                    if (lor) {
                        p.left = left;
                        left.parent = p;
                        p.left.RightAdd(right);
                    } else {
                        p.right = right;
                        right.parent = p;
                        p.right.LeftAdd(left);
                    }
                }
            } else if (n.left == null && n.right != null) {
                var x = LeftOrLight(n.parent, n);
                if (x) {
                    n.parent.left = n.right;
                    n.right.parent = n.parent;
                } else {
                    n.parent.right = n.right;
                    n.right.parent = n.parent;
                }
                n = null;
            } else if ((n.left != null && n.right == null)) {
                var x = LeftOrLight(n.parent, n);
                if (x) {
                    n.parent.left = n.left;
                    n.left.parent = n.parent;
                } else {
                    n.parent.right = n.left;
                    n.left.parent = n.parent;
                }
                n = null;
            }
            var index = 0;
            foreach (var item in Query2(root)) {
                item.index = index++;
            }
        }
        public void Remove(T data) {
            var x = from item in Query2(root) where item.Data.CompareTo(data) == 0 select item;
            foreach (var item in x) {
                Remove(item);
            }
        }

        public IEnumerable<T> Query(Node n, TreeTraversalType ttt = TreeTraversalType.In) {
            switch (ttt) {
                case TreeTraversalType.Pre:
                    yield return n.Data;
                    if (n.left != null)
                        foreach (var item in Query(n.left, ttt))
                            yield return item;
                    if (n.right != null)
                        foreach (var item in Query(n.right, ttt))
                            yield return item;
                    break;
                case TreeTraversalType.In:
                    if (n.left != null)
                        foreach (var item in Query(n.left, ttt))
                            yield return item;
                    yield return n.Data;
                    if (n.right != null)
                        foreach (var item in Query(n.right, ttt))
                            yield return item;
                    break;
                case TreeTraversalType.Un:
                    if (n.right != null)
                        foreach (var item in Query(n.right, ttt))
                            yield return item;
                    yield return n.Data;
                    if (n.left != null)
                        foreach (var item in Query(n.left, ttt))
                            yield return item;
                    break;
                case TreeTraversalType.Post:
                    if (n.left != null)
                        foreach (var item in Query(n.left, ttt))
                            yield return item;
                    if (n.right != null)
                        foreach (var item in Query(n.right, ttt))
                            yield return item;
                    yield return n.Data;
                    break;
                default:
                    break;
            }
        }
        public IEnumerable<T> Query(Func<T, bool> filter) {
            return from item in Query(root, TreeTraversalType.Un)
                   where filter.Invoke(item)
                   select item;
        }
        public IEnumerable<Node> Query2(Node n, TreeTraversalType ttt = TreeTraversalType.In) {
            switch (ttt) {
                case TreeTraversalType.Pre:
                    yield return n;
                    if (n.left != null)
                        foreach (var item in Query2(n.left, ttt))
                            yield return item;
                    if (n.right != null)
                        foreach (var item in Query2(n.right, ttt))
                            yield return item;
                    break;
                case TreeTraversalType.In:
                    if (n.left != null)
                        foreach (var item in Query2(n.left, ttt))
                            yield return item;
                    yield return n;
                    if (n.right != null)
                        foreach (var item in Query2(n.right, ttt))
                            yield return item;
                    break;
                case TreeTraversalType.Un:
                    if (n.right != null)
                        foreach (var item in Query2(n.right, ttt))
                            yield return item;
                    yield return n;
                    if (n.left != null)
                        foreach (var item in Query2(n.left, ttt))
                            yield return item;
                    break;
                case TreeTraversalType.Post:
                    if (n.left != null)
                        foreach (var item in Query2(n.left, ttt))
                            yield return item;
                    if (n.right != null)
                        foreach (var item in Query2(n.right, ttt))
                            yield return item;
                    yield return n;
                    break;
            }
        }
        public List<string> GetPath(T t) {
            var ls = new List<string>();
            var r = "";
            var x = from item in Query2(root) where item.Data.CompareTo(t) == 0 select item;
            foreach (var item in x) {
                r = GetPath(item);
                ls.Add(r);
            }
            return ls;

        }

        public int Count { get; protected set; }
        public int Depth { get; protected set; }
        public void LoadData(IEnumerable<T> data) {
            foreach (var item in data) {
                Add(item);
            }
        }
        protected int GetDepth(Node n, int l, int r) {
            if (n.left != null) {
                GetDepth(n.left, l++, r);
            }
            if (n.right != null) {
                GetDepth(n.right, l, r++);
            }
            return l > r ? l : r;
        }
        public IEnumerator<T> GetEnumerator() {
            foreach (var item in Query2(root)) {
                yield return item.Data;
            }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }
    }
    public class AvlTree<T> : BinFinderTree<T> where T : IComparable<T> {
        readonly System.Collections.Generic.Stack<(bool?, Node)> path;
        public AvlTree() {
            path = new System.Collections.Generic.Stack<(bool?, Node)>();
            Count = 0;
        }
        public new void Remove(T data) { }
        #region add
        public new void Add(T value) {
            if (root == null) {
                root = new Node(value);
                root.depth = 0;
                Count++;
            } else {
                path.Clear();
                var temp = root;
                var leftOrRight = default(bool?);
                while (temp != null) {
                    path.Push((leftOrRight, temp));
                    if (value.CompareTo(temp.Data) >= 0) {
                        temp = temp.right;
                        leftOrRight = false;
                    } else {
                        temp = temp.left;
                        leftOrRight = true;
                    }
                }
                var n = new Node(value);
                n.depth = path.Count;
                var toptemp = path.Peek();
                if (toptemp.Item1 == null) {
                    if (value.CompareTo(toptemp.Item2.Data) >= 0) { toptemp.Item2.right = n; toptemp.Item2.bf--; } else { toptemp.Item2.left = n; toptemp.Item2.bf++; }
                } else {
                    if (value.CompareTo(toptemp.Item2.Data) < 0) { toptemp.Item2.left = n; toptemp.Item2.bf++; } else { toptemp.Item2.right = n; toptemp.Item2.bf--; }
                }
                n.parent = toptemp.Item2;
                n.index = Count;
                Count++;
                //下面检查树的平衡因子----bf
                /*
                * 当旋转根的BF值为2时：
                如果旋转根的左孩子的BF值为1，则进行LL型旋转；
                如果旋转根的左孩子的BF值为-1，则进行LR型旋转。
                当旋转根的BF值为-2时：
                如果旋转根的右孩子的BF值为1，则进行RL型旋转；
                如果旋转根的右孩子的BF值为-1，则进行RR型旋转。
                */
                SetBf(path);
                foreach (var item in path) {
                    switch (item.Item2.bf) {
                        case 2:
                            temp = item.Item2.left;
                            if (temp != null && temp.bf == 1) LL(item.Item2);
                            else if (temp != null && temp.bf == -1) LR(item.Item2);
                            break;
                        case -2:
                            temp = item.Item2.right;
                            if (temp != null && temp.bf == 1) RL(item.Item2);
                            else if (temp != null && temp.bf == -1) RR(item.Item2);
                            break;
                    }
                }
            }
        }
        /*
         * avl平衡旋转 的意思是平衡左右子树的高度，让高度大的一方向小的一方旋移
         * 旋转一共4种类型，分左右两类相互对称，"八"字形的一对为LL,RR "><"形的一对为RL,LR，其中
         * LL,LR与RR,RL 相互对称故只需要搞清楚一边另一边自动明白。
         * 你注意到了所有4种类型的旋转都在3个路径节点上展开理解这点是理解旋转的关键。其识别规律是
         * 沿失衡节点开始向下追述到路径节点的孙子节点。
         * 旋转的基本规则是 LL变成 ^,LR先旋转为LL再 转为^,与之对称的 右边与之完全一至只是方向相反
         * 旋转形状变化了解以后 需要解决的是 “重构冲突”规则，从失衡节点的父节点开始需要断开重连
         * 以失衡节点为旋转原根节点，旋转时需要寻找新根，对于LL来说 原根节点是 最上面的，其实所
         * 有失衡节点（原根）都是三节点中最上面的节点，LL的新根节点将是原根（失衡）节点的左子节点，
         * 对于LR来说是其孙子节点（左子之右子），所以只需要描述LL则LR就自然清楚，LL旋转时原根节点
         * 所左子树和父节点被断开了，它的新父节点将是它原来的左子（新根节点）它的新左子节点，将是
         * 其左子的右子节点，不论其是否为空。LL中原根断开的父连接将由新根接续起来，所有这些断开然后
         * 接续的变化就是“重构冲突”的解决，为什么称之为冲突呢，因为旋转后如果不交换子节点有节点就会
         * 在没有空位的之上依然需要加在此位置上加上新子节点，所以叫冲突。交换这些子节点使之位置匹配
         * 就是冲突的解决，这就是通过节点断开与接续来完成。
         */
        void LR(Node n) {
            var x = R(n.left);
            switch (x.bf) {
                case -1: x.left.bf = 1; x.bf = 1; break;
                case 1: x.left.bf = 0; x.bf = 2; break;
                case 0: x.left.bf = 0; x.bf = 1; break;
                default:
                    break;
            }
            var x2 = L(n);
            switch (x2.bf) {
                case 1: x2.right.bf = 0; x2.bf = 0; break;
                case 2: x2.right.bf = -1; x2.bf = 0; break;
                default:
                    break;
            }
        }
        void RL(Node n) {
            var x = L(n.right);
            switch (x.bf) {
                case 1: x.right.bf = -1; x.bf = -1; break;
                case -1: x.right.bf = 0; x.bf = -2; break;
                case 0: x.right.bf = 0; x.bf = -1; break;
            }
            var x2 = R(n);
            switch (x2.bf) {
                case -2: x2.left.bf = 1; x2.bf = 0; break;
                case -1: x2.left.bf = 0; x2.bf = 0; break;
            }
        }
        void LL(Node n) {
            var x = L(n);
            //下面两个bf为0有深刻的逻辑判断支撑
            //路径节点必然为长节点必然为左左支，中心点上去后必然为0，right.bf也一样，都是通过失衡节点为2以及左左支为路径必
            //为长轴（子树高支）为判断依据得到的
            x.bf = 0;
            x.right.bf = 0;
        }
        void RR(Node n) {
            var x = R(n);
            x.bf = 0;
            x.left.bf = 0;
        }
        /*
         * 旋转其实只有两种R和L 这两种旋转互为镜像，这两种旋转只是在两个节点上展开
         * 由它们组合构成4种所有avl的基本旋转分类 LL RR LR RL, LL RR旋转就是L R旋转
         * 而LR RL旋转分别 是先做R旋转再做L旋转，RL是先做L旋转再做R旋转，旋转以后计
         * 算它们的bf值，就是两个旋转相差节点的bf值就可以了。这个也很容易
         */
        Node L(Node n) {
            var o = n.left;
            n.left = o.right;
            if (n.left != null)
                n.left.parent = n;
            o.right = n;
            var p = n.parent;
            n.parent = o;
            if (p != null) {
                if (p.Data.CompareTo(o.Data) > 0) { p.left = o; } else { p.right = o; }
                o.parent = p;
            } else {
                root = o;
                root.parent = null;
            }
            return o;
        }
        Node R(Node n) {
            var o = n.right;
            n.right = o.left;
            if (n.right != null)
                n.right.parent = n;
            o.left = n;
            var p = n.parent;
            n.parent = o;
            if (p != null) {
                if (p.Data.CompareTo(o.Data) > 0) { p.left = o; } else { p.right = o; }
                o.parent = p;
            } else {
                root = o;
                root.parent = null;
            }
            return o;
        }
        int GetBf(Node n) {
            if (n.left != null && n.right != null) {
                return Math.Abs(n.left.bf) - Math.Abs(n.right.bf);
            } else if (n.left == null && n.right != null) {
                return -(Math.Abs(n.right.bf) + 1);
            } else if (n.left != null && n.right == null) {
                return Math.Abs(n.left.bf) + 1;
            } else {
                return 0;
            }
        }
        void SetBf(System.Collections.Generic.Stack<(bool?, Node)> path) {
            var len = path.Count() - 1;
            var path2 = path.ToArray();
            for (var i = 0; i < len; i++) {
                //|| path2[i].Item2.bf == 2 || path2[i].Item2.bf == -2
                if (path2[i].Item2.bf == 0 || path2[i].Item2.bf == 2 || path2[i].Item2.bf == -2) {
                    //为什么这些值要跳出？ 
                    //为0的时候说明 其祖节点集都不会增加相差树高
                    //为2或-2是因为插入节点最多只能引起一次旋转来抵销，而抵销旋转只需要找到第一个遇到2，或-2的节点即可
                    break;
                }
                if (path2[i].Item1.Value) {
                    path2[i + 1].Item2.bf++;
                } else {
                    path2[i + 1].Item2.bf--;
                }
            }
        }
        #endregion
        #region remove
        enum RemoveNodeType {
            onlyleft, onlyright, all, leaf
        }
        RemoveNodeType CheckRemoveNodeType(Node n) {
            var nt = RemoveNodeType.leaf;
            if (n.left != null && n.right == null) nt = RemoveNodeType.onlyleft;
            else if (n.left == null && n.right != null) nt = RemoveNodeType.onlyright;
            else if (n.left != null && n.right != null) nt = RemoveNodeType.all;
            return nt;
        }
        Node FindMaxRightNode(Node n) {
            var b = false;
            var r = default(Node);
            foreach (var item in Query2(n, TreeTraversalType.In)) {
                if (b) {
                    r = item;
                    break;
                }
                if (n.Equals(item)) {
                    b = true;
                }
            }
            return r;
        }
        Node FindMinLeftNode(Node n) {
            var b = false;
            var r = default(Node);
            foreach (var item in Query2(n, TreeTraversalType.Un)) {
                if (b) {
                    r = item;
                    break;
                }
                if (n.Equals(item)) {
                    b = true;
                }
            }
            return r;
        }
        Node FindInLeafMapping(Node n) {
            var b = false;
            var r = default(Node);
            foreach (var item in Query2(n, TreeTraversalType.In)) {
                if (b) {
                    r = item;
                    break;
                }
                if (n.Equals(item)) {
                    b = true;
                }
            }
            return r;
        }
        Node Swap(Node target, Node leaf) {
            if (!target.Equals(leaf)) {
                var p2 = leaf.parent;
                leaf.parent = target.parent;
                if (target.left != null && !target.left.Equals(leaf)) {
                    leaf.left = target.left;
                    target.left.parent = leaf;
                    target.left = null;
                }
                if (target.right != null && !target.right.Equals(leaf)) {
                    leaf.right = target.right;
                    target.right.parent = leaf;
                    target.right = null;
                }
                if (target.parent != null) {
                    if (LeftOrLight(target.parent, target)) {
                        target.parent.left = leaf;
                    } else {
                        target.parent.right = leaf;
                    }
                } else {
                    root = leaf;
                }
                if (!p2.Equals(target)) {
                    if (LeftOrLight(p2, leaf)) {
                        p2.left = target;
                    } else {
                        p2.right = target;
                    }
                    target.parent = p2;
                } else {
                    if (LeftOrLight(p2, leaf)) {
                        leaf.left = target;
                    } else {
                        leaf.right = target;
                    }
                    target.parent = leaf;
                }
            }
            return target;
        }
        Node LL2(Node n) {
            var x = L(n);
            x.bf = 0;
            x.right.bf = 0;
            return x;
        }
        Node LL2A(Node n) {
            var x = L(n);
            x.bf = 0;
            x.right.bf = 1;
            return x;
        }
        Node LR2(Node n) {
            var x = R(n.left);
            switch (x.bf) {
                case -1: x.left.bf = 1; x.bf = 1; break;
                case 1: x.left.bf = 0; x.bf = 2; break;
                case 0: x.left.bf = 0; x.bf = 1; break;
                default:
                    break;
            }
            var x2 = L(n);
            switch (x2.bf) {
                case 1: x2.right.bf = 0; x2.bf = 0; break;
                case 2: x2.right.bf = -1; x2.bf = 0; break;
                default:
                    break;
            }
            return x2;
        }
        Node RR2(Node n) {
            var x = L(n);
            x.bf = 0;
            x.left.bf = 0;
            return x;
        }
        Node RR2A(Node n) {
            var x = L(n);
            x.bf = 0;
            x.left.bf = 1;
            return x;
        }
        Node RL2(Node n) {
            var x = L(n.right);
            switch (x.bf) {
                case 1: x.right.bf = -1; x.bf = -1; break;
                case -1: x.right.bf = 0; x.bf = -2; break;
                case 0: x.right.bf = 0; x.bf = -1; break;
            }
            var x2 = R(n);
            switch (x2.bf) {
                case -2: x2.left.bf = 1; x2.bf = 0; break;
                case -1: x2.left.bf = 0; x2.bf = 0; break;
            }
            return x2;
        }
        void RefreshBf(Node target) {
            var temp = target;
            while (temp.parent != null) {
                if (LeftOrLight(temp.parent, temp)) {
                    temp.parent.bf--;
                } else {
                    temp.parent.bf++;
                }
                temp = temp.parent;
                switch (temp.bf) {
                    case 2:
                        if (temp.left.bf == 1) {
                            temp = LL2(temp);
                        } else if (temp.left.bf == 0) {
                            temp = LL2A(temp);
                        } else if (temp.left.bf == -1) {
                            temp = LR2(temp);
                        }
                        break;
                    case -2:
                        if (temp.right.bf == 1) {
                            temp = RL2(temp);
                        } else if (temp.right.bf == 0) {
                            temp = RR2A(temp);
                        } else if (temp.right.bf == -1) {
                            temp = RR2(temp);
                        }
                        break;
                }
                if (temp.bf == 1 || temp.bf == -1) break;
            }
        }
        public new void Remove(Node n) {
            var x = CheckRemoveNodeType(n);
            var leaf = default(Node);
            switch (x) {
                case RemoveNodeType.onlyleft:
                    leaf = FindMaxRightNode(n);
                    break;
                case RemoveNodeType.onlyright:
                    leaf = FindMinLeftNode(n);
                    break;
                case RemoveNodeType.all:
                    leaf = FindInLeafMapping(n);
                    break;
                case RemoveNodeType.leaf:
                    leaf = n;
                    break;
            }
            var x2 = Swap(n, leaf);
            RefreshBf(x2);
            DeleteNode(x2);
            Count--;
        }
        void DeleteNode(Node n) {
            this[Count - 1].index = n.index;
            if (LeftOrLight(n.parent, n)) {
                n.parent.left = null;
            } else {
                n.parent.right = null;
            }
            n.parent = null;
            n.left = null;
            n.right = null;
            n = null;
        }
        #endregion
    }
    public class RbTree<T> : BinFinderTree<T> where T : IComparable<T> {
        //平衡因子bf 为0时为黑节点，为1时为红节点
    }
    public class Graph<T> {
        public class Node {
            public T Data { get; set; }
            /// <summary>index 可用于ID此值是唯一的</summary>
            public int Index { get; set; }
            /// <summary>备注，注释，标记</summary>
            public string Key { get; set; }
            public bool IsVisited { get; set; }
            /// <summary>
            /// 已经顶点的关联关系，注意这些顶点只是已经有顶点的引用 并不来自新添加
            /// </summary>
            public List<Node> Association { get; set; }
            public int GroupIndex { get; set; }
            public string GroupName { get; set; }
            public int x, y, z;//节点的相对位置坐标(3d Decare coordinate)
            public Node(T data, string key = "", string gn = "", int gi = 0, int x = 0, int y = 0, int z = 0) {
                Data = data; Key = key; GroupName = gn; GroupIndex = gi; this.x = x; this.y = y; this.z = z;
                Association = new List<Node>();
            }
        }
        /// <summary>顶点集合</summary>
        List<Node> vertexs { get; set; }
        public int Count { get { return vertexs.Count; } }
        /// <summary>
        /// 边权表：
        /// 开头是个数组[] 每个数组是个List<Tople<int,float>>
        /// 每个数组代表一个顶点的索引，顶点索引必须以连接整数序列
        /// 每个顶点包含一组边集合，这个边集合为一个List<Tople<int,float>>
        /// 表示，每个List中的Tople<int,float>第一个Item(Item1)表示其它某
        /// 个顶点的索引，而第二个值表示由数组索引到List索引方向的一个连接边
        /// float就代表这个边的权值
        /// </summary>
        List<(int, float)>[] sides;
        internal List<(int, float)>[] Sides { get { return sides; } }
        /// <summary>索引和设置边的权值</summary>
        public float? GetEdge(int index1, int index2) {
            try {
                var x = sides[index1].Single(i => i.Item1 == index2);
                //var x = sides[index1][index2];
                return x.Item2;
            } catch {
                return null;
            }
        }
        public void SetEdge(int index1, int index2, float value) {
            var target = sides[index1].Single(i => i.Item1 == index2);
            sides[index1].Remove(target);
            sides[index1].Add((index2, value));
        }
        #region ctor collection
        public Graph() {
            vertexs = new List<Node>();
        }
        public Graph(params T[] ts) {
            vertexs = new List<Node>();
            var index = 0;
            foreach (var item in ts) {
                vertexs.Add(new Node(item) { Index = index++ });
            }
        }
        public Graph(params T[][] ts) {
            vertexs = new List<Node>();
            for (var i = 0; i < ts.Length; i++) { var ylen = ts[i].Length; for (var j = 0; j < ylen; j++) { vertexs.Add(new Node(ts[i][j], x: i, y: j)); } }
        }
        public Graph(params T[][][] ts) {
            vertexs = new List<Node>();
            for (var i = 0; i < ts.Length; i++) {
                var ylen = ts[i].Length;
                for (var j = 0; j < ylen; j++) {
                    var zlen = ts[i][j].Length;
                    for (var k = 0; k < zlen; k++) { vertexs.Add(new Node(ts[i][j][k], x: i, y: j, z: k)); }
                }
            }
        }
        /// <summary>
        /// 传入邻接表，第一维是所有的顶点集合，第二维元组中的第一个值代表
        /// 第一维索引相关的关联节点的索引值，第二个值为这个关联所代表的有向或无向边的权值
        /// </summary>
        /// <param name="ts"></param>
        public Graph((int, float)[][] ts) {
            var count = ts.Length;
            for (var i = 0; i < count; i++) { Add(default(T)); }
            sides = new List<(int, float)>[count];
            for (var i = 0; i < ts.Length; i++) {
                sides[i] = new List<(int, float)>();
                for (var j = 0; j < ts[i].Length; j++) {
                    this[i].Association.Add(this[ts[i][j].Item1]);
                    sides[i].Add((ts[i][j].Item1, ts[i][j].Item2));
                }
            }
        }
        #endregion
        #region add
        /// <summary>添加一个顶点</summary>
        public void Add(T t, string zone = "", string gn = "", int gi = 0, int x = 0, int y = 0, int z = 0) {
            vertexs.Add(new Node(t, key: zone, gn: gn, gi: gi, x: x, y: y, z: z) { Index = vertexs.Count, Key = vertexs.Count.ToString() });
        }
        /// <summary>根据邻接表构造一个完整的图</summary>
        public void AddRange((int, float)[][] ts) {
            var count = ts.Length;
            for (var i = 0; i < count; i++) { Add(default(T)); }
            sides = new List<(int, float)>[count];
            for (var i = 0; i < ts.Length; i++) {
                sides[i] = new List<(int, float)>();
                for (var j = 0; j < ts[i].Length; j++) {
                    this[i].Association.Add(this[ts[i][j].Item1]);
                    sides[i].Add((ts[i][j].Item1, ts[i][j].Item2));
                }
            }
        }
        /// <summary>根据2维索引添加一组顶点</summary>
        public void AddRange(T[] ts) { for (var i = 0; i < ts.Length; i++) { Add(ts[i]); } }
        #endregion
        #region set associate
        public Node[] Vertexs { get { return vertexs.ToArray(); } }
        /// <summary>设置可计算的长度全为1.0f 的边</summary>
        public void InitialUnitEdges() {
            var length = Count;
            sides = new List<(int, float)>[length];
            for (var i = 0; i < length; i++) {
                var len = this[i].Association.Count;
                sides[i] = new List<(int, float)>();
                for (var j = 0; j < len; j++) {
                    sides[i].Add((this[i].Association[j].Index, 1f));
                }
            }
        }
        public Node this[params int[] ids] {
            get {
                if (ids.Length == 1)
                    return vertexs.Single(item => item.Index == ids[0]);
                else if (ids.Length == 2)
                    return vertexs.Single(item => item.x == ids[0] && item.y == ids[1]);
                else if (ids.Length == 3)
                    return vertexs.Single(item => item.x == ids[0] && item.y == ids[1] && item.z == ids[2]);
                else
                    throw new Exception("len must less than 3");
            }
        }
        public Node this[string key] { get { return vertexs.Single(item => item.Key == key); } }
        /// <summary>
        /// 建立关系通过 index
        /// </summary>
        /// <param name="nodeindex">待添加节点索引</param>
        /// <param name="assos">待添加节点的子节点索引集</param>
        public void SetAssociateByIndex(int nodeindex, params int[] assos) {
            foreach (var item in assos) {
                this[nodeindex].Association.Add(this[item]);
            }
        }
        public void SetAssociateBy2DCoordinate((int, int) nodeindex, params (int, int)[] assos) {
            foreach (var item in assos) {
                this[nodeindex.Item1, nodeindex.Item2].Association.Add(this[item.Item1, item.Item2]);
            }
        }
        public void SetAssociateBy3DCoordinate((int, int, int) nodeindex, params (int, int, int)[] assos) {
            foreach (var item in assos) {
                this[nodeindex.Item1, nodeindex.Item2, nodeindex.Item3].Association.Add(this[item.Item1, item.Item2, item.Item3]);
            }
        }
        public void SetAssociate(string key, params string[] otherkeys) {
            foreach (var item in otherkeys) {
                this[key].Association.Add(this[item]);
            }
        }
        #endregion
        public void ResetVisit() {
            foreach (var item in vertexs) {
                item.IsVisited = false;
            }
        }
        /// <summary>
        /// 获取图中一个未访问的节点
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        protected Node GetNodeUnVisited() {
            foreach (var node in vertexs)
                if (!node.IsVisited)
                    return node;
            return null;
        }
        protected Node GetNodeUnVisited(IEnumerable<Node> nodes) {
            foreach (var node in nodes)
                if (!node.IsVisited)
                    return node;
            return null;
        }
        IEnumerable<Node> BreadthFirstTraverse(int index = 0) {
            var current = vertexs.Single(item => item.Index == index);
            if (!current.IsVisited) {
                current.IsVisited = true;
                yield return current;
                var q = new Queue<Node>();
                q.Enqueue(current);
                while (q.count > 0) {
                    foreach (var item in current.Association) {
                        if (!item.IsVisited) {
                            q.Enqueue(item);
                            item.IsVisited = true;
                        }
                    }
                    current = q.Dequeue();
                    yield return current;
                }
            }
        }
        public HashSet<T> BFT() {
            var x = new HashSet<T>();
            ResetVisit();
            var node = GetNodeUnVisited();
            while (node != null) {
                var x2 = (from item in BreadthFirstTraverse(node.Index) select item.Data).ToArray();
                x.UnionWith(x2);
                node = GetNodeUnVisited();
            }
            return x;
        }
        IEnumerable<Node> DeepFirstTraverse(int index = 0) {
            var current = vertexs.Single(item => item.Index == index);
            if (!current.IsVisited) {
                current.IsVisited = true;
                yield return current;
                var node = GetNodeUnVisited(current.Association);
                if (node != null)
                    foreach (var item2 in DeepFirstTraverse(node.Index)) {
                        yield return item2;
                    }
            }
        }
        public HashSet<T> DFT() {
            var x = new HashSet<T>();
            ResetVisit();
            var node = GetNodeUnVisited();
            while (node != null) {
                var x2 = (from item in DeepFirstTraverse(node.Index) select item.Data).ToArray();
                x.UnionWith(x2);
                node = GetNodeUnVisited();
            }
            return x;
        }
    }
    /// <summary>
    /// a 星搜索 标准2度数组
    /// </summary>
    public class Astar {
        protected class Node {
            public double estimation;//估值
            public int step;//步数值
            public double Value { get { return estimation + step; } }
            public byte mark;
            public (int, int) coordinate;
            public Node(int x, int y, byte mark) {
                coordinate = (x, y);
                this.mark = mark;
            }
            public bool CheckOne((int, int) target) {
                return coordinate.Item1 == target.Item1 && coordinate.Item2 == target.Item2;
            }
            public bool CheckRelationship(Node n) {
                return Math.Abs(coordinate.Item1 - n.coordinate.Item1) <= 1 && Math.Abs(coordinate.Item2 - n.coordinate.Item2) <= 1;
            }
        }

        HashSet<Node> map; int x, y;
        Node this[(int, int) key] {
            get { return map.Single(item => item.CheckOne(key)); }
        }
        public static Astar Create(byte[,] map, bool? movestyle = null) {
            var r = new Astar();
            r.movestyle = movestyle;
            r.x = map.GetLength(0);
            r.y = map.GetLength(1);
            r.map = new HashSet<Node>();
            for (var i = 0; i < r.x; i++) {
                for (var j = 0; j < r.y; j++) {
                    r.map.Add(new Node(i, j, map[i, j]));
                }
            }
            return r;
        }
        Astar() {
            openlist = new HashSet<Node>();
            closelist = new HashSet<Node>();
        }
        public List<(int, int)> Search((int, int) start, (int, int) end, byte stopvalue = 255) {
            var r = new List<(int, int)>();
            var isNothing = true;
            var x = this[start];
            openlist.Clear();
            closelist.Clear();
            closelist.Add(x);
            while (!x.CheckOne(end)) {
                var x2 = GetCurrentList(x.coordinate);
                var x3 = x2.Except(closelist);
                var x33 = x3.Except((from item in x3 where item.mark == stopvalue select item).ToArray());
                foreach (var item in x33) {
                    item.estimation = GetValue(item, this[end]);
                }
                openlist.UnionWith(x33);
                if (openlist.Count > 0) {
                    var x4 = openlist.Min(item => item.estimation);
                    x = (from item in openlist where item.estimation == x4 select item).First();
                    openlist.Remove(x);
                    x.step = closelist.Count;
                    closelist.Add(x);
                } else {
                    isNothing = false;
                    break;
                }
            }
            if (isNothing) {
                var x5 = from item in closelist orderby item.step descending select item;
                var x6 = x5.ToArray();
                var length = closelist.Count;
                var key = x6[0];
                for (var i = 0; i < length; i++) {
                    if (!key.CheckRelationship(x6[i])) {
                        continue;
                    }
                    r.Add(key.coordinate);
                    key = x6[i];
                    if (key.CheckOne(start)) {
                        break;
                    }
                }
            }
            return r;
        }

        readonly HashSet<Node> openlist;
        readonly HashSet<Node> closelist;
        Node GetMaxValueCoordinate(Node p) {
            var x = default(Node);
            var x2 = 0d;
            foreach (var item in openlist) {
                var x3 = GetValue(p, item);
                if (x3 > x2) {
                    x = item;
                    x3 = x2;
                }
            }
            return x;
        }
        bool? movestyle;
        HashSet<Node> GetCurrentList((int, int) thepoint) {
            var ol = new HashSet<Node>();
            switch (movestyle) {
                case null: //周围8格（全）
                    if (thepoint.Item1 + 1 < x) {
                        ol.Add(this[(thepoint.Item1 + 1, thepoint.Item2)]);
                    }
                    if (thepoint.Item1 - 1 >= 0) {
                        ol.Add(this[(thepoint.Item1 - 1, thepoint.Item2)]);
                    }
                    if (thepoint.Item2 + 1 < y) {
                        ol.Add(this[(thepoint.Item1, thepoint.Item2 + 1)]);
                    }
                    if (thepoint.Item2 - 1 >= 0) {
                        ol.Add(this[(thepoint.Item1, thepoint.Item2 - 1)]);
                    }
                    //
                    if (thepoint.Item1 + 1 < x && thepoint.Item2 + 1 < y) {
                        ol.Add(this[(thepoint.Item1 + 1, thepoint.Item2 + 1)]);
                    }
                    if (thepoint.Item1 - 1 >= 0 && thepoint.Item2 - 1 >= 0) {
                        ol.Add(this[(thepoint.Item1 - 1, thepoint.Item2 - 1)]);
                    }
                    if (thepoint.Item1 + 1 < y && thepoint.Item2 - 1 >= 0) {
                        ol.Add(this[(thepoint.Item1 + 1, thepoint.Item2 - 1)]);
                    }
                    if (thepoint.Item1 - 1 >= 0 && thepoint.Item2 + 1 < y) {
                        ol.Add(this[(thepoint.Item1 - 1, thepoint.Item2 + 1)]);
                    }
                    break;
                case true: //正4格
                    if (thepoint.Item1 + 1 < x) {
                        ol.Add(this[(thepoint.Item1 + 1, thepoint.Item2)]);
                    }
                    if (thepoint.Item1 - 1 >= 0) {
                        ol.Add(this[(thepoint.Item1 - 1, thepoint.Item2)]);
                    }
                    if (thepoint.Item2 + 1 < y) {
                        ol.Add(this[(thepoint.Item1, thepoint.Item2 + 1)]);
                    }
                    if (thepoint.Item2 - 1 >= 0) {
                        ol.Add(this[(thepoint.Item1, thepoint.Item2 - 1)]);
                    }
                    break;
                case false: //4隅格
                    if (thepoint.Item1 + 1 < x && thepoint.Item2 + 1 < y) {
                        ol.Add(this[(thepoint.Item1 + 1, thepoint.Item2 + 1)]);
                    }
                    if (thepoint.Item1 - 1 >= 0 && thepoint.Item2 - 1 >= 0) {
                        ol.Add(this[(thepoint.Item1 - 1, thepoint.Item2 - 1)]);
                    }
                    if (thepoint.Item1 + 1 < y && thepoint.Item2 - 1 >= 0) {
                        ol.Add(this[(thepoint.Item1 + 1, thepoint.Item2 - 1)]);
                    }
                    if (thepoint.Item1 - 1 >= 0 && thepoint.Item2 + 1 < y) {
                        ol.Add(this[(thepoint.Item1 - 1, thepoint.Item2 + 1)]);
                    }
                    break;
            }
            return ol;
        }
        double GetValue((int, int) a, (int, int) b) {
            return Math.Abs(b.Item1 - a.Item1) + Math.Abs(b.Item2 - a.Item2);
        }
        double GetValue(Node a, Node b) {
            return Math.Abs(b.coordinate.Item1 - a.coordinate.Item1) + Math.Abs(b.coordinate.Item2 - a.coordinate.Item2);
        }
    }
}
