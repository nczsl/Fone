using System;
using System.Linq;
using System.Collections.Generic;

namespace Utilcmd {
    public interface ICmder {
        void Help(IEnumerable<string> parameters);
        void Orm(IEnumerable<string> parameters);
        void Clip(IEnumerable<string> parameters);
        void Crawler(IEnumerable<string> parameters);
        void Actions(IEnumerable<string> parameters);
        void Entities(IEnumerable<string> parameters);
        void GenerateDts(IEnumerable<string> jsmoduls);
        void Netlibs(IEnumerable<string> parameters);
        void Bizallview(IEnumerable<string> parameters);
        Action<IEnumerable<string>> Restart { get; set; }
    }
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("welcome use utilcmd tools");
            ICmder icmd = new DefaultExcuter();//need to setting
            ParseArgs(args, icmd);
        }
        static void ParseArgs(string[] args, ICmder icmd) {
            // System.Console.WriteLine(string.Concat(args));
            if (icmd == null) {
                Console.WriteLine("nothing executer ..");
                return;
            }
            var parameters = args.Skip(1);
            var cmd = "";
            if (args.Length == 0) {
                cmd = "-h";
            } else {
                cmd = args[0];
            }
            switch (cmd) {
                case Cmdx.actions: icmd.Actions(parameters); break;
                case Cmdx.bizallview: icmd.Bizallview(parameters); break;
                case Cmdx.clip: icmd.Clip(parameters); break;
                case Cmdx.crawler: icmd.Crawler(parameters); break;
                case Cmdx.dts: icmd.GenerateDts(parameters); break;
                case Cmdx.entities: icmd.Entities(parameters); break;
                case Cmdx.h: icmd.Help(parameters); break;
                case Cmdx.help: icmd.Help(parameters); break;
                case Cmdx.netlibs: icmd.Netlibs(parameters); break;
                case Cmdx.orm: icmd.Orm(parameters); break;
            }
            System.Console.WriteLine("开启检查是否需要重启！");
            icmd.Restart?.Invoke(parameters);
        }
    }
}
