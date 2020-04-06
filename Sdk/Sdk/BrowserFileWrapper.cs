using System;
using System.IO;
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

        public override MemoryStream DecryptedData()
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
