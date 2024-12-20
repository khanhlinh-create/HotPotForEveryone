using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int playerScore;
    public int playerLevel;

    private void Awake()
    {
        if (FindObjectsByType<PlayerManager>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(this.gameObject); // X�a n?u ?� t?n t?i
            return;
        }
        DontDestroyOnLoad(this.gameObject); // Kh�ng x�a khi chuy?n scene
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
