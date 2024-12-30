using UnityEngine;

public class ModeSelectionManager : MonoBehaviour
{
    public GameObject masterServerManager; // Gắn Prefab hoặc GameObject chứa script MasterServerManager
    public GameObject subServerManager;   // Gắn Prefab hoặc GameObject chứa script SubServerManager

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Đảm bảo tất cả các chế độ tắt ban đầu
        if (masterServerManager) masterServerManager.SetActive(false);
        if (subServerManager) subServerManager.SetActive(false);
    }

    // Gọi khi nhấn button MasterServer
    public void ActivateMasterServer()
    {
        if (masterServerManager)
        {
            masterServerManager.SetActive(true); // Bật MasterServerManager
            Debug.Log("MasterServer mode activated.");
        }
    }

    // Gọi khi nhấn button SubServer
    public void ActivateSubServer()
    {
        if (subServerManager)
        {
            subServerManager.SetActive(true); // Bật SubServerManager
            Debug.Log("SubServer mode activated.");
        }
    }
}
