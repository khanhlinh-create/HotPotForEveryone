using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class Room
{
    private int roomID;
    private List<TcpClient> players = new List<TcpClient>();
    private const int MaxPlayers = 3;

    // Dữ liệu phòng
    private string hotPotState = "Empty"; // Trạng thái nồi lẩu
    private List<string> chatLog = new List<string>();

    public Room(int id)
    {
        roomID = id;
    }

    public void AddPlayer(TcpClient player)
    {
        if (players.Count < MaxPlayers)
        {
            players.Add(player);
        }
    }

    public bool HasSpace()
    {
        return players.Count < MaxPlayers;
    }

    public void SyncRoomData()
    {
        string data = $"RoomData|HotPotState:{hotPotState}|Players:{players.Count}";
        BroadcastToPlayers(data);
    }

    public void UpdateHotPotState(string newState)
    {
        hotPotState = newState;
        SyncRoomData();
    }

    public void AddChatMessage(string message)
    {
        chatLog.Add(message);
        BroadcastToPlayers($"Chat|{message}");
    }

    private void BroadcastToPlayers(string message)
    {
        foreach (var player in players)
        {
            NetworkStream stream = player.GetStream();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
    }
}
