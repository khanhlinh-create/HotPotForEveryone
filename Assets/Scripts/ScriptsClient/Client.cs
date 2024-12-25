//Quản lý kết nối socket client đến server, gửi/nhận dữ liệu
using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class Client : MonoBehaviour
{
    private TcpClient socket;
    private NetworkStream stream;
    private byte[] receiveBuffer;

    public static Client Instance; // Singleton pattern

    public int serverPort = 7777;


    private void Awake()
    {
        if (transform.parent != null)
        {
            transform.parent = null; // Tách khỏi GameObject cha
        }
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
        if (RoomHandler.Instance == null)
        {
            GameObject roomHandlerObject = new GameObject("RoomHandler");
            roomHandlerObject.AddComponent<RoomHandler>();
            DontDestroyOnLoad(roomHandlerObject); // Đảm bảo RoomHandler không bị xóa khi chuyển scene
        }


    }

    // Hàm khởi tạo tùy chỉnh, chỉ được gọi khi kích hoạt ClientObjects
    public void InitializeClient()
    {
        Debug.Log("Client started.");
    }

    public void Connect(string ipAddress)
    {
        socket = new TcpClient();
        try
        {
            socket.Connect(ipAddress, serverPort);
            stream = socket.GetStream();
            receiveBuffer = new byte[4096];
            Debug.Log("Connecting to server at: " + ipAddress);
            stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, ReceiveCallback, null);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to connect to server: {ex.Message}");
        }
    }

    public void SendData(string message)
    {
        if (socket == null || !socket.Connected)
        {
            Debug.LogWarning("Client is not connected to server.");
            return;
        }

        byte[] data = Encoding.ASCII.GetBytes(message);
        stream.Write(data, 0, data.Length);
    }

    private void ReceiveCallback(IAsyncResult result)
    {
        try
        {
            int byteLength = stream.EndRead(result);
            if (byteLength <= 0)
            {
                Debug.LogWarning("Server stream ended. Waiting for reconnection...");
                return;
            }

            string data = Encoding.ASCII.GetString(receiveBuffer, 0, byteLength);
            Debug.Log($"Data received: {data}");
            // Xử lý phản hồi từ server
            if (!string.IsNullOrEmpty(data))
            {
                HandleServerResponse(data);
            }

            // Tiếp tục đọc dữ liệu
            stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, ReceiveCallback, null);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error receiving data: {ex.Message}");
            Disconnect();
        }
    }

    private void HandleServerResponse(string data)
    {
        if (RoomHandler.Instance != null)
        {
            RoomHandler.Instance.HandleServerMessage(data);
        }
        else
        {
            Debug.LogWarning("RoomHandler.Instance is null. Cannot forward server response.");
        }
        // Forward data to RoomHandler or other systems
        RoomHandler.Instance.HandleServerMessage(data);
    }

    public bool IsConnected()
    {
        return socket != null && socket.Connected;
    }

    private void Disconnect()
    {
        socket.Close();
        stream = null;
        socket = null;
        Debug.Log("Disconnected from server.");
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
