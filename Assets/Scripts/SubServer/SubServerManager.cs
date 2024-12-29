using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

public class SubServerManager
{
    private List<Room> rooms = new List<Room>();

    public void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);

        string message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
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
    }

    private void CreateRoom(string roomID, TcpClient client)
    {
        Room room = new Room(roomID);
        room.AddPlayer(client);
        rooms.Add(room);
    }

    private void JoinRoom(string roomID, TcpClient client)
    {
        Room room = rooms.Find(r => r.RoomID == roomID);
        room?.AddPlayer(client);
    }
}

public class Room
{
    public string RoomID { get; private set; }
    private List<TcpClient> players = new List<TcpClient>();

    public Room(string id)
    {
        RoomID = id;
    }

    public void AddPlayer(TcpClient client)
    {
        players.Add(client);
        // Đồng bộ hóa dữ liệu nếu cần.
    }
}

