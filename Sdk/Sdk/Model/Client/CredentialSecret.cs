namespace Stratumn.Sdk.Model.Client
{
    public class CredentialSecret :Secret
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public CredentialSecret(string email, string password)
        {
            if (string.ReferenceEquals(email, null))
            {
                throw new System.ArgumentException("email cannot be null in CredentialSecret");
            }
            if (string.ReferenceEquals(password, null))
            {
                throw new System.ArgumentException("password cannot be null in CredentialSecret");
            }
            this.Email = email;
            this.Password = password;
        }
    }
}
