using System;
using System.Collections.Generic;
using System.Linq;

namespace WpfOlzServer
{
    /// <summary>
    /// 서버의 모든 일정 데이터를 관리하고 관련 비즈니스 로직을 처리하는 클래스입니다.
    /// </summary>
    public class ScheduleManager
    {
        private readonly List<Schedule> _schedules = new List<Schedule>();
        private readonly object _lock = new object();

        public ScheduleManager()
        {
            // 테스트용 초기 데이터
            _schedules.Add(new Schedule
            {
                Id = 101,
                Date = new DateTime(2025, 8, 28, 19, 0, 0),
                Location = "대전 월드컵 보조경기장 A",
                Participants = new List<string> { "10195" }
            });
            _schedules.Add(new Schedule
            {
                Id = 102,
                Date = new DateTime(2025, 9, 4, 20, 0, 0),
                Location = "한밭 종합운동장 풋살장",
                Participants = new List<string>()
            });
        }

        public List<Schedule> GetAllSchedules()
        {
            lock (_lock)
            {
                return new List<Schedule>(_schedules);
            }
        }

        public List<Schedule> GetMySchedules(string userId)
        {
            lock (_lock)
            {
                return _schedules.Where(s => s.Participants.Contains(userId)).ToList();
            }
        }

        public bool ApplyForSchedule(int scheduleId, string userId)
        {
            lock (_lock)
            {
                var schedule = _schedules.FirstOrDefault(s => s.Id == scheduleId);
                if (schedule != null && !schedule.Participants.Contains(userId))
                {
                    schedule.Participants.Add(userId);
                    return true;
                }
                return false;
            }
        }

        public bool CancelApplication(int scheduleId, string userId)
        {
            lock (_lock)
            {
                var schedule = _schedules.FirstOrDefault(s => s.Id == scheduleId);
                if (schedule != null && schedule.Participants.Contains(userId))
                {
                    schedule.Participants.Remove(userId);
                    return true;
                }
                return false;
            }
        }

        public List<string> GetParticipants(int scheduleId)
        {
            lock (_lock)
            {
                var schedule = _schedules.FirstOrDefault(s => s.Id == scheduleId);
                return schedule?.Participants;
            }
        }

        // --- ▼▼▼ 이 두 메서드가 추가되었습니다 ▼▼▼ ---

        /// <summary>
        /// 새 일정을 목록에 추가합니다.
        /// </summary>
        public void AddSchedule(Schedule newSchedule)
        {
            lock (_lock)
            {
                _schedules.Add(newSchedule);
            }
        }

        /// <summary>
        /// ID를 기반으로 일정을 삭제합니다.
        /// </summary>
        public bool DeleteSchedule(int scheduleId)
        {
            lock (_lock)
            {
                var scheduleToRemove = _schedules.FirstOrDefault(s => s.Id == scheduleId);
                if (scheduleToRemove != null)
                {
                    return _schedules.Remove(scheduleToRemove);
                }
                return false;
            }
        }
    }
}