using System;

namespace Stratumn.Sdk.Model.File
{
    /// <summary>
    /// A file information interface.
    /// </summary>
    public class FileInfo
    {
        private string mimetype;
        private long size;
        private string name;
        private string key;
        private DateTime createdAt;

        public FileInfo(string name, long size, string mimetype, string key)
        {
            if (string.ReferenceEquals(name, null))
            {
                throw new System.ArgumentException("name cannot be null");
            }
            if (string.ReferenceEquals(mimetype, null))
            {
                throw new System.ArgumentException("mimetype cannot be null");
            }

            this.name = name;
            this.size = size;
            this.mimetype = mimetype;
            this.key = key;
            this.createdAt = DateTime.UtcNow;
        }

        public virtual string Mimetype
        {
            get
            {
                return this.mimetype;
            }
            set
            {
                this.mimetype = value;
            }
        }

        public virtual long Size
        {
            get
            {
                return this.size;
            }
            set
            {
                this.size = value;
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

        public virtual string Key
        {
            get
            {
                return key;
            }
            set
            {
                this.key = value;
            }
        }

        public virtual DateTime CreatedAt
        {
            get
            {
                return createdAt;
            }
            set
            {
                this.createdAt = value;
            }
        }
    }
}
