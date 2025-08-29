using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// <br/> TCP Server 통신 클래스 - 1:1통신용
    /// <br/> 작 성 자 : 장봉석
    /// <br/> 작 성 일 : 2025년 07월 03일
    /// </summary>
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class TCPServer
    {
        private int             m_nPort = 0;                    // TCP PORT

        private bool            m_isServerStart = true;         // server 가동 유무        
        private TcpListener     m_cTCPListen = null;            // 클라이언트 연결 요청 리스너        
        private Thread          m_threadSelect = null;          // Select 스레드

        private List<Socket>    m_listClient = new List<Socket>(); // 접속 클라이언트들



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> 신규 클라이언트 접속 알림 이벤트
        /// <br/> 
        /// <br/> 파라미터 : 
        /// <br/>       [in] Socket        -  신규 접속 클라이언트 소켓        
        /// <br/> 
        /// <br/> 반 환 값 : -
        /// <br/> 
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 07월 03일
        /// </summary>        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public event Action<Socket> Event_NotifyConnectClient;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> 클라이언트 접속 종료 알림 이벤트
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
        public event Action<Socket> Event_NotifyDisconnectClient;


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> 클라이언트 패킷 수신 알림 이벤트
        /// <br/> 
        /// <br/> 파라미터 : 
        /// <br/>       [in] Socket        -  해당 클라이언트 소켓        
        /// <br/> 
        /// <br/> 반 환 값 : -
        /// <br/> 
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 07월 03일
        /// </summary>        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public event Action<Socket> Event_NotifyReceived;


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> 생성자
        /// <br/> 
        /// <br/> 파라미터 : 
        /// <br/>       [in] nPort        - TCP Port
        /// <br/> 
        /// <br/> 반 환 값 : -
        /// <br/> 
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 07월 03일
        /// </summary>        
        /// <param name="strIPAddress"> [in] Server IP    </param>
        /// <param name="nPort">     [in] TCP Port     </param>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public TCPServer(int nPort)
        {
            m_isServerStart = false;
            m_nPort = nPort;

            // 클라이언트 접속 해제 처리
            this.Event_NotifyDisconnectClient += DisconnectClient;
        }

        public TCPServer()
        {
            m_isServerStart = false;
            // 클라이언트 접속 해제 처리
            this.Event_NotifyDisconnectClient += DisconnectClient;
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> TCP 서버 시작
        /// <br/> 
        /// <br/> 파라미터 : -
        /// <br/> 
        /// <br/> 반 환 값 : -
        /// <br/> 
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 07월 03일
        /// </summary>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void StartServer(IPAddress IP,int Port)
        {
            if (m_isServerStart == false)
            {
                m_nPort = Port;
                m_isServerStart = true;
                // Host 컴퓨터 전역 IP
                IPAddress ipAddress = IPAddress.Any;

                // listener 생성
                m_cTCPListen = new TcpListener(ipAddress, m_nPort);
                m_cTCPListen.Start();

                // Select 스레드 생성
                m_threadSelect = new Thread(ProcThread_Select);
                m_threadSelect.IsBackground = true;
                m_threadSelect.Start();

            }
            else
            {
                Console.WriteLine("[TCP_SERVER] 서버가 동작중입니다.");
            }


        }

        public void StartServer(int nport)
        {
            if (m_isServerStart == false)
            {
                m_isServerStart = true;

                // Host 컴퓨터 전역 IP
                IPAddress ipAddress = IPAddress.Any;

                m_nPort=nport;
                // listener 생성
                m_cTCPListen = new TcpListener(ipAddress, m_nPort);
                m_cTCPListen.Start();

                // Select 스레드 생성
                m_threadSelect = new Thread(ProcThread_Select);
                m_threadSelect.IsBackground = true;
                m_threadSelect.Start();

            }
            else
            {
                Console.WriteLine("[TCP_SERVER] 서버가 동작중입니다.");
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
        /// <br/> 작 성 일 : 2025년 07월 03일
        /// </summary>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void StopServer()
        {
            // 서버 종료 플래그
            m_isServerStart = false;

            // 리스너 종료 및 소켓 해제
            m_cTCPListen.Stop();
            m_cTCPListen.Server.Close();
            m_cTCPListen.Server.Dispose();

            // 클라이언트 소켓 해제 및 초기화
            // 접속 종료 알렸을 때 삭제되어 오류가 발생한다. 역방향으로 삭제하여 오류를 없앤다.
            for (int count = m_listClient.Count - 1; count >= 0; count--)
            {
                // 접속 종료 알림
                Event_NotifyDisconnectClient?.Invoke(m_listClient[count]);
            }

            // Select 스레드 종료 확인 // 최대 1초 대기
            m_threadSelect.Join(1000);

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
        /// <br/> 작 성 일 : 2025년 07월 03일
        /// </summary>
        /// <returns>   t-동작, f-동작안함    </returns>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public bool IsRunServer()
        {
            return m_isServerStart;
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> 클라이언트 연결 해제
        /// <br/> 
        /// <br/> 파라미터 : 
        /// <br/>       [in] socketClient        -  연결해제 클라이언트 소켓        
        /// <br/> 
        /// <br/> 반 환 값 : -
        /// <br/> 
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 07월 03일
        /// </summary>                
        /// <param name="socketClient"> [in] 연결해제 클라이언트 소켓       </param>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void DisconnectClient(Socket socketClient)
        {
            if (socketClient != null)
            {
                // 소켓 해제
                socketClient.Dispose();
                socketClient.Close();

                // 리스트 삭제
                m_listClient.Remove(socketClient);
            }

        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> 패킷 송신
        /// <br/> 
        /// <br/> 파라미터 : 
        /// <br/>       [in] socketClient       -  클라이언트 소켓        
        /// <br/>       [in] byPacket           -  송신할 패킷 byte 배열
        /// <br/> 
        /// <br/> 반 환 값 : -
        /// <br/> 
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 07월 03일
        /// </summary>  
        /// <param name="socketClient"> [in] 클라이언트 소켓         </param>
        /// <param name="byPacket">     [in] 송신할 패킷 byte 배열   </param>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void Send(Socket socketClient, byte[] byPacket)
        {
            try
            {
                socketClient.Send(byPacket);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                // 접속 종료 알림
                Event_NotifyDisconnectClient?.Invoke(socketClient);
            }
            
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> 패킷 수신
        /// <br/> - 입력된 패킷 사이즈 전부 수신되면 반환
        /// <br/> 
        /// <br/> 파라미터 : 
        /// <br/>       [in] socketClient  -  수신 클라이언트 소켓
        /// <br/>       [in] nRecvSize     -  수신 패킷 사이즈
        /// <br/> 
        /// <br/> 반 환 값 : 수신 패킷 byte 배열, null - 접속 종료
        /// <br/> 
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 07월 03일
        /// </summary>        
        /// <param name="socketClient">       [in] 수신 클라이언트 소켓   </param>
        /// <param name="nRecvSize">    [in] 수신 패킷 사이즈         </param>
        /// <returns>   수신 패킷 byte 배열, null - 접속 종료   </returns>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public byte[] Recieve(Socket socketClient, int nRecvSize)
        {
            int nTotalSize = nRecvSize;
            byte[] byTotalPacket = new byte[nTotalSize];

            int nSumSize = 0;

            try
            {
                // 입력 사이즈까지 패킷 수신
                while (nSumSize < nTotalSize)
                {
                    byte[] byRcvBuffer = new byte[nTotalSize];

                    // 버퍼 수신
                    int nRcvPartSize = socketClient.Receive(byRcvBuffer, nTotalSize - nSumSize, SocketFlags.None);

                    // 수신된 버퍼 합치기
                    Buffer.BlockCopy(byRcvBuffer, 0, byTotalPacket, nSumSize, nRcvPartSize);
                    nSumSize += nRcvPartSize;

                    // 0이 리턴되면 연결 종료된 것.
                    if (nRcvPartSize == 0)
                    {
                        nTotalSize = 0;
                        byTotalPacket = null;
                        Console.WriteLine($"[TCP_SERVER] Disconnect Client = {socketClient.RemoteEndPoint}");

                        // 접속 종료 알림
                        Event_NotifyDisconnectClient?.Invoke(socketClient);

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine($"[TCP_SERVER] Disconnect Client = {socketClient.RemoteEndPoint}");

                // 접속 종료 알림
                Event_NotifyDisconnectClient?.Invoke(socketClient);
            }

            return byTotalPacket;
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <br/> Select 스레드
        /// <br/> - 클라이언트 접속, 패킷 수신 확인 및 알림
        /// <br/> 
        /// <br/> 파라미터 : -
        /// <br/> 
        /// <br/> 반 환 값 : -
        /// <br/> 
        /// <br/> 작 성 자 : 장봉석
        /// <br/> 작 성 일 : 2025년 07월 03일
        /// </summary>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void ProcThread_Select()
        {
            // Select에 사용할 소켓 리스트
            List<Socket> listCheckRead = new List<Socket>();

            while (m_isServerStart == true)
            {
                // Select에 사용할 소켓 리스트 설정
                listCheckRead.Clear();
                listCheckRead.Add(m_cTCPListen.Server);             // 수신 대기 소켓
                listCheckRead.AddRange(m_listClient);               // 연결된 클라이언트 소켓들

                // Select 호출 (읽기 가능 여부 검사)
                Socket.Select(listCheckRead, null, null, 1000 * 1000); // 타임아웃 마이크로초 (1초)

                try
                {
                    // 리드 체크
                    // socket 별 병렬처리를 위해 Parallel.ForEach 사용                    
                    // Accept()를 동시에 쓰면 안되나. 매번 한번씩 시도하기에 상관없음.

                    //foreach (Socket socket in listCheckRead)
                    Parallel.ForEach(listCheckRead, socket =>
                    {
                        // 클라이언트 접속 시도
                        if (socket == m_cTCPListen.Server)
                        {
                            // 새 클라이언트 접속
                            Socket socketClient = m_cTCPListen.Server.Accept();
                            m_listClient.Add(socketClient);

                            Console.WriteLine($"[TCP_SERVER] Connect Client = {socketClient.RemoteEndPoint}");

                            // 클라이언트 접속 알림
                            Event_NotifyConnectClient?.Invoke(socketClient);

                        }
                        // 클라이언트 패킷 수신
                        else
                        {
                            // 패킷 수신 알림
                            Event_NotifyReceived?.Invoke(socket);
                        }
                    });

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TCP_SERVER] Select 오류 : {ex.Message}");
                }

            }

        }


    }
}
