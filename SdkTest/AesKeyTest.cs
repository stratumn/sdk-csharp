using System;
using Xunit;
using Stratumn.Sdk;
using System.IO;
using System.Text;

namespace AesKeyTest
{
    public class AesKeyTest
    {
        [Fact]
        public void TestEncrypt()
        {
            string message = "coucou, tu veux voir mon message ?";
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            AesKey k = new AesKey();

            var data = new MemoryStream(bytes);
            var ciphertext = k.Encrypt(data);
            var plaintext = k.Decrypt(ciphertext);

            Assert.Equal(
                message,
                Encoding.UTF8.GetString(plaintext.ToArray())
            );
        }

        [Fact]
        public void TestDecrypt()
        {
            String key = "dXRdc1KYm8DVFFxc0Hq65ZVoZvHAD/PBx0GUgSMmPEw=";
            byte[] ct = Convert.FromBase64String("FfogaZ5Wy4oDfCDqQQUtciiZf/6CsZxrBQr2ZHVswimxB7IwQw9Z8brNocu3O5q1DKYaP4cBmzcPi++1mE4=");
            string message = "coucou, tu veux voir mon message ?";
            byte[] bytes = Encoding.UTF8.GetBytes(message);

            AesKey k = new AesKey(key);

            var ciphertext = new MemoryStream(ct);
            var plaintext = k.Decrypt(ciphertext);

            Assert.Equal(
                message,
                Encoding.UTF8.GetString(plaintext.ToArray())
            );
        }

        [Fact]
        public void TestExport()
        {
            String key = "dXRdc1KYm8DVFFxc0Hq65ZVoZvHAD/PBx0GUgSMmPEw=";
            AesKey k = new AesKey(key);
            Assert.Equal(key, k.Export());
        }
    }
}