using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfLogin.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }

        private int _currentParticipants;
        public int CurrentParticipants
        {
            get => _currentParticipants;
            set { _currentParticipants = value; OnAllPropertiesChanged(); }
        }

        private int _maxParticipants = 10;
        public int MaxParticipants
        {
            get => _maxParticipants;
            set { _maxParticipants = value; OnAllPropertiesChanged(); }
        }

        private bool _isApplied;
        public bool IsApplied
        {
            get => _isApplied;
            set { _isApplied = value; OnAllPropertiesChanged(); }
        }

        public ObservableCollection<string> Participants { get; set; }

        public string StatusText => IsApplied ? "신청 취소" : (CurrentParticipants >= MaxParticipants ? "모집 마감" : "신청 가능");
        public string ParticipantStatusText => $"({CurrentParticipants}/{MaxParticipants}명)";
        public bool IsButtonEnabled => !(CurrentParticipants >= MaxParticipants && !IsApplied);

        public Schedule()
        {
            Participants = new ObservableCollection<string>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnAllPropertiesChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }
}
