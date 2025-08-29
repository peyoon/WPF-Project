using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace WpfOlzServer
{
 
    public class UserProfile
    {
        public string ProfileImagePath { get; set; }
        public string UserName { get; set; }
        public string UserGrade { get; set; }
        public string UserEmail { get; set; }
        public string UserJoinDate { get; set; }


        public string ToJson()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            string jsonstring = JsonSerializer.Serialize(this,options);
            return jsonstring;
        }
    }
}