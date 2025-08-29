// WpfLogin/Helpers/CsvReader.cs
using System.Collections.Generic;
using System.IO;
using System.Text; // Encoding.UTF8을 위해 추가
using WpfLogin.Models;

namespace WpfLogin.Helpers
{
    public static class CsvReader
    {
        public static List<OrganizationNode> ReadOrganizationData(string filePath)
        {
            var list = new List<OrganizationNode>();

            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            {
                reader.ReadLine(); // 헤더 라인(첫 줄)은 건너뛰기

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    // [수정 1] 빈 줄이 있으면 건너뛰도록 방어 코드 추가
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    var values = line.Split(',');

                    // [수정 2] 각 줄에 4개의 값이 있는지 확인
                    if (values.Length < 4)
                    {
                        continue;
                    }

                    // [수정 3] int.Parse 대신 int.TryParse를 사용하여 오류 방지
                    bool idParsed = int.TryParse(values[0], out int id);
                    bool parentIdParsed = int.TryParse(values[1], out int parentId);

                    // Id와 ParentId가 정상적인 숫자로 변환될 때만 리스트에 추가
                    if (idParsed && parentIdParsed)
                    {
                        list.Add(new OrganizationNode
                        {
                            Id = id,
                            ParentId = parentId,
                            Name = values[2],
                            Position = values[3]
                        });
                    }
                }
            }
            return list;
        }
    }
}