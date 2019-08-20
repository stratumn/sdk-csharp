namespace stratumn.sdk.Model.Clients
{
    public class Secret
    {
        public SecretType secretType;

        private CredentialSecret credentialSecret;
        private PrivateKeySecret privateKeySecret;
        private ProtectedKeySecret protectedKeySecret;

        public string Email
        {
            get
            {
                switch (this.secretType)
                {
                    case SecretType.CREDENTIAL:
                        return this.credentialSecret.Email;
                    default:
                        return null;
                }
            }
            set
            {
                switch (this.secretType)
                {
                    case SecretType.CREDENTIAL:
                        this.credentialSecret.Email = value;
                        break;
                    default:
                        break;
                }
            }
        }


        public string Password
        {
            get
            {
                switch (this.secretType)
                {
                    case SecretType.CREDENTIAL:
                        return this.credentialSecret.Password;
                    case SecretType.PROTECTED_KEY:
                        return this.protectedKeySecret.Password;
                    default:
                        return null;
                }
            }
            set
            {
                switch (this.secretType)
                {
                    case SecretType.CREDENTIAL:
                        this.credentialSecret.Password = value;
                        break;
                    case SecretType.PROTECTED_KEY:
                        this.protectedKeySecret.Password = value;
                        break;
                    default:
                        break;
                }
            }
        }

        public string PrivateKey
        {
            get
            {
                switch (this.secretType)
                {
                    case SecretType.PRIVATE_KEY:
                        return this.privateKeySecret.PrivateKey;
                    default:
                        break;

                }
                return null;
            }
            set
            {
                switch (this.secretType)
                {
                    case SecretType.PRIVATE_KEY:
                        this.privateKeySecret.PrivateKey = value;
                        break;
                    default:
                        break;
                }

            }
        }

        public string PublicKey
        {
            get
            {
                switch (this.secretType)
                {
                    case SecretType.PROTECTED_KEY:
                        return this.protectedKeySecret.PublicKey;
                    default:
                        return null;
                }
            }
            set
            {
                switch (this.secretType)
                {
                    case SecretType.PROTECTED_KEY:
                        this.protectedKeySecret.PublicKey = value;
                        break;
                    default:
                        break;
                }
            }
        }


        private Secret(CredentialSecret credentialSecret)
        {
            if (credentialSecret == null)
            {
                throw new System.ArgumentException("input cannot be null in Secret constructor");
            }
            this.secretType = SecretType.CREDENTIAL;
            this.credentialSecret = credentialSecret;
        }

        private Secret(PrivateKeySecret privateKeySecret)
        {
            if (privateKeySecret == null)
            {
                throw new System.ArgumentException("input cannot be null in Secret constructor");
            }
            this.secretType = SecretType.PRIVATE_KEY;
            this.privateKeySecret = privateKeySecret;
        }

        private Secret(ProtectedKeySecret s)
        {
            if (s == null)
            {
                throw new System.ArgumentException("input cannot be null in Secret constructor");
            }
            this.secretType = SecretType.PROTECTED_KEY;
            this.protectedKeySecret = s;
        }

        public static Secret NewCredentialSecret(string email, string password)
        {
            return new Secret(new CredentialSecret(email, password));
        }

        public static Secret NewPrivateKeySecret(string privateKey)
        {
            return new Secret(new PrivateKeySecret(privateKey));
        }

        public static Secret NewProtectedKeySecret(string publicKey, string password)
        {
            throw new System.NotSupportedException("Not implemented yet");
        }



    }
}
