using System;
using System.Collections.Generic;
using System.Text;

namespace Utilcmd {
    public class TestEn {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
    }
    public class RequestAction {
        public string name;
        public string url;
        public string httpMethod;
        public string isTagPackage;
        public string _params;
        public string paramsFormat;         
    }
    public class ControllerRas {
        public string name;
        public RequestAction[] dalist;
    }
}
