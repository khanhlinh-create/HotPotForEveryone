using UnityEngine;
using TMPro; // Nếu dùng TextMeshPro
using System.Net.Sockets;
using System.Text;

public class RoomManager : MonoBehaviour
{
    public TMP_InputField RoomCodeInputField; // InputField nhập RoomCode
    public GameObject JoinRoomButton; // Button "Join Room"
    public GameObject CreateRoomButton; // Button "Create Room"

    private TcpClient subServerClient;

    private void Start()
    {
        // Kết nối với SubServer (kết nối này đã được thiết lập từ trước)
        subServerClient = ConnectionManager.subServerClient;
    }

    public void CreateRoom()
    {
        string message = "CreateRoom|";
        SendMessageToSubServer(message);
        Debug.Log("Requested to create a room.");
    }

    public void JoinRoom()
    {
        string roomCode = RoomCodeInputField.text.Trim();
        if (!string.IsNullOrEmpty(roomCode))
        {
            string message = $"JoinRoom|{roomCode}";
            SendMessageToSubServer(message);
            Debug.Log($"Requested to join room: {roomCode}");
        }
        else
        {
            Debug.LogWarning("Room code is empty!");
        }
    }

    private void SendMessageToSubServer(string message)
    {
        try
        {
            NetworkStream stream = subServerClient.GetStream();
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
        catch (SocketException ex)
        {
            Debug.LogError($"Error sending message to SubServer: {ex.Message}");
        }
    }
}
