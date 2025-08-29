using System.Net.Sockets;

namespace WpfOlzServer
{
    public class ClientInfo
    {
        public Socket TcpClient { get; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public System.DateTime ConnectTime { get; set; } = System.DateTime.Now;
        public string IpAddress => TcpClient.RemoteEndPoint?.ToString() ?? "N/A";

        public ClientInfo(Socket client)
        {
            TcpClient = client;
        }
    }
}