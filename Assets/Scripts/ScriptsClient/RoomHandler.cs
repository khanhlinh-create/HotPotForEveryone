using UnityEngine;

public class RoomHandler : MonoBehaviour
{
    public static RoomHandler Instance;

    public string roomCode; // Current room code

    private void Awake()
    {
        // Tìm tất cả các RoomHandler mà không cần sắp xếp (tăng hiệu năng)
        if (FindObjectsByType<RoomHandler>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(this.gameObject); // Xóa nếu đã tồn tại
            return;
        }
        DontDestroyOnLoad(this.gameObject); // Không xóa khi chuyển scene
    }

    public void CreateRoom()
    {
        Client.Instance.SendData("CreateRoom");
    }

    public void JoinRoom(string code)
    {
        roomCode = code;
        Client.Instance.SendData($"JoinRoom:{code}");
    }

    public void HandleServerMessage(string message)
    {
        if (message.StartsWith("RoomCreated"))
        {
            roomCode = message.Split(':')[1];
            Debug.Log($"Room created with code: {roomCode}");
        }
        else if (message.StartsWith("JoinedRoom"))
        {
            Debug.Log($"Joined room: {roomCode}");
        }
        else if (message.StartsWith("RoomUpdate"))
        {
            Debug.Log("Room updated with data.");
            // Parse room data and update UI
        }
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
