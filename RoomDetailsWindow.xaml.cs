using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
using System.IO;

namespace KTVApp
{
    /// <summary>
    /// RoomDetailsWindow.xaml 的互動邏輯
    /// </summary>
    public partial class RoomDetailsWindow : Window
    {
        private readonly Room _room;

        public RoomDetailsWindow(Room room)
        {
            InitializeComponent();
            _room = room;
            DataContext = _room;
            CenterWindowOnMainWindow();
        }

        private void CenterWindowOnMainWindow()
        {
            if (Application.Current.MainWindow != null)
            {
                this.Owner = Application.Current.MainWindow;
                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            await SendTcpSignal(_room.RoomNumber.Substring(1), "O");
        }

        private async void Shutdown_Click(object sender, RoutedEventArgs e)
        {
            await SendTcpSignal(_room.RoomNumber.Substring(1), "X");
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("取消");
        }

        private void OpenAccount_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("包廂開帳");
        }

        private void CloseAccount_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("包廂關帳");
        }

        private async Task SendTcpSignal(string roomNumber, string command)
        {
            try
            {
                string message = $"{roomNumber},{command}";
                LogToFile($"嘗試發送: {message}");
                using (TcpClient client = new TcpClient(_room.RoomPC, 1000)) // 使用电腦主機名稱和端口
                {
                    NetworkStream stream = client.GetStream();
                    byte[] data = Encoding.ASCII.GetBytes(message);
                    await stream.WriteAsync(data, 0, data.Length);
                    MessageBox.Show($"成功發送{command}指令到 {_room.RoomPC}");

                    // 根據指令更新 Room.Status
                    if (command == "O")
                    {
                        // 當收到"O"指令時，將TimeRange更新為當下時間的 "HH:mm-" 格式
                        _room.TimeRange = DateTime.Now.ToString("HH:mm") + "-";
                        // 將Status更新為"Occupied"
                        _room.Status = "已占用";
                    }
                    else if (command == "X")
                    {
                        _room.TimeRange = "Not Set";
                        _room.Status = "可用";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"發送指令失敗: {ex.Message}");
            }
        }

        private void LogToFile(string logMessage)
        {
            string logFilePath = "log.txt"; // 你可以根據需要更改文件路徑
            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine($"{DateTime.Now}: {logMessage}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"寫入日誌失敗: {ex.Message}");
            }
        }
    }
}
