using System;

namespace Stratumn.Sdk
{
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

        public int Status
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

        public override string Message
        {
            get
            {
                return this.message;
            }
        }
    }
}
