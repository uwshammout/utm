using System.Security.Cryptography;
using System.Text;

namespace CronBlocks.Helpers.Extensions;

public static class HashExtensions
{
    public static string Hash(this string text)
    {
        SHA512 sha512 = SHA512.Create();
        byte[] hash = sha512.ComputeHash(UTF32Encoding.UTF8.GetBytes(text));
        return Convert.ToBase64String(hash);
    }
}
