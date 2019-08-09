using StratumSdk.Model.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StratumSdk
{
    public class SdkSecret
    {

        public static bool IsCredentialSecret(Secret secret)
        {
            return secret.Email != null;
        }

        public static bool IsPrivateKeySecret(Secret secret)
        {
            return secret.PrivateKey != null;
        }

        public static bool IsProtectedKeySecret(Secret secret)
        {
            return secret.PublicKey != null;
        }

    }

}
