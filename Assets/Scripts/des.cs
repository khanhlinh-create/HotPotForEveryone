using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class DESHelper
{
    private static readonly string SecretKey = "MySecr3t"; // Khóa bí mật (8 ký tự).

    public static string Encrypt(string plainText)
    {
        using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
        {
            byte[] key = Encoding.UTF8.GetBytes(SecretKey);
            byte[] iv = Encoding.UTF8.GetBytes(SecretKey);
            byte[] data = Encoding.UTF8.GetBytes(plainText);

            des.Key = key;
            des.IV = iv;

            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(data, 0, data.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    public static string Decrypt(string encryptedText)
    {
        using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
        {
            byte[] key = Encoding.UTF8.GetBytes(SecretKey);
            byte[] iv = Encoding.UTF8.GetBytes(SecretKey);
            byte[] data = Convert.FromBase64String(encryptedText);

            des.Key = key;
            des.IV = iv;

            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(data, 0, data.Length);
                cs.FlushFinalBlock();
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }
}
