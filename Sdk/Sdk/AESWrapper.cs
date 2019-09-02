using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratumn
{
    class AesWrapper
    { 

        public AesWrapper(String secret , int length)
        {

        }

        public AesWrapper(string key): this(key,16)
        { 
        }

        public MemoryStream Encrypt(MemoryStream data)
        {
            return data;
        }

        public MemoryStream Decrypt(MemoryStream data)
        {
            return data;
        }

    }
}
