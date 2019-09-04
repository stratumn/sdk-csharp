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
        public MemoryStream Blob { get; set; }
        public Model.File.FileInfo FileInfo { get; set; }

        public FileBlobWrapper(MemoryStream blob, Model.File.FileInfo fileInfo)
        {
            this.Blob = blob;
            this.FileInfo = fileInfo;
        }

        public override MemoryStream DecrytptedData()
        {
            return Blob; 
        }

        public override MemoryStream EncryptedData()
        {
            MemoryStream data = base.EncryptData(this.Blob);
            return data;
        }

        public override Model.File.FileInfo Info()
        {
           return  this.FileInfo;
        }
    }
}
