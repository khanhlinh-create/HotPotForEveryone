using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void QuickPlay()
    {
        GameObject musicObject = GameObject.Find("AudioManager");
        if (musicObject != null)
        {
            Destroy(musicObject);
        }
        SceneManager.LoadSceneAsync("HotPotScene");
    }

    public void Return()
    {
        GameObject musicObject = GameObject.Find("AudioManager");
        if (musicObject != null)
        {
            Destroy(musicObject);
        }
        SceneManager.LoadSceneAsync("MainMenu");
    }
}
