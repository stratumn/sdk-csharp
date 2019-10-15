using System;
using System.IO;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Stratumn.Sdk
{

    public class AesKey
    {

        private const int SALT_LENGTH = 12;
        private const int TAG_LENGTH = 16;
        private const int KEY_LENGTH = 32;

        private byte[] key;

        private static byte[] generateRanbomBytes(int length)
        {
            SecureRandom s = new SecureRandom();
            var res = new byte[length];
            s.NextBytes(res);
            return res;

        }

        public AesKey() : this(null) { }

        public AesKey(string secret)
        {
            if (secret != null)
            {
                this.key = Convert.FromBase64String(secret);
            } else
            {
                this.key = generateRanbomBytes(KEY_LENGTH);
            }
        }

        public string Export()
        {
            return Convert.ToBase64String(this.key);

        }

        public MemoryStream Encrypt(MemoryStream data)
        {

            byte[] nonce = generateRanbomBytes(SALT_LENGTH);
            GcmBlockCipher cipher = new GcmBlockCipher(new AesEngine());
            AeadParameters parameters = new AeadParameters(new KeyParameter(this.key), TAG_LENGTH * 8, nonce, null);
            cipher.Init(true, parameters);


            var plaintext = data.ToArray();
            byte[] ciphertext = new byte[cipher.GetOutputSize(plaintext.Length)];
            int len = cipher.ProcessBytes(plaintext, 0, plaintext.Length, ciphertext, 0);

            cipher.DoFinal(ciphertext, len);

            // Assemble message
            var combinedStream = new MemoryStream();
            var binaryWriter = new BinaryWriter(combinedStream);

            //Prepend Nonce
            binaryWriter.Write(nonce);
            //Write Cipher Text
            binaryWriter.Write(ciphertext);

            return combinedStream;


        }

        public MemoryStream Decrypt(MemoryStream data)

        {
            if (data.Length > int.MaxValue)
            {
                throw new ArgumentException("Data to decrypt should be smaller than 2GB");
            }

            using (var cipherReader = new BinaryReader(data))
            {
                data.Position = 0;
                var nonce = cipherReader.ReadBytes(SALT_LENGTH);
                
                var cipher = new GcmBlockCipher(new AesEngine());
                var parameters = new AeadParameters(new KeyParameter(this.key), TAG_LENGTH * 8, nonce, null);
                cipher.Init(false, parameters);

                var ciphertext = cipherReader.ReadBytes((int)data.Length - SALT_LENGTH);
                var plaintext = new byte[cipher.GetOutputSize((int)data.Length)];

                var len = cipher.ProcessBytes(ciphertext, 0, ciphertext.Length, plaintext, 0);
                cipher.DoFinal(plaintext, len);

                return new MemoryStream(plaintext);
            }
        }
    }
}
