using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace BenchmarkEventHub;

public static class SasTokenHelper
{
    public static string GenerateSasToken(string resourceUri, string keyName, string key)
    {
        var sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
        var expiry = Convert.ToString((int)sinceEpoch.TotalSeconds + 3600); // Token valid for 1 hour

        var stringToSign = HttpUtility.UrlEncode(resourceUri) + "\n" + expiry;
        var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));

        var sasToken = String.Format(CultureInfo.InvariantCulture,
            "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}",
            HttpUtility.UrlEncode(resourceUri), HttpUtility.UrlEncode(signature), expiry, keyName);

        return sasToken;
    }  
}