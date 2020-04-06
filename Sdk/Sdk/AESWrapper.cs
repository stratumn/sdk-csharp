using Stratumn.Sdk;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Stratumn.Sdk
{
    public class AesWrapper
    {
        private SymmetricAlgorithm SecretKey;

        private byte[] IV = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        private int BlockSize = 128;

        public AesWrapper(string secret, int length)
        {
            //Encrypt
            SecretKey = Aes.Create();
            HashAlgorithm hash = MD5.Create();
            SecretKey.BlockSize = BlockSize;
            SecretKey.Key = hash.ComputeHash(Encoding.Unicode.GetBytes(secret));
            SecretKey.IV = IV;
        }

        public AesWrapper(string key): this(key,16)
        { 
        }

        public MemoryStream Encrypt (Stream data)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream =
                   new CryptoStream(memoryStream, SecretKey.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    byte[] buffer = new byte[1048576];
                    int read; 
                    try
                    {
                        while ((read = data.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            cryptoStream.Write(buffer, 0, read);
                        }

                        // Close up
                        data.Close();
                    }
                    catch (Exception ex)
                    {
                        throw new TraceSdkException(ex);
                    }
                    finally
                    { 
                        data.Close();
                    }
                }
                return memoryStream;
            }
        }

        public MemoryStream Decrypt(MemoryStream data)
        {
            return data;
        }
    }
}
