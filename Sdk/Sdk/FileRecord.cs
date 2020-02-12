using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stratumn.CanonicalJson;
using Stratumn.Chainscript.utils;
using Stratumn.Sdk.Model.File;
using Stratumn.Sdk.Model.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        { }

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
        public string Name
        {
            get; private set;

        }
        [JsonProperty(PropertyName = "mimetype")]
        public string Mimetype
        {
            get; private set;
        }
        [JsonProperty(PropertyName = "key")]
        public string Key
        {
            get; private set;
        }

        [JsonProperty(PropertyName = "size")]
        public long Size
        {
            get; private set;
        }

        [JsonProperty(PropertyName = "digest")]
        public string Digest
        {
            get; private set;
        }

        public static FileRecord FromObject(Object obj)
        {
            if (obj is FileRecord)
                return (FileRecord)obj;
            return JsonHelper.ObjectToObject<FileRecord>(obj);
        }


        public static bool IsFileRecord(Object obj)
        {
            bool isFileRecord = false;
            try
            {
                string json = null;

                if (typeof(FileRecord).IsInstanceOfType(obj))
                    isFileRecord = true;
                else
                if (obj != null)
                {
                    if (obj is JObject)
                        json = JsonHelper.ToCanonicalJson(obj);
                    else
                      if (obj is String)//assume json
                        json = Canonicalizer.Canonicalize((String)obj);
                    else
                        json = JsonHelper.ToCanonicalJson(obj);
                    if (json != null)
                    {
                        //attempt to generate FileRecord from json.
                        json = removeAdditionalFields(json);

                        Object ob = JsonHelper.FromJson<FileRecord>(json);
                        String json2 = JsonHelper.ToCanonicalJson(ob);

                        if (json2.Equals(json))
                            isFileRecord = true;
                    }
                }

            }
            catch (Exception) { }
            return isFileRecord;
        }

        // Since we do a canonicalized JSON string comparison to check if the object is a 
        // FileRecord above, we need to remove the additional fields that can be added
        // by the media API.
        private static String removeAdditionalFields(String json)
        {
            String[] recordFields = { "id", "key", "digest", "name", "mimetype", "size"};
            var o = JsonHelper.FromJson<JObject>(json);

            foreach (var p in o.Properties())
            {
                if (!Array.Exists(recordFields, p.Name.Equals))
                {
                    p.Remove();
                }
            }

            return JsonHelper.ToJson(o);

   }


}
}
