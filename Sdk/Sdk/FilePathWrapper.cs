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
        public string Path { get; set; }

        public FilePathWrapper(string path)
        {
            this.Path = path;
        }

        public override MemoryStream DecrytptedData()
        {
            throw new NotImplementedException();
        }

        public override MemoryStream EncryptedData()
        {
            return base.EncryptedData(Data());
        }

       
        public override Model.File.FileInfo Info()
        {
            System.IO.FileInfo fl = new System.IO.FileInfo(Path);
            if (!fl.Exists)
                throw new TraceSdkException("Error while loading file " + fl.FullName);

            long size = fl.Length;
            string mimetype = Helpers.GetMimeType(Path);
            String name = fl.Name;

            Stratumn.Sdk.Model.File.FileInfo fileInfo = new Stratumn.Sdk.Model.File.FileInfo(name, size, mimetype, null);

            return AddKeyToFileInfo(fileInfo);


        }

 

        private MemoryStream Data()
        {
            System.IO.FileInfo file = new System.IO.FileInfo(Path);


            if (!file.Exists)
            {
                throw new TraceSdkException("File not found " + Path);
            }

            MemoryStream destBuffer = new MemoryStream(); ;

            using (Stream source = File.OpenRead(Path))
            {
                byte[] buffer = new byte[2048];
                int bytesRead;
                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    destBuffer.Write(buffer, 0, bytesRead);
                }
            }
            return destBuffer;
        }

    }
}