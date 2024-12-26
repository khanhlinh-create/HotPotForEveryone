using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void QuickPlay()
    {
        SceneManager.LoadSceneAsync("HotPotScene");
    }
}
