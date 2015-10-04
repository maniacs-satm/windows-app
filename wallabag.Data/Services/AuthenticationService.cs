using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using wallabag.Common;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace wallabag.Services
{
    public class AuthenticationService
    {
        private static string Username { get; } = AppSettings.Username;
        private static string Password { get; } = AppSettings.Password;
        private static string Url { get; } = AppSettings.wallabagUrl;

        public static string hashedPassword = string.Empty;

        public static async Task HashPasswordAsync()
        {
            string salt = string.Empty;
            using (HttpClient http = new HttpClient())
            {
                try
                {
                    string response = await http.GetStringAsync($"{Url}/api/salts/{Username}.json");
                    JArray result = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<JArray>(response));
                    salt = result[0].ToString();
                }
                catch { }
            }

            HashPassword(Password, Username, salt);
        }
        public static void HashPassword(string password, string username, string salt)
        {
            string combined = $"{password}{username}{salt}";
            string hash = GetHash(HashAlgorithmNames.Sha1, combined);

            hashedPassword = hash;
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

        private static async Task<string> GenerateDigestAsync()
        {
            return await GenerateDigestAsync(GenerateNonce(), GetTimestamp());
        }
        public static async Task<string> GenerateDigestAsync(string nonce, string timestamp, bool IsTest = false)
        {
            if (!IsTest)
                await HashPasswordAsync();
            string combined = $"{nonce}{timestamp}{hashedPassword}";
            string digest = GetHash(HashAlgorithmNames.Sha1, combined, true);

            return digest;
        }

        public static async Task<string> GetAuthenticationHeaderAsync()
        {
            string nonce = GenerateNonce();
            string timestamp = GetTimestamp();
            return GetHeader(Username, await GenerateDigestAsync(nonce, timestamp), nonce, timestamp);
        }
        public static string GetHeader(string username, string digest, string nonce, string timestamp)
        {
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
