using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// The user id
        /// </summary>
        private string userId;
        /// <summary>
        /// The account id
        /// </summary>
        private string accountId;
        /// <summary>
        /// The group id
        /// </summary>
        private string groupId;

        /// <summary>
        /// The owner id
        /// </summary>
        private string ownerId;
        /// <summary>
        /// The private key used for signing links
        /// </summary>
        private Ed25519PrivateKeyParameters signingPrivateKey;

        public SdkConfig()
        {
        }

        public SdkConfig(string workflowId, string userId, string accountId, string groupId, string ownerId, Ed25519PrivateKeyParameters signingPrivateKey)
        {
            this.workflowId = workflowId;
            this.userId = userId;
            this.accountId = accountId;
            this.groupId = groupId;
            this.ownerId = ownerId;
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


        public virtual string UserId
        {
            get
            {
                return userId;
            }
            set
            {
                this.userId = value;
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


        public virtual string OwnerId
        {
            get
            {
                return ownerId;
            }
            set
            {
                this.ownerId = value;
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
