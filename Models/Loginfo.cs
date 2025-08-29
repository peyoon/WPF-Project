using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WpfLogin.Models
{
    public class Loginfo
    {
        private string m_employnumber;
        public string employnumber
        {
            get
            {
                return m_employnumber;
            }
            set
            {
                m_employnumber = value;
            }
        }


        private string m_password;
        public string Password { get => m_password; set => m_password = value; }

        public Loginfo()
        {

        }

        public Loginfo(string employnum, string pw)
        {
            this.employnumber = employnum;
            this.Password = pw;
        }

        public static (bool, Loginfo) ToLoginfo(string message)
        {
            bool flag = true;
            Loginfo loginfo = new Loginfo();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            try
            {
                loginfo = JsonSerializer.Deserialize<Loginfo>(message, options);
            }
            catch (Exception ex)
            {
                flag = false;
            }
            return (flag, loginfo);
        }

        public string ToJson()
        {
            string json = System.Text.Json.JsonSerializer.Serialize(this);
            return json;
        }


        public override string ToString()
        {
            return $"{this.employnumber}:{this.Password}";
        }

    }
}
