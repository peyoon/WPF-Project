using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OzNet
{
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// <br/> 심플 TCP Server 통신 클래스 - 1:1통신용
    /// <br/> 작 성 자 : 장봉석
    /// <br/> 작 성 일 : 2025년 05월 14일
    /// </summary>
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class TCPServer_Simple
    {
        private bool m_isServerStart        = true;     // server 가동 유무
        private bool m_isClientConnection   = false;    // client 접속 유무

        private TcpListener m_cTCPListen;               // listen 소켓
        private TcpClient   m_cTCPClient;               // client 소켓

        private string  m_strIPAddress = "";            // Server IP
        private int     m_iPortNum = 0;                 // TCP PORT


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> 클라이언트 접속 이벤트
        /// <br/> 
        /// <br/> 파라미터 : -
        /// <br/> 
        /// <br/> 반 환 값 : -
        /// <br/> 
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 05월 14일
        /// </summary>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public delegate void Handler_ConnectClient();
        public event Handler_ConnectClient Event_ConnectClient;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> 클라이언트 접속 종료 이벤트
        /// <br/> 
        /// <br/> 파라미터 : -
        /// <br/> 
        /// <br/> 반 환 값 : -
        /// <br/> 
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 05월 14일
        /// </summary>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public delegate void Handler_DisconnectClient();
        public event Handler_DisconnectClient Event_DisconnectClient;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> 생성자
        /// <br/> 
        /// <br/> 파라미터 : 
        /// <br/>       [in] strIPAddress    - Server IP
        /// <br/>       [in] iPortNum        - TCP Port
        /// <br/> 
        /// <br/> 반 환 값 : -
        /// <br/> 
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 05월 14일
        /// </summary>        
        /// <param name="strIPAddress"> [in] Server IP    </param>
        /// <param name="iPortNum">     [in] TCP Port     </param>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public TCPServer_Simple(string strIPAddress, int iPortNum)
        {
            m_isServerStart = false;
            m_isClientConnection = false;

            m_strIPAddress = strIPAddress;
            m_iPortNum = iPortNum;
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> TCP 서버 시작
        /// <br/> 
        /// <br/> 파라미터 : -
        /// <br/> 
        /// <br/> 반 환 값 : -
        /// <br/> 
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 05월 14일
        /// </summary>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void StartServer()
        {
            if (m_isServerStart == false)
                m_isServerStart = true;

            while (m_isServerStart == true)
            {
                IPAddress ipAddress = IPAddress.Parse(m_strIPAddress);

                if (m_cTCPListen != null)
                {
                    m_cTCPListen = null;
                }
                m_cTCPListen = new TcpListener(ipAddress, m_iPortNum);
                m_cTCPListen.Start();

                // Client 접속 대기
                m_cTCPClient = m_cTCPListen.AcceptTcpClient();

                m_isClientConnection = true;
                if (Event_ConnectClient != null)
                    Event_ConnectClient();

                // client 연결 체크
                while (m_isClientConnection == true)
                {
                    Thread.Sleep(100);
                }

                if (Event_DisconnectClient != null)
                    Event_DisconnectClient();
                

                // 소켓 종료
                if (m_cTCPClient != null)
                {
                    m_cTCPClient.Close();
                    m_cTCPClient.Dispose();
                    m_cTCPClient = null;
                }

                if (m_cTCPListen != null)
                {
                    m_cTCPListen.Stop();
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> TCP 서버 종료
        /// <br/> 
        /// <br/> 파라미터 : -
        /// <br/> 
        /// <br/> 반 환 값 : -
        /// <br/> 
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 05월 14일
        /// </summary>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void StopServer()
        {
            //클라이언트 연결 루프 종료
            m_isClientConnection = false;

            m_cTCPListen.Stop();
            m_cTCPListen.Server.Close();
            m_cTCPListen.Server.Dispose();

            if (m_cTCPClient != null)
            {
                m_cTCPClient.Client.Dispose();
                m_cTCPClient.Client.Close();
                m_cTCPClient.Close();
            }
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> TCP 서버 동작 여부
        /// <br/> 
        /// <br/> 파라미터 : -
        /// <br/> 
        /// <br/> 반 환 값 : t-동작, f-동작안함
        /// <br/> 
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 05월 14일
        /// </summary>
        /// <returns>   t-동작, f-동작안함    </returns>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public bool IsRunServer()
        {
            return m_isServerStart;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> 패킷 송신
        /// <br/>
        /// <br/> 파라미터 :  [in] bySendBuffer   - 송신 패킷 데이터
        /// <br/>             [in] iSize          - 송신 패킷 데이터 사이즈
        /// <br/>
        /// <br/> 반 환 값 : -
        /// <br/>
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 05월 14일
        /// </summary>        
        /// <param name="bySendBuffer"> [in] 송신 패킷 데이터     </param>
        /// <param name="iSize">        [in] 송신 패킷 데이터 사이즈 </param>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void Send(byte[] bySendBuffer, int iSize)
        {
            if (m_isClientConnection == true && m_cTCPClient.Client != null)
            {
                try
                {
                    m_cTCPClient.Client.Send(bySendBuffer, iSize, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    m_isClientConnection = false;
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> 패킷 수신
        /// <br/>
        /// <br/> 파라미터 : 
        /// <br/>       [in/out] iRcvSize   - 수신할 패킷 사이즈, 오류시 0
        /// <br/>
        /// <br/> 반 환 값 : 수신 패킷 데이터
        /// <br/>
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 05월 14일
        /// </summary>        
        /// <param name="iRcvSize">     [in/out] 수신할 패킷 사이즈, 오류시 0 </param>
        /// <returns>   수신 패킷 데이터   </returns>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public byte[] Recieve(ref int iRcvSize)
        {
            int iTotalSize = iRcvSize;
            int iSumSize = 0;

            byte[] byTotalBuffer = new byte[iTotalSize];

            try
            {
                // 수신할 패킷 사이즈를 전부 받을 때까지 반복해서 수신
                while (iSumSize < iRcvSize)
                {
                    byte[] byRcvBuffer = new byte[iTotalSize];

                    int iRcvPartSize = m_cTCPClient.Client.Receive(byRcvBuffer, iTotalSize - iSumSize, SocketFlags.None);

                    Buffer.BlockCopy(byRcvBuffer, 0, byTotalBuffer, iSumSize, iRcvPartSize);
                    iSumSize += iRcvPartSize;

                    // Client가 끊어진것을 체크 못하고 0만 수신 받고 있다..
                    // 0을 받았을 때 종료 처리
                    if (iRcvPartSize <= 0)
                    {
                        iRcvSize = 0;
                        System.Console.WriteLine("[TCP_SERVER] CLIENT RECV 0 - DISCONNECT");
                        break;
                    }
                }

                // 0 이면 종료처리
                if (iRcvSize <= 0)
                {
                    iRcvSize = 0;
                    m_isClientConnection = false;
                    byTotalBuffer = null;
                }
            }
            catch (Exception ex)
            {
                iRcvSize = 0;
                m_isClientConnection = false;
                byTotalBuffer = null;

                Console.WriteLine(ex);
            }

            return byTotalBuffer;
        }

        

    }
}
