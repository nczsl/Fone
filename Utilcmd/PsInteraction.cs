using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;

namespace Utilcmd
{
    /// <summary>
    /// 与外部的ps脚交互，执行，并处理约定的返回结果等
    /// </summary>
    public class PsInteraction
    {
        public static async Task ExcutePs(Action<IEnumerable<PSObject>> handler, params string[] cmds)
        {
            using (var ps = PowerShell.Create())
            {
                var script = string.Join("\n", cmds);
                var results = await ps.AddScript(script).InvokeAsync();
                if (handler == null)
                    foreach (var result in results)
                    {
                        Console.WriteLine(result);
                    }
                else
                    handler.Invoke(results);
            }
        }
        public static void ExcutePsRunspace(Action<IEnumerable<PSObject>> handler, params string[] cmds)
        {
            using (var runspace = RunspaceFactory.CreateRunspace())
            {
                runspace.Open();
                using (var ps = PowerShell.Create())
                {
                    ps.Runspace = runspace;
                    var script = string.Join("\n", cmds);
                    var result = ps.AddScript(script).Invoke();
                    if (handler == null)
                        foreach (var line in result)
                        {
                            Console.WriteLine(result);
                            // ps.AddCommand("write-output").AddArgument(line).Invoke();
                        }
                    else
                        handler.Invoke(result);
                }
                runspace.Close();
            }
        }
        static public void ReportHtml(string content)
        {
            var mark = DateTime.Now.ToString("yyyyMMddHHmmss");
            var htmlFileName = $"testReport{mark}.html";
            var cmd1 =
                "$workDir = Split-Path -Parent $MyInvocation.MyCommand.Definition";
            var cmd2 =
                $"\"{content}\"|out-file  \"$workDir\\{htmlFileName}\"";
            var cmd3 = $"invoke-item \"$workDir\\{htmlFileName}\"";
            ExcutePs(null, cmd1, cmd2, cmd3).Wait();
        }
    }
}
