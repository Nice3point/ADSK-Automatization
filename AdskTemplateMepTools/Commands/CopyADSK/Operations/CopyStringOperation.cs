using Newtonsoft.Json;

namespace AdskTemplateMepTools.Commands.CopyADSK.Operations
{
    public class CopyStringOperation : IOperation
    {
        public CopyStringOperation(int sourceColumn, string parameter)
        {
            SourceColumn = sourceColumn;
            Parameter = parameter;
        }

        public CopyStringOperation(string value, string parameter)
        {
            Value = value;
            Parameter = parameter;
        }

        [JsonConstructor]
        public CopyStringOperation(string value, int sourceColumn, string parameter)
        {
            Value = value;
            SourceColumn = sourceColumn;
            Parameter = parameter;
        }

        public Operation Name => Operation.CopyString;

        [JsonProperty("Название параметра")] public string Parameter { get; }
        [JsonProperty("Исходный столбец")] public int SourceColumn { get; }
        [JsonProperty("Значение")] public string Value { get; }
    }
}