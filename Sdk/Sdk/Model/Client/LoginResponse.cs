namespace Stratumn.Sdk.Model.Client
{
    /// <summary>
    /// The response format for a login request
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// The authentication token
        /// </summary>
        private string token;

        public LoginResponse(string token)
        {
            if (string.ReferenceEquals(token, null))
            {
                throw new System.ArgumentException("token cannot be null");
            }
            this.token = token;
        }

        public string Token
        {
            get
            {
                return this.token;
            }
            set
            {
                this.token = value;
            }
        }
    }
}
