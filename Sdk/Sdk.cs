using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StratumSdk
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
