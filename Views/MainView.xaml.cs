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

using Microsoft.Win32;
using WpfLogin.ViewModels;
using WpfLogin.Views;

namespace WpfLogin
{
    public partial class MainView : Window
    {
        //mainview창이 생성될 때 생성자 코드에서 mainviewmodel의 새 객체를 만들어 datacontext속성에 할당, mainviewxaml안의 모든 ui요소들이 mainviewmodel 의속성과 메서드를 참조할 수 있게됨
        public MainView()
        {
            InitializeComponent();
            this.DataContext = new ViewModels.MainViewModel(); // ViewModel 연결
            this.Closing += MainView_Closing;

        }


        private void ProfileImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "이미지 파일|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var vm = DataContext as ViewModels.MainViewModel;
                if (vm != null)
                {
                    vm.ProfileImagePath = openFileDialog.FileName;

                    // 경로 저장
                    Properties.Settings.Default.ProfileImagePath = vm.ProfileImagePath;
                    Properties.Settings.Default.Save();
                }
            }
        }
        private void MainView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // DataContext를 ViewModel 타입으로 가져옵니다.
            if (this.DataContext is MainViewModel viewModel)
            {
                // ViewModel에 만들어둔 Cleanup 메서드를 호출하여 이벤트 구독을 취소합니다.
                viewModel.Cleanup();
            }
        }



        private void BtnEditProfile_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel vm)
            {
                vm.IsEditing = true;
            }
        }

        private void BtnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel vm)
            {
                // 프로필 이미지 경로 저장
                Properties.Settings.Default.ProfileImagePath = vm.ProfileImagePath;
                Properties.Settings.Default.Save();

                // 편집 모드 종료 → 텍스트 표시로 전환
                vm.IsEditing = false;

                MessageBox.Show("프로필이 저장되었습니다.");
            }
        }

        private void BtnUploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "이미지 파일|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var vm = DataContext as ViewModels.MainViewModel;
                if (vm != null)
                {
                    vm.ProfileImagePath = openFileDialog.FileName;

                    // 경로 저장 (임시)
                    Properties.Settings.Default.ProfileImagePath = vm.ProfileImagePath;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // *** 핵심 수정: 로그아웃 패킷을 서버에 전송하는 로직 추가 ***
            var packet = new { opcode = 20 }; // Opcode 20: CS_LOGOUT
            // SendPacketAsync는 비동기 메서드이지만, 여기서는 응답을 기다릴 필요 없이
            // 일단 보내기만 하고 바로 창을 닫아도 되므로 await를 사용하지 않습니다.
            //var a = NetworkService.Instance.SendPacketAsync(packet);

            // 기존 로직: 새 로그인 창을 열고 현재 창을 닫음
            var loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}

