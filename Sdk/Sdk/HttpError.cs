using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratumn.Sdk
{
    using System;
    using Lucene.Net.Support;

    public class HttpError : Exception
    {
        /// 
        private const long serialVersionUID = -1942318828228006196L;
        private int status;
        private string message;

        public HttpError(int status, string message)
        {
            this.status = status;
            this.message = message;
        }

        public override string ToString()
        {
            return "Http error [ Status : " + this.status + " Message : " + message + " ]";
        }

        public int Satus
        {
            set
            {
                this.status = value;
            }
            get
            {
                return this.status;
            }
        }

        public string Message
        {
            set
            {
                this.message = value;
            }
            get
            {
                return this.message;
            }
        }

    }

}
