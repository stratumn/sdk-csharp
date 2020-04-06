using Newtonsoft.Json.Linq;
using Stratumn.CanonicalJson;
using Stratumn.Chainscript.utils;
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

        public string GetId()
        {
            return this.id;
        }

        private AesKey _key;

        public FileWrapper() : this(false, null)
        {
        }

        public FileWrapper(Boolean disableEncryption, String key)
        {
            if (!disableEncryption)
            {
                this._key = new AesKey(key);
            }
        }

        /// <summary>
        /// Encrypts data in memory stream
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected MemoryStream EncryptData(MemoryStream data)
        {
            if (this._key == null)
            {
                return data;
            }

            // AES encryption 
            return this._key.Encrypt(data);
        }

        /// <summary>
        /// Decrypts the data in memory stream
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected MemoryStream DecryptData(MemoryStream data)
        {
            if (this._key == null)
            {
                return data;
            }

            // AES decryption
            return this._key.Decrypt(data);
        }

        /// <summary>
        /// Adds the key info to the fileInfo
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>        
        protected Model.File.FileInfo AddKeyToFileInfo(Model.File.FileInfo info)
        {
            if (this._key == null)
            {
                return info;
            }

            info.Key = this._key.Export();
            return info;
        }

        public abstract Model.File.FileInfo Info();

        public abstract MemoryStream EncryptedData();

        public abstract MemoryStream DecryptedData();

        public static FileWrapper FromBrowserFile(FileInfo file)
        {
            return new BrowserFileWrapper(file);
        }

        public static FileWrapper FromFilePath(String path)
        {
            return new FilePathWrapper(path);
        }

        public static FileWrapper FromFileBlob(MemoryStream blob, Model.File.FileInfo fileInfo)
        {
            return new FileBlobWrapper(blob, fileInfo);
        }

        public static Boolean isFileWrapper(Object obj)
        {
            bool isFileWrapper = false;
            try
            {
                string json = null;

                if (typeof(FileWrapper).IsInstanceOfType(obj))
                {
                    isFileWrapper = true;
                }
                else if (obj != null)
                {
                    if (obj is JObject)
                    {
                        json = JsonHelper.ToCanonicalJson(obj);
                    }
                    else if (obj is String) //assume json
                    {
                        try {
                            json = Canonicalizer.Canonicalize((String)obj);
                        } catch (Exception) {}
                    }
                    else
                    {
                        json = JsonHelper.ToCanonicalJson(obj);
                    }

                    if (json != null)
                    {
                        Object ob = null;
                        // attempt to generate FileWrapper from json.
                        try
                        {
                            if (json.ToUpper().Contains("PATH"))
                            {
                                ob = JsonHelper.FromJson<FilePathWrapper>(json);
                            }
                            else if (json.ToUpper().Contains("FILEINFO"))
                            {
                                ob = JsonHelper.FromJson<FileBlobWrapper>(json);
                            }
                            else if (json.ToUpper().Contains("FILE"))
                            {
                                ob = JsonHelper.FromJson<BrowserFileWrapper>(json);
                            }
                        }
                        catch (Exception)
                        { // obj can not be converted
                        }
                        if (ob != null)
                        {
                            String json2 = JsonHelper.ToCanonicalJson(ob);
                            if (json2.Equals(json))
                            {
                                isFileWrapper = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return isFileWrapper;
        }

        public static FileWrapper FromObject(Object obj)
        {
            if (obj is FileWrapper)
            {
                return (FileWrapper)obj;
            }

            string json = JsonHelper.ToJson(obj);
            if (json.ToUpper().Contains("FILEINFO"))
            {
                return JsonHelper.ObjectToObject<FileBlobWrapper>(obj);
            }
            else if (json.ToUpper().Contains("PATH"))
            {
                return JsonHelper.ObjectToObject<FilePathWrapper>(obj);
            }
            else if (json.ToUpper().Contains("FILE"))
            {
                return JsonHelper.ObjectToObject<BrowserFileWrapper>(obj);
            }
            else throw new TraceSdkException($"cannot convert {obj} to FileWrapper");
        }
    }
}
