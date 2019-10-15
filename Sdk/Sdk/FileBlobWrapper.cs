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

        public FileBlobWrapper(MemoryStream blob, Model.File.FileInfo fileInfo) : base(fileInfo.Key == null || fileInfo.Key == "", fileInfo.Key)
        {
            this.Blob = blob;
            this.FileInfo = fileInfo;

        }

        public override MemoryStream DecrytptedData()
        {
            return this.DecryptData(Blob); 
        }

        public override MemoryStream EncryptedData()
        {
            return this.EncryptData(this.Blob);
        }

        public override Model.File.FileInfo Info()
        {
            return this.AddKeyToFileInfo(this.FileInfo);
        }
    }
}
