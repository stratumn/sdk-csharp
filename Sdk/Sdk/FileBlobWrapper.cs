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
    /// The implementation of a FileWrapper using the blob and info to represent it.
    /// </summary>
    public class FileBlobWrapper : FileWrapper
    {
        private MemoryStream blob;
        private Model.File.FileInfo fileInfo;

        public FileBlobWrapper(MemoryStream blob, Model.File.FileInfo fileInfo)
        {
            this.blob = blob;
            this.fileInfo = fileInfo;
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
