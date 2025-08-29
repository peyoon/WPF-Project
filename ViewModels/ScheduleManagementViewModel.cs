using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace WpfOlzServer
{
    public class ScheduleManagementViewModel
    {
        private readonly ScheduleManager _scheduleManager;
        public ObservableCollection<Schedule> Schedules { get; set; }
        public ICommand AddScheduleCommand { get; }
        public ICommand DeleteScheduleCommand { get; }

        public ScheduleManagementViewModel(ScheduleManager scheduleManager)
        {
            _scheduleManager = scheduleManager;
            Schedules = new ObservableCollection<Schedule>(_scheduleManager.GetAllSchedules());

            AddScheduleCommand = new RelayCommand(p => AddSchedule());
            DeleteScheduleCommand = new RelayCommand(DeleteSchedule, p => p is Schedule);
        }

        private void AddSchedule()
        {
            var newSchedule = new Schedule
            {
                Id = (_scheduleManager.GetAllSchedules().Any() ? _scheduleManager.GetAllSchedules().Max(s => s.Id) : 100) + 1,
                Date = System.DateTime.Now.Date.AddDays(7).AddHours(19),
                Location = "새로운 장소"
            };

            _scheduleManager.AddSchedule(newSchedule);
            Schedules.Add(newSchedule);
        }

        private void DeleteSchedule(object scheduleObject)
        {
            if (scheduleObject is Schedule scheduleToDelete)
            {
                _scheduleManager.DeleteSchedule(scheduleToDelete.Id);
                Schedules.Remove(scheduleToDelete);
            }
        }
    }

    // RelayCommand 클래스가 없다면 ViewModel 파일 하단이나 별도 파일에 추가
    public class RelayCommand : ICommand
    {
        private readonly System.Action<object> _execute;
        private readonly System.Predicate<object> _canExecute;
        public event System.EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public RelayCommand(System.Action<object> execute, System.Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new System.ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
    }
}