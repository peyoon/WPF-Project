using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace WpfOlzServer
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<ServerInstance> _serverInstances = new ObservableCollection<ServerInstance>();
        private int _serverCounter = 1;

        public MainWindow()
        {
            InitializeComponent();
            ServerListBox.ItemsSource = _serverInstances;
        }

        private void Btn_AddServer_Click(object sender, RoutedEventArgs e)
        {
            var serverControl = new ServerCtrl();

            // *** 수정된 부분: ServerInstance 생성 방식 변경 ***
            // 1. 각 서버가 사용할 ScheduleManager를 먼저 생성합니다.
            var scheduleManager = new ScheduleManager();

            // 2. 생성자에 scheduleManager를 전달하여 ServerInstance를 만듭니다.
            var newServer = new ServerInstance(scheduleManager)
            {
                Name = $"Futsal Server {_serverCounter++}",
                Control = serverControl
            };
            // *** 수정 끝 ***

            serverControl.OnLogMessage += (logMessage) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    newServer.Log.AppendLine(logMessage);
                    if (ServerListBox.SelectedItem == newServer)
                    {
                        LogTextBox.Text = newServer.Log.ToString();
                        LogTextBox.ScrollToEnd();
                    }
                });
            };

            _serverInstances.Add(newServer);

            ServerListBox.SelectedItem = newServer;
        }

        private void ServerListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedServer = ServerListBox.SelectedItem as ServerInstance;

            if (selectedServer != null)
            {
                ServerControlContent.Content = selectedServer.Control;
                LogTextBox.Text = selectedServer.Log.ToString();
                LogTextBox.ScrollToEnd();
            }
            else
            {
                ServerControlContent.Content = null;
                LogTextBox.Text = string.Empty;
            }
        }

        private void Btn_ClearLog_Click(object sender, RoutedEventArgs e)
        {
            var selectedServer = ServerListBox.SelectedItem as ServerInstance;

            if (selectedServer != null)
            {
                selectedServer.Log.Clear();

                LogTextBox.Text = string.Empty;
            }
        }
    }
}