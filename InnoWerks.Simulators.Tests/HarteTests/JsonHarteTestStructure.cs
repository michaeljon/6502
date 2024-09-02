using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;

#pragma warning disable CA1002, CA2227

namespace InnoWerks.Simulators.Tests
{
    public enum CycleType
    {
        Read,

        Write
    }

    public class JsonHarteRamEntry
    {
        public ushort Address { get; set; }

        public byte Value { get; set; }
    }

    public class JsonHarteTestState
    {
        [JsonPropertyName("pc")]
        public ushort ProgramCounter { get; set; }

        [JsonPropertyName("s")]
        public byte S { get; set; }

        [JsonPropertyName("a")]
        public byte A { get; set; }

        [JsonPropertyName("x")]
        public byte X { get; set; }

        [JsonPropertyName("y")]
        public byte Y { get; set; }

        [JsonPropertyName("p")]
        public byte P { get; set; }

        [JsonPropertyName("ram")]
        public List<JsonHarteRamEntry> Ram { get; set; }

        public JsonHarteTestState Clone()
        {
            var clone = (JsonHarteTestState)MemberwiseClone();
            clone.Ram = this.Ram.OrderBy(r => r.Address).ToList();
            return clone;
        }
    }

    [DebuggerDisplay("{Address} {Value} {Type}")]
    public class JsonHarteTestBusAccess
    {
        public ushort Address { get; set; }

        public int Value { get; set; }

        public CycleType Type { get; set; }

        public override string ToString()
        {
            return $"${Address:X4}: ${Value:X2} ({Address}:{Value}) {Type.ToString().ToLowerInvariant()}";
        }
    }

    [DebuggerDisplay("{Name}")]
    public class JsonHarteTestStructure
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("initial")]
        public JsonHarteTestState Initial { get; set; }

        [JsonPropertyName("final")]
        public JsonHarteTestState Final { get; set; }

        [JsonPropertyName("cycles")]
        public IEnumerable<JsonHarteTestBusAccess> BusAccesses { get; set; }

        public JsonHarteTestStructure Clone()
        {
            return new JsonHarteTestStructure
            {
                Name = this.Name,
                Initial = this.Initial.Clone(),
                Final = this.Final.Clone(),
                BusAccesses = this.BusAccesses
            };
        }
    }
}
