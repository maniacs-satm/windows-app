using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using wallabag.DataModel;
using System.Threading.Tasks;

namespace wallabag.Tests
{
    [TestClass]
    public class AuthenticationTests
    {
        [TestMethod]
        [DataRow("wallabag", "9dc07af7d1d88c183c6ae42c903650ab19ada2bb")]
        [DataRow("is", "b47f363e2b430c0647f14deea3eced9b0ef300ce")]
        [DataRow("awesome", "03d67c263c27a453ef65b29e30334727333ccbcd")]
        public void ComputeSHA1Hashes(string s, string expectedResult)
        {
            string actualResult = Authentication.GetHash("SHA1", s);
            Assert.AreEqual(expectedResult, actualResult);
        }

        [TestMethod]
        [DataRow("d6f4d12d3eed0304", "2015-04-01T15:36:12Z", "UsernameToken Username=\"wallabag\", PasswordDigest=\"Op45hP3eWbXDgVSjzEVq0NGJbwE=\", Nonce=\"ZDZmNGQxMmQzZWVkMDMwNA==\", Created=\"2015-04-01T15:36:12Z\"")]
        [DataRow("2f5cffd012ce587e", "2015-04-01T15:37:24Z", "UsernameToken Username=\"wallabag\", PasswordDigest=\"EIb58IKdGp+sDHn7aVYsHw9eXng=\", Nonce=\"MmY1Y2ZmZDAxMmNlNTg3ZQ==\", Created=\"2015-04-01T15:37:24Z\"")]
        [DataRow("1de4b902055e63e7", "2015-04-01T15:38:19Z", "UsernameToken Username=\"wallabag\", PasswordDigest=\"CZ0oPxhv54elAqrwtriYluYBnyQ=\", Nonce=\"MWRlNGI5MDIwNTVlNjNlNw==\", Created=\"2015-04-01T15:38:19Z\"")]
        public async Task GenerateWSSEHeader(string nonce, string timestamp, string expectedHeader)
        {
            Authentication.hashedPassword = "02b92862ab03de17e217ee5fa53fbf52eed039ca"; // 'wallabag' hashed and salted
            string digest = Authentication.GenerateDigest(nonce, timestamp);
            string header = await Authentication.GetHeader("wallabag", digest, nonce, timestamp);
            Assert.AreEqual(expectedHeader, header);
        }

        [TestMethod]
        public void PasswordHashing()
        {
            string user = "wallabag";
            string password = "wallabag";
            string salt = "a7bdce59b6077a014d22c6f749e681f7";
            string expected = "49e5b5e8469bc313f78df0640a61d374dea8e4d8";

            Authentication.hashPassword(password, user, salt);
            Assert.AreEqual(expected, Authentication.hashedPassword);
        }
    }
}
