using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stratumn.Sdk.Model.File;

namespace Stratumn.Sdk
{
    /// <summary>
    /// The browser implementation of a FileWrapper.
    /// </summary>
    public class BrowserFileWrapper : FileWrapper
    { 
        public System.IO.FileInfo File { get; set; }

        
        public BrowserFileWrapper(System.IO.FileInfo File)
        {
            this.File = File;
        }

        public override MemoryStream DecrytptedData()
        {
            throw new NotImplementedException();
        }

        public override MemoryStream EncryptedData()
        {
            throw new NotImplementedException();
        }

        public override Model.File.FileInfo Info()
        {
            throw new NotImplementedException();
        }
    }
}
