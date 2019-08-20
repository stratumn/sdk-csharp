using System;
using System.Threading.Tasks;

namespace stratumn.sdk
{
    public class Sdk
    {
        private SdkOptions opts;

        public Sdk(SdkOptions opts)
        {
            this.opts = opts;
        }

        public async Task<string> LoginAsync() {
            Client client = new Client(this.opts);
            try
            {
            return   await client.Login();
            }
            catch(Exception ex)
            {
                return ex.Message;
            }

        }
    }
}
