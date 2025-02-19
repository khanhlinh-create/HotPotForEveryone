﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Threading;

public class SubServerManager : MonoBehaviour
{
    private TcpListener server;
    private bool isServerRunning = false;
    private List<Room> rooms = new List<Room>();
    private Dictionary<string, string> globalGameState = new Dictionary<string, string>(); // Trạng thái toàn cục (VD: nồi lẩu và đĩa ăn)
    private List<TcpClient> connectedClients = new List<TcpClient>();

    public string masterServerIP = "192.168.15.106"; // Địa chỉ IP của Master Server
    public int masterServerPort = 5000;        // Cổng của Master Server
    public int subServerPort = 6000;           // Cổng của SubServer này

    void Start()
    {
        
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        StartSubServer(subServerPort);
    }
    //Hàm kết nối với Master Server 
    private void RegisterToMasterServer()
    {
        try
        {
            TcpClient masterClient = new TcpClient(masterServerIP, masterServerPort);
            NetworkStream stream = masterClient.GetStream();
            string registerMessage = $"RegisterSubServer|{GetLocalIPAddress()}|{subServerPort}";
            byte[] data = Encoding.UTF8.GetBytes(registerMessage);
            stream.Write(data, 0, data.Length);
            masterClient.Close();

            Debug.Log("SubServer registered with Master Server.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error registering SubServer: {ex.Message}");
        }
    }
    //Hàm khởi chạy Sub Server, bắt đầu lắng nghe 
    private void StartSubServer(int port)
    {
        if (isServerRunning) return; // Đảm bảo không khởi động lại nếu đã chạy
        isServerRunning = true;
        RegisterToMasterServer();
        server = new TcpListener(IPAddress.Any, port);
        server.Start();
        isServerRunning = true;

        Thread serverThread = new Thread(ListenForClients);
        serverThread.Start();
        Debug.Log($"SubServer started on port {port}.");
    }

    //Hàm luôn lắng nghe kết nối khi chạy 
    private void ListenForClients()
    {
        while (isServerRunning)
        {
            try
            {
                // Chấp nhận Client kết nối
                TcpClient client = server.AcceptTcpClient();
                lock (connectedClients)
                {
                    connectedClients.Add(client); // Thêm client vào danh sách
                }
                Debug.Log("Client connected!");

                // Tạo một luồng để xử lý Client này
                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.Start();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error accepting client: {ex.Message}");
            }
        }
    }
    //Hàm xử lý yêu cầu của client 
    public void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        while (true)
        {
            try
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                string[] command = message.Split('|');
                //Khi client nhấn button Create room
                if (command[0] == "CreateRoom")
                {
                    string roomID = GenerateRandomRoomID(); // Tạo mã phòng ngẫu nhiên
                    CreateRoom(roomID, client);

                    // Gửi lại mã phòng cho client
                    string response = $"RoomCreated|{roomID}";
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseData, 0, responseData.Length);
                }

                /*else if (command[0] == "GetSubServer")
                {
                    string roomID = command[1];
                    if (rooms.Exists(r => r.RoomID == roomID))
                    {
                        string response = $"SubServerIP:{GetLocalIPAddress()}";
                        byte[] responseData = Encoding.UTF8.GetBytes(response);
                        stream.Write(responseData, 0, responseData.Length);
                        Debug.Log($"SubServer info sent for room {roomID}");
                    }
                    else
                    {
                        byte[] responseData = Encoding.UTF8.GetBytes("RoomNotFound");
                        stream.Write(responseData, 0, responseData.Length);
                        Debug.LogWarning($"Room {roomID} not found. SubServer info not sent.");
                    }
                }*/
                //Khi client nhấn button Join Room 
                else if (command[0] == "JoinRoom")
                {
                    string roomID = command[1];
                    bool success = JoinRoom(roomID, client);
                    string response = success ? $"JoinSuccess|{roomID}" : "JoinFail|RoomNotFound";
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseData, 0, responseData.Length);

