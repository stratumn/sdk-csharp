using Newtonsoft.Json;
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
            return JsonHelper.ObjectToObject<FileRecord>(obj);
        }


        public static bool IsFileRecord(Object obj)
        {
            bool isFileRecord = false;
            if (obj is FileRecord)
                isFileRecord = true;
            return isFileRecord;

        }


    }
}
