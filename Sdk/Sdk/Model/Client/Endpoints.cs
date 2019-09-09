using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratumn.Sdk.Model.Client
{
    public class Endpoints
    {
        public string Trace { get; set; }
        public string Account { get; set; }
        public string Media { get; set; }

        public Endpoints()
        { }

        public Endpoints(string Account, string Trace, string Media)
        {
            this.Account = Account;
            this.Trace = Trace;
            this.Media = Media;
        }

        public string GetEndpoint(Service service)
        {
            switch (service)
            {
                case Service.ACCOUNT:
                    return this.Account;

                case Service.TRACE:
                    return Trace;

                case Service.MEDIA:
                    return Media;

            }
            return null;
        }
    }
}
