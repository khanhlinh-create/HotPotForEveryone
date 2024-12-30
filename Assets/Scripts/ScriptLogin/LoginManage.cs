using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    public InputField LoginUsernameInput, LoginPasswordInput;
    public Text LoginErrorText;
    public Button LoginButton;

    void Start()
    {
        LoginButton.onClick.AddListener(LoginUser);
    }

    void LoginUser()
    {
        string username = LoginUsernameInput.text;
        string password = LoginPasswordInput.text;
        byte[] passwordHash = HashPassword(password);

        using (SqlConnection connection = SQLManager.GetConnection())
        {
            SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Username = @Username AND PasswordHash = @PasswordHash", connection);
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@PasswordHash", passwordHash);

            int userExists = (int)command.ExecuteScalar();

            if (userExists > 0)
            {
                Debug.Log("Login successful.");
            }
            else
            {
                LoginErrorText.text = "Invalid username or password!";
            }
        }
    }

    byte[] HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }
}