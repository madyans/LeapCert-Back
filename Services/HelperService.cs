using System.Security.Cryptography;
using System.Text;

namespace leapcert_back.Helpers;

public class HelperService
{
    public static string HashMd5(string text)
    {
        using MD5 md5Hash = MD5.Create();
        byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(text));
        StringBuilder sBuilder = new StringBuilder();

        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }
        return sBuilder.ToString();
    }
}