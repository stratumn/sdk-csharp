using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stratumn.CanonicalJson;
using Stratumn.Chainscript.utils;
using Stratumn.Sdk.Model.File;
using Stratumn.Sdk.Model.Misc;

namespace Stratumn.Sdk
{
    /// <summary>
    ///  A file record contains file information (FileInfo) and Media service record

    ///   information(MediaRecord). It corresponds to a file stored in the Media
    ///   service.
    /// </summary>
    public class FileRecord : Identifiable
    {
        public FileRecord()
        {
        }

        public FileRecord(MediaRecord media, FileInfo info)
        {
            this.Name = media.Name;
            this.Digest = media.Digest;
            this.Mimetype = info.Mimetype;
            this.Size = info.Size;
            this.Key = info.Key;
        }

        public FileInfo GetFileInfo()
        {
            return new FileInfo(Name, Size, Mimetype, Key);
        }

        public MediaRecord GetMediaRecord()
        {
            return new MediaRecord(this.Name, this.Digest);
        }

        public string GetId()
        {
            return Digest;
        }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }

        [JsonProperty(PropertyName = "mimetype")]
        public string Mimetype { get; private set; }

        [JsonProperty(PropertyName = "key")]
        public string Key { get; private set; }

        [JsonProperty(PropertyName = "size")]
        public long Size { get; private set; }

        [JsonProperty(PropertyName = "digest")]
        public string Digest { get; private set; }

        public static FileRecord FromObject(Object obj)
        {
            if (obj is FileRecord) return (FileRecord)obj;
            return JsonHelper.ObjectToObject<FileRecord>(obj);
        }

        public static bool IsFileRecord(Object obj)
        {
            bool isFileRecord = false;
            try
            {
                string json = null;

                if (typeof(FileRecord).IsInstanceOfType(obj))
                {
                    isFileRecord = true;
                }
                else if (obj != null)
                {
                    if (obj is JObject)
                    {
                        json = JsonHelper.ToCanonicalJson(obj);
                    }
                    else if (
                        obj is String //assume json
                    )
                    {
                        json = Canonicalizer.Canonicalize((String)obj);
                    }
                    else
                    {
                        json = JsonHelper.ToCanonicalJson(obj);
                    }
                    if (json != null)
                    {

                        // Attempt to generate FileRecord from json.
                        FileRecord ob = JsonHelper.FromJson<FileRecord>(json);
                        // Validate that all of the required fields are not null
                        // Key can be undefined if the file is unencrypted.
                        if (
                            ob.Digest != null &&
                            ob.Name != null &&
                            ob.Size != 0 &&
                            ob.Mimetype != null
                        )
                        {
                            isFileRecord = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return isFileRecord;
        }
    }
}
