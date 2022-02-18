using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Utilcmd {
    public static class Cmdx {
        ///<summary>生成cs代码类 orm,根据指定dll 或 exe (包括完整路径)</summary>
        public const string orm = "-" + nameof(orm);
        /// <summary>通过powershell core 来使用剪贴板，将消息存储到系统剪贴板上</summary>
        public const string clip = "-" + nameof(clip);
        /// <summary>本工具帮助文档</summary>
        public const string help = "-" + nameof(help);
        /// <summary>help 命令简写</summary>
        public const string h = "-" + nameof(h);
        /// <summary>爬取指定网络数据</summary>
        public const string crawler = "-" + nameof(crawler);
        /// <summary>获取指定程序集中的所有controller的所有action,并生成相应前端代码</summary>
        public const string actions = "-" + nameof(actions);
        /// <summary>获取指定程序集中的所有DbContext中所包含的实体类,并生成相应前端代码</summary>
        public const string entities = "-" + nameof(entities);
        /// <summary>根据输入的js模块生成一个typescript文档</summary>
        public const string dts = "-" + nameof(dts);
        /// <summary> 对Util,Fone进行命令交互式测试</summary>
        public const string netlibs = "-" + nameof(netlibs);
        public const string bizallview = "-" + nameof(bizallview);
        static public char[] parametersSplit = ",;|".ToCharArray();
        static public IEnumerable<string> CmdCatalog {
            get {
                yield return orm;
                yield return clip;
                yield return h;
                yield return help;
                yield return crawler;
                yield return actions;
                yield return entities;
                yield return dts;
                yield return netlibs;                
                yield return bizallview;                
            }
        }
		static public string CmdInfo {
            get {
                var sb = new StringBuilder();				
                sb.AppendLine("-orm:生成cs代码类 orm,根据指定dll 或 exe (包括完整路径)");
                sb.AppendLine("-clip:通过powershell core 来使用剪贴板，将消息存储到系统剪贴板上");
                sb.AppendLine("-help/-h:显示本帮助文档");
                sb.AppendLine("-crawler:爬取指定网络数据");
                sb.AppendLine("-actions:获取指定程序集中的所有controller的所有action,并生成相应前端代码");
                sb.AppendLine("-entities:获取指定程序集中的所有DbContext中所包含的实体类,并生成相应前端代码");
                sb.AppendLine("-dts:根据输入的js模块生成一个typescript文档");
                sb.AppendLine("-netlibs:开启交互式测试，测试目标为，core/Util项目，所需要参数，为类名和测试方法名");
                sb.AppendLine("-bizallview:开启交互式测试，测试目标为，core/Util项目，所需要参数，为类名和测试方法名");
                sb.AppendLine("-------------------------------------------");
                sb.AppendLine("各命令参数通过空格在其后面输入，多参数命令通过 ',' ';' '|' 等三种中任一一种符号进行分割表示" );
                return sb.ToString();
            }
        }
    }
}