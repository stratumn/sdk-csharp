using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratumn.Sdk
{
    /// <summary>
    ///*
    ///  Exception to wrap Sdk exceptiosn
    /// </summary>
    public class TraceSdkException : Exception
    {

        /// 
        private const long serialVersionUID = 1L;

        public TraceSdkException() : base()
        {
        }

        public TraceSdkException(string message, Exception cause) : base(message, cause)
        {
        }

        public TraceSdkException(string message) : base(message)
        {
        }

        public TraceSdkException(Exception cause) : base(cause.ToString())
        {
        }

    }
}
