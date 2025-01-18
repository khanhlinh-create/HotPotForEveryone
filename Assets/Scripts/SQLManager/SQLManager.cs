/*using UnityEngine;
using System.Data.SqlClient;

public class SQLManager : MonoBehaviour
{
    private string connectionString = "Server=LAPTOP-8TJ5C168; Database=QLNGUOIDUNG; Trusted_Connection=True";

    public bool CheckUsernameExists(string username)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            string query = "SELECT COUNT(*) FROM Users WHERE Username = @username";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@username", username);
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }
    }

    public void RegisterUser(string username, string encryptedPassword)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            string query = "INSERT INTO Users (Username, PasswordHash) VALUES (@username, @password)";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", encryptedPassword);
                cmd.ExecuteNonQuery();
            }
        }
    }

    public bool ValidateUser(string username, string encryptedPassword)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            string query = "SELECT COUNT(*) FROM Users WHERE Username = @username AND PasswordHash = @password";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", encryptedPassword);
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }
    }
}*/
