using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;

namespace Utilcmd.cmdlets {
    public class Test1 :Cmdlet{
        //override 
        [Parameter]
        public string Path { get; set; }

    }
}
