using System;
using System.Collections.Generic;

namespace WpfOlzServer
{
    /// <summary>
    /// 풋살 일정 하나의 데이터를 나타내는 모델 클래스입니다.
    /// 이 객체는 JSON으로 변환되어 클라이언트와 통신하는 데 사용됩니다.
    /// </summary>
    public class Schedule
    {
        /// <summary>
        /// 일정의 고유 식별자(ID)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 일정 날짜 및 시간
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 장소
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// 해당 일정에 참가 신청한 사용자들의 ID 목록
        /// </summary>
        public List<string> Participants { get; set; } = new List<string>();
    }
}