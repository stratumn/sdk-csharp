using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stratumn.Sdk;
using Stratumn.Sdk.Model.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratumn.Chainscript.utils
{
   
    public class IdentifiableConverter : JsonConverter
    { 
        public override bool CanConvert(Type objectType)
        {
            if (typeof(Identifiable).Equals(objectType))
                return true;
            return false;
        }

        public override object ReadJson(JsonReader reader,
         Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            if (jsonObject["digest"] != null)
            {
                return serializer.Deserialize(jsonObject.CreateReader(), typeof(FileRecord));
            }
            else
                return serializer.Deserialize(jsonObject.CreateReader(), typeof(FileWrapper));
            
        }

        public override void WriteJson(JsonWriter writer,
            object value, JsonSerializer serializer)
        { 
            serializer.Serialize(writer, value);
        }
    }
}
