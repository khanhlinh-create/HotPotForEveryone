using UnityEngine;

public class HotPotManager : MonoBehaviour
{
    public static HotPotManager Instance;

    private void Awake()
    {
        if (FindObjectsByType<HotPotManager>(FindObjectsSortMode.None).Length > 1)
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

    public void AddTopping(string topping)
    {
        Client.Instance.SendData($"AddTopping:{topping}");
    }

    public void RemoveTopping(string topping)
    {
        Client.Instance.SendData($"RemoveTopping:{topping}");
    }

    public void HandleHotPotUpdate(string data)
    {
        Debug.Log($"HotPot updated: {data}");
        // Update hotpot visuals
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
