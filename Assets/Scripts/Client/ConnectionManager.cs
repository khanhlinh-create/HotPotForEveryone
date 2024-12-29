using UnityEngine;
using UnityEngine.UI;
using TMPro; // Namespace dành cho TextMeshPro
using System.Net.Sockets;
using System.Text;

public class ConnectionManager : MonoBehaviour
{
    public TMP_InputField IP_InputField;

    private string subServerIP;
    private int subServerPort;
    public static TcpClient subServerClient; // Kết nối với SubServer (static)

    public void ConnectToMasterServer()
    {
        string masterServerIP = IP_InputField.text; // Lấy IP từ TMP_InputField.
        TcpClient masterClient = new TcpClient();
        try
        {
            masterClient.Connect(masterServerIP, 8080);

            NetworkStream stream = masterClient.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            string serverInfo = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            ParseServerInfo(serverInfo);

            Debug.Log($"Connected to Master Server: {masterServerIP}");

            masterClient.Close();

            // Kết nối tới SubServer
            ConnectToSubServer();
        }
        catch (SocketException ex)
        {
            Debug.LogError($"Failed to connect to Master Server: {ex.Message}");
        }
    }

    private void ParseServerInfo(string serverInfo)
    {
        string[] info = serverInfo.Split('|');
        foreach (string entry in info)
        {
            if (entry.StartsWith("ServerIP:"))
                subServerIP = entry.Replace("ServerIP:", "");
            if (entry.StartsWith("ServerPort:"))
                subServerPort = int.Parse(entry.Replace("ServerPort:", ""));
        }
    }

    private void ConnectToSubServer()
    {
        try
        {
            subServerClient = new TcpClient(); // Gán vào biến static
            subServerClient.Connect(subServerIP, subServerPort);
            Debug.Log($"Connected to SubServer at {subServerIP}:{subServerPort}");
        }
        catch (SocketException ex)
        {
            Debug.LogError($"Failed to connect to SubServer: {ex.Message}");
        }
    }
}

