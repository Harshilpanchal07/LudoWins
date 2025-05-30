using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

public static class UniqueId
{
    private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string GenerateShortID(string playerId, int length = 6)
    {
        using (var sha256 = SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(playerId));
            return new string(Enumerable.Range(0, length).Select(i => chars[hash[i] % chars.Length]).ToArray());
        }
    }
}
