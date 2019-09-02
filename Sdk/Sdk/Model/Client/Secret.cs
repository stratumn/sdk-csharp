using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratumn.Sdk.Model.Client
{
    public class Secret
    {
 
        public static Secret NewCredentialSecret(string email, string password)
        {
            return new CredentialSecret(email, password);
        }

        public static Secret NewPrivateKeySecret(string privateKey)
        {
            return new PrivateKeySecret(privateKey);
        }

        public static Secret NewProtectedKeySecret(string publicKey, string password)
        {
            return new ProtectedKeySecret(publicKey, password);
        }


        /// <summary>
        /// Helper method to test that an object is of type CredentialSecret </summary>
        /// <param name="secret">
        /// @return </param>
        public static bool IsCredentialSecret(Secret secret)
        {
            return (secret is CredentialSecret);
        }

        /// <summary>
        ///*
        /// Helper method to test that an object is of type PrivateKeySecret </summary>
        /// <param name="secret">
        /// @return </param>
        public static bool IsPrivateKeySecret(Secret secret)
        {
            return secret is PrivateKeySecret;
        }

        /// <summary>
        ///*
        /// Helper method to test that an object is of type ProtectedKeySecret </summary>
        /// <param name="secret">
        /// @return </param>
        public static bool IsProtectedKeySecret(Secret secret)
        {
            return secret is ProtectedKeySecret;
        }




    }
}
