using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratumn.Sdk.Model.Client
{
    public class ProtectedKeySecret :Secret
    {

        public string PublicKey { get; set; }
        public string Password { get; set; }

        public ProtectedKeySecret(string publicKey, string password)
        { 
            if (string.ReferenceEquals(publicKey, null))
            {
                throw new System.ArgumentException("publicKey cannot be null in ProtectedKeySecret");
            }
            if (string.ReferenceEquals(password, null))
            {
                throw new System.ArgumentException("password cannot be null in ProtectedKeySecret");
            }
            this.PublicKey = publicKey;
            this.Password = password;
        }



    }

}
