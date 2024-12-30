using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class MasterServerManager
{
    private List<SubServerInfo> subServers = new List<SubServerInfo>();
    private int lastAssignedIndex = -1;

    public void StartMasterServer(int port)
    {
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Master Server started on port {port}.");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            HandleClient(client);
        }
    }

    private void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        SubServerInfo assignedServer = GetNextSubServer();
        if (assignedServer != null)
        {
            string response = $"ServerIP:{assignedServer.IP}|ServerPort:{assignedServer.Port}";
            byte[] data = Encoding.UTF8.GetBytes(response);
            stream.Write(data, 0, data.Length);
        }
        client.Close();
    }

    private SubServerInfo GetNextSubServer()
    {
        if (subServers.Count == 0) return null;
        lastAssignedIndex = (lastAssignedIndex + 1) % subServers.Count;
        return subServers[lastAssignedIndex];
    }
}

public class SubServerInfo
{
    public string IP { get; set; }
    public int Port { get; set; }
}
