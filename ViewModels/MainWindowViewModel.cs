using System.Text.Json;
using OJTToolKit.Utils;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfLogin.Network;
using WpfLogin.ViewModels;

namespace WpfLogin
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event EventHandler LoginSucceeded;

        // --- 제거 ---
        // private OzTcpClient m_client; // NetworkService가 관리하므로 제거
        // private CancellationTokenSource TaskClinetCts; // NetworkService가 관리하므로 제거

        #region 속성 (Properties)
        public string EmployeeId { get; set; } = "10194";
        public string Password { get; set; } = "1234";
        public string IpAddress { get; set; } = "127.0.0.1";
        public string Port { get; set; } = "9999";

        private string _statusText = "오프라인";
        public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

        // ✨✨✨ 핵심 수정: IsConnected 상태를 NetworkService에서 직접 가져옴 ✨✨✨
        public bool IsConnected => NetworkService.Instance.IsConnected;
        public bool IsDisconnected => !NetworkService.Instance.IsConnected;

        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand LoginCommand { get; }
        #endregion

        public MainWindowViewModel()
        {
            // ✨✨✨ 핵심 수정: NetworkService의 이벤트를 구독하여 UI 상태를 업데이트 ✨✨✨
            NetworkService.Instance.LoginSuccessResponseReceived += OnLoginSuccess;
            NetworkService.Instance.LoginFailResponseReceived += OnLoginFail;
            NetworkService.Instance.Disconnected += OnDisconnected;

            ConnectCommand = new RelayCommand(async _ => await ConnectToServerAsync(), _ => IsDisconnected);
            DisconnectCommand = new RelayCommand(_ => DisconnectFromServer(), _ => IsConnected);
            LoginCommand = new RelayCommand(_ => ExecuteLogin(), _ => IsConnected);
        }

        #region 네트워크 이벤트 핸들러 (Network Event Handlers)
        // NetworkService로부터 로그인 성공 이벤트를 받았을 때 실행
        // WpfLogin/MainWindowViewModel.cs -> OnLoginSuccess 메서드 수정

        public void Cleanup()
        {
            // 생성자에서 구독했던 모든 이벤트를 "구독 취소"합니다.
            NetworkService.Instance.LoginSuccessResponseReceived -= OnLoginSuccess;
            NetworkService.Instance.LoginFailResponseReceived -= OnLoginFail;
            NetworkService.Instance.Disconnected -= OnDisconnected;
        }
        private void OnLoginSuccess(dynamic loginData)
        {
            // ✨✨✨ [추가] 로그인 성공 후, 서버에 내 프로필 정보를 요청하는 패킷을 보냅니다. ✨✨✨
            byte[] headBytes = HeadModel.HeadMessage(E_OPCode.CS_PROFILE_GET_REQ, E_HeadType.Raw, 0);
            NetworkService.Instance.Send(headBytes);

            // 기존 로직: View에 로그인 성공 알림
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }

        // NetworkService로부터 로그인 실패 이벤트를 받았을 때 실행
        private void OnLoginFail(string reason)
        {
            MessageBox.Show($"로그인 실패: {reason}");
        }

        // NetworkService로부터 연결 끊김 이벤트를 받았을 때 실행
        private void OnDisconnected()
        {
            // UI 스레드에서 안전하게 UI 업데이트
            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusText = "서버와 연결이 끊어졌습니다.";
                // IsConnected, IsDisconnected 프로퍼티 변경을 UI에 알림
                OnPropertyChanged(nameof(IsConnected));
                OnPropertyChanged(nameof(IsDisconnected));
            });
        }
        #endregion

        #region 커맨드 실행 메서드 (Command Execution Methods)
        private async Task ConnectToServerAsync()
        {
            if (!int.TryParse(Port, out int portNum)) { StatusText = "잘못된 포트 번호입니다."; return; }
            StatusText = "서버 연결 중...";

            // ✨✨✨ 핵심 수정: 모든 연결 로직을 NetworkService에 위임 ✨✨✨
            bool success = await NetworkService.Instance.ConnectAsync(IpAddress, portNum);

            if (success)
            {
                StatusText = "연결 성공";
            }
            else
            {
                StatusText = "연결 실패";
            }
            // 프로퍼티 변경을 UI에 알려 버튼 활성화/비활성화 상태 등을 갱신
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(IsDisconnected));
        }

        private void DisconnectFromServer()
        {
            // ✨✨✨ 핵심 수정: 연결 끊기 로직을 NetworkService에 위임 ✨✨✨
            NetworkService.Instance.Disconnect();
        }

        private void ExecuteLogin()
        {
            // 이 메서드는 이제 데이터를 만들어 NetworkService에 보내기만 하면 됨
            var loginData = new { emp_no = this.EmployeeId, Password = this.Password };
            string jsonBody = JsonSerializer.Serialize(loginData);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(jsonBody);
            byte[] headBytes = HeadModel.HeadMessage(E_OPCode.UserCheck, E_HeadType.JSON, bodyBytes.Length);
            byte[] fullPacket = headBytes.ToAddbytearrEx(bodyBytes);

            NetworkService.Instance.Send(fullPacket);
        }
        #endregion

        // --- 제거 ---
        // 자체적으로 처리하던 Receive, HandlePacket, M_client_Event_NotifyDisconnect 메서드는
        // 모두 NetworkService에서 중앙 관리하므로 ViewModel에서 삭제합니다.

        #region INotifyPropertyChanged 구현부
        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false; field = value; OnPropertyChanged(propertyName); return true;
        }
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}