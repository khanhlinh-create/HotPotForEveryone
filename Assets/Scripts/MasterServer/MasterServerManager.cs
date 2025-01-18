using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class MasterServerManager : MonoBehaviour
{
    private List<SubServerInfo> subServers = new List<SubServerInfo>();
    private Dictionary<string, SubServerInfo> roomToSubServerMap = new Dictionary<string, SubServerInfo>();
    private int lastAssignedIndex = -1;
    private bool isServerRunning = false;
    public int port = 5000; // Cổng của Master Server
    private TcpListener listener;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        StartMasterServer(port);
    }

    void Start()
    {
        
    }
    //hàm khởi chạy master server 
    public void StartMasterServer(int port)
    {
        if (isServerRunning) return; // Đảm bảo không khởi động lại nếu đã chạy
        isServerRunning = true;

        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Debug.Log($"Master Server started on port {port}.");

        Thread listenerThread = new Thread(ListenForConnections);
        listenerThread.Start();
    }
    //hàm lắng nghe kết nối
    private void ListenForConnections()
    {
        while (true)
        {
            try
            {
                TcpClient client = listener.AcceptTcpClient();
                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.Start();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error accepting client: {ex.Message}");
            }
        }
    }
    //hàm xử lý yêu cầu của client (client, subserver)
    private void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        //khi subserver đăng ký với masterserver (lúc subserver khởi chạy)
        if (message.StartsWith("RegisterSubServer"))
        {
            // Xử lý đăng ký SubServer
            string[] parts = message.Split('|');
            string subServerIP = parts[1];
            int subServerPort = int.Parse(parts[2]);
            RegisterSubServer(subServerIP, subServerPort);
        }
        //khi client nhấn button Create room, yêu cầu 1 SubServer, Master Server sẽ chỉ định
        else if (message.StartsWith("RequestSubServer"))
        {
            // Xử lý yêu cầu phân công SubServer từ Client theo thuật toán cân bằng tải 
            SubServerInfo assignedServer = GetNextSubServer();
            if (assignedServer != null)
            {
                string response = $"ServerIP:{assignedServer.IP}|ServerPort:{assignedServer.Port}";
                byte[] data = Encoding.UTF8.GetBytes(response);
                stream.Write(data, 0, data.Length);
                Debug.Log($"Assigned SubServer: {assignedServer.IP}:{assignedServer.Port}");
            }
            else
            {
                Debug.LogError("No available SubServer to assign.");
            }
        }
        /*Sau khi client nhấn button Create room, SubServer được chỉ định sẽ tạo 1 mã phòng và gửi mã đó cho Master
         Server quản lý */
        else if (message.StartsWith("AddRoom"))
        {
            // SubServer gửi mã phòng lên
            string[] parts = message.Split('|');
            if (parts.Length >= 3)
            {
                string roomCode = parts[1];  //đầu tiên là roomcode 
                string subServerIP = parts[2];  //thứ 2 là SubIP 
                int subServerPort = int.Parse(parts[3]);  //thứ 3 là SubPort 

                if (!roomToSubServerMap.ContainsKey(roomCode))
                {
                    SubServerInfo subServer = new SubServerInfo { IP = subServerIP, Port = subServerPort };
                    roomToSubServerMap[roomCode] = subServer;
                    Debug.Log($"Room {roomCode} mapped to SubServer {subServerIP}:{subServerPort}");
                }
            }
        }
        /*Khi 1 client nào đó nhập roomcode vào InputField và nhấn button Join Room, client gửi yêu cầu tìm SubServer 
         đang quản lý mã phòng tương ứng cho Master Server*/
        else if (message.StartsWith("GetSubServer"))
        {
            // Xử lý yêu cầu tìm SubServer cho một mã phòng
            string[] parts = message.Split('|');
            if (parts.Length >= 2)
            {
                string roomCode = parts[1].Trim();
                if (roomToSubServerMap.TryGetValue(roomCode, out SubServerInfo subServer))
                {
                    // Trả về thông tin SubServer đang quản lý phòng
                    string response = $"SubServerIP:{subServer.IP}|Port:{subServer.Port}";
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseData, 0, responseData.Length);
                    Debug.Log($"Returned SubServer for room {roomCode}: {subServer.IP}:{subServer.Port}");
                }
                else
                {
                    // Phòng không tồn tại
                    string response = "RoomNotFound";
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseData, 0, responseData.Length);
                    Debug.LogWarning($"Room {roomCode} not found.");
                }
            }
        }

    }
    //Hàm quản lý các SubServer: roomcode|IP|Port 
    private void RegisterSubServer(string ip, int port)
    {
        SubServerInfo subServer = new SubServerInfo { IP = ip, Port = port };
        subServers.Add(subServer);
        Debug.Log($"SubServer registered: {ip}:{port}");

        // Ví dụ: SubServer gửi thông tin các phòng nó quản lý sau khi đăng ký
        string[] rooms = {}; // Đây là giả định, bạn cần nhận thông tin từ SubServer
        foreach (string room in rooms)
        {
            roomToSubServerMap[room] = subServer;
            Debug.Log($"Mapped room {room} to SubServer {ip}:{port}");
        }
    }
    //Hàm để chỉ định SubServer tiếp theo bằng thuật toán cân bằng tải 
    private SubServerInfo GetNextSubServer()
    {
        if (subServers.Count == 0) return null;
        lastAssignedIndex = (lastAssignedIndex + 1) % subServers.Count;
        return subServers[lastAssignedIndex];
    }

    void OnApplicationQuit()
    {
        listener.Stop();
        Debug.Log("Master Server stopped.");
    }
}
//Lớp SubServerInfo gồm có IP và Port 
public class SubServerInfo
{
    public string IP { get; set; }
    public int Port { get; set; }
}