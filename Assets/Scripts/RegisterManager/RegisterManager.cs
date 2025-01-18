/*using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RegisterManager : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;
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

    public void Register()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            messageText.text = "Username and password cannot be empty!";
            return;
        }

        if (password != confirmPassword)
        {
            messageText.text = "Passwords do not match!";
            return;
        }

        if (sqlManager != null && sqlManager.CheckUsernameExists(username))
        {
            messageText.text = "Username already exists!";
            return;
        }

        string encryptedPassword = DESHelper.Encrypt(password);
        sqlManager.RegisterUser(username, encryptedPassword);
        messageText.text = "Registration successful!";
    }
}*/