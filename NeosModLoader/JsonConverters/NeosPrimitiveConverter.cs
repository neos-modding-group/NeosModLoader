using BaseX;
using Newtonsoft.Json;
using System;
using System.Reflection;

namespace NeosModLoader.JsonConverters
{
    class NeosPrimitiveConverter : JsonConverter
    {
        private static readonly Assembly BASEX = typeof(color).Assembly;

        public override bool CanConvert(Type objectType)
        {
            return BASEX.Equals(objectType.Assembly) && Coder.IsNeosPrimitive(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string serialized = (string)reader.Value;
            return typeof(Coder<>).MakeGenericType(objectType).GetMethod("DecodeFromString").Invoke(null, new object[] { serialized });
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string serialized = (string)typeof(Coder<>).MakeGenericType(value.GetType()).GetMethod("EncodeToString").Invoke(null, new object[] { value });
            writer.WriteValue(serialized);
        }
    }
}
