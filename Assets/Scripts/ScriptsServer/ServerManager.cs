using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using System.Text;

public class ServerManager : MonoBehaviour
{
    public static ServerManager Instance { get; private set; } // Singleton instance

    private TcpListener server;
    private Thread serverThread;
    private Dictionary<string, Room> rooms = new Dictionary<string, Room>(); // Danh sách các phòng
    private System.Random random = new System.Random();

    private bool isServerRunning = false; // Cờ để kiểm tra trạng thái server

    private void Awake()
    {
        if (transform.parent != null)
        {
            transform.parent = null; // Tách khỏi GameObject cha
        }
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Giữ lại ServerManager khi chuyển scene
    }

    // Hàm khởi tạo tùy chỉnh, chỉ được gọi khi kích hoạt ServerObjects
    public void InitializeServer()
    {
        serverThread = new Thread(StartServer);
        serverThread.Start();
        Debug.Log("Server started.");
    }

    private void StartServer()
    {
        server = new TcpListener(IPAddress.Any, 7777);
        server.Start();
        isServerRunning = true;
        while (isServerRunning)
        {
            if (server.Pending())
            {
                TcpClient client = server.AcceptTcpClient();
                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.Start();
            }
            else
            {
                Thread.Sleep(100); // Giảm tải CPU khi không có kết nối
            }
        }
    }

    private void OnApplicationQuit()
    {
        StopServer();
    }

    private void StopServer()
    {
        isServerRunning = false; // Dừng vòng lặp
        server?.Stop();          // Đóng TcpListener
        serverThread?.Join();    // Đợi luồng hoàn thành
        Debug.Log("Server stopped safely.");
    }

    private void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead;

        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
        {
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
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
                JoinRoom(client, data);
                break;

            default:
                SendToClient(client, "Error|UnknownCommand");
                break;
        }
    }

    private void CreateRoom(TcpClient client)
    {
        string roomCode = GenerateRandomCode();
        if (!rooms.ContainsKey(roomCode))
        {
            Room newRoom = new Room(roomCode);
            rooms.Add(roomCode, newRoom);
            newRoom.AddPlayer(client);

            Debug.Log($"Room {roomCode} created.");
            SendToClient(client, $"RoomCreated|{roomCode}");
        }
    }

    private void JoinRoom(TcpClient client, string roomCode)
    {
        if (rooms.TryGetValue(roomCode, out Room room))
        {
            if (room.HasSpace())
            {
                room.AddPlayer(client);
                Debug.Log($"Client joined room {roomCode}");
                SendToClient(client, $"JoinedRoom|{roomCode}");
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
        byte[] data = Encoding.UTF8.GetBytes(message);
        stream.Write(data, 0, data.Length);
    }

    private string GenerateRandomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] code = new char[6];
        for (int i = 0; i < 6; i++)
        {
            code[i] = chars[random.Next(chars.Length)];
        }
        return new string(code);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
