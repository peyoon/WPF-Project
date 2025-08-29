using System.Text.Json;
using OzNet;
using OJTToolKit.Utils;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WpfLogin.Network;
using WpfLogin.Models;
using WpfOlzServer; // ✨ [수정] UserProfile 모델의 네임스페이스를 명확히 사용합니다.

namespace WpfLogin.ViewModels
{
    public class NetworkService
    {
        private static readonly Lazy<NetworkService> _instance = new Lazy<NetworkService>(() => new NetworkService());
        public static NetworkService Instance => _instance.Value;

        private TCPClient _client;
        private CancellationTokenSource _cts;

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public string CurrentUserId { get; set; }
        public string CurrentUserName { get; set; }

        public event Action<dynamic> LoginSuccessResponseReceived;
        public event Action<string> LoginFailResponseReceived;
        public event Action ProfileUpdateSuccessReceived;
        public event Action Disconnected;
        public event Action<UserProfile> ProfileGetResponseReceived;

        public bool IsConnected => _client?.IsClientRun() ?? false;

        private NetworkService() { }

        // ... ConnectAsync, Disconnect, Send, OnDisconnectedFromServer, ReceiveLoopAsync 메서드는 수정사항 없음 ...
        public async Task<bool> ConnectAsync(string ip, int port)
        {
            if (IsConnected) Disconnect();
            _client = new TCPClient();
            _client.Event_NotifyDisconnect += OnDisconnectedFromServer;
            Socket socket = await Task.Run(() => _client.Connect(ip, port));
            if (socket != null)
            {
                _cts = new CancellationTokenSource();
                Task.Run(() => ReceiveLoopAsync(socket, _cts.Token), _cts.Token);
                return true;
            }
            _client = null;
            return false;
        }
        public void Disconnect()
        {
            _cts?.Cancel();
            _client?.Disconnect();
            _client = null;
            _cts = null;
            Application.Current.Dispatcher.Invoke(() => Disconnected?.Invoke());
        }
        public void Send(byte[] packet)
        {
            if (IsConnected)
            {
                _client.Send(packet, packet.Length);
            }
        }
        private void OnDisconnectedFromServer()
        {
            Disconnect();
        }
        private async Task ReceiveLoopAsync(Socket socket, CancellationToken token)
        {
            while (!token.IsCancellationRequested && IsConnected)
            {
                try
                {
                    byte[] headBuffer = new byte[10];
                    int headRead = await socket.ReceiveAsync(new ArraySegment<byte>(headBuffer), SocketFlags.None);
                    if (headRead < 10) { OnDisconnectedFromServer(); break; }
                    T_HeadModel header = HeadModel.Frombytes(headBuffer);
                    byte[] bodyBuffer = new byte[0];
                    if (header.Size > 0)
                    {
                        bodyBuffer = new byte[header.Size];
                        await socket.ReceiveAsync(new ArraySegment<byte>(bodyBuffer), SocketFlags.None);
                    }
                    Application.Current.Dispatcher.Invoke(() => HandlePacket(header, bodyBuffer));
                }
                catch { if (!token.IsCancellationRequested) OnDisconnectedFromServer(); break; }
            }
        }

        private void HandlePacket(T_HeadModel header, byte[] body)
        {
            string json = Encoding.UTF8.GetString(body);

            try
            {
                switch ((E_OPCode)header.Opcode)
                {
                    case E_OPCode.SC_LOGIN_SUCCESS:
                        var loginRes = JsonSerializer.Deserialize<LoginSuccessResponse>(json, _jsonOptions);
                        if (loginRes != null)
                        {
                            CurrentUserId = loginRes.user_id;
                            CurrentUserName = loginRes.user_name;
                            LoginSuccessResponseReceived?.Invoke(loginRes);
                        }
                        break;

                    case E_OPCode.SC_LOGIN_FAIL:
                        var failRes = JsonSerializer.Deserialize<LoginFailResponse>(json, _jsonOptions);
                        if (failRes != null)
                        {
                            LoginFailResponseReceived?.Invoke(failRes.reason);
                        }
                        break;

                    case E_OPCode.SC_PROFILE_UPDATE_RES:
                        ProfileUpdateSuccessReceived?.Invoke();
                        break;

                    case E_OPCode.SC_PROFILE_GET_RES:
                        var profile = JsonSerializer.Deserialize<UserProfile>(json, _jsonOptions);
                        if (profile != null)
                        {
                            ProfileGetResponseReceived?.Invoke(profile);
                        }
                        break;
                }
            }
            catch (JsonException jsonEx)
            {
                System.Diagnostics.Debug.WriteLine($"JSON Deserialization failed: {jsonEx.Message}");
            }
        }
    }

    // ✨ [수정] record를 하위 버전과 호환되는 class로 변경
    internal class LoginSuccessResponse
    {
        public string user_id { get; set; }
        public string user_name { get; set; }
    }

    internal class LoginFailResponse
    {
        public string reason { get; set; }
    }
}