using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AdskTemplateMepTools.Commands.CopyADSK
{
    public class Operation
    {
        public Operation(Command command) => Command = command;

        public Operation(Command command, int sourceColumn)
        {
            Command = command;
            SourceColumn = sourceColumn;
        }

        public Operation(Command command, string parameter)
        {
            Command = command;
            Parameter = parameter;
        }

        public Operation(Command command, double reserveLength, string reserveParameter)
        {
            Command = command;
            ReserveLength = reserveLength;
            ReserveParameter = reserveParameter;
        }

        [JsonConstructor]
        public Operation(Command command, int sourceColumn, double reserveLength, string reserveParameter,
            string parameter)
        {
            Command = command;
            SourceColumn = sourceColumn;
            ReserveLength = reserveLength;
            ReserveParameter = reserveParameter;
            Parameter = parameter;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [DefaultValue(-1)]
        [JsonProperty("Operation")]
        public Command Command { get; }

        [DefaultValue(default(int))]
        [JsonProperty("Source column")]
        public int SourceColumn { get; }

        [DefaultValue(default(double))]
        [JsonProperty("Reserve length")]
        public double ReserveLength { get; }

        [DefaultValue(default(string))]
        [JsonProperty("Reserve parameter")]
        public string ReserveParameter { get; }

        [DefaultValue(default(string))]
        [JsonProperty("Parameter")]
        public string Parameter { get; }
    }

    public enum Command
    {
        [EnumMember(Value = "Copy Name")] CopyName =10,
        CopyLength,
        CopyCount,
        CopyArea,
        CopyVolume,
        CopyComment,
        CopyTemperature,
        CopyMass
    }
}