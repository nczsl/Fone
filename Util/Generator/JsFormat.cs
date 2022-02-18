// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Util;

namespace Util.Generator {
    /// <summary>
    /// 解析js文档
    /// </summary>
    public class JsFormat : CodeFormat {
        public List<JsModules> modules;
        public JsFormat() {
            base.LoadDoc += JsDom_LoadDoc;
            this.modules = new List<JsModules>();
        }
        string content;
        int doclen;
        readonly int idx;
        JsModules mod;

        private void JsDom_LoadDoc(string obj) {
            this.content = obj;
            this.doclen = content.Length;
            this.mod = new JsModules();
            modules.Add(mod);
        }
        /*
         * 设计思路：
         * 两个处理，
         * 1，先处理大括号
         * 2，再处理分号
         * 接下来需要，一个全局跟踪状态
         * 1，需要跟踪，全局文档位置索引 int类型
         * 2，需要跟踪block的嵌套，有一个始终指向当前的block
         */
        void ParseBlock() {
            var i = 0; ;
            while (i < doclen) {
                var c = content[i];
                switch (c) {
                    case '{': OpenBlock(); break;
                    case '}': CloseBlock(); break;
                    case ';': StaticSentence(); break;
                }
            }
        }

        private void OpenParameter() {

        }

        private void CloseParameter() {

        }

        private void StaticSentence() {

        }

        private void OpenBlock() {
            //content.IndexOf()
        }

        private void CloseBlock() {

        }
        void ParseSentences(string content) {

        }
    }

    /// <summary>
    /// 代表一个js文件所包含的对象模型
    /// </summary>
    public class JsModules : CodeFormat {
        public string path;
        public string name => this.path.Split(new[] { '/', '\\' }).Last();
        public List<JsFormat> jsformats;
        public JsModules() {
            this.jsformats = new List<JsFormat>();
        }
    }
    /// <summary>
    /// js文档里的大括号{} 这个代表一个代码块
    /// </summary>
    public class JsBlock : JsFormat {
        public JsKey _type;
        public string name;
        public List<JsBlock> subs;
        public override string ToString() {
            return base.ToString();
        }
    }
    public class JsMethod : JsBlock {
        public JsMethod() {
        }
    }
    public class JsFunction : JsBlock {
        public JsFunction() {
            this._type = JsKey.function;
        }
    }
    public class JsClass : JsBlock {
        public JsClass() {
            this._type = JsKey._class;
        }

    }
    public class JsInterface : JsBlock {
        public JsInterface() {
            this._type = JsKey._interface;
        }

    }
    /// <summary>
    /// js的对象字面量
    /// </summary>
    public class JsObject : JsBlock {

    }
    /// <summary>
    /// js的语句逻辑块，比如 if else for do while 等
    /// </summary>
    public class JsLogic : JsBlock {

    }
    /// <summary>
    /// js的语句
    /// </summary>
    public class JsSentence : JsFormat {

    }
    public struct JsKey {
        public static JsKey _break {
            get {
                return nameof(_break).Substring(1);
            }
        }
        public static JsKey _case => nameof(_case).Substring(1);
        public static JsKey _catch => nameof(_catch).Substring(1);
        public static JsKey _continue => nameof(_continue).Substring(1);
        public static JsKey _default => nameof(_default).Substring(1);
        public static JsKey delete => nameof(delete);
        public static JsKey _do => nameof(_do).Substring(1);
        public static JsKey _else => nameof(_else).Substring(1);
        public static JsKey _finally => nameof(_finally).Substring(1);
        public static JsKey _for => nameof(_for).Substring(1);
        public static JsKey function => nameof(function);
        public static JsKey _if => nameof(_if).Substring(1);
        public static JsKey _in => nameof(_in).Substring(1);
        public static JsKey instanceof => nameof(instanceof);
        public static JsKey _new => nameof(_new).Substring(1);
        public static JsKey _return => nameof(_return).Substring(1);
        public static JsKey _switch => nameof(_switch).Substring(1);
        public static JsKey _this => nameof(_this).Substring(1);
        public static JsKey _throw => nameof(_throw).Substring(1);
        public static JsKey _try => nameof(_try).Substring(1);
        public static JsKey _typeof => nameof(_typeof).Substring(1);
        public static JsKey _var => nameof(_var).Substring(1);
        public static JsKey _void => nameof(_void).Substring(1);
        public static JsKey _while => nameof(_while).Substring(1);
        public static JsKey with => nameof(with);
        public static JsKey let => nameof(let).Substring(1);
        public static JsKey _abstract => nameof(_abstract).Substring(1);
        public static JsKey arguments => nameof(arguments);
        public static JsKey boolean => nameof(boolean);
        public static JsKey _string => nameof(_string).Substring(1);
        public static JsKey _byte => nameof(_byte).Substring(1);
        public static JsKey _char => nameof(_char).Substring(1);
        public static JsKey _class => nameof(_class).Substring(1);
        public static JsKey _const => nameof(_const).Substring(1);
        public static JsKey debugger => nameof(debugger).Substring(1);
        public static JsKey _double => nameof(_double).Substring(1);
        public static JsKey _enum => nameof(_enum).Substring(1);
        public static JsKey eval => nameof(eval);
        public static JsKey export => nameof(export);
        public static JsKey extends => nameof(extends);
        public static JsKey _false => nameof(_false);
        public static JsKey final => nameof(final);
        public static JsKey _float => nameof(_float).Substring(1);
        public static JsKey _goto => nameof(_goto).Substring(1);
        public static JsKey implements => nameof(implements);
        public static JsKey import => nameof(import);
        public static JsKey _int => nameof(_int).Substring(1);
        public static JsKey _interface => nameof(_interface).Substring(1);
        public static JsKey _long => nameof(_long).Substring(1);
        public static JsKey native => nameof(native).Substring(1);
        public static JsKey _null => nameof(_null).Substring(1);
        public static JsKey package => nameof(package);
        public static JsKey _private => nameof(_private).Substring(1);
        public static JsKey _protected => nameof(_protected).Substring(1);
        public static JsKey _public => nameof(_public).Substring(1);
        public static JsKey _short => nameof(_short).Substring(1);
        public static JsKey _static => nameof(_static).Substring(1);
        public static JsKey super => nameof(super);
        public static JsKey synchronized => nameof(synchronized);
        public static JsKey throws => nameof(throws);
        public static JsKey transient => nameof(transient);
        public static JsKey _true => nameof(_true).Substring(1);
        public static JsKey _volatile => nameof(_volatile).Substring(1);
        public static JsKey _yield => nameof(_yield).Substring(1);
        string value;
        public static implicit operator string(JsKey key) {
            return key.value;
        }
        public static implicit operator JsKey(string key) {
            JsKey kkey;
            kkey.value = key;
            return kkey;
        }
    }
}
