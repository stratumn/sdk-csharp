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
    /// The   implementation of a FileWrapper using a file path to point to the actual file.
    /// </summary>
    public class FilePathWrapper : FileWrapper
    {
        private string path;

        public FilePathWrapper(string path)
        {
            this.path = path;
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
