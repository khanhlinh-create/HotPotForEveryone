using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int playerScore;
    public int playerLevel;

    private void Awake()
    {
        if (FindObjectsByType<PlayerManager>(FindObjectsSortMode.None).Length > 1)
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

    public void UpdatePlayerData(string data)
    {
        // Parse data to update score and level
        Debug.Log($"Player data updated: {data}");
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