                    Debug.Log($"SubServer: Client requested to join room {roomID}. Response: {response}");
                }
                else if (command[0] == "UpdateState")
                {
                    string itemName = command[1]; // Tên của topping
                    string position = command[2]; // Vị trí "x,y,z"

                    // Cập nhật trạng thái toàn cục
                    globalGameState[itemName] = position;

                    // Phát lại trạng thái tới tất cả các client
                    string broadcastMessage = $"UpdateState|{itemName}|{position}";
                    BroadcastGlobalState(broadcastMessage);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling client: {ex.Message}");
                break;
            }
        }
    }
    //Hàm tạo mã phòng 
    private string GenerateRandomRoomID()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new System.Random();
        char[] buffer = new char[6]; // Mã phòng có 6 ký tự
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = chars[random.Next(chars.Length)];
        }
        return new string(buffer);
    }
    //Lấy IP trả về cho Master Server để đăng ký
    private string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                // Kiểm tra xem IP có thuộc dải mạng LAN không (192.168.x.x hoặc 10.x.x.x)
                if (ip.ToString().StartsWith("192.168.15."))
                {
                    return ip.ToString();
                }
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }
    //Hàm Tạo phòng 
    private void CreateRoom(string roomID, TcpClient client)
    {
        Room room = new Room(roomID);
        room.AddPlayer(client);
        rooms.Add(room);
        Debug.Log($"Room {roomID} created!");

        // Gửi mã phòng lên MasterServer
        NotifyMasterServerRoomCreation(roomID);
    }

    private void NotifyMasterServerRoomCreation(string roomID)
    {
        try
        {
            TcpClient masterClient = new TcpClient(masterServerIP, masterServerPort);
            NetworkStream stream = masterClient.GetStream();
            //gửi thông tin về cho Master Server gồm roomcode|subIP|subPort 
            string message = $"AddRoom|{roomID}|{GetLocalIPAddress()}|{subServerPort}";
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);

            Debug.Log($"Notified MasterServer of room {roomID}.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error notifying MasterServer of room creation: {ex.Message}");
        }
    }
    //Hàm thực hiện cho client Join Room khi clietn yêu cầu Join 
    private bool JoinRoom(string roomID, TcpClient client)
    {
        Room room = rooms.Find(r => r.RoomID == roomID);
        if (room != null)
        {
            room.AddPlayer(client);
            Debug.Log($"Client joined room {roomID}");
            return true;
        }
        else
        {
            Debug.LogWarning($"Room {roomID} not found!");
            return false;
        }
    }

    /*private void UpdateRoomState(string roomID, string key, string value)
    {
        globalGameState[key] = value; // Cập nhật trạng thái toàn cục
        BroadcastGlobalState();       // Phát sóng trạng thái tới mọi client
    }*/

    private void BroadcastGlobalState(string message)
    {
        foreach (TcpClient client in connectedClients) // `connectedClients` là danh sách các client đang kết nối
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                Debug.Log($"Broadcasted state: {message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error broadcasting to client: {ex.Message}");
            }
        }
    }

    private string SerializeGlobalState()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var entry in globalGameState)
        {
            sb.Append($"{entry.Key}:{entry.Value}|");
        }
        return sb.ToString().TrimEnd('|');
    }

    void OnApplicationQuit()
    {
        isServerRunning = false;
        server.Stop();
        Debug.Log("SubServer stopped.");
    }
}

public class Room
{
    public string RoomID { get; private set; }
    private List<TcpClient> players = new List<TcpClient>(); //quản lý người chơi 
    private Dictionary<string, string> roomState = new Dictionary<string, string>(); // Trạng thái của phòng

    public Room(string id)
    {
        RoomID = id;
    }

    public void AddPlayer(TcpClient client)
    {
        players.Add(client);
        Debug.Log($"Player added to room {RoomID}");
    }

    public void UpdateState(string key, string value)
    {
        roomState[key] = value; // Cập nhật trạng thái
        BroadcastState(); // Gửi trạng thái tới tất cả các Client trong phòng
    }
    //Hàm để đồng bộ hóa tới người chơi trong phòng 
    private void BroadcastState()
    {
        string stateData = SerializeState();

        foreach (TcpClient player in players)
        {
            try
            {
                NetworkStream stream = player.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(stateData);
                stream.Write(data, 0, data.Length);
            }
            catch (SocketException ex)
            {
                Debug.LogError($"Error broadcasting to client in room {RoomID}: {ex.Message}");
            }
        }
    }

    private string SerializeState()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var entry in roomState)
        {
            sb.Append($"{entry.Key}:{entry.Value}|");
        }
        return sb.ToString().TrimEnd('|'); // VD: "HotPot:Boiling|Item1:Cooked"
    }
}