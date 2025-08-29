using OzNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WpfLogin.Network
{
    public class OzTcpClient : TCPClient
    {
        private bool m_IsClientRunning = false;    // client 동작 여부


        public bool IsConnectState
        {
            get
            {
                return m_IsClientRunning;
            }
            set
            {
                m_IsClientRunning=value;
            }
        }

        public OzTcpClient()
        {

        }
    }
}
