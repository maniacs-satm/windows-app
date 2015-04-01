using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace wallabag.DataModel
{
    public class Authentication
    {
        private static string Username = "wallabag";
        private static string Password = "wallabag";

        private static async Task hashPassword(string cleanPassword)
        {
            string salt = string.Empty;
            using (HttpClient http = new HttpClient())
            {
                string response = await http.GetStringAsync(string.Format("http://v2.wallabag.org/api/salts/{0}.json", Username));
                JArray result = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<dynamic>(response));
                salt = result[0].ToString();
            }

            string combined = string.Format("{0}{1}{2}", cleanPassword, Username, salt);
            string hash = GetHash(HashAlgorithmNames.Sha1, combined);

            Password = hash;
        }

        private static string GetTimestamp()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }
        private static string GenerateNonce()
        {
            IBuffer buff = CryptographicBuffer.GenerateRandom(32);
            return CryptographicBuffer.EncodeToBase64String(buff);
        }
        private static string GenerateDigest()
        {
            string combined = string.Format("{0}{1}{2}", GenerateNonce(), GetTimestamp(), Password);
            string digest = GetHash(HashAlgorithmNames.Sha1, combined, true);

            return digest;
        }

        public static async Task<string> GetHeader()
        {
            await hashPassword("wallabag");
            StringBuilder header = new StringBuilder();
            header.Append("UsernameToken Username=\"");
            header.Append(Username);
            header.Append("\", PasswordDigest=\"");
            header.Append(GenerateDigest());
            header.Append("\", Nonce=\"");
            header.Append(GenerateNonce());
            header.Append("\", Created=\"");
            header.Append(GetTimestamp());
            header.Append("\"");
            return header.ToString();
        }

        /// <summary>
        /// Computes a hash of a specific string.
        /// </summary>
        /// <param name="algorithm">The hash algorithm that should be used.</param>
        /// <param name="s">The string.</param>
        /// <param name="Base64Encoded">A property if the string should be base64 encoded.</param>
        /// <returns>A hash of string s in the specific algorithm.</returns>
        private static string GetHash(string algorithm, string s, bool Base64Encoded = false)
        {
            HashAlgorithmProvider alg = HashAlgorithmProvider.OpenAlgorithm(algorithm);
            IBuffer buff = CryptographicBuffer.ConvertStringToBinary(s, BinaryStringEncoding.Utf8);
            var hashed = alg.HashData(buff);
            if (Base64Encoded)
                return CryptographicBuffer.EncodeToBase64String(hashed);
            else
                return CryptographicBuffer.EncodeToHexString(hashed);
        }
    }
}
