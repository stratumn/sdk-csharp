namespace Stratumn.Sdk.Model.Client
{
    /// <summary>
    /// The response format for a salt request
    /// </summary>
    public class SaltResponse
    {
        /// <summary>
        /// The salt value
        /// </summary>
        private string salt;

        public SaltResponse(string salt)
        {
            if (string.ReferenceEquals(salt, null))
            {
                throw new System.ArgumentException("salt cannot be null");
            }
            this.salt = salt;
        }

        public virtual string Salt
        {
            get
            {
                return this.salt;
            }
            set
            {
                this.salt = value;
            }
        }
    }
}
