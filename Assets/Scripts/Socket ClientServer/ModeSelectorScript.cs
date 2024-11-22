using UnityEngine;

public class ModeSelectorScript : MonoBehaviour
{
    public GameObject serverManager; 
    public GameObject clientManager;
    void Start()
    {
        serverManager.SetActive(false);
        clientManager.SetActive(false);

        Debug.Log("Press 'S' to start as Server, 'C' to start as Client.");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            serverManager.SetActive(true);
            Debug.Log("Starting as Server...");
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            clientManager.SetActive(true);
            Debug.Log("ClientManager activated!");
            Debug.Log("Starting as Client...");
            this.gameObject.SetActive(false);
        }
    }
}
