using OSDesign.Component;
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
    /// Already Deprecated
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

        public MainWindow() {
            InitializeComponent();
            InitializeMemory();
            AddProcess();
            AllProcessesGrid.IsReadOnly = true;
            AllProcessesGrid.ItemsSource = pcb.ProcessList.ToList();
            numTS.Content = timeSlice;
        }
        
        private void InitializeMemory() {
            // 初始化存储管理模块
            memory = new(10, 5, 50);
        }

        private void AddProcess() {
            var addresses = memory.GenerateAddresses(20);
            // TODO
            pcb.AddProcess(new(autoGenId++, addresses, new()));
            RefreshAll();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            AddProcess();
        }

        private void nextTsBtn_Click(object sender, RoutedEventArgs e) {
            timeSlice += 1;
            pcb.NextTimeSlice();
            numTS.Content = timeSlice;
        }

    }
}
