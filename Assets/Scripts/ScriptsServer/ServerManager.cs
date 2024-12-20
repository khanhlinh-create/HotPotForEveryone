using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    public static ServerManager Instance { get; private set; } // Singleton instance

    private TcpListener server;
    private Thread serverThread;
    private Dictionary<int, Room> rooms = new Dictionary<int, Room>(); // Danh sách các phòng
    private int nextRoomID = 1; // ID phòng tiếp theo

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Giữ lại ServerManager khi chuyển scene
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // Khởi động server trên cổng 7777
        serverThread = new Thread(StartServer);
        serverThread.Start();
        Debug.Log("Server started.");
    }

    private void StartServer()
    {
        server = new TcpListener(IPAddress.Any, 7777);
        server.Start();

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }

    private void OnApplicationQuit()
    {
        // Dừng server khi ứng dụng tắt
        server?.Stop();
        serverThread?.Abort();
    }

    private void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead;

        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
        {
            string message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
            ProcessMessage(client, message);
        }
    }

    private void ProcessMessage(TcpClient client, string message)
    {
        Debug.Log($"Received: {message}");
        string[] parts = message.Split('|');
        string command = parts[0];
        string data = parts.Length > 1 ? parts[1] : "";

        switch (command)
        {
            case "CreateRoom":
                CreateRoom(client);
                break;

            case "JoinRoom":
                int roomId = int.Parse(data);
                JoinRoom(client, roomId);
                break;

            default:
                Debug.LogWarning("Unknown command.");
                break;
        }
    }

    private void CreateRoom(TcpClient client)
    {
        int roomId = nextRoomID++;
        Room newRoom = new Room(roomId);
        rooms.Add(roomId, newRoom);
        newRoom.AddPlayer(client);
        Debug.Log($"Room {roomId} created.");

        SendToClient(client, $"RoomCreated|{roomId}");
    }

    private void JoinRoom(TcpClient client, int roomId)
    {
        if (rooms.ContainsKey(roomId))
        {
            Room room = rooms[roomId];
            if (room.HasSpace())
            {
                room.AddPlayer(client);
                Debug.Log($"Client joined room {roomId}");
                SendToClient(client, "JoinedRoom");
                room.SyncRoomData();
            }
            else
            {
                SendToClient(client, "RoomFull");
            }
        }
        else
        {
            SendToClient(client, "RoomNotFound");
        }
    }

    private void SendToClient(TcpClient client, string message)
    {
        NetworkStream stream = client.GetStream();
        byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
        stream.Write(data, 0, data.Length);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
