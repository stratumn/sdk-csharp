using Org.BouncyCastle.Crypto.Parameters;

namespace Stratumn.Sdk.Model.Sdk
{
    public class SdkConfig
    {
        /// <summary>
        /// The workflow id
        /// </summary>
        private string workflowId;
        /// <summary>
        /// The workflow config id
        /// </summary>
        private string configId;
        /// <summary>
        /// The account id
        /// </summary>
        private string accountId;
        /// <summary>
        /// The group id
        /// </summary>
        private string groupId;

        /// <summary>
        /// The private key used for signing links
        /// </summary>
        private Ed25519PrivateKeyParameters signingPrivateKey;

        public SdkConfig()
        {
        }

        public SdkConfig(string workflowId, string configId, string accountId, string groupId, Ed25519PrivateKeyParameters signingPrivateKey)
        {
            this.workflowId = workflowId;
            this.configId = configId;
            this.accountId = accountId;
            this.groupId = groupId;
            this.signingPrivateKey = signingPrivateKey;
        }

        public virtual string WorkflowId
        {
            get
            {
                return workflowId;
            }
            set
            {
                this.workflowId = value;
            }
        }

        public virtual string ConfigId
        {
            get
            {
                return configId;
            }
            set
            {
                this.configId = value;
            }
        }

        public virtual string AccountId
        {
            get
            {
                return accountId;
            }
            set
            {
                this.accountId = value;
            }
        }

        public virtual string GroupId
        {
            get
            {
                return groupId;
            }
            set
            {
                this.groupId = value;
            }
        }

        public virtual Ed25519PrivateKeyParameters SigningPrivateKey
        {
            get
            {
                return signingPrivateKey;
            }
            set
            {
                this.signingPrivateKey = value;
            }
        }
    }
}
