using UnityEngine;
using UnityEngine.UI;
using TMPro; // Thêm thư viện TextMeshPro

public class ConnectManager : MonoBehaviour
{
    public TMP_InputField ipInputField; // Dùng TMP_InputField thay vì InputField
    public Button playButton; // Tham chiếu đến nút "Chơi"
    public Client clientScript; // Tham chiếu đến script Client

    void Start()
    {
        // Gắn sự kiện click vào nút Play
        playButton.onClick.AddListener(() => ConnectToServer(ipInputField.text));
    }

    // Hàm để kết nối tới server
    public void ConnectToServer(string ipAddress)
    {
        if (clientScript != null)
        {
            clientScript.Connect(ipAddress); // Gọi hàm Connect trong script Client
        }
        else
        {
            Debug.LogError("Client script is not assigned!");
        }
    }
}
