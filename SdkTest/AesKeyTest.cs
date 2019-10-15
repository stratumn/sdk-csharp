using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Stratumn.Sdk;
using System.IO;
using System.Text;

namespace AesKeyTest
{

    [TestClass]
    public class AesKeyTest
    {


        [TestMethod]
        public void TestEncrypt()
        {
            byte[] message = Encoding.UTF8.GetBytes("coucou, tu veux voir mon message ?");

            AesKey k = new AesKey();
            try
            {
                var data = new MemoryStream(message);


                var ciphertext = k.Encrypt(data);
                var plaintext = k.Decrypt(ciphertext);

                CollectionAssert.Equals(plaintext, message);

            }
            catch(Exception e)
            {
                Assert.Fail("Failed to encrypt data: {0}", e.Message);
            }
        }

        [TestMethod]
        public void TestDecrypt()
        {
            String key = "dXRdc1KYm8DVFFxc0Hq65ZVoZvHAD/PBx0GUgSMmPEw=";
            byte[] ct = Convert.FromBase64String("FfogaZ5Wy4oDfCDqQQUtciiZf/6CsZxrBQr2ZHVswimxB7IwQw9Z8brNocu3O5q1DKYaP4cBmzcPi++1mE4=");
            byte[] message = Encoding.UTF8.GetBytes("coucou, tu veux voir mon message ?");

            AesKey k = new AesKey(key);
            try
            {
                var ciphertext = new MemoryStream(ct);
                var plaintext = k.Decrypt(ciphertext);

                CollectionAssert.Equals(plaintext, message);

            }
            catch (Exception e)
            {
                Assert.Fail("Failed to encrypt data: {0}", e.Message);
            }
        }

        [TestMethod]
        public void TestExport()
        {
            String key = "dXRdc1KYm8DVFFxc0Hq65ZVoZvHAD/PBx0GUgSMmPEw=";
            AesKey k = new AesKey(key);

            Assert.AreEqual(key, k.Export());

        }

    }
}