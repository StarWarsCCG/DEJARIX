using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dejarix.Swccg
{
    public class StateChangeJsonConverter : JsonConverter<IStateChange>
    {
        public override IStateChange Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(
            Utf8JsonWriter writer,
            IStateChange value,
            JsonSerializerOptions options)
        {
            if (value is null)
                writer.WriteNullValue();
            else
                value.Serialize(writer, options);
        }
    }
}