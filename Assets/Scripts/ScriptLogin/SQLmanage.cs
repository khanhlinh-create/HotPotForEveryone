using System.Data.SqlClient;
using UnityEngine;

public class SQLManager : MonoBehaviour
{
    private static string connectionString = "Server=LAPTOP-8TJ5C168;Database=laulogin;Trusted_Connection=True;";

    public static SqlConnection GetConnection()
    {
        SqlConnection connection = new SqlConnection(connectionString);
        try
        {
            connection.Open();
            Debug.Log("Database connected.");
        }
        catch (SqlException ex)
        {
            Debug.LogError("SQL Connection Error: " + ex.Message);
        }
        return connection;
    }
}

