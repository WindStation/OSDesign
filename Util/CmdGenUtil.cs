using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDesign.Util {
    internal class CmdGenUtil {
        public static CmdGenUtil Instance { get; } = new CmdGenUtil();
        public List<CommandModel> COMMANDS { get; }
        private CmdGenUtil() {
            // 为方便表示，0-read，1-write，2-input，3-output
            COMMANDS = new() {
                new(new int[] {0,0,1,2,1,1,3,1 }),
                new(new int[] {1,0,2,0,1,0,0,3,0,1,0}),
                // TODO
            };
        }

    }

    internal class CommandModel {
        public List<string> Commands { get; set; }
        public int AddressCount { get; set; }
        public CommandModel(int[] intCmds) {
            Commands = new List<string>(intCmds.Select(MapIntToCmd));
            AddressCount = Commands.Count(cmd => cmd == Cmd.READ || cmd == Cmd.WRITE);
        }

        private string MapIntToCmd(int value) {
            switch (value) {
                case 0:
                    return Cmd.READ;
                case 1:
                    return Cmd.WRITE;
                case 2:
                    return Cmd.INPUT;
                case 3:
                    return Cmd.OUTPUT;
                default:
                    throw new ArgumentException("Invalid Integer Value");
            }
        }
    }
}
