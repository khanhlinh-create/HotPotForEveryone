using UnityEngine;

public class ChatManager : MonoBehaviour
{
    private void Awake()
    {
        if (FindObjectsByType<ChatManager>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(this.gameObject); // Xóa n?u ?ã t?n t?i
            return;
        }
        if (transform.parent != null)
        {
            transform.parent = null; // Tách khỏi GameObject cha
        }
        DontDestroyOnLoad(this.gameObject); // Không xóa khi chuy?n scene
    }

    public void SendMessageToRoom(string message)
    {
        Client.Instance.SendData($"Chat:{message}");
    }

    public void HandleChatMessage(string message)
    {
        Debug.Log($"Chat received: {message}");
        // Update chat UI
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
