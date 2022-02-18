// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Util.Mathematics.Discrete {
    //public class Btree<T> : Tree<T>, IDisposable, IEnumerable<T> {
    //    new public Add(T t) {

    //    }
    //}
    public class Queue<T> : IDisposable, IEnumerable<T> {
        Queue<T> current;
        public T data;
        public int count;
        public void Enqueue(T t) {
            if (current == null) {
                current = new Queue<T> { data = t };
                count++;
                return;
            }
            var temp = current;
            while (temp.current != null) {
                temp = temp.current;
            }
            temp.current = new Queue<T> { data = t };
            count++;
        }
        public T Dequeue() {
            var temp = current;
            current = current.current;
            count--;
            var data = temp.data;
            temp = null;
            return data;
        }
        public void Dispose() {
            var temp = current;
            while (temp != null) {
                var n = temp;
                temp = temp.current;
                n.count = 0;
                n.current = null;
                n.data = default(T);
            }
        }
        public IEnumerator<T> GetEnumerator() {
            var temp = current;
            while (temp != null) {
                yield return temp.data;
                temp = temp.current;
            }
        }
        //
        IEnumerator IEnumerable.GetEnumerator() {
            var temp = current;
            while (temp != null) {
                yield return temp.data;
                temp = temp.current;
            }
        }
    }
    /// <summary>
    /// 先进后出
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Stack<T> : IDisposable, IEnumerable<T> {
        Stack<T> current;
        public T data;
        public int count;
        /// <summary>压栈</summary>
        public void Push(T data) {
            if (current == null) {
                current = new Stack<T>();
                current.data = data;
                count++;
                return;
            }
            var temp = new Stack<T>();
            temp.data = data;
            temp.current = current;
            current = temp;
            count++;
        }
        public T Pop() {
            var temp = current;
            var t = temp.data;
            current.data = default(T);
            current = current.current;
            temp = null;
            count--;
            return t;
        }
        public T Peek() => current.data;
        public void Clear() {
            while (current != null) {
                current.data = default(T);
                var temp = current;
                current = current.current;
                temp.current = null;
            }
            count = 0;
        }
        public void Dispose() {
            Clear();
        }

        public IEnumerator<T> GetEnumerator() {
            var temp = current;
            while (temp != null) {
                yield return temp.data;
                temp = temp.current;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            var temp = current;
            while (temp != null) {
                yield return temp.data;
                temp = temp.current;
            }
        }
    }
    public class Chain<T> : IDisposable, IEnumerable<T> {
        Chain<T> root, current, parent;
        T data;
        public int Count;
        public int Index;
        public Chain(bool headToTail = false) {
            HeadToTail = headToTail;
            Count = Index = 0;
        }
        public T RootData => root.data;
        public T CurrentData => current.data;
        public T Upon(int i = 1) {
            var temp = current;
            while (i > 1 && temp.parent != null) {
                temp = temp.parent;
                i--;
            }
            return temp.data;
        }
        public T Upon(T current) {
            var temp = default(Chain<T>);
            foreach (var item in Traversal()) {
                if (item.data.Equals(current)) {
                    temp = item;
                    break;
                }
            }
            if (temp != null)
                return temp.parent.data;
            return default(T);
        }

        /// <summary>
        /// 从头指向尾：false
        /// 从尾指向头：true 
        /// 默认是从尾指向头
        /// </summary>
        public bool HeadToTail { get; set; }
        #region add insert remove clear
        public void Add(T data) {
            if (current == null) {
                root = parent = current = new Chain<T>(HeadToTail) { data = data };
                current.Index = Count;
                Count++;
                return;
            }
            var temp = new Chain<T>(HeadToTail) { data = data };
            if (HeadToTail) {
                current.current = temp;
                current = temp;
            } else {
                temp.current = current;
                current = temp;
            }
            temp.parent = current;
            current.Index = Count;
            Count++;
        }
        public Chain<T> IndexOf(int idx) {
            foreach (var item in Traversal()) {
                if (item.Index == idx) {
                    return item;
                }
            }
            return null;
        }
        public IEnumerable<Chain<T>> ValueOf(T t) => from i in Traversal() where t.Equals(i.data) select i;
        public void Remove(Chain<T> t) {
            if (t == null) return;
            var fornt = t.current;
            var back = HeadToTail ? root : current;
            var isback = true;
            while (back.current != t) {
                if (back.current == null) {
                    isback = false;
                    break;
                }
                back = back.current;
            }
            if (fornt != null && !isback) {
                Clear(t);
            } else if (fornt == null && isback) {
                Clear(t);
                back.current = null;
            } else if (fornt != null && isback) {
                Clear(t);
                back.current = fornt;
            }
            if (t == current) {
                if (HeadToTail) {
                    current = back;
                } else {
                    current = fornt;
                }
            } else if (t == root) {
                if (HeadToTail) {
                    root = fornt;
                } else {
                    root = back;
                }
            }
        }
        void Remove2(Chain<T> t) {
            if (t == null) return;
            var fornt = t.current;
            var back = HeadToTail ? root : current;
            var isback = true;
            while (back.current != t) {
                if (back.current == null) {
                    isback = false;
                    break;
                }
                back = back.current;
            }
            if (fornt != null && !isback) {
                Clear(t);
            } else if (fornt == null && isback) {
                Clear(t);
                back.current = null;
            } else if (fornt != null && isback) {
                Clear(t);
                back.current = fornt;
            }
            if (t == current) {
                if (HeadToTail) {
                    current = back;
                } else {
                    current = fornt;
                }
            } else if (t == root) {
                if (HeadToTail) {
                    root = fornt;
                } else {
                    root = back;
                }
            }
        }
        public void Remove(T t) {
            var x = ValueOf(t).ToArray();
            for (var i = 0; i < x.Length; i++) {
                Remove2(x[i]);
            }
            Reset();
        }
        void Reset() {
            var temp = HeadToTail ? root : current;
            var count = 0;
            while (temp != null) {
                temp.Index = count;
                temp = temp.current;
                count++;
            }
            Count = count;
        }
        #endregion
        public IEnumerable<Chain<T>> Traversal() {
            var temp = HeadToTail ? root : current;
            while (temp != null) {
                yield return temp;
                temp = temp.current;
            }
        }
        public void Dispose() {
            var x = Traversal().ToArray();
            for (var i = 0; i < x.Length; i++) {
                x[i].data = default(T);
                x[i].Index = x[i].Count = 0;
                x[i].current = x[i].root = null;
                x[i].HeadToTail = default(bool);
                x[i] = null;
            }
        }
        void Clear(Chain<T> target) {
            target.data = default(T);
            target.Index = target.Count = 0;
            target.HeadToTail = default(bool);
            target.current = target.root = null;
        }

        public IEnumerator<T> GetEnumerator() {
            var temp = HeadToTail ? root : current;
            while (temp != null) {
                yield return temp.data;
                temp = temp.current;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            var temp = HeadToTail ? root : current;
            while (temp != null) {
                yield return temp.data;
                temp = temp.current;
            }
        }
    }
    public class Tree<T> : IDisposable, IEnumerable<T> {
        public Tree<T> Parent;
        public List<Tree<T>> Childen { get; private set; }
        #region add remove insert removesub        
        public Tree<T> Add(T t) {
            var temp = new Tree<T>(t, this);
            temp.GroupNo = Childen.Count;
            Childen.Add(temp);
            return this;
        }
        public Tree<T> Addat(T t) {
            var temp = new Tree<T>(t, this);
            temp.GroupNo = Childen.Count;
            Childen.Add(temp);
            return temp;
        }
        public Tree<T> Add(Tree<T> t) {
            t.Parent = this;
            t.Level = Level + 1;
            t.GroupNo = Childen.Count;
            Childen.Add(t);
            return this;
        }
        public Tree<T> Addat(Tree<T> t) {
            t.Parent = this;
            t.Level = Level + 1;
            t.GroupNo = Childen.Count;
            Childen.Add(t);
            return t;
        }
        public Tree<T> Upon(int i = 1) {
            var temp = this;
            while (i > 0) {
                temp = temp.Parent;
                i--;
            }
            return temp;
        }
        public void Remove(T t) => Childen.Remove(Childen.Find(i => i.data.Equals(t)));
        public void Remove(Tree<T> target) => Childen.Remove(target);
        #endregion
        public Tree(T t, Tree<T> parent = null) {
            Parent = parent;
            data = t;
            Childen = new List<Tree<T>>();
            if (Parent != null) {
                Level = Parent.Level + 1;
            }
        }
        public Tree(Tree<T> parent = null) {
            Parent = parent;
            Childen = new List<Tree<T>>();
            if (Parent != null) {
                Level = Parent.Level + 1;
            }
        }
        public T data;
        public bool IsLeaf { get; set; }
        public bool IsRoot { get; set; }
        private int level;
        public int Level {
            get { return level; }
            private set {
                level = value;
                if (Childen.Count > 0 && level != Childen[0].level + 1) {
                    foreach (var item in Childen) {
                        item.Level = level + 1;
                    }
                }
            }
        }
        public static List<Tree<(int id, int? pid, D data)>> Build<D>(IEnumerable<(int id, int? pid, D data)> source) {
            var r = new List<Tree<(int id, int? pid, D data)>>();
            foreach (var item in source) {
                if (item.pid == null) {
                    var tree = new Tree<(int id, int? pid, D data)>(item);
                    Build(source, tree);
                    r.Add(tree);
                }
            }
            return r;
        }
        static void Build<D>(IEnumerable<(int id, int? pid, D data)> source, Tree<(int id, int? pid, D data)> parent) {
            /*
             * 查找传入根节点的所有子节点
             */
            var childen = from i in source
                          where i.pid == parent.data.id && i.id != parent.data.id
                          select i;
            foreach (var item in childen) {
                var subtree = parent.Addat(item);
                Build(from i in source where i.id != parent.data.id select i, subtree);
            }
        }
        public IEnumerable<Tree<T>> GetParents(Tree<T> current) {
            var temp = current;
            while (current.Parent != null) {
                yield return temp.Parent;
                temp = temp.Parent;
            }
        }
        public int GroupNo { get; private set; }
        protected List<int> DnaTable {
            get {
                var table = new List<int>();
                var temp = this;
                while (temp != null) {
                    table.Add(temp.GroupNo);
                    temp = temp.Parent;
                }
                return table;
            }
        }
        public string Dna => string.Join(",", DnaTable);
        int getSubCount(Tree<T> n) {
            var temp = n.Childen.Count;
            foreach (var item in n.Childen) {
                temp += getSubCount(item);
            }
            return temp;
        }
        public int Count => getSubCount(this) + 1;
        /// <summary>
        /// true: 深度优先，false 广度优先
        /// </summary>
        public bool DeepOrBreadthFirst { get; set; }
        public IEnumerable<Tree<T>> Traversal(Tree<T> node = null) {
            var n = node ?? this;
            if (DeepOrBreadthFirst) {
                foreach (var item in n.Childen) {
                    yield return item;
                    foreach (var it in Traversal(item)) {
                        yield return it;
                    }
                }
            } else {
                var temp = new Queue<Tree<T>>();
                temp.Enqueue(n);
                foreach (var item in Traversal(temp)) {
                    yield return item;
                }
            }
        }
        IEnumerable<Tree<T>> Traversal(Tree<T> node, Action<T, bool> otherwork) {
            if (DeepOrBreadthFirst) {
                foreach (var item in node.Childen) {
                    if (otherwork != null) otherwork.Invoke(item.data, false);
                    foreach (var it in Traversal(item, otherwork)) {
                        yield return it;
                        if (otherwork != null) otherwork.Invoke(item.data, true);
                    }
                }
                yield return node;
            } else {
                var temp = new Queue<Tree<T>>();
                temp.Enqueue(node);
                foreach (var item in Traversal(temp)) {
                    if (otherwork != null) otherwork.Invoke(item.data, false);
                    yield return item;
                }
            }
        }
        public void TraversalDatas(Action<T, bool> otherwork = null) {
            var n = this;
            foreach (var it in Traversal(n, otherwork)) ;
        }
        IEnumerable<Tree<T>> Traversal(Queue<Tree<T>> qs) {
            var qs2 = new Queue<Tree<T>>();
            while (qs.count > 0) {
                var item = qs.Dequeue();
                yield return item;
                foreach (var it in item.Childen) {
                    qs2.Enqueue(it);
                }
            }
            if (qs2.count > 0)
                foreach (var item in Traversal(qs2)) {
                    yield return item;
                }
        }
        public void Dispose() {
            foreach (var item in Childen) {
                item.Dispose();
            }
            Childen.Clear();
            data = default(T);
            GroupNo = Level = default(int);
            DeepOrBreadthFirst = IsLeaf = IsRoot = default(bool);
        }
        public IEnumerator<T> GetEnumerator() {
            var n = this;
            if (DeepOrBreadthFirst) {
                foreach (var item in n.Childen) {
                    foreach (var it in Traversal(item)) {
                        yield return it.data;
                    }
                }
                yield return n.data;
            } else {
                var temp = new Queue<Tree<T>>();
                temp.Enqueue(n);
                foreach (var item in Traversal(temp)) {
                    yield return item.data;
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator() {
            var n = this;
            if (DeepOrBreadthFirst) {
                foreach (var item in n.Childen) {
                    foreach (var it in Traversal(item)) {
                        yield return it.data;
                    }
                }
                yield return n.data;
            } else {
                var temp = new Queue<Tree<T>>();
                temp.Enqueue(n);
                foreach (var item in Traversal(temp)) {
                    yield return item.data;
                }
            }
        }
    }
    namespace Functional {
        public class Context<T> {
            public T Data { get; set; }
        }
        public delegate Task<Context<T>> PipelineTask<T>(Context<T> context);
        //public delegate PipelineTask<T> PipelineHandler<T>(PipelineTask<T> task);
        /// <summary>
        /// C# 要形成函数式编程范式，对于扩展方法来说是非用不可的，重要提扩展委托
        /// </summary>
        public static class Fchain {
            public static PipelineTask<T> Start<T>() {
                return async c => await Task.FromResult(c);
            }
            public static PipelineTask<T> PipelineHandler<T>(this PipelineTask<T> task, Func<T, T> handle) {
                return async ctx => {
                    ctx.Data = handle.Invoke(ctx.Data);
                    return await task.Invoke(ctx);
                };
            }
            public static T End<T>(this PipelineTask<T> task, T ctx) {
                var x = task.Invoke(new Context<T> { Data = ctx }).Result;
                return x.Data;
            }
        }
    }
}
