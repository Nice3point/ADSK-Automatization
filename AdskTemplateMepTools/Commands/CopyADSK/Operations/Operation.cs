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
        [EnumMember(Value = "Копирование дробного")] CopyDouble,
        [EnumMember(Value = "Копирование квадратных метров")] CopyArea,
        [EnumMember(Value = "Копирование кубических метров")] CopyVolume,
        [EnumMember(Value = "Копирование температуры в Кельвинах")] CopyTemperature,
        [EnumMember(Value = "Копирование массы в килограммах")] CopyMass
    }
}