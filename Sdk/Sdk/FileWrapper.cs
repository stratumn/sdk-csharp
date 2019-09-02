using Stratumn.Sdk.Model.Misc;
using System;
using System.IO;

namespace Stratumn.Sdk
{
    public abstract class FileWrapper : Identifiable
    {
        /// <summary>
        /// A unique identifier of the file wrapper. Satisfies the Identifiable constraint.
        /// </summary>
        private string id = System.Guid.NewGuid().ToString();
         
        public string Id
        {
            get
            {
                return this.id;
            }
        }


        private String _key;
        public string Key
        {
            get
            {
                return this._key;
            }

        }

        public FileWrapper():this(true,null)
        { 
        }

        public FileWrapper(Boolean disableEncryption, String key)
        {
            if (!disableEncryption)
                this._key = key;
        }

        /// <summary>
        /// Encrypts data in memory stream
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected MemoryStream EncryptData(MemoryStream data)
        {
            if (this._key == null)
                return data;
            //AES encryption 

            AesWrapper AESKey = new AesWrapper(this._key);
            return AESKey.Encrypt(data);

        }

        /// <summary>
        /// Decrypts the data in memory stream
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected MemoryStream DecryptData(MemoryStream data)
        {
            if (this._key == null)
                return data;
            //AES encryption 

            AesWrapper AESKey = new AesWrapper(this._key);
            return AESKey.Decrypt(data);

        }

        /// <summary>
        /// Adds the key info to the fileInfo
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>        
        protected Sdk.Model.File.FileInfo AddKeyToFileInfo(Sdk.Model.File.FileInfo info)
        {
            if (this._key == null)
                return info;
            info.Key = this._key;

            return info;

        }

        public abstract Sdk.Model.File.FileInfo Info();

        public abstract MemoryStream EncryptedData();

        public abstract MemoryStream DecrytptedData();


        public static FileWrapper FromBrowserFile(FileInfo file)
        {
            return new BrowserFileWrapper(file);
        }

        public static FileWrapper FromFilePath(String path)
        {
            return new FilePathWrapper(path);
        }

        public static FileWrapper FromFileBlob(MemoryStream blob,Sdk.Model.File.FileInfo fileInfo)
        {
            return new FileBlobWrapper(blob,fileInfo);
        }

        public static Boolean isFileWrapper(Object obj)
        {
            return (obj is FileWrapper);
        }
    }
}
