using DevExpress.Xpf.Core;
using System.ComponentModel; // Closing 이벤트를 위해 추가
using System.Windows;
using WpfLogin.ViewModels;

namespace WpfLogin.Views
{
    public partial class MainWindow : DXWindow
    {
        // ViewModel을 클래스 필드로 유지하여 다른 메서드에서 접근할 수 있도록 합니다.
        private readonly MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainWindowViewModel();

            // ViewModel의 로그인 성공 이벤트를 구독합니다.
            _viewModel.LoginSucceeded += ViewModel_LoginSucceeded;

            this.DataContext = _viewModel;

            // ✨✨✨ [핵심 추가 1] 창이 닫힐 때 실행될 이벤트 핸들러를 등록합니다. ✨✨✨
            this.Closing += MainWindow_Closing;
        }

        // 로그인 성공 이벤트가 발생하면 이 메서드가 호출됩니다.
        private void ViewModel_LoginSucceeded(object sender, System.EventArgs e)
        {
            // MainView를 새로 만들어서 보여줍니다.
            MainView mainView = new MainView();
            mainView.Show();

            // 현재 로그인 창을 닫습니다. (이때 Closing 이벤트가 발생합니다)
            this.Close();
        }

        // ✨✨✨ [핵심 추가 2] 창이 닫히기 직전에 호출되는 메서드 ✨✨✨
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (_viewModel != null)
            {
                // ViewModel에 만들어둔 Cleanup 메서드를 호출하여 NetworkService 이벤트 구독을 취소합니다.
                _viewModel.Cleanup();

                // 이 창에서 직접 구독했던 이벤트도 안전하게 구독 취소합니다.
                _viewModel.LoginSucceeded -= ViewModel_LoginSucceeded;
            }
        }
    }
}