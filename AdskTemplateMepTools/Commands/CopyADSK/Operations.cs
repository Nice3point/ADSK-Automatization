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
        public Operation(Command command, int sourceColumn, double reserveLength, string reserveParameter, string parameter)
        {
            Command = command;
            SourceColumn = sourceColumn;
            ReserveLength = reserveLength;
            ReserveParameter = reserveParameter;
            Parameter = parameter;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [DefaultValue(-1)]
        [JsonProperty("Название")]
        public Command Command { get; }

        [DefaultValue(default(int))]
        [JsonProperty("Исходный столбец")]
        public int SourceColumn { get; }

        [DefaultValue(default(double))]
        [JsonProperty("Коэфициент запаса")]
        public double ReserveLength { get; }

        [DefaultValue(default(string))]
        [JsonProperty("Название параметра запаса")]
        public string ReserveParameter { get; }

        [DefaultValue(default(string))]
        [JsonProperty("Название параметра")]
        public string Parameter { get; }
    }

    public enum Command
    {
        [EnumMember(Value = "Копирование длины")] CopyName =10,
        CopyLength,
        CopyCount,
        CopyArea,
        CopyVolume,
        CopyComment,
        CopyTemperature,
        CopyMass
    }
}