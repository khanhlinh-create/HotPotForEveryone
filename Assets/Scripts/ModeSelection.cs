using UnityEngine;

public class ModeSelection : MonoBehaviour
{
    [SerializeField] private GameObject ServerObjects; // Nhóm GameObject liên quan đến server
    [SerializeField] private GameObject ClientObjects; // Nhóm GameObject liên quan đến client

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // Tắt cả hai nhóm khi bắt đầu
        ServerObjects.SetActive(false);
        ClientObjects.SetActive(false);

        // Hiển thị thông báo
        Debug.Log("Nhấn S để chạy chế độ Server. Nhấn C để chạy chế độ Client");
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Server Mode selected.");
            ServerObjects.SetActive(true); // Kích hoạt server
            ServerManager.Instance.InitializeServer(); // Khởi tạo server
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Client Mode selected.");
            ClientObjects.SetActive(true); // Kích hoạt client
            Client.Instance.InitializeClient(); // Khởi tạo client
        }
    }
}
