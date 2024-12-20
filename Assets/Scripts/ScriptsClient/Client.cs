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

    public string serverIP = "127.0.0.1";
    public int serverPort = 7777;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void ConnectToServer()
    {
        socket = new TcpClient();
        try
        {
            socket.Connect(serverIP, serverPort);
            stream = socket.GetStream();
            receiveBuffer = new byte[4096];
            Debug.Log("Connected to server.");
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
                Disconnect();
                return;
            }

            string data = Encoding.ASCII.GetString(receiveBuffer, 0, byteLength);
            Debug.Log($"Data received: {data}");
            HandleServerResponse(data);

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
        // Forward data to RoomHandler or other systems
        RoomHandler.Instance.HandleServerMessage(data);
    }

    private void Disconnect()
    {
        socket.Close();
        stream = null;
        socket = null;
        Debug.Log("Disconnected from server.");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
