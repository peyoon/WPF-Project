// WpfOlzServer/ServerCtrl.xaml.cs

using System.Text.Json; // ✨ [수정] using 구문 변경
using System.Text.Json.Serialization; // ✨ [수정] [JsonPropertyName]을 위해 추가
using OJTToolKit.Utils;
using OzNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using WpfOlzServer.Network;
using WpfOlzServer.Helpers;
using WpfOlzServer.Models;

namespace WpfOlzServer
{
    public partial class ServerCtrl : System.Windows.Controls.UserControl
    {
        // ... (UI 및 서버 시작/종료, 네트워크 이벤트 핸들러 부분은 기존과 동일) ...
        public event Action<string> OnLogMessage;
        private OzTCPServer tcpserver;
        private readonly List<ClientState> _clients = new List<ClientState>();

        // ✨ [수정] JsonSerializer에 사용할 공통 옵션을 미리 정의합니다.
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true // JSON을 읽을 때 키의 대/소문자를 구분하지 않음
            ,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            ,
            WriteIndented = true
        };

        public ServerCtrl() { InitializeComponent(); }

        #region UI 및 서버 시작/종료
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!IPAddress.TryParse(txtIpAddress.Text, out IPAddress ip)) { AddLog("오류: 유효한 IP 주소가 아닙니다."); return; }
            if (!int.TryParse(txtPort.Text, out int port)) { AddLog("오류: 유효한 포트 번호가 아닙니다."); return; }

            tcpserver = new OzTCPServer();
            try
            {
                tcpserver.StartServer(ip, port);
                tcpserver.Event_NotifyConnectClient += Tcpserver_Event_NotifyConnectClient;
                tcpserver.Event_NotifyReceived += Tcpserver_Event_NotifyReceived;
                tcpserver.Event_NotifyDisconnectClient += Tcpserver_Event_NotifyDisconnectClient;
                AddLog($"서버가 시작되었습니다. ({ip}:{port})");
                UpdateUI(true);
            }
            catch (Exception ex)
            {
                AddLog($"서버 시작 오류: {ex.Message}");
                tcpserver?.StopServer();
                UpdateUI(false);
            }
        }
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                tcpserver?.StopServer();
                AddLog("서버가 종료되었습니다.");
                UpdateUI(false);
            }
            catch (Exception ex) { AddLog($"서버 종료 오류: {ex.Message}"); }
        }
        private void UpdateUI(bool isRunning) => Dispatcher.Invoke(() =>
        {
            btnStart.IsEnabled = !isRunning; btnStop.IsEnabled = isRunning;
            txtIpAddress.IsReadOnly = isRunning; txtPort.IsReadOnly = isRunning;
        });
        private void AddLog(string message) => Dispatcher.Invoke(() => OnLogMessage?.Invoke(message));
        #endregion

        #region 네트워크 이벤트 핸들러
        private void Tcpserver_Event_NotifyConnectClient(Socket socket)
        {
            lock (_clients)
            {
                if (!_clients.Any(c => c.TcpClient == socket))
                {
                    _clients.Add(new ClientState(socket));
                }
            }
            AddLog($"클라이언트 접속: {socket.RemoteEndPoint}");
        }

        private void Tcpserver_Event_NotifyDisconnectClient(Socket socket)
        {
            lock (_clients)
            {
                var clientToRemove = _clients.FirstOrDefault(c => c.TcpClient == socket);
                if (clientToRemove != null)
                {
                    _clients.Remove(clientToRemove);
                    AddLog($"클라이언트 접속 종료: {socket.RemoteEndPoint}");
                }
            }
        }

        private void Tcpserver_Event_NotifyReceived(Socket socket)
        {
            ClientState client;
            lock (_clients) { client = _clients.FirstOrDefault(c => c.TcpClient == socket); }
            if (client == null || !Monitor.TryEnter(client.ReadLock)) return;

            try
            {
                byte[] headBuffer = tcpserver.Recieve(socket, 10);
                if (headBuffer == null) { return; }

                T_HeadModel header = HeadModel.Frombytes(headBuffer);

                byte[] bodyBuffer = new byte[0];
                if (header.Size > 0)
                {
                    bodyBuffer = tcpserver.Recieve(socket, header.Size);
                    if (bodyBuffer == null) { return; }
                }

                ProcessPacket(client, header, bodyBuffer);
            }
            catch (Exception ex)
            {
                AddLog($"데이터 처리 오류 ({socket.RemoteEndPoint}): {ex.Message}");
                tcpserver.DisconnectClient(socket);
            }
            finally
            {
                Monitor.Exit(client.ReadLock);
            }
        }
        #endregion


        #region 패킷 처리
        private void ProcessPacket(ClientState client, T_HeadModel header, byte[] body)
        {
            switch (header.Opcode)
            {
                case E_OPCode.UserCheck:
                    HandleLoginRequest(client, body);
                    break;
                case E_OPCode.CS_PROFILE_UPDATE_REQ:
                    HandleProfileUpdateRequest(client, body);
                    break;
                case E_OPCode.CS_PROFILE_GET_REQ:
                    HandleProfileGetRequest(client);
                    break;
                default:
                    AddLog($"알 수 없는 Opcode 수신: {header.Opcode}");
                    break;
            }
        }

        private void HandleProfileGetRequest(ClientState client)
        {
            try
            {
                if (string.IsNullOrEmpty(client.UserId))
                {
                    AddLog($"[오류] 로그인되지 않은 클라이언트(IP:{client.TcpClient.RemoteEndPoint})가 프로필 정보를 요청했습니다.");
                    return;
                }
                AddLog($"프로필 정보 요청 수신 (ID: {client.UserId})");
                var userProfile = JsonHelper.LoadUserProfile(client.UserId);
                if (userProfile == null)
                {
                    userProfile = new UserProfile { UserJoinDate = client.UserId, UserName = client.UserName };
                }
                SendJsonResponse(client.TcpClient, E_OPCode.SC_PROFILE_GET_RES, userProfile);
                AddLog($"프로필 정보 전송 완료 (ID: {client.UserId})");
            }
            catch (Exception ex)
            {
                AddLog($"프로필 정보 요청 처리 오류: {ex.Message}");
            }
        }

        private void HandleLoginRequest(ClientState client, byte[] body)
        {
            try
            {
                string userInfoJson = Encoding.UTF8.GetString(body);
                // ✨ [수정] JsonConvert를 JsonSerializer로 변경
                var loginInfo = JsonSerializer.Deserialize<Loginfo>(userInfoJson, _jsonOptions);

                if (loginInfo == null || string.IsNullOrEmpty(loginInfo.EmployeeId))
                {
                    throw new Exception("수신된 로그인 정보(JSON)가 올바르지 않습니다.");
                }

                var users = new Dictionary<string, (string pw, string name)> { { "10194", ("1234", "윤은평") }, { "10159", ("1234", "장봉석") } };

                if (users.TryGetValue(loginInfo.EmployeeId, out var userInfo) && userInfo.pw == loginInfo.Password)
                {
                    client.UserId = loginInfo.EmployeeId;
                    client.UserName = userInfo.name;
                    AddLog($"로그인 성공: {client.UserName} ({client.UserId})");

                    var response = new { user_name = client.UserName, user_id = client.UserId };
                    SendJsonResponse(client.TcpClient, E_OPCode.SC_LOGIN_SUCCESS, response);
                }
                else
                {
                    AddLog($"로그인 실패: ID({loginInfo.EmployeeId}) 또는 PW 불일치");
                    var response = new { reason = "사원번호 또는 비밀번호가 일치하지 않습니다." };
                    SendJsonResponse(client.TcpClient, E_OPCode.SC_LOGIN_FAIL, response);
                }
            }
            catch (Exception ex) { AddLog($"로그인 처리 오류: {ex.Message}"); }
        }

        private void HandleProfileUpdateRequest(ClientState client, byte[] body)
        {
            try
            {
                string profileJson = Encoding.UTF8.GetString(body);
                // ✨ [수정] JsonConvert를 JsonSerializer로 변경
                var profileData = JsonSerializer.Deserialize<UserProfile>(profileJson, _jsonOptions);

                AddLog($"프로필 업데이트 수신 (ID: {profileData.UserJoinDate}): 이름={profileData.UserName}, 직급={profileData.UserGrade}, 이메일={profileData.UserEmail}");

                try
                {
                    JsonHelper.SaveUserProfile(profileData);
                    AddLog($"[파일 저장 성공] {profileData.UserJoinDate}.json 파일이 업데이트되었습니다.");
                }
                catch (Exception fileEx)
                {
                    AddLog($"[파일 저장 오류] {fileEx.Message}");
                }

                var response = new { success = true, message = "프로필이 서버에 성공적으로 업데이트되었습니다." };
                SendJsonResponse(client.TcpClient, E_OPCode.SC_PROFILE_UPDATE_RES, response);
            }
            catch (Exception ex)
            {
                AddLog($"프로필 업데이트 처리 오류: {ex.Message}");
            }
        }

        private void SendJsonResponse(Socket socket, E_OPCode opcode, object data)
        {
            // ✨ [수정] JsonConvert를 JsonSerializer로 변경
            var json = JsonSerializer.Serialize(data);
            var bodyBytes = Encoding.UTF8.GetBytes(json);
            var header = HeadModel.HeadMessage(opcode, E_HeadType.JSON, bodyBytes.Length);
            tcpserver.Send(socket, header.ToAddbytearrEx(bodyBytes));
        }
        #endregion
    }

    public class ClientState
    {
        public Socket TcpClient { get; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public readonly object ReadLock = new object();
        public ClientState(Socket client) { TcpClient = client; }
    }

    public class Loginfo
    {
        // ✨ [수정] JsonProperty를 JsonPropertyName으로 변경
        [JsonPropertyName("emp_no")]
        public string EmployeeId { get; set; }
        public string Password { get; set; }
    }
}