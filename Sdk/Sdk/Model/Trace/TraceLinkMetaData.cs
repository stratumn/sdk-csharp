using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratumn.Sdk.Model.Trace
{

    /// <summary>
    /// The link metadata
    /// </summary>
    public class TraceLinkMetaData
    {

        [JsonProperty(PropertyName = "ownerId")]
        public string OwnerId { get; set; }

        [JsonProperty(PropertyName = "configId")]
        public string ConfigId { get; set; }

        [JsonProperty(PropertyName = "groupId")]
        public string GroupId { get; set; }

        [JsonProperty(PropertyName = "formId")]
        public string FormId { get; set; }

        [JsonProperty(PropertyName = "lastFormId")]
        public string LastFormId { get; set; }

        [JsonProperty(PropertyName = "createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty(PropertyName = "createdById")]
        public string CreatedById { get; set; }

        [JsonProperty(PropertyName = "set")]
        public string Set { get; set; }

        [JsonProperty(PropertyName = "inputs")]
        public string[] Inputs { get; set; }

        public TraceLinkMetaData() : base()
        {
        }

        internal TraceLinkMetaData(string ownerId, string configId, string groupId, string formId, string lastFormId, DateTime createdAt, string createdById, string[] inputs)
        {
            if (string.ReferenceEquals(ownerId, null))
            {
                throw new System.ArgumentException("ownerId cannot be null");
            }
            if (string.ReferenceEquals(configId, null))
            {
                throw new System.ArgumentException("configId cannot be null");
            }
            if (string.ReferenceEquals(groupId, null))
            {
                throw new System.ArgumentException("groupId cannot be null");
            }
            if (string.ReferenceEquals(formId, null))
            {
                throw new System.ArgumentException("formId cannot be null");
            }
            if (string.ReferenceEquals(lastFormId, null))
            {
                throw new System.ArgumentException("lastFormId cannot be null");
            }
            if (createdAt == null)
            {
                throw new System.ArgumentException("createdAt cannot be null");
            }
            if (string.ReferenceEquals(createdById, null))
            {
                throw new System.ArgumentException("createdById cannot be null");
            }
            if (inputs == null)
            {
                throw new System.ArgumentException("inputs cannot be null");
            }

            this.OwnerId = ownerId;
            this.ConfigId = configId;
            this.GroupId = groupId;
            this.FormId = formId;
            this.LastFormId = lastFormId;
            this.CreatedAt = createdAt;
            this.CreatedById = createdById;
            this.Inputs = inputs;
        }







    }

}
