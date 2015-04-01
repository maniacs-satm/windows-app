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
        public static string Username = "wallabag";
        public static string Password = "wallabag";
        public static string hashedPassword { get; set; }

        public static async Task hashPassword()
        {
            string salt = string.Empty;
            using (HttpClient http = new HttpClient())
            {
                string response = await http.GetStringAsync(string.Format("http://v2.wallabag.org/api/salts/{0}.json", Username));
                JArray result = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<dynamic>(response));
                salt = result[0].ToString();
            }

            hashPassword(Password, Username, salt);
        }
        public static void hashPassword(string password, string username, string salt)
        {
            string combined = string.Format("{0}{1}{2}", password, username, salt);
            string hash = GetHash(HashAlgorithmNames.Sha1, combined);

            hashedPassword = hash;
        }

        public static string GetTimestamp()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }
        public static string GenerateNonce()
        {
            IBuffer buff = CryptographicBuffer.GenerateRandom(32);
            return CryptographicBuffer.EncodeToBase64String(buff);
        }

        public static string GenerateDigest()
        {
            return GenerateDigest(GenerateNonce(), GetTimestamp());
        }
        public static string GenerateDigest(string nonce, string timestamp)
        {
            string combined = string.Format("{0}{1}{2}", nonce, timestamp, hashedPassword);
            string digest = GetHash(HashAlgorithmNames.Sha1, combined, true);

            return digest;
        }

        public static async Task<string> GetHeader()
        {
            return await GetHeader(Username, GenerateDigest(), GenerateNonce(), GetTimestamp());
        }
        public static async Task<string> GetHeader(string username, string digest, string nonce, string timestamp)
        {
            await hashPassword();
            StringBuilder header = new StringBuilder();
            header.Append("UsernameToken Username=\"");
            header.Append(username);
            header.Append("\", PasswordDigest=\"");
            header.Append(digest);
            header.Append("\", Nonce=\"");
            header.Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(nonce)));
            header.Append("\", Created=\"");
            header.Append(timestamp);
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
        public static string GetHash(string algorithm, string s, bool Base64Encoded = false)
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
