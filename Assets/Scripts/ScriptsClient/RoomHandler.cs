using UnityEngine;
using TMPro; // TextMeshPro for UI handling

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
        if (transform.parent != null)
        {
            transform.parent = null; // Tách khỏi GameObject cha
        }
        DontDestroyOnLoad(this.gameObject); // Không xóa khi chuyển scene
    }

    // Tạo phòng
    public void CreateRoom()
    {
        Debug.Log("Đang tạo phòng...");
        Client.Instance.SendData("CreateRoom"); // Gửi yêu cầu tạo phòng tới server
    }

    // Tham gia phòng
    public void JoinRoom(string inputRoomCode)
    {
        if (string.IsNullOrEmpty(inputRoomCode))
        {
            Debug.LogWarning("Vui lòng nhập mã phòng!"); // Nếu chưa nhập mã phòng
            return;
        }

        Debug.Log($"Đang gửi yêu cầu vào phòng với mã: {inputRoomCode}"); // Gửi yêu cầu vào phòng
        Client.Instance.SendData($"JoinRoom|{inputRoomCode}");
    }

    // Xử lý phản hồi từ server
    public void HandleServerMessage(string message)
    {
        string[] parts = message.Split('|');
        string command = parts[0];
        string data = parts.Length > 1 ? parts[1] : "";

        switch (command)
        {
            case "RoomCreated":
                Debug.Log($"Phòng được tạo thành công! Mã phòng: {data}");
                roomCode = data; // Lưu mã phòng vào biến toàn cục nếu cần
                break;

            case "RoomNotFound":
                Debug.LogWarning("Không thể vào phòng: Mã phòng không tồn tại!");
                break;

            case "RoomFull":
                Debug.LogWarning("Không thể vào phòng: Phòng đã đủ người!");
                break;

            case "JoinedRoom":
                Debug.Log($"Thành công! Bạn đã tham gia phòng có mã: {data}");
                break;

            default:
                Debug.LogError("Lỗi không xác định khi xử lý phản hồi từ server.");
                break;
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
