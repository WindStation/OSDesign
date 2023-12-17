using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace OSDesign.Component {
    internal class MemoryManage {
        public List<Page> PageTable { get; set; }   // 页表
        public static List<Block> Blocks { get; set; } = new List<Block>(); // 记录物理块信息
        public int PageCapacity { get; set; }
        public LinkedList<Block> LoadedList { get; set; } = new();  // 已经加载到内存的页表
        int numPage, numBlock; // 外部要获取这两个值，可以通过两个List的 Count 来获取
        int startBlockId;   // 记录给该进程分配的物理块的起始，采用连续分配

        public MemoryManage(int numPage, int numBlock, int pageCapacity) {
            this.numPage = numPage;
            this.numBlock = numBlock;
            PageTable = new List<Page>();
            for (int i = 0; i < numPage; i++) {
                PageTable.Add(new(i));
            }
            startBlockId = Blocks.Count;    // 记录给这个进程分配的物理块的起始
            for (int i = 0; i < numBlock; i++) {
                Blocks.Add(new(startBlockId + i));
            }
            PageCapacity = pageCapacity;
        }

        public static List<int> GenerateAddresses(int totalAddresses) {
            var addresses = new List<int>();
            var random = new Random();
            // 暂时用固定的
            var numPage = 10;
            var PageCapacity = 50;

            // 先生成50%随机地址
            for (int i = 0; i < totalAddresses / 2; i++) {
                addresses.Add(random.Next(0, numPage * PageCapacity));
            }
            // 后50%为循环连续地址
            if (totalAddresses % 2 != 0) {
                totalAddresses += 1;
            }
            int start = random.Next(0, numPage * PageCapacity);
            for (int i = 0; i < totalAddresses / 2; i++) {
                addresses.Add((start + i) % (numPage * PageCapacity));
            }

            return addresses;
        }

        public string Visit(int processId, int address) {
            int pageId = address / PageCapacity;
            string message = "需要访问页面 " + pageId + "。\n";
            // 判断是否命中
            if (PageTable[pageId].Status == 1) {
                // 命中
                HitPage(processId, pageId);
                message += "命中内存。";
            } else {
                // 需要把这一页加载进来
                message += "未命中，";
                message += LoadPage(processId, pageId);
            }
            return message;
        }

        void HitPage(int processId, int pageId) {
            // 命中后，将命中的页面重新提到LoadedList的尾部
            var item = LoadedList.First;
            while (item != null) {
                if (item.Value.PageId == pageId) {
                    LoadedList.Remove(item);
                    LoadedList.AddLast(item);
                    break;
                }
                item = item.Next;
            }
        }

        string LoadPage(int processId, int targetPageId) {
            string message;
            int availableId = -1;
            if (LoadedList.Count < numBlock) {
                // 还有物理块可用
                for (int i = 0; i < numBlock; i++) {
                    if (Blocks[i + startBlockId].Status == 1) {
                        availableId = i + startBlockId;
                        break;
                    }
                }
                message = "将所需页面 " + targetPageId + " 加载到空闲的物理块 " + availableId + " 上。";
            } else {
                // 缺页
                int oldPageId = -1;
                availableId = Replace(ref oldPageId);
                message = "且无空闲物理块，按照LRU算法，将物理块 " + availableId + " 上原页面 " + oldPageId + " 调出，并调入该页面。";
            }
            // 将页面载入物理块，修改信息
            PageTable[targetPageId].Status = 1;
            PageTable[targetPageId].BlockId = availableId;
            Blocks[availableId].Status = 0;
            Blocks[availableId].PageId = targetPageId;
            // 最后将其加入LoadedList尾部
            LoadedList.AddLast(Blocks[availableId]);
            return message;
        }

        int Replace(ref int oldPageId) {
            Debug.Assert(LoadedList.First != null);
            var oldBlock = LoadedList.First.Value;
            oldPageId = oldBlock.PageId;
            LoadedList.RemoveFirst();
            // 然后修改页表等信息
            PageTable[oldBlock.PageId].Status = 0;
            Blocks[oldBlock.BlockId].Status = 1;
            // 返回值为可用块的块号
            return oldBlock.BlockId;
        }
    }

    internal class Page {
        public int PageId { get; set; } // 页号
        public int BlockId { get; set; }// 物理块号
        public int Status { get; set; } // 0为未装入内存，1为装入
        public bool Modify { get; set; } // 是否更改过 
        public string DisplayBlockId { // 用于界面上展示的，只有当已经装入物理块时，才显示信息
            get {
                return Status == 1 ? Convert.ToString(BlockId) : "";
            }
        }
        public SolidColorBrush DisplayColor {
            get {
                return Status == 1 ? Brushes.DarkGreen : Brushes.OrangeRed;
            }
        }

        public Page(int id) {
            PageId = id;
            Status = 0;
            Modify = false;
        }
    }

    internal class Block {
        public int BlockId { get; set; }    // 物理块号
        public int PageId { get; set; } // 装入该块的页号
        public int Status { get; set; } // 1为空闲，0为已装载页面
        public string DisplayStatus {   // 供展示用，若空闲显示"物理块号 | —"，不空闲则显示装入的页
            get {
                if (Status == 1) {
                    return BlockId + " | —";
                }
                return BlockId + " | " + PageId;
            }
        }
        public SolidColorBrush DisplayColor {   // 供展示用，设置显示的颜色
            get {
                if (Status==1) {
                    return Brushes.DarkGreen;
                }
                return Brushes.OrangeRed;
            }
        }

        public Block(int blockId) {
            BlockId = blockId;
            Status = 1;
        }
    }

    internal class Test {
        static void Main(string[] args) {
            Console.WriteLine("------------- Test: MemoryManage::Test -------------");
            var memory = new MemoryManage(10, 5, 50);
            var addresses = MemoryManage.GenerateAddresses(4);
            foreach (var address in addresses) {
                Console.Write(address + ", ");
            }
            foreach (var address in addresses) {
                memory.Visit(0, address);
            }
        }
    }

}
