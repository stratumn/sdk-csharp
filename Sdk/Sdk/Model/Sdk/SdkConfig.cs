using System.Linq;
using System.Collections.Generic;
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
        /// The group label
        /// </summary>
        private string groupLabel;

        /// <summary>
        /// Map label to group id
        /// </summary>
        private Dictionary<string, string> groupLabelToIdMap;

        /// <summary>
        /// The private key used for signing links
        /// </summary>
        private Ed25519PrivateKeyParameters signingPrivateKey;

        public SdkConfig()
        {
        }

        public SdkConfig(string workflowId, string configId, string accountId,
        Dictionary<string, string> groupLabelToIdMap, Ed25519PrivateKeyParameters signingPrivateKey)
        {
            this.workflowId = workflowId;
            this.configId = configId;
            this.accountId = accountId;
            this.groupLabelToIdMap = groupLabelToIdMap;
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

        public string GetGroupId()
        {
            return this.GetGroupIdByLabel(null);
        }

        public string GetGroupId(string groupLabel)
        {
            return this.GetGroupIdByLabel(groupLabel);
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

        public virtual string GroupLabel
        {
            get
            {
                return groupLabel;
            }
            set
            {
                this.groupLabel = value;
            }
        }

        private string GetGroupIdByLabel(string groupLabelParam)
        {
            string resultGroupId = null;
            if (null != groupLabelToIdMap && 0 < groupLabelToIdMap.Count)
            {
                if (null == groupLabelParam)
                {
                    if (groupLabelToIdMap.Count == 1)
                    {
                        // return the id of the only element
                        resultGroupId = groupLabelToIdMap[groupLabelToIdMap.Keys.ToArray()[0]];
                    }
                    else if (groupLabelToIdMap.Count > 1)
                    {
                        // Last check if groupId has been set manually
                        if (null != this.GroupLabel && null != groupLabelToIdMap[this.GroupLabel])
                        {
                            resultGroupId = groupLabelToIdMap[this.GroupLabel];
                        }
                        else
                        {
                            throw new TraceSdkException(
                                    "Multiple groups to select from, please specify the group label you wish to perform the action with.");
                        }
                    }
                }
                else
                {
                    resultGroupId = groupLabelToIdMap[groupLabelParam];
                }
            }

            if (null == resultGroupId)
            {
                throw new TraceSdkException(
                        "No group to select from. At least one group is required to perform an action.");
            }

            return resultGroupId;
        }
    }
}
