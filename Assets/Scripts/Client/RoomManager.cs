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
        // Đảm bảo kết nối SubServer
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

    private void UpdateSubServerClient()
    {
        if (subServerClient == null && ConnectionManager.subServerClient != null)
        {
            subServerClient = ConnectionManager.subServerClient;
            Debug.Log("SubServerClient updated from ConnectionManager.");
        }
    }

    public void JoinRoom()
    {
        string roomCode = RoomCodeInputField.text.Trim();
        if (string.IsNullOrEmpty(roomCode))
        {
            Debug.LogError("Room code is empty. Cannot join room.");
            // TODO: Hiển thị thông báo lỗi lên UI
            return;
        }

        Debug.Log($"Requested to join room: {roomCode}");
        ConnectionManager connectionManager = FindObjectsByType<ConnectionManager>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)[0];
        if (connectionManager == null)
        {
            Debug.LogError("No ConnectionManager found in the scene.");
            return;
        }
        connectionManager.JoinRoomByCode(roomCode);    
    }

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


    public void HandleRoomResponse(string response)
    {
        Debug.Log($"RoomManager received response: {response}");
        if (response.StartsWith("RoomCreated"))
        {
            string roomID = response.Split('|')[1];
            Debug.Log($"Room '{roomID}' created successfully.");
            // Hiển thị mã phòng lên UI hoặc chuyển scene
            //RoomCodeInputField.text = roomID; // VD: Hiển thị mã phòng ở InputField
        }
        else if (response.StartsWith("RoomJoined"))
        {
            string roomID = response.Split('|')[1];
            Debug.Log($"Joined room '{roomID}' successfully.");
            // Cập nhật UI hoặc chuyển scene
        }
    }
}
