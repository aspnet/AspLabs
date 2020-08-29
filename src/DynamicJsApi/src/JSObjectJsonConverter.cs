using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.DynamicJS
{
    internal class JSObjectJsonConverter : JsonConverter<JSObject>
    {
        private static readonly string _jsObjectIdKey = "__jsObjectId";

        public override JSObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new InvalidOperationException($"{typeof(JSObject)} does not support deserialization.");
        }

        public override void Write(Utf8JsonWriter writer, JSObject value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber(_jsObjectIdKey, value.Id);
            writer.WriteEndObject();
        }
    }
}
