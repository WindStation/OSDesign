﻿using OSDesign.Component;
using OSDesign.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OSDesign {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        PCB pcb = new();
        //MemoryManage memory;
        int autoGenId = 0;
        int timeSlice = 0;

        private void RefreshAll() {
            // 刷新“全部进程”表
            AllProcessesGrid.ItemsSource = null;
            AllProcessesGrid.ItemsSource = pcb.ProcessList;
        }

        private void RefreshReady() {
            // 刷新“就绪队列”，三个同时刷新
            ReadyGrid1.ItemsSource = null;
            ReadyGrid1.ItemsSource = pcb.ReadyQueue1.Reverse();
            ReadyGrid2.ItemsSource = null;
            ReadyGrid2.ItemsSource = pcb.ReadyQueue2.Reverse();
            ReadyGrid3.ItemsSource = null;
            ReadyGrid3.ItemsSource = pcb.ReadyQueue3.Reverse();
        }

        private void RefreshBlock() {
            // 刷新阻塞队列
            BlockQueueGrid.ItemsSource = null;
            BlockQueueGrid.ItemsSource= pcb.BlockQueue.Reverse();
        }
        private void RefreshPageTable(int pid) {
            PageGrid.ItemsSource = null;
            if (pid == -1) {
                PageTableTitle.Content = "当前没有正在运行的进程。";
                return;
            }
            PageGrid.ItemsSource = pcb.ProcessList[pid].memory.PageTable;
            // 刷新标题
            PageTableTitle.Content = "页表 #" + pid;
        }
        private void RefreshLoaded(int pid) {
            LoadedPageGrid.ItemsSource = null;
            if (pid == -1) {
                return;
            }
            LoadedPageGrid.ItemsSource = pcb.ProcessList[pid].memory.LoadedList;
        }
        private void RefreshMemoryDisplay() {
            MemoryDisplay.ItemsSource = null;
            MemoryDisplay.ItemsSource = MemoryManage.Blocks;
        }
        public MainWindow() {
            InitializeComponent();
            //InitializeMemory();
            AddProcess();
            AllProcessesGrid.IsReadOnly = true;
            AllProcessesGrid.ItemsSource = pcb.ProcessList;
            // 设置只读
            ReadyGrid1.IsReadOnly = true;
            ReadyGrid2.IsReadOnly = true;
            ReadyGrid3.IsReadOnly = true;

            BlockQueueGrid.IsReadOnly= true;

            PageGrid.IsReadOnly = true;
            //PageGrid.ItemsSource = memory.PageTable;
            numTS.Content = timeSlice;
        }
        
        //private void InitializeMemory() {
        //    // 初始化存储管理模块
        //    // 设置为：10页面，5物理块，每个页面容量为50地址
        //    memory = new(10, 5, 50);
        //}

        private void AddProcess() {
            var cmdGen = CmdGenUtil.Instance;
            var commandModel = cmdGen.COMMANDS[autoGenId % cmdGen.COMMANDS.Count];

            var commands = new List<string>(commandModel.Commands);
            var addresses = MemoryManage.GenerateAddresses(commandModel.AddressCount);

            pcb.AddProcess(new(autoGenId++, addresses, commands));
            RefreshAll();
            RefreshReady();
            RefreshMemoryDisplay();
        }

        private void AllProcessesGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var row = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow;
            if(row != null) {
                Process? targetProcess = row.Item as Process;
                if (targetProcess != null) {
                    new ProcessDetail(targetProcess).Show();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            AddProcess();
        }

        private void nextTsBtn_Click(object sender, RoutedEventArgs e) {
            timeSlice += 1;
            int currentPid = -1;    // 用于记录这个时间片执行了哪个进程
            var message = pcb.NextTimeSlice(ref currentPid);
            //MessageBox.Content = message;
            MessageBoxBlock.Text = message;
            numTS.Content = timeSlice;
            RefreshAll();
            RefreshReady();
            RefreshBlock();
            RefreshPageTable(currentPid);
            RefreshLoaded(currentPid);
            RefreshMemoryDisplay();
        }

        
    }
}
