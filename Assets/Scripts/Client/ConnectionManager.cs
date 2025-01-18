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
    public Button PlayButton;            // Nút Play
    public static TcpClient masterClient; // Kết nối đến MasterServer
    public static TcpClient subServerClient; // Kết nối đến SubServer (sử dụng ở scene 3)

    private string masterServerIP;   // IP của MasterServer
    private string subServerIP;     // Biến toàn cục để lưu IP của SubServer
    private int subServerPort;      // Biến toàn cục để lưu Port của SubServer

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    //Hàm để Client kết nối với Master Server khi client nhập IP Master Server và nhấn button Play (scene 1)
    public async void ConnectToMasterServer()
    {
        masterServerIP = IP_InputField.text.Trim();
        masterClient = new TcpClient();

        try
        {
            await masterClient.ConnectAsync(masterServerIP, 5000);
            Debug.Log($"Connected to Master Server at {masterServerIP}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to connect to Master Server: {ex.Message}");
            ShowErrorToUser("Cannot connect to Master Server. Check IP or try again.");
        }
    }
    //Hàm yêu cầu Master Server chỉ định SubServer khi client nhấn button Create Room 
    public async void RequestSubServer()
    {
        if (masterClient == null || !masterClient.Connected)
        {
            Debug.LogError("Not connected to Master Server. Cannot request SubServer.");
            return;
        }

        try
        {
            NetworkStream stream = masterClient.GetStream();
            byte[] requestData = Encoding.UTF8.GetBytes("RequestSubServer");
            await stream.WriteAsync(requestData, 0, requestData.Length);

            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead > 0)
            {
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                ParseSubServerInfo(response);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to request SubServer: {ex.Message}");
        }
    }
    /*Yêu cầu Master Server trả về thông tin của SubServer đang quản lý mã phòng khi client nhập mã phòng
    vào roomcode_InputField và nhấn button Join Room*/
    public async Task<(string, int)> GetSubServerIP(string roomCode)
    {
        if (masterClient == null || !masterClient.Connected)
        {
            Debug.LogError("Not connected to MasterServer. Cannot request SubServer IP.");
            return (null, 0); // Trả về giá trị mặc định khi không kết nối
        }

        try
        {
            NetworkStream stream = masterClient.GetStream();
            string requestMessage = $"GetSubServer|{roomCode}";
            byte[] requestData = Encoding.UTF8.GetBytes(requestMessage);
            await stream.WriteAsync(requestData, 0, requestData.Length);

            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            if (response.StartsWith("SubServerIP:"))
            {
                string[] parts = response.Split('|');
                string subServerIP = parts[0].Replace("SubServerIP:", "").Trim();
                int subServerPort = int.Parse(parts[1].Replace("Port:", "").Trim());
                Debug.Log($"Received SubServer IP for room {roomCode}: {subServerIP}:{subServerPort}");
                return (subServerIP, subServerPort);
            }
            else
            {
                Debug.LogError("Failed to get SubServer IP. Room may not exist.");
                return (null, 0); // Trả về giá trị mặc định nếu không tìm thấy phòng
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error getting SubServer IP: {ex.Message}");
            return (null, 0); // Trả về giá trị mặc định khi gặp lỗi
        }
    }


    //Hàm lấy thông tin (IP, Port) của SubServer để kết nối 
    private void ParseSubServerInfo(string serverInfo)
    {
        string[] info = serverInfo.Split('|');
        // Gán giá trị cho các biến toàn cục
        foreach (string entry in info)
        {
            if (entry.StartsWith("ServerIP:"))
                subServerIP = entry.Replace("ServerIP:", "").Trim();
            if (entry.StartsWith("ServerPort:"))
                subServerPort = int.Parse(entry.Replace("ServerPort:", "").Trim());
        }

        Debug.Log($"Received SubServer Info - IP: {subServerIP}, Port: {subServerPort}");
        ConnectToSubServer(subServerIP, subServerPort);
    }
    //Hàm kết nối với SubServer 
    public async Task ConnectToSubServer(string subServerIP, int subServerPort)
    {
        try
        {
            subServerClient = new TcpClient();
            await subServerClient.ConnectAsync(subServerIP, subServerPort);

            Debug.Log($"Connected to SubServer at {subServerIP}:{subServerPort}");
            await StartReceivingData();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to connect to SubServer: {ex.Message}");
            ReconnectToSubServer();
        }
    }
    /*dùng sau khi client nhập roomcode và nhấn button Join Room đã
     kết nối với SubServer quản lý phòng thành công, yêu cầu SubServer cho mình vào phòng*/
    public async void JoinRoomByCode(string roomCode)
    {
        Debug.Log($"Attempting to join room with code: {roomCode}");

        if (subServerClient == null || !subServerClient.Connected)
        {
            Debug.LogError("Not connected to SubServer. Cannot join room.");
            return;
        }

        try
        {
            NetworkStream stream = subServerClient.GetStream();
            string joinMessage = $"JoinRoom|{roomCode}";
            byte[] data = Encoding.UTF8.GetBytes(joinMessage);
            await stream.WriteAsync(data, 0, data.Length);

            Debug.Log($"Sent join room request: {joinMessage}");

            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            if (response.StartsWith("JoinSuccess"))
            {
                Debug.Log($"Successfully joined room {roomCode}");
                // TODO: Chuyển sang màn hình phòng chơi
            }
            else if (response.StartsWith("JoinFail"))
            {
                Debug.LogError("Failed to join room. Room not found.");
                // TODO: Hiển thị thông báo lỗi lên UI
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error joining room: {ex.Message}");
        }
    }

    //Nhận thông tin từ SubServer 
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
                HandleReceivedData(receivedData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error receiving data: {ex.Message}");
                break;
            }
        }
    }
    //Nhận dữ liệu liên quan đến phòng
    private void HandleReceivedData(string data)
    {
        // Phân loại dữ liệu và chuyển tới các script khác
        if (data.StartsWith("Room"))
        {
            FindFirstObjectByType<RoomManager>()?.HandleRoomResponse(data);
        }
        else
        {
            UpdateGameState(data);
        }
    }
    //Hàm để cập nhập GameState khi nhận các thay đổi từ SubServer 
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
    //gửi thay đổi cho SubServer 
    public void SendGameStateUpdate(string gameState)
    {
        if (subServerClient != null)
        {
            NetworkStream stream = subServerClient.GetStream();
            byte[] data = Encoding.UTF8.GetBytes(gameState);
            stream.Write(data, 0, data.Length);
        }
    }

    //Hàm để cập nhập đĩa ăn 
    private void UpdateItemUI(string itemName, string state)
    {
        // Tìm đối tượng UI dựa trên tên item và thay đổi trạng thái
        Debug.Log($"Updating UI for {itemName}: {state}");
    }
    //Hàm để cập nhập nồi lẩu 
    private void UpdateHotPotState(string state)
    {
        // Thay đổi trạng thái nồi lẩu 
        Debug.Log($"Updating HotPot state: {state}");
    }
    //Hàm để kết nối lại với SubServer khi kết nối gián đoạn 
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