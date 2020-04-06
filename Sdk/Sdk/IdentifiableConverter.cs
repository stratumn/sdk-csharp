using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stratumn.Sdk;
using Stratumn.Sdk.Model.Misc;
using System;

namespace Stratumn.Chainscript.utils
{
    public class IdentifiableConverter : JsonConverter
    { 
        public override bool CanConvert(Type objectType)
        {
            return typeof(Identifiable).Equals(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            return serializer.Deserialize(
                jsonObject.CreateReader(),
                jsonObject["digest"] != null ? typeof(FileRecord) : typeof(FileWrapper)
            );
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        { 
            serializer.Serialize(writer, value);
        }
    }
}
