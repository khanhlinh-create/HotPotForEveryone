using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Threading;

public class SubServerManager : MonoBehaviour
{
    private TcpListener server;
    private bool isRunning;
    private List<Room> rooms = new List<Room>();
    private Dictionary<string, string> globalGameState = new Dictionary<string, string>(); // Trạng thái toàn cục (VD: nồi lẩu và đĩa ăn)

    public string masterServerIP = "127.0.0.1"; // Địa chỉ IP của Master Server
    public int masterServerPort = 5000;        // Cổng của Master Server
    public int subServerPort = 6000;           // Cổng của SubServer này

    void Start()
    {
        RegisterToMasterServer();
        StartSubServer(subServerPort);
    }

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

    private void StartSubServer(int port)
    {
        server = new TcpListener(IPAddress.Any, port);
        server.Start();
        isRunning = true;

        // Lắng nghe kết nối từ Client trên một luồng riêng
        Thread serverThread = new Thread(ListenForClients);
        serverThread.Start();

        Debug.Log($"SubServer started on port {port}.");
    }


    private void ListenForClients()
    {
        while (isRunning)
        {
            try
            {
                // Chấp nhận Client kết nối
                TcpClient client = server.AcceptTcpClient();
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

                if (command[0] == "CreateRoom")
                {
                    string roomID = command[1];
                    CreateRoom(roomID, client);
                }
                else if (command[0] == "JoinRoom")
                {
                    string roomID = command[1];
                    JoinRoom(roomID, client);
                }
                else if (command[0] == "UpdateState")
                {
                    string roomID = command[1];
                    string key = command[2];
                    string value = command[3];
                    UpdateRoomState(roomID, key, value);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling client: {ex.Message}");
                break;
            }
        }
    }

    private string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    private void CreateRoom(string roomID, TcpClient client)
    {
        Room room = new Room(roomID);
        room.AddPlayer(client);
        rooms.Add(room);
        Debug.Log($"Room {roomID} created!");
    }

    private void JoinRoom(string roomID, TcpClient client)
    {
        Room room = rooms.Find(r => r.RoomID == roomID);
        if (room != null)
        {
            room.AddPlayer(client);
            Debug.Log($"Client joined room {roomID}");
        }
        else
        {
            Debug.LogWarning($"Room {roomID} not found!");
        }
    }

    private void UpdateRoomState(string roomID, string key, string value)
    {
        Room room = rooms.Find(r => r.RoomID == roomID);
        if (room != null)
        {
            room.UpdateState(key, value); // Cập nhật trạng thái của phòng
        }
        else
        {
            Debug.LogWarning($"Room {roomID} not found for state update!");
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        server.Stop();
        Debug.Log("SubServer stopped.");
    }
}

public class Room
{
    public string RoomID { get; private set; }
    private List<TcpClient> players = new List<TcpClient>();
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
