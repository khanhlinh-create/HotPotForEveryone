using UnityEngine;
using TMPro; // Nếu dùng TextMeshPro
using System.Net.Sockets;
using System.Text;
using System.Collections;

public class RoomManager : MonoBehaviour
{
    public TMP_InputField RoomCodeInputField; // InputField nhập RoomCode
    public GameObject JoinRoomButton; // Button "Join Room"
    public GameObject CreateRoomButton; // Button "Create Room"

    private TcpClient subServerClient;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Kết nối với SubServer (kết nối này đã được thiết lập từ trước)
        subServerClient = ConnectionManager.subServerClient;
    }

    public void CreateRoom()
    {
        StartCoroutine(EnsureConnectionThenCreateRoom());
    }

    private IEnumerator EnsureConnectionThenCreateRoom()
    {
        // Đảm bảo kết nối SubServer trước khi client gửi yêu cầu Create Room đến SubServer 
        while (subServerClient == null || !subServerClient.Connected)
        {
            Debug.Log("Waiting for connection to SubServer...");
            yield return new WaitForSeconds(0.5f);
            UpdateSubServerClient();
        }

        // Khi đã kết nối, gửi yêu cầu tạo phòng
        string message = "CreateRoom|";
        SendMessageToSubServer(message);
        Debug.Log("Requested to create a room.");
    }
    //Hàm quan trọng để kết nối 2 script ConnectionManager và RoomManager 
    private void UpdateSubServerClient()
    {
        if (subServerClient == null && ConnectionManager.subServerClient != null)
        {
            subServerClient = ConnectionManager.subServerClient;
            Debug.Log("SubServerClient updated from ConnectionManager.");
        }
    }
    //Khi client nhập roomcode và nhấn button Join Room 
    public async void JoinRoom()
    {
        string roomCode = RoomCodeInputField.text.Trim();
        if (string.IsNullOrEmpty(roomCode))
        {
            Debug.LogError("Room code is empty. Cannot join room.");
            return;
        }
        ConnectionManager connectionManager = FindFirstObjectByType<ConnectionManager>();

        // Việc 1: Kết nối tới SubServer
        if (ConnectionManager.subServerClient == null || !ConnectionManager.subServerClient.Connected)
        {
            Debug.Log("Connecting to SubServer...");
            //yêu cầu Master Server trả về thông tin SubServer đang quản lý mã phòng đã nhập
            // Nhận tuple (subServerIP, subServerPort)`
            var (subServerIP, subServerPort) = await connectionManager.GetSubServerIP(roomCode);
            if (string.IsNullOrEmpty(subServerIP) || subServerPort == 0)
            {
                Debug.LogError("SubServer not found for the given room code."); /*thông báo khi Master Server không tìm thấy SubServer
                                                                                    tương ứng */
                return;
            }
            // Kết nối tới SubServer
            await connectionManager.ConnectToSubServer(subServerIP, subServerPort);
        }

        // Việc 2: Sau khi kết nối SubServer thành công, gửi yêu cầu tham gia phòng
        if (ConnectionManager.subServerClient != null && ConnectionManager.subServerClient.Connected)
        {
            connectionManager.JoinRoomByCode(roomCode); //client yêu cầu SubServer cho vào phòng 
        }
        else
        {
            Debug.LogError("Failed to connect to SubServer. Cannot join room.");
        }
    }
    //Hàm để gửi thông tin cho SubServer 
    private void SendMessageToSubServer(string message)
    {
        if (subServerClient == null || !subServerClient.Connected)
        {
            Debug.LogError("SubServerClient is not connected. Cannot send message.");
            return;
        }

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

    //Hàm nhận thông tin về phòng được gửi bởi SubServer 
    public void HandleRoomResponse(string response)
    {
        Debug.Log($"RoomManager received response: {response}");
        if (response.StartsWith("RoomCreated"))
        {
            string roomID = response.Split('|')[1];
            Debug.Log($"Room '{roomID}' created successfully.");
            // Hiển thị mã phòng lên UI hoặc chuyển scene
        }
        else if (response.StartsWith("RoomJoined"))
        {
            string roomID = response.Split('|')[1];
            Debug.Log($"Joined room '{roomID}' successfully.");
            // Cập nhật UI hoặc chuyển scene
        }
    }
}
