using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace wallabag.DataModel
{
    public class Authentication
    {
        private static string Username = "wallabag";
        private static string Password = "wallabag";

        private static async Task hashPassword(string clearPassword)
        {
            Password = clearPassword;
            string salt = string.Empty;

            using (HttpClient http = new HttpClient())
            {
                string response = await http.GetStringAsync(string.Format("http://v2.wallabag.org/api/salts/{0}.json", Username));
                JArray result = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<dynamic>(response));
                salt = result[0].ToString();
            }

            string hash = string.Empty;
            try
            {
                string salted = string.Empty;
                salted = clearPassword + "{" + salt + "}";

                byte[] sha = GetHash("SHA512", salted);
                for (int i = 0; i < 5000; i++)
                {
                    List<byte> temp = new List<byte>();
                    temp.AddRange(sha);
                    temp.AddRange(Encoding.UTF8.GetBytes(salted));
                    sha = GetHash("SHA512", temp.ToArray());
                }
                hash = Convert.ToBase64String(sha);
            }
            catch
            {
                //TODO: handle exceptions
            }
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
            string digest = string.Empty;
            try
            {
                StringBuilder s = new StringBuilder();
                s.Append(GenerateNonce());
                s.Append(GetTimestamp());
                s.Append(Password);
                byte[] sha = GetHash("SHA1", Encoding.UTF8.GetBytes(s.ToString()));
                digest = Convert.ToBase64String(sha);
            }
            catch (Exception)
            {
                //TODO: Handle exceptions.
            }
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
            var data = new DataPackage();
            data.SetText(header.ToString());
            Clipboard.SetContent(data);
            return header.ToString();
        }

        /// <summary>
        /// Computes a hash of a specific string.
        /// </summary>
        /// <param name="algorithm">The hash algorithm that should be used.</param>
        /// <param name="s">The string.</param>
        /// <returns>A hash of string s in the specific algorithm.</returns>
        private static byte[] GetHash(string algorithm, string s)
        {
            HashAlgorithmProvider alg = HashAlgorithmProvider.OpenAlgorithm(algorithm);
            IBuffer buff = CryptographicBuffer.ConvertStringToBinary(s, BinaryStringEncoding.Utf8);
            var hashed = alg.HashData(buff);
            byte[] res;
            CryptographicBuffer.CopyToByteArray(buff, out res);
            return res;
        }
        /// <summary>
        /// Computes a hash of a specific byte array.
        /// </summary>
        /// <param name="algorithm">The hash algorithm that should be used.</param>
        /// <param name="b">The byte array.</param>
        /// <returns>A hash of byte array b in the specific algorithm.</returns>
        private static byte[] GetHash(string algorithm, byte[] b)
        {
            HashAlgorithmProvider alg = HashAlgorithmProvider.OpenAlgorithm(algorithm);
            IBuffer buff = CryptographicBuffer.ConvertStringToBinary(Encoding.UTF8.GetString(b, 0, b.Length), BinaryStringEncoding.Utf8);
            var hashed = alg.HashData(buff);
            byte[] res;
            CryptographicBuffer.CopyToByteArray(buff, out res);
            return res;
        }
    }
}
