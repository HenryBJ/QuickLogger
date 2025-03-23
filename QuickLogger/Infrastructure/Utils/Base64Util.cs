using System.Text;

namespace QuickLogger.Infrastructure.Utils;

public static class Base64Util
{
    public static bool TryFromBase64String(string base64, out string plain)
    {
        if (string.IsNullOrEmpty(base64) || base64.Length % 4 != 0)
        {
            plain = "";
            return false;
        }
            

        try
        {
            plain = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
            return true;
        }
        catch (FormatException)
        {
            plain = "";
            return false;
        }
    }
}
