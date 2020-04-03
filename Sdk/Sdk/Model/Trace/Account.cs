namespace Stratumn.Sdk.Model.Trace
{
    /// <summary>
    /// A Stratumn Account object
    /// </summary>
    public class Account
    {
        private string account;

        public string GetAccount()
        {
            return account;
        }

        public void SetAccount(string account)
        {
            this.account = account;
        }

        public Account(string account) : base()
        {
            this.account = account;
        }
    }
}
