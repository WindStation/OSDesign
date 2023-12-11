using OSDesign.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDesign.Component {
    internal class PCB {
        public List<Process> ProcessList { get; set; } // 记录了所有的进程

        // 下面是多级反馈队列调度所用到的三个就绪队列，优先级从高到低，时间片从小到大
        public LinkedList<Process> ReadyQueue1 { get; set; }  // 就绪队列1，优先级最高，进程能分到4个时间片
        public LinkedList<Process> ReadyQueue2 { get; set; }  // 就绪队列2，进程能分到8个时间片
        public LinkedList<Process> ReadyQueue3 { get; set; }  // 就绪队列3，优先级最低，分到12个时间片

        public LinkedList<Process> BlockQueue { get; set; }  // 阻塞队列
        public PCB() {
            ProcessList = new List<Process>();

            ReadyQueue1 = new LinkedList<Process>();
            ReadyQueue2 = new LinkedList<Process>();
            ReadyQueue3 = new LinkedList<Process>();

            BlockQueue = new LinkedList<Process>();
        }
        public void AddProcess(Process process) {
            //!!只有初始化时能调用这个方法。不能对一个进程多次调用该方法!!
            ProcessList.Add(process);
            ProcessReady1(process);
        }
        void ProcessReady1(Process process) {
            // 表示到来了一个进程，则要将其加入最高优先级的队列，并赋予其较小的时间片（4）
            // 对于阻塞完毕的进程，也可以调用这个方法
            process.Status = ST.READY;
            process.RemainingTimeSlice = 4;
            ReadyQueue1.AddFirst(process);
        }
        void ProcessReady2(Process process) {
            // 调用这个方法时，一般代表第一队列的进程用完了时间片都还没执行完毕，则需要调入队列2，并赋予其更大的时间片（8）
            process.Status = ST.READY;
            process.RemainingTimeSlice = 8;
            ReadyQueue2.AddFirst(process);
        }
        void ProcessReady3(Process process) {
            // 调用这个方法时，一般代表第二队列的进程用完了时间片都还没执行完毕，则调入队列3，并赋予更大的时间片（12）
            process.Status = ST.READY;
            process.RemainingTimeSlice = 12;
            ReadyQueue3.AddFirst(process);
        }
        public string NextTimeSlice(MemoryManage memory) {
            // 这里做进程执行的事情，要负责进程阻塞或访问内存，并调整时间片数量，时间到未做完
            // 每次执行一条指令，就从CommandList中移除这条已执行的指令
            // 考虑一种特殊情况：当最后一条指令是IO指令时，先移除指令（此时CmdList为空）再执行阻塞，那么就需要阻塞结束的检查功能注意这一特殊情况
            // 如果最后一条指令是读写指令，那直接在这里就处理掉了
            int pid = ChooseProcess();
            // 一种特殊情况：就绪队列中没有进程，则ChooseProcess会返回-1，此时直接返回提示信息
            if (pid == -1) {
                return "当前就绪队列中没有进程。";
            }
            var targetProcess = ProcessList[pid];

            string message = "";    // 返回给界面的信息
            targetProcess.Status = ST.RUNNING;  // 修改状态为执行中
            // 取出其下一步的指令
            var nextCommand = targetProcess.CommandSequence[0];

            if (nextCommand == Cmd.INPUT || nextCommand == Cmd.OUTPUT) {
                // IO操作，应当阻塞
                message = "进程 " + pid + " 执行指令 " + nextCommand + "，移入阻塞队列";
                // 从就绪队列中找到并移除该进程
                if (ReadyQueue1.Contains(targetProcess)) {
                    ReadyQueue1.Remove(targetProcess);
                } else if (ReadyQueue2.Contains(targetProcess)) {
                    ReadyQueue2.Remove(targetProcess);
                } else {
                    ReadyQueue3.Remove(targetProcess);
                }
                // 将其加入阻塞队列
                BlockQueue.AddLast(targetProcess);
                // 修改阻塞时间为3
                targetProcess.RemainingBlockTime = 3;
                targetProcess.Status = ST.BLOCK;
                return message;
            }
            // 否则为读写操作，访问内存
            var nextAddress = targetProcess.AddressSequence[0];
            message = "进程 " + pid + " 执行指令 " + nextCommand + " " + nextAddress + "，访问内存：";
            message += memory.Visit(pid, nextAddress);
            // 把可用时间片减一
            targetProcess.RemainingTimeSlice -= 1;
            // 然后判断一下执行完毕没
            if (targetProcess.CommandSequence.Count == 0) {
                message += "\n该进程已执行完毕。";
                targetProcess.Status = ST.FINISH;
                return message;
            }
            // 再看一看时间片有没有用完，用完了就要移到低一个优先级的队列，并赋予其更大的时间片
            if (targetProcess.RemainingTimeSlice == 0) {
                // 找这个进程在哪个队列里
                if (ReadyQueue1.Contains(targetProcess)) {
                    ReadyQueue1.Remove(targetProcess);
                    ProcessReady2(targetProcess);
                } else if (ReadyQueue2.Contains(targetProcess)) {
                    ReadyQueue2.Remove(targetProcess);
                    ProcessReady3(targetProcess);
                } else {
                    ReadyQueue3.Remove(targetProcess);
                    ProcessReady3(targetProcess);
                }
                message += "\n该进程已用完当前时间片。";
                return message;
            }
            // 到这里是一般情况，一个进程既没执行完，也还有剩余的时间片
            return message;
        }
        public int ChooseProcess() {
            // 这里做进程调度的事情，要兼顾就绪队列和阻塞队列的阻塞完毕
            // 首先过一遍阻塞队列，将阻塞时间-1，并检查有没有阻塞结束的
            var blockItem = BlockQueue.First;
            while (blockItem != null) {
                blockItem.Value.RemainingBlockTime -= 1;
                var nextItem = blockItem.Next;  // 先保存下个节点的引用
                // 判断是否阻塞结束了
                if (blockItem.Value.RemainingBlockTime < 0) {
                    // 阻塞结束要做的事情包括：
                    // 1. 常规的，将其加入第一级队列，移出阻塞队列
                    // 2. 对应前面的特殊情况，如果引起阻塞的IO指令是最后一条指令，那么阻塞完毕后应该宣告该进程执行完毕
                    // 因此，就先判断这个进程是不是已经执行完毕了
                    if (blockItem.Value.CommandSequence.Count == 0) {
                        blockItem.Value.Status = ST.FINISH;
                    } else {
                        ProcessReady1(blockItem.Value);
                    }
                    // 最后再从阻塞队列中删除这项
                    BlockQueue.Remove(blockItem);
                }
                blockItem = nextItem;
            }
            // 接下来挑选一个进程去执行
            int pid = -1;

            if (ReadyQueue1.Count > 0) {
                // 先看第一队列
                Debug.Assert(ReadyQueue1.Last != null);
                pid = ReadyQueue1.Last.Value.Id;
            } else if (ReadyQueue2.Count > 0) {
                // 然后第二队列
                Debug.Assert(ReadyQueue2.Last != null);
                pid = ReadyQueue2.Last.Value.Id;
            } else if (ReadyQueue3.Count > 0) {
                // 最后第三队列
                Debug.Assert(ReadyQueue3.Last != null);
                pid = ReadyQueue3.Last.Value.Id;
            }
            // 如果就绪队列里没有进程，就会返回一个特殊值-1.
            return pid;
        }
    }

    internal class Process {
        public int Id { get; set; }
        public List<int> AddressSequence { get; set; }
        public List<string> CommandSequence { get; set; }
        public string Status { get; set; }
        public int RemainingTimeSlice { get; set; }
        public int RemainingBlockTime { get; set; }

        public Process(int id, List<int> addressSequence, List<string> commandSequence) {
            Id = id;
            AddressSequence = addressSequence;
            CommandSequence = commandSequence;
            Status = ST.READY;
            RemainingBlockTime = -1;    // 表示初始未被阻塞
        }
    }
}
