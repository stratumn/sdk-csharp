namespace StratumSdk.Model.Clients
{
    public class PrivateKeySecret
    {
        public string PrivateKey { get; set; }

        public PrivateKeySecret(string privateKey)
        {
            if (string.ReferenceEquals(privateKey, null))
            {
                throw new System.ArgumentException("privateKey cannot be null in PrivateKeySecret");
            }
            this.PrivateKey = privateKey;
        }


    }

}
