using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using wallabag.DataModel;

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
    }
}
