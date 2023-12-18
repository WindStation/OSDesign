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
using System.Windows.Shapes;

namespace OSDesign {
    /// <summary>
    /// ProcessDetail.xaml 的交互逻辑
    /// </summary>
    public partial class ProcessDetail : Window {
        internal ProcessDetail(Process process) {
            
            InitializeComponent();
            DetailTitle.Content = "进程 #" + process.Id + " 信息";
            PageTitle.Content = "进程 #" + process.Id + " 页表";
            PidText.Content = process.Id;
            StatusText.Content = process.Status;
            StatusText.Foreground = process.StatusColor;
            DetailCmdTable.ItemsSource = process.DisplayDetail;

            PageGrid.ItemsSource = process.memory.PageTable;
            LoadedPageGrid.ItemsSource = process.memory.LoadedList;
        }
    }
}
