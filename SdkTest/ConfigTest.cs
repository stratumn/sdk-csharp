namespace SdkTest
{
    public class ConfigTest
    {
        // Env dependant
        // FIXME : set these in semaphore ?
        public const string WORKFLOW_ID = "812";
        public const string MY_GROUP = "4822";
        public const string TRACE_ID = "f1c1d4fa-961e-4b2c-9519-ccbda4ecc2d4";
        public const string OTHER_GROUP = "4823";

        public const string TRACE_URL = "https://trace-api.staging.stratumn.com";
        public const string ACCOUNT_URL = "https://account-api.staging.stratumn.com";
        public const string MEDIA_URL = "https://media-api.staging.stratumn.com";

        // public const string TRACE_URL = "http://trace-api.local.stratumn.com:4100";
        // public const string ACCOUNT_URL = "http://account-api.local.stratumn.com:4200";
        // public const string MEDIA_URL = "http://media-api.local.stratumn.com:4500";

        // Non env dependant
        public const string COMMENT_ACTION_KEY = "comment";
        public const string INIT_ACTION_KEY = "init";
        public const string UPLOAD_DOCUMENTS_ACTION_KEY = "uploadDocuments";

        public const string IMPORT_TA_ACTION_KEY = "importTa";
        // public const string PEM_PRIVATEKEY = "-----BEGIN ED25519 PRIVATE KEY-----\nMFACAQAwBwYDK2VwBQAEQgRAjgtjpc1iOR4zYm+21McRGoWr0WM1NBkm26uZmFAx\n853QZ8CRL/HWGCPpEt18JrHZr9ZwA9UyoEosPR8gPakZFQ==\n-----END ED25519 public KEY-----\n";
        // Bot 1 
        public const string PEM_PRIVATEKEY = "-----BEGIN ED25519 PRIVATE KEY-----\nMFACAQAwBwYDK2VwBQAEQgRAP7BEfm6Smg9h3mmOM3zayeAyPk4/VvT927NN5Y8e\nsgqwoZr++UHatd9r9cg2NZvCleMojySIsLKQpZYEwr21uw==\n-----END ED25519 PRIVATE KEY-----\n";
        // public const string PEM_PRIVATEKEY_2 = "-----BEGIN ED25519 PRIVATE KEY-----\nMFACAQAwBwYDK2VwBQAEQgRArbo87/1Yd/nOqFwmmcuxm01T9/pqkeARQxK9y4iG\nF3Xe1W+/2UOr/rYuQPFHQC4a/F0r6nVJGgCI1Ghc/luHZw==\n-----END ED25519 public KEY-----\n";
        // Bot 2
        public const string PEM_PRIVATEKEY_2 = "-----BEGIN ED25519 PRIVATE KEY-----\nMFACAQAwBwYDK2VwBQAEQgRAtMoOToj7bv+A+7dOrM5UyG2buHgsSu0OriTJfv7/\nEqKUzdjgxvAvTtOA7RCIY1/FoDWjHZ/wG5hPcA3Bj3BRkQ==\n-----END ED25519 PRIVATE KEY-----\n";
        public const string MY_GROUP_LABEL = "group1";
        public const string OTHER_GROUP_LABEL = "group2";
        public const string OTHER_GROUP_NAME = "SDKs Group 2";
    }
}