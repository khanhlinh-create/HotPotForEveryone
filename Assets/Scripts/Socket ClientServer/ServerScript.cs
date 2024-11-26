using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;


public class ServerScript : MonoBehaviour
{
    private TcpListener server;
    private Thread serverThread;
    private List<TcpClient> connectedClients = new List<TcpClient>();

    private void Start()
    {
        serverThread = new Thread(StartServer);
        serverThread.Start();
    }

    private void StartServer()
    {
        server = new TcpListener(IPAddress.Any, 7777); 
        server.Start();
        Debug.Log("Server started. Waiting for clients..");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            if (!connectedClients.Contains(client))
            {
                connectedClients.Add(client);
                Debug.Log("Client connected!");
                Thread clientThread = new Thread(HandleClient);
                clientThread.Start(client);
            }
        }
    }

    private void HandleClient(object clientObj)
    {
        TcpClient client = (TcpClient)clientObj; 
        NetworkStream stream = client.GetStream(); 

        byte[] buffer = new byte[1024];
        int bytesRead;

        try
        {
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Debug.Log("Received from client: " + dataReceived);

                byte[] dataToSend = Encoding.ASCII.GetBytes("Message received: " + dataReceived);
                stream.Write(dataToSend, 0, dataToSend.Length);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Client disconnected. Error: " + e.Message);
        }
        finally
        {
            client.Close(); 
        }
    }

    private void OnApplicationQuit()
    {
        if (server != null)
        {
            server.Stop();
            Debug.Log("Server stopped.");
        }
    }

}
