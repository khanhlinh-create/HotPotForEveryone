using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;

public class ClientScript : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private bool isConnected = false;

    private void Start()
    {
        client = new TcpClient("127.0.0.1", 7777);
        stream = client.GetStream();

        if (isConnected) return;
        try
        {
            client = new TcpClient("127.0.0.1", 7777);
            stream = client.GetStream();
            isConnected = true;
            Debug.Log(gameObject.name + "Connected to server.");
        }
        catch (SocketException ex)
        {
            Debug.LogError($"Connection failed: {ex.Message}");
        }
    }

    public void SendData(string message)
    {
        if (stream == null) return;
        byte[] data = Encoding.ASCII.GetBytes(message);
        stream.Write(data, 0, data.Length);
        Debug.Log(gameObject.name + " sent: " + message);
    }

    public void ReceiveData()
    {
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
        Debug.Log(gameObject.name + " received: " + dataReceived);
    }

    void OnApplicationQuit()
    {
        if (client != null)
        {
            client.Close();
            Debug.Log("Client disconnected.");
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
