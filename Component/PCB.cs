using OSDesign.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDesign.Component {
    internal class PCB {
        public LinkedList<Process> ProcessList; // 记录了所有的进程
        public LinkedList<Process> ReadyQueue;  // 就绪队列
        public LinkedList<Process> BlockQueue;  // 阻塞队列
        public PCB() {
            ProcessList = new LinkedList<Process>();
            ReadyQueue = new LinkedList<Process>();
            BlockQueue = new LinkedList<Process>();
        }
        public void AddProcess(Process process) {
            ProcessList.AddLast(process);
        }
        public void NextTimeSlice() {
            // 下一个时间片，进程调度和执行
        }
        public int ChooseProcess() {
            // 进程调度，选择一个进程去执行
            return 1;
        }
    }

    internal class Process {
        public int Id { get; set; }
        public List<int> AddressSequence { get; set; }
        public List<string> CommandSequence { get; set; }
        public string Status { get; set; }
        public int Priority { get; set; }
        public int RemainingTimeSlice { get; set; }

        public Process(int id, List<int> addressSequence, List<string> commandSequence) {
            Id = id;
            AddressSequence = addressSequence;
            CommandSequence = commandSequence;
            Status = ST.READY;
        }
    }
}
