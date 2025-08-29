using System.Collections.ObjectModel;
using System.Text;

namespace WpfOlzServer
{
    public class ServerInstance
    {
        public string Name { get; set; }
        public ServerCtrl Control { get; set; }
        public StringBuilder Log { get; } = new StringBuilder();
        public ObservableCollection<ClientInfo> ConnectedClients { get; } = new ObservableCollection<ClientInfo>();
        //public ObservableCollection<Vote> Votes { get; } = new ObservableCollection<Vote>();

        //public VoteManagementViewModel VoteViewModel { get; }
        public ScheduleManagementViewModel ScheduleViewModel { get; }

        public ServerInstance(ScheduleManager scheduleManager)
        {
            // 각 탭의 ViewModel을 생성하고, 필요한 데이터를 전달합니다.
           // VoteViewModel = new VoteManagementViewModel(this.Votes);
            ScheduleViewModel = new ScheduleManagementViewModel(scheduleManager);
        }
    }
}