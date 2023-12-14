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
        MemoryManage memory;
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
            ReadyGrid1.ItemsSource = pcb.ReadyQueue1;
            ReadyGrid2.ItemsSource = null;
            ReadyGrid2.ItemsSource = pcb.ReadyQueue2;
            ReadyGrid3.ItemsSource = null;
            ReadyGrid3.ItemsSource = pcb.ReadyQueue3;
        }

        public MainWindow() {
            InitializeComponent();
            InitializeMemory();
            AddProcess();
            AllProcessesGrid.IsReadOnly = true;
            AllProcessesGrid.ItemsSource = pcb.ProcessList;
            numTS.Content = timeSlice;
        }
        
        private void InitializeMemory() {
            // 初始化存储管理模块
            memory = new(10, 5, 50);
        }

        private void AddProcess() {
            var cmdGen = CmdGenUtil.Instance;
            var commandModel = cmdGen.COMMANDS[autoGenId % cmdGen.COMMANDS.Count];

            var commands = new List<string>(commandModel.Commands);
            var addresses = memory.GenerateAddresses(commandModel.AddressCount);

            pcb.AddProcess(new(autoGenId++, addresses, commands));
            RefreshAll();
            RefreshReady();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            AddProcess();
        }

        private void nextTsBtn_Click(object sender, RoutedEventArgs e) {
            timeSlice += 1;
            var message = pcb.NextTimeSlice(memory);
            MessageBox.Content = message;
            numTS.Content = timeSlice;
            RefreshAll();
            RefreshReady();
        }

    }
}
