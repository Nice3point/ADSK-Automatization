using System.ComponentModel;
using System.Runtime.Serialization;
using AdskTemplateMepTools.Commands.CopyADSK.Operations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AdskTemplateMepTools.Commands.CopyADSK
{
    public class CopyStringOperation : IOperation
    {
        public Command Name => Command.CopyString;

        public CopyStringOperation(int sourceColumn, string parameter)
        {
            SourceColumn = sourceColumn;
            Parameter = parameter;
        }

        [JsonProperty("Исходный столбец")]
        public int SourceColumn { get; }

        [JsonProperty("Название параметра")]
        public string Parameter { get; }

    }

    public class CopyDoubleOperation : IOperation
    {
        public CopyDoubleOperation(int sourceColumn, double reserveLength, string reserveParameter)
        {
            SourceColumn = sourceColumn;
            ReserveLength = reserveLength;
            ReserveParameter = reserveParameter;
        }

        public Command Name => Command.CopyDouble;

        [JsonProperty("Исходный столбец")]
        public int SourceColumn { get; }
        
        [JsonProperty("Коэфициент запаса")]
        public double ReserveLength { get; }

        [JsonProperty("Название параметра запаса")]
        public string ReserveParameter { get; }
    }
    
    public class CopyAreaOperation : IOperation
    {
        public Command Name => Command.CopyArea;
    }
    public class CopyVolumeOperation : IOperation
    {
        public Command Name => Command.CopyVolume;
    }
    
    public class CopyTemperatureOperation : IOperation
    {
        public Command Name => Command.CopyTemperature;
    }
    public class CopyMassOperation : IOperation
    {
        public Command Name => Command.CopyMass;
    }
    
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Command
    {
        [EnumMember(Value = "Копирование текста")] CopyString,
        [EnumMember(Value = "Копирование целого")] CopyInteger,
        [EnumMember(Value = "Копирование дробного")] CopyDouble,
        [EnumMember(Value = "Копирование квадратных метров")] CopyArea,
        [EnumMember(Value = "Копирование кубических метров")] CopyVolume,
        [EnumMember(Value = "Копирование температуры в Кельвинах")] CopyTemperature,
        [EnumMember(Value = "Копирование массы в килограммах")] CopyMass
    }
}