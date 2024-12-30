using System.Data.SqlClient;
using UnityEngine;
using UnityEngine.UI;
using System.Security.Cryptography;
using System.Text;

public class RegisterManager : MonoBehaviour
{
    public InputField UsernameInput, PasswordInput, ConfirmPasswordInput;
    public Image AvatarImage;
    public Text ErrorText;
    public Button ChooseImageButton, RegisterButton;

    private Texture2D selectedImage;

    void Start()
    {
        RegisterButton.onClick.AddListener(RegisterUser);
        ChooseImageButton.onClick.AddListener(ChooseAvatar);
    }

    void ChooseAvatar()
    {
        string path = UnityEditor.EditorUtility.OpenFilePanel("Select Avatar", "", "png,jpg");
        if (!string.IsNullOrEmpty(path))
        {
            byte[] imageData = System.IO.File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);

            selectedImage = ResizeAndCrop(texture, 256, 256);
            AvatarImage.sprite = Sprite.Create(selectedImage, new Rect(0, 0, 256, 256), Vector2.zero);
        }
    }

    Texture2D ResizeAndCrop(Texture2D original, int width, int height)
    {
        Texture2D cropped = new Texture2D(width, height);
        int size = Mathf.Min(original.width, original.height);
        cropped.SetPixels(original.GetPixels((original.width - size) / 2, (original.height - size) / 2, size, size));
        cropped.Apply();
        return cropped;
    }

    void RegisterUser()
    {
        string username = UsernameInput.text;
        string password = PasswordInput.text;
        string confirmPassword = ConfirmPasswordInput.text;

        if (password != confirmPassword)
        {
            ErrorText.text = "Passwords do not match!";
            return;
        }

        byte[] passwordHash = HashPassword(password);
        byte[] avatarData = selectedImage ? selectedImage.EncodeToPNG() : null;

        using (SqlConnection connection = SQLManager.GetConnection())
        {
            SqlCommand command = new SqlCommand("INSERT INTO Users (Username, PasswordHash, AvatarImage) VALUES (@Username, @PasswordHash, @AvatarImage)", connection);
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@PasswordHash", passwordHash);
            command.Parameters.AddWithValue("@AvatarImage", avatarData);

            try
            {
                command.ExecuteNonQuery();
                Debug.Log("User registered successfully.");
            }
            catch (SqlException ex)
            {
                ErrorText.text = "Error: " + ex.Message;
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
