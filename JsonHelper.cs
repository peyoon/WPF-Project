// WpfOlzServer/Helpers/JsonHelper.cs

using System.Text.Json;
using System;
using System.IO;
using WpfOlzServer.Models;

// using Newtonsoft.Json; // 더 이상 필요 없으므로 이 줄은 삭제합니다.
// using System.Xml; // 사용하지 않으므로 삭제합니다.

namespace WpfOlzServer.Helpers
{
    public static class JsonHelper
    {
        // 프로필을 저장할 기본 폴더 경로
        private static readonly string BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserProfiles");

        // JsonSerializer에 사용할 공통 옵션을 미리 정의해두면 편리합니다.
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = true, // JSON을 쓸 때 예쁘게 들여쓰기
            PropertyNameCaseInsensitive = true // JSON을 읽을 때 대/소문자를 구분하지 않음
        };

        // UserProfile 데이터를 파일에 저장하는 메서드
        public static void SaveUserProfile(UserProfile profile)
        {
            // UserProfiles 폴더가 없으면 생성
            Directory.CreateDirectory(BasePath);

            // 파일 경로를 "사번.json" 으로 지정
            string filePath = Path.Combine(BasePath, $"{profile.UserJoinDate}.json");
            // ✨ [수정] JsonSerializer.Serialize에 옵션을 전달하여 들여쓰기를 적용합니다.
            string json = profile.ToJson();
            File.WriteAllText(filePath, json);
        }

        // 특정 사번의 프로필을 불러오는 메서드 (필요 시 사용)
        public static UserProfile LoadUserProfile(string employeeId)
        {
            string filePath = Path.Combine(BasePath, $"{employeeId}.json");

            if (!File.Exists(filePath))
            {
                return null; // 해당 사번의 프로필이 없으면 null 반환
            }

            string json = File.ReadAllText(filePath);

            // ✨ [수정] JsonConvert.DeserializeObject를 JsonSerializer.Deserialize로 변경합니다.
            return JsonSerializer.Deserialize<UserProfile>(json, _options);
        }
    }
}