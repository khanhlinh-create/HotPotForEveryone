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
    private int lastAssignedIndex = -1;
    private bool isServerRunning = false;
    public int port = 5000; // Cổng của Master Server
    private TcpListener listener;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
       
    }

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

    private void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        if (message.StartsWith("RegisterSubServer"))
        {
            // Xử lý đăng ký SubServer
            string[] parts = message.Split('|');
            string subServerIP = parts[1];
            int subServerPort = int.Parse(parts[2]);
            RegisterSubServer(subServerIP, subServerPort);
        }
        else if (message.StartsWith("RequestSubServer"))
        {
            // Xử lý yêu cầu phân công SubServer từ Client
            SubServerInfo assignedServer = GetNextSubServer();
            if (assignedServer != null)
            {
                string response = $"ServerIP:{assignedServer.IP}|ServerPort:{assignedServer.Port}";
                byte[] data = Encoding.UTF8.GetBytes(response);
                stream.Write(data, 0, data.Length);
            }
        }

        client.Close();
    }

    private void RegisterSubServer(string ip, int port)
    {
        subServers.Add(new SubServerInfo { IP = ip, Port = port });
        Debug.Log($"SubServer registered: {ip}:{port}");
    }

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

public class SubServerInfo
{
    public string IP { get; set; }
    public int Port { get; set; }
}