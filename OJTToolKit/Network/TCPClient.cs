using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OzNet
{
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// <br/> TCP Client 통신 클래스
    /// <br/> 작 성 자 : 장봉석
    /// <br/> 작 성 일 : 2025년 05월 14일
    /// </summary>
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class TCPClient
    {
        private Socket  m_cClientSocket     = null;     // client 소켓
        private bool    m_isClientRunning   = false;    // client 동작 여부

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> 연동 해제 알림 이벤트
        /// <br/> 
        /// <br/> 파라미터 : 
        /// <br/>       [in] Socket        -  접속 종료 클라이언트 소켓        
        /// <br/> 
        /// <br/> 반 환 값 : -
        /// <br/> 
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 07월 03일
        /// </summary>        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public event Action Event_NotifyDisconnect;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> 생성자
        /// <br/> 반 환 값 : -
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 05월 14일
        /// </summary>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public TCPClient()
        {
            m_cClientSocket = null;
            m_isClientRunning = false;

            Event_NotifyDisconnect += Disconnect;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> TCP 서버 접속
        /// <br/>
        /// <br/> 파라미터 : 
        /// <br/>       [in] strIPAddress    - Server IP
        /// <br/>       [in] iPort           - TCP Port
        /// <br/>
        /// <br/> 반 환 값 : client 소켓
        /// <br/>
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 05월 14일
        /// </summary>        
        /// <param name="strIPAddress">     [in] Server IP    </param>
        /// <param name="iPort">            [in] TCP Port     </param>
        /// <returns>   client 소켓   </returns>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Socket Connect(string strIPAddress, int iPort)
        {
            try
            {
                m_cClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //서버 설정한 주소 포트번호 입력
                m_cClientSocket.Connect(IPAddress.Parse(strIPAddress), iPort);

                m_isClientRunning = true;

                return m_cClientSocket;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                return null;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> TCP 서버 접속 종료
        /// <br/>
        /// <br/> 파라미터 : -
        /// <br/>
        /// <br/> 반 환 값 : -
        /// <br/>
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 05월 14일
        /// </summary>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void Disconnect()
        {
            if (m_isClientRunning == true)
            {
                m_isClientRunning = false;

                m_cClientSocket.Close();
                m_cClientSocket.Dispose();

                m_cClientSocket = null;
            }
            
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> TCP 클라이언트 동작 여부
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
        public bool IsClientRun()
        {
            return m_isClientRunning;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> 패킷 송신
        /// <br/>
        /// <br/> 파라미터 :  
        /// <br/>       [in] bySendBuffer   - 송신 패킷 데이터
        /// <br/>       [in] iSize          - 송신 패킷 데이터 사이즈
        /// <br/>
        /// <br/> 반 환 값 : 송신완료 패킷 사이즈
        /// <br/>
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 05월 14일
        /// </summary>        
        /// <param name="bySendBuffer">     [in] 송신 패킷 데이터     </param>
        /// <param name="iSize">            [in] 송신 패킷 데이터 사이즈 </param>
        /// <returns>   송신완료 패킷 사이즈 </returns>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public int Send(byte[] bySendBuffer, int iSize)
        {
            int iSendSize = 0;

            if (m_isClientRunning == true && m_cClientSocket != null)
            {
                try
                {
                    iSendSize = m_cClientSocket.Send(bySendBuffer, iSize, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);

                    m_isClientRunning = false;

                    if (m_cClientSocket != null)
                    {
                        m_cClientSocket.Close();
                        m_cClientSocket = null;
                    }
                }
            }

            return iSendSize;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> 패킷 수신
        /// <br/>
        /// <br/> 파라미터 : 
        /// <br/>       [in] iRcvSize   - [in] 수신할 패킷 사이즈
        /// <br/>
        /// <br/> 반 환 값 : 수신 패킷 데이터, 오류시 null
        /// <br/>
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 05월 14일
        /// </summary>        
        /// <param name="iRcvSize"> [in] 수신할 패킷 사이즈   </param>
        /// <returns>   수신 패킷 데이터   </returns>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public byte[] Recieve(int iRcvSize)
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

                    int nRcvPartSize = m_cClientSocket.Receive(byRcvBuffer, iTotalSize - iSumSize, SocketFlags.None);

                    Buffer.BlockCopy(byRcvBuffer, 0, byTotalBuffer, iSumSize, nRcvPartSize);
                    iSumSize += nRcvPartSize;

                    // size == 0 이면 통신 끊어진것.
                    if (nRcvPartSize <= 0)
                    {
                        iRcvSize = 0;
                        byTotalBuffer = null;

                        System.Console.WriteLine("[TCP_CLIENT] Sever RECV 0 - DISCONNECT");

                        Event_NotifyDisconnect?.Invoke();
                        break;
                    }
                }

                //if (iRcvSize > 0)
                //    iRcvSize = iSumSize;

                // 0 이면 종료처리
                //if (iRcvSize <= 0)
                //{
                //    iRcvSize = 0;
                //    m_isClientRunning = false;
                //    byTotalBuffer = null;

                //    if (m_cClientSocket != null)
                //    {
                //        m_cClientSocket.Close();
                //        m_cClientSocket = null;
                //    }
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                System.Console.WriteLine("[TCP_CLIENT] Sever RECV 0 - DISCONNECT");

                Event_NotifyDisconnect?.Invoke();

                iRcvSize = 0;                
                byTotalBuffer = null;
                
            }

            return byTotalBuffer;
        }

       
    }
}
