using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDesign.Component {
    internal class MemoryManage {
        public List<Page> PageTable { get; set; }   // 页表
        public List<Block> Blocks { get; set; } // 记录物理块信息
        public int PageCapacity { get; set; }
        public LinkedList<Block> LoadedList { get; set; } = new();  // 已经加载到内存的页表
        int numPage, numBlock; // 外部要获取这两个值，可以通过两个List的 Count 来获取

        public MemoryManage(int numPage, int numBlock, int pageCapacity) {
            this.numPage = numPage;
            this.numBlock = numBlock;
            PageTable = new List<Page>();
            for (int i = 0; i < numPage; i++) {
                PageTable.Add(new(i));
            }
            Blocks = new List<Block>();
            for (int i = 0; i < numBlock; i++) {
                Blocks.Add(new(i));
            }
            PageCapacity = pageCapacity;
        }
        public List<int> GenerateAddresses(int totalAddresses) {
            var addresses = new List<int>();
            var random = new Random();
            // 先生成50%随机地址
            for (int i = 0; i < totalAddresses / 2; i++) {
                addresses.Add(random.Next(0, numPage * PageCapacity));
            }
            // 后50%为循环连续地址
            int start = random.Next(0, numPage * PageCapacity);
            for (int i = 0; i < totalAddresses / 2; i++) {
                addresses.Add((start + i) % (numPage * PageCapacity));
            }

            return addresses;
        }

        public void Visit(int processId, int address) {
            int pageId = address % PageCapacity;
            // 判断是否命中
            if (PageTable[pageId].Status == 1) {
                // 命中
                HitPage(processId, pageId);
            } else {
                // 需要把这一页加载进来
                LoadPage(processId, pageId);
            }
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

        void LoadPage(int processId, int targetPageId) {
            int availableId = -1;
            if (LoadedList.Count < numBlock) {
                // 还有物理块可用
                for (int i = 0; i<numBlock; i++) {
                    if (Blocks[i].Status == 1) {
                        availableId = i;
                        break;
                    }
                }
            } else {
                // 缺页
                availableId = Replace();
            }
            // 将页面载入物理块，修改信息
            PageTable[targetPageId].Status = 1;
            PageTable[targetPageId].BlockId = availableId;
            Blocks[availableId].Status = 0;
            Blocks[availableId].PageId = targetPageId;
            // 最后将其加入LoadedList尾部
            LoadedList.AddLast(Blocks[availableId]);
        }

        int Replace() {
            Debug.Assert(LoadedList.First != null);
            var oldBlock = LoadedList.First.Value;
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

        public Block(int blockId) {
            BlockId = blockId;
            Status = 1;
        }
    }

    internal class Test {
        static void Main(string[] args) {
            Console.WriteLine("------------- Test: MemoryManage::Test -------------");
            var memory = new MemoryManage(10, 5, 50);
            var addresses = memory.GenerateAddresses(100);
            foreach (var address in addresses) {
                Console.Write(address+", ");
            }
        }
    }

}
