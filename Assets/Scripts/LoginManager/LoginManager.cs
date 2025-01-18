using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text messageText;

    [SerializeField] private SQLManager sqlManager;

    void Start()
    {
        if (sqlManager == null)
        {
            sqlManager = Object.FindFirstObjectByType<SQLManager>();
            if (sqlManager == null)
            {
                Debug.LogError("SQLManager not found in the scene. Please ensure it is added and properly configured.");
            }
        }
    }

    public void Login()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            messageText.text = "Username and password cannot be empty!";
            return;
        }

        string encryptedPassword = DESHelper.Encrypt(password);
        if (sqlManager != null && sqlManager.ValidateUser(username, encryptedPassword))
        {
            messageText.text = "Login successful!";
            // Chuyển sang giao diện khác nếu cần.
        }
        else
        {
            messageText.text = "Invalid username or password!";
        }
    }
}