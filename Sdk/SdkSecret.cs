using StratumSdk.Model.Clients;

namespace stratumn.sdk
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
