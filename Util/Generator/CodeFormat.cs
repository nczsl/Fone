// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Util.Generator {
    public class CodeFormat {
        int spaceNumber;
        /// <summary>
        /// 获取或设置固定缩进增量，是以空格的累加来构造
        /// 数值代表空格个数，默认值为4
        /// </summary>
        public int SpaceNum {
            get => this.spaceNumber;
            set {
                this.spaceNumber = value;
                var r = string.Empty;
                for (int i = 0; i < spaceNumber; i++) {
                    r += space;
                }
                this.Indent = r;
            }
        }
        /// <summary>
        /// 固定的缩进增量
        /// </summary>
        internal virtual string Indent { get; private set; }
        internal const string space = " ";
        internal string NewLine => Environment.NewLine;
        public event EventHandler<string> IndentonChenaged;
        /// <summary>
        ///  当前缩进，默认从父节点继承,值改变时会触发一个事件
        /// </summary>
        protected internal string Indenton {
            get => indenton;
            set {
                indenton = value;
                IndentonChenaged?.Invoke(this, value);
            }
        }
        /// <summary>
        ///  当前缩进，默认从父节点继承
        /// </summary>
        internal string indenton;
        protected StringBuilder root;
        protected CodeFormat(int sn = 4) {
            this.spaceNumber = sn;
            var r = string.Empty;
            for (int i = 0; i < spaceNumber; i++) {
                r += space;
            }
            this.Indent = r;
            root = new StringBuilder();
        }
        public override string ToString() {
            return root.ToString();
        }
        public void AppendLine(string value) {
            root.AppendLine($"{Indenton}{value}");
        }
        /// <summary>
        /// 父节点调用
        /// </summary>
        /// <param name="retractcur"></param>
        protected void Push(ref string retractcur) {
            retractcur = indenton + Indent;
        }
        /// <summary>
        /// 父节点调用
        /// </summary>
        /// <param name="retractcur"></param>
        /// <param name="sps"></param>
        protected void Push(ref string retractcur, string sps) {
            retractcur = indenton + sps;
        }
        /// <summary>
        /// 子节点调用
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public CodeFormat Push(CodeFormat parent) {
            this.indenton = parent.indenton + Indent;
            return this;
        }
        //
        public static T ReadDocumentByPath<T>(string path) where T : CodeFormat, new() {
            var cf = new T();
            try {
                using (var sr = new StreamReader(File.OpenRead(path))) {
                    cf.OnLoad(sr.ReadToEnd());
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            return cf;
        }
        protected virtual event Action<string> LoadDoc;
        protected virtual void OnLoad(string doc) => LoadDoc?.Invoke(doc);
        public static T ReadDocument<T>(string doc) where T : CodeFormat, new() {
            var cf = new T();
            try {
                cf.OnLoad(doc);
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            return cf;
        }
    }
}
