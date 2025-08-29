using System.Text.Json;
using OJTToolKit.Utils;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using WpfLogin.Models;      // UserProfile 모델을 사용하기 위해 추가
using WpfLogin.Network;
using WpfOlzServer;

namespace WpfLogin.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region 속성 (Properties)
        // --- 마이페이지 프로필 관련 속성 ---
        private string _userName;
        private string _grade;
        private string _email;
        private string _joinDate;
        private bool _isEditing = false;
        private string _profileImagePath;

        public string ProfileImagePath { get => _profileImagePath; set { _profileImagePath = value; OnPropertyChanged(); } }
        public string UserName { get => _userName; set { _userName = value; OnPropertyChanged(); } }
        public string Grade { get => _grade; set { _grade = value; OnPropertyChanged(); } }
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
        public string JoinDate { get => _joinDate; set { _joinDate = value; OnPropertyChanged(); } }
        public bool IsEditing { get => _isEditing; set { _isEditing = value; OnPropertyChanged(); } }

        // --- 커맨드 ---
        public ICommand SaveProfileCommand { get; }
        #endregion

        #region 생성자 (Constructor)
        public MainViewModel()
        {
            // 1. (선택적) 로컬 캐시에서 이전 프로필 정보 불러오기
            // 앱이 시작될 때 UI가 비어있지 않도록 마지막으로 성공했던 정보를 보여주는 역할입니다.
            LoadUserProfileFromLocal();

            // 2. NetworkService로부터 로그인된 최신 사용자 정보 가져오기
            if (NetworkService.Instance != null)
            {
                // 로그인 시 받은 정보가 로컬 정보보다 우선됩니다.
                this.JoinDate = NetworkService.Instance.CurrentUserId;
                this.UserName = NetworkService.Instance.CurrentUserName ?? "사용자";

                // ✨✨✨ 핵심 수정 1: 서버의 프로필 업데이트 성공 이벤트 구독 ✨✨✨
                // NetworkService가 서버로부터 "성공" 응답을 받으면 OnProfileUpdateSuccess 메서드를 호출하도록 연결합니다.
                NetworkService.Instance.ProfileUpdateSuccessReceived += OnProfileUpdateSuccess;
                NetworkService.Instance.ProfileGetResponseReceived += OnProfileReceivedFromServer;

            }


            // 3. '프로필 저장' 커맨드를 초기화합니다.
            SaveProfileCommand = new RelayCommand(p => ExecuteSaveProfile());
        }
        #endregion

        #region 메소드 (Methods)
        public void Cleanup()
        {
            // 생성자에서 구독했던 모든 NetworkService 이벤트를 "구독 취소"합니다.
            if (NetworkService.Instance != null)
            {
                NetworkService.Instance.ProfileUpdateSuccessReceived -= OnProfileUpdateSuccess;
                NetworkService.Instance.ProfileGetResponseReceived -= OnProfileReceivedFromServer;
            }
        }
        // 로컬 설정(캐시)에서 프로필 정보를 불러오는 메서드
        private void LoadUserProfileFromLocal()
        {
            // 앱 실행 시 UI 초기값을 채우기 위한 부분입니다.
            ProfileImagePath = Properties.Settings.Default.ProfileImagePath;
            UserName = Properties.Settings.Default.UserName;
            Grade = Properties.Settings.Default.UserGrade;
            Email = Properties.Settings.Default.UserEmail;
            JoinDate = Properties.Settings.Default.UserJoinDate;
        }

        /// <summary>
        /// '저장' 버튼 클릭 시 실행되는 주 메서드
        /// </summary>
        private void ExecuteSaveProfile()
        {
            // 1. UI의 현재 데이터를 UserProfile 모델 객체에 담습니다.
            //    서버와 데이터 형식을 맞추기 위해 모델을 사용하는 것이 안정적입니다.
            var profile = new UserProfile
            {
                ProfileImagePath = this.ProfileImagePath,
                UserName = this.UserName,
                UserGrade = this.Grade,
                UserEmail = this.Email,
                UserJoinDate = this.JoinDate // 사번
            };

            // 2. ✨✨✨ 핵심 수정 2: 서버로 프로필 업데이트 정보 "요청"만 보냅니다. ✨✨✨
            SendProfileUpdateToServer(profile);

            // 3. UI를 '보기' 모드로 변경합니다.
            IsEditing = false;

            // ❌ [제거] 여기서 바로 성공 메시지를 띄우지 않습니다. 서버의 응답을 기다려야 합니다.
            // MessageBox.Show("...");
        }

        /// <summary>
        /// 서버로 프로필 업데이트를 요청하는 네트워크 전송 메서드
        /// </summary>
        private void SendProfileUpdateToServer(UserProfile profile)
        {
            if (NetworkService.Instance == null || !NetworkService.Instance.IsConnected)
            {
                MessageBox.Show("서버에 연결되어 있지 않아 프로필을 전송할 수 없습니다.");
                return;
            }

            // UserProfile 객체를 직접 JSON 문자열로 변환 (직렬화)
            string jsonBody = profile.ToJson();
            byte[] bodyBytes = Encoding.UTF8.GetBytes(jsonBody);

            // 약속된 Opcode(CS_PROFILE_UPDATE_REQ)를 사용하여 헤더 생성
            byte[] headBytes = HeadModel.HeadMessage(E_OPCode.CS_PROFILE_UPDATE_REQ, E_HeadType.JSON, bodyBytes.Length);
            byte[] fullPacket = headBytes.ToAddbytearrEx(bodyBytes);

            // NetworkService를 통해 서버로 패킷 전송
            NetworkService.Instance.Send(fullPacket);
        }

        /// <summary>
        /// ✨✨✨ 핵심 수정 3: 서버로부터 성공 응답을 받았을 때 실행될 이벤트 핸들러 ✨✨✨
        /// </summary>
        private void OnProfileUpdateSuccess()
        {
            // 이 메서드는 서버가 "수정 완료(SC_PROFILE_UPDATE_RES)" 신호를 보냈을 때만 호출됩니다.
            MessageBox.Show("프로필이 저장되었습니다.", "저장 완료", MessageBoxButton.OK, MessageBoxImage.Information);

            // 성공적으로 서버에 저장되었으니, 다음 실행을 위해 로컬 캐시(설정)에도 저장해줍니다.
            Properties.Settings.Default.ProfileImagePath = this.ProfileImagePath;
            Properties.Settings.Default.UserName = this.UserName;
            Properties.Settings.Default.UserGrade = this.Grade;
            Properties.Settings.Default.UserEmail = this.Email;
            Properties.Settings.Default.UserJoinDate = this.JoinDate;
            Properties.Settings.Default.Save();
        }
        private void OnProfileReceivedFromServer(UserProfile profile)
        {
            // 서버에서 받은 데이터로 UI와 바인딩된 속성들을 모두 업데이트
            ProfileImagePath = profile.ProfileImagePath;
            UserName = profile.UserName;
            Grade = profile.UserGrade;
            Email = profile.UserEmail;
            JoinDate = profile.UserJoinDate;

            // ✨ 중요: 이제 이 최신 정보가 진실이므로, 클라이언트 로컬 캐시에도 저장합니다.
            Properties.Settings.Default.ProfileImagePath = this.ProfileImagePath;
            Properties.Settings.Default.UserName = this.UserName;
            // ... (모든 속성 저장) ...
            Properties.Settings.Default.Save();
        }

        #endregion

        #region INotifyPropertyChanged 구현부
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}