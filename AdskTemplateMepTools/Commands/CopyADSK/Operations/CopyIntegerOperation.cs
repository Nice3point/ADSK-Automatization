using Newtonsoft.Json;

namespace AdskTemplateMepTools.Commands.CopyADSK.Operations
{
    public class CopyIntegerOperation : IOperation
    {
        public Command Name => Command.CopyInteger;

        public CopyIntegerOperation(string parameter, int integerValue)
        {
            Parameter = parameter;
            IntegerValue = integerValue;
            Reserve = 1;
        }

        public CopyIntegerOperation(string parameter, int sourceColumn, string reserveParameter)
        {
            Parameter = parameter;
            SourceColumn = sourceColumn;
            ReserveParameter = reserveParameter;
            Reserve = 1;
        }

        [JsonConstructor]
        public CopyIntegerOperation(string parameter, int integerValue, int sourceColumn, int reserve, string reserveParameter)
        {
            Parameter = parameter;
            IntegerValue = integerValue;
            SourceColumn = sourceColumn;
            Reserve = reserve;
            ReserveParameter = reserveParameter;
        }

        [JsonProperty("Название параметра")]
        public string Parameter { get; }
        
        [JsonProperty("Целое значение")]
        public int IntegerValue { get; }

        [JsonProperty("Исходный столбец")]
        public int SourceColumn { get; }
        
        [JsonProperty("Множитель")]
        public int Reserve { get; }

        [JsonProperty("Название параметра множителя")]
        public string ReserveParameter { get; }
    }
}