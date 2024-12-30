using UnityEngine;
using UnityEngine.UI;
using TMPro; // Namespace dành cho TextMeshPro
using System.Net.Sockets;
using System.Text;
using System;
using System.Threading.Tasks;

public class ConnectionManager : MonoBehaviour
{
    public TMP_InputField IP_InputField;

    private string subServerIP;
    private int subServerPort;
    public static TcpClient subServerClient; // Kết nối với SubServer (static)

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

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
            ShowErrorToUser("Failed to connect to Master Server. Please try again.");
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

    public async void ConnectToSubServer()
    {
        try
        {
            subServerClient = new TcpClient(); // Gán vào biến static
            subServerClient.Connect(subServerIP, subServerPort);
            Debug.Log($"Connected to SubServer at {subServerIP}:{subServerPort}");
            await StartReceivingData();
        }
        catch (SocketException ex)
        {
            Debug.LogError($"Failed to connect to SubServer: {ex.Message}");
            ReconnectToSubServer();
        }
    }

    private async Task StartReceivingData()
    {
        NetworkStream stream = subServerClient.GetStream();
        byte[] buffer = new byte[1024];

        while (true)
        {
            try
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    Debug.LogWarning("Disconnected from SubServer.");
                    break;
                }

                string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Debug.Log($"Received from SubServer: {receivedData}");

                UpdateGameState(receivedData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error receiving data: {ex.Message}");
                break;
            }
        }
    }

    private void UpdateGameState(string data)
    {
        string[] entries = data.Split('|');
        foreach (string entry in entries)
        {
            string[] keyValue = entry.Split(':');
            if (keyValue.Length == 2)
            {
                string key = keyValue[0];
                string value = keyValue[1];
                Debug.Log($"GameState Updated: {key} -> {value}");

                if (key.StartsWith("Item"))
                {
                    UpdateItemUI(key, value); // Cập nhật trạng thái UI của đĩa ăn
                }
                else if (key == "HotPot")
                {
                    UpdateHotPotState(value); // Cập nhật trạng thái nồi lẩu
                }
            }
        }
    }

    public void SendGameStateUpdate(string gameState)
    {
        if (subServerClient != null)
        {
            NetworkStream stream = subServerClient.GetStream();
            byte[] data = Encoding.UTF8.GetBytes(gameState);
            stream.Write(data, 0, data.Length);
        }
    }


    private void UpdateItemUI(string itemName, string state)
    {
        // Tìm đối tượng UI dựa trên tên item và thay đổi trạng thái
        Debug.Log($"Updating UI for {itemName}: {state}");
    }

    private void UpdateHotPotState(string state)
    {
        // Thay đổi trạng thái nồi lẩu (VD: Đang sôi, nguội, chín)
        Debug.Log($"Updating HotPot state: {state}");
    }

    private async void ReconnectToSubServer()
    {
        while (!subServerClient.Connected)
        {
            Debug.Log("Attempting to reconnect...");
            try
            {
                subServerClient.Connect(subServerIP, subServerPort);
                Debug.Log("Reconnected to SubServer.");
                await StartReceivingData();
                break;
            }
            catch
            {
                await Task.Delay(5000); // Thử lại sau 5 giây.
            }
        }
    }

    private void ShowErrorToUser(string message)
    {
        // TODO: Hiển thị thông báo lỗi lên UI.
        Debug.LogError(message);
    }

    private void OnApplicationQuit()
    {
        if (subServerClient != null)
        {
            subServerClient.Close();
            Debug.Log("SubServer connection closed.");
        }
    }
}
