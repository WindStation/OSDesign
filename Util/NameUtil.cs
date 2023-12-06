using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDesign.Util {
    internal class Cmd {
        public readonly static string READ = "read";
        public readonly static string WRITE = "write";
        public readonly static string INPUT = "input";
        public readonly static string OUTPUT = "output";
    }

    internal class ST {
        public readonly static string READY = "ready";
        public readonly static string FINISH = "finish";
        public readonly static string BLOCK = "block";
        public readonly static string RUNNING = "running";
    }
}
