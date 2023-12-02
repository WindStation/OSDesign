using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDesign.Component {
    internal class MemoryManage {
        public List<Page> PageTable { get; set; }   // 页表
        public List<Block> Blocks { get; set; } // 记录物理块信息
        public int PageCapacity { get; set; }
        public LinkedList<Block> LoadedList { get; set; } = new();  // 已经加载到内存的页表
        int numPage, numBlocks;

        public MemoryManage(int numPage, int numBlock, int pageCapacity) {
            this.numPage = numPage;
            this.numBlocks = numBlock;
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
            Console.WriteLine("Main in DemandPage");
        }
    }

}
