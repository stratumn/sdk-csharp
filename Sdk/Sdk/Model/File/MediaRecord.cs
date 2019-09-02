using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratumn.Sdk.Model.File
{
    /// <summary>
    /// A record of a file in the Media service.
    /// </summary>
    public class MediaRecord
    {
        private string name;
        private string digest;

        public MediaRecord(string name, string digest)
        {
            if (string.ReferenceEquals(name, null))
            {
                throw new System.ArgumentException("name cannot be null");
            }
            if (string.ReferenceEquals(digest, null))
            {
                throw new System.ArgumentException("digest cannot be null");
            }

            this.name = name;
            this.digest = digest;
        }

        public virtual string Digest
        {
            get
            {
                return this.digest;
            }
            set
            {
                this.digest = value;
            }
        }

        public virtual string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }
    }

}
