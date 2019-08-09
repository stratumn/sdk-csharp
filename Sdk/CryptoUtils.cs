using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StratumSdk
{
    public static class CryptoUtils
    {
        private const string Ed25519 = "Ed25519";

        /// <summary>
        /// Encode the signature in base 64 with the begin and end message 
        /// </summary>
        /// <param name="sig"></param>
        /// <returns></returns>
        public static String EncodeSignature(byte[] sig)
        {
            return String.Format("-----BEGIN MESSAGE-----\n{0}\n-----END MESSAGE-----",
                  Convert.ToBase64String(sig));
        }


        /// <summary>
        /// Signg the message and convert it ot base64
        /// </summary>
        /// <param name="key"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string Sign(Ed25519PrivateKeyParameters key, string message)
        {
            ISigner signer = SignerUtilities.GetSigner(Ed25519);

            signer.Init(true, key);
            signer.BlockUpdate(Encoding.UTF8.GetBytes(message), 0, message.Length);
            var signature = signer.GenerateSignature();
            return CryptoUtils.EncodeSignature(signature);
        }


        /// <summary>
        /// Get the private key object from the raw key
        /// </summary>
        /// <param name="pem"></param>
        /// <returns></returns>
        public static Ed25519PrivateKeyParameters DecodeEd25519PrivateKey(string pem)
        {
            pem = pem.Replace("\n", "").Replace("-----BEGIN ED25519 PRIVATE KEY-----", "")
                .Replace("-----END ED25519 PRIVATE KEY-----", "");

            var privateKeyBase64 = Convert.FromBase64String(pem);
            byte[] seed = CopyOfRange(privateKeyBase64, 18, 50);

            var keyParameters = new Ed25519PrivateKeyParameters(seed, 0);

            return keyParameters;

        }

        /// <summary>
        /// Todo :Need to be enhanced to return the correct public key but not used for now
        /// </summary>
        /// <param name="pem"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetPublicKeyFromPrivateKeyEx(string pem, Ed25519PrivateKeyParameters key)
        {
            pem = pem.Replace("\n", "").Replace("-----BEGIN ED25519 PRIVATE KEY-----", "")
            .Replace("-----END ED25519 PRIVATE KEY-----", "");

            var privateKeyBase64 = Convert.FromBase64String(pem);
            var byteArray = new Ed25519PublicKeyParameters(privateKeyBase64, 0).GetEncoded();

            var base64 = Convert.ToBase64String(byteArray);
            base64 = string.Format("-----BEGIN ED25519 PUBLIC KEY-----\n{0}\n-----END ED25519 PUBLIC KEY-----", base64);
            return base64;

        }

        /// <summary>
        /// Todo : modify to use the Ed25519P
        /// </summary>
        /// <param name="message"></param>
        /// <param name="publicKey"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        public static bool VerifySignature(string message, string publicKey, string signature)
        {
            var curve = SecNamedCurves.GetByName("secp256k1");
            var domain = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);

            var publicKeyBytes = Convert.FromBase64String(publicKey);

            var q = curve.Curve.DecodePoint(publicKeyBytes);

            var keyParameters = new
                    Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters(q,
                    domain);

            ISigner signer = SignerUtilities.GetSigner("SHA-256withECDSA");

            signer.Init(false, keyParameters);
            signer.BlockUpdate(Encoding.ASCII.GetBytes(message), 0, message.Length);

            var signatureBytes = Convert.FromBase64String(signature);

            return signer.VerifySignature(signatureBytes);
        }
        /// <summary>
        /// Copy range of array from start index to end index
        /// </summary>
        /// <param name="src"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static byte[] CopyOfRange(byte[] src, int start, int end)
        {
            int len = end - start;
            byte[] dest = new byte[len];
            Array.Copy(src, start, dest, 0, len);
            return dest;
        }





    }
}
