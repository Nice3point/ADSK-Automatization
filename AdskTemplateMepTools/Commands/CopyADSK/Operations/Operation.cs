using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AdskTemplateMepTools.Commands.CopyADSK.Operations
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Operation
    {
        [EnumMember(Value = "Копирование текста")] CopyString,
        [EnumMember(Value = "Копирование целого")] CopyInteger,
        [EnumMember(Value = "Копирование длины")] CopyLength,
        [EnumMember(Value = "Копирование площади")] CopyArea,
        [EnumMember(Value = "Копирование объема")] CopyVolume,
        [EnumMember(Value = "Копирование температуры системы")] CopyTemperature,
        [EnumMember(Value = "Копирование массы")] CopyMass
    }
}