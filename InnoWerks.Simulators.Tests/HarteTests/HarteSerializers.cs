using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InnoWerks.Simulators.Tests
{
    public class HarteCycleConverter : JsonConverter<JsonHarteTestBusAccess>
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeof(JsonHarteTestBusAccess).IsAssignableFrom(typeToConvert);

        public override JsonHarteTestBusAccess Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException($"JsonHarteTestCycle has incorrect form (not an array)");
            }

            var cycle = new JsonHarteTestBusAccess();

            reader.Read();
            cycle.Address = reader.GetUInt16();

            reader.Read();
            cycle.Value = reader.GetInt32();

            reader.Read();
            cycle.Type = Enum.Parse<CycleType>(reader.GetString(), true);

            reader.Read();
            if (reader.TokenType != JsonTokenType.EndArray)
            {
                throw new JsonException($"JsonHarteTestCycle has incorrect form (too many values)");
            }

            return cycle;
        }

        public override void Write(Utf8JsonWriter writer, JsonHarteTestBusAccess value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
