using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
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
using System.Runtime.InteropServices;
using System.Globalization;

namespace KTVApp
{
    public class StringNullOrEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrEmpty(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<Room> _rooms;
        private TcpServer _tcpServer;

        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();

        public MainWindow()
        {
            InitializeComponent();
            //AllocConsole(); // 分配一个控制台窗口
            Rooms = new ObservableCollection<Room>();
            LoadRoomsFromFile("room.txt");
            DataContext = this;

            // 启动 TCP 服务器
            _tcpServer = new TcpServer(1000);
            _tcpServer.MessageReceived += OnMessageReceived; // 订阅事件
            _tcpServer.Start();
        }

        public ObservableCollection<Room> Rooms
        {
            get => _rooms;
            set
            {
                _rooms = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var rect = sender as Rectangle;
            var room = rect.DataContext as Room;

            // Open a new window and pass the room information
            var roomDetailsWindow = new RoomDetailsWindow(room);
            roomDetailsWindow.Show();
        }

        private void LoadRoomsFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"File not found: {filePath}");
                return;
            }

            try
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    var parts = line.Split(';');
                    if (parts.Length == 2)
                    {
                        var floor = parts[0];
                        var roomNumbers = parts[1].Split(',');

                        foreach (var roomNumber in roomNumbers)
                        {
                            // 获取roomNumber的末三码
                            var lastThreeDigits = roomNumber.Length > 3 ? roomNumber.Substring(roomNumber.Length - 3) : roomNumber;
                            var newRoomNumber = "0" + lastThreeDigits;

                            Rooms.Add(new Room
                            {
                                RoomNumber = newRoomNumber,
                                RoomPC = roomNumber,
                                Status = "可用", // 默认状态，可以根据需要修改
                                TimeRange = "Not Set", // 默认时间范围，可以根据需要修改
                                ServiceStatus = "NonService" // 默认服务状态
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading file: {ex.Message}");
            }
        }

        private void OnMessageReceived(string message)
        {
            // 解析消息
            var parts = message.Split(',');
            if (parts.Length >= 2)
            {
                var roomNumber = parts[0].PadLeft(4, '0'); // 确保房间号是4位数
                var command = parts[1];

                // 查找对应的房间并更新服务状态
                var room = Rooms.FirstOrDefault(r => r.RoomNumber == roomNumber);
                if (room != null)
                {
                    if (command == "S")
                    {
                        room.ServiceStatus = "Service";
                    }
                    else if (command == "N")
                    {
                        room.ServiceStatus = "NonService";
                    }
                    else if (command == "O")
                    {
                        // 當收到"O"指令時，將TimeRange更新為當下時間的 "yyyy-MM-dd HH:mm-" 格式
                        room.TimeRange = DateTime.Now.ToString("MM-dd HH:mm") + "-";
                        // 將Status更新為"Occupied"
                        room.Status = "已占用";
                    }
                    else if (command == "X")
                    {
                        room.TimeRange = "Not Set";
                        room.Status = "可用";
                    }
                    else if (parts.Length >= 3 && command == "J")
                    {
                        // 當收到類似"J"指令時，解析時間並更新TimeRange
                        var dateTimeString = parts[2].Replace('．', '.');
                        DateTime parsedDateTime;
                        if (DateTime.TryParse(dateTimeString, out parsedDateTime))
                        {
                            var timePart = parsedDateTime.ToString("MM-dd HH:mm");
                            room.TimeRange += timePart;
                            room.Status = "已占用";
                        }
                    }
                }
            }
        }
    }

    public class Room : INotifyPropertyChanged
    {
        private string _status;
        private string _serviceStatus;
        private string _timeRange;

        private string _roomNumber;
        public string RoomPC { get; set; }

        public string RoomNumber
        {
            get => _roomNumber;
            set
            {
                if (_roomNumber != value)
                {
                    _roomNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TimeRange
        {
            get => _timeRange;
            set
            {
                if (_timeRange != value)
                {
                    _timeRange = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ServiceStatus
        {
            get => _serviceStatus;
            set
            {
                if (_serviceStatus != value)
                {
                    _serviceStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
