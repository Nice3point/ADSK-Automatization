using System.ComponentModel;
using Newtonsoft.Json;

namespace AdskTemplateMepTools.Commands.CopyADSK.Operations
{
    public class CopyStringOperation : IOperation
    {
        public CopyStringOperation(string parameter, int sourceColumn)
        {
            SourceColumn = sourceColumn;
            Parameter = parameter;
        }

        public CopyStringOperation(string parameter, string value)
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

        [JsonProperty("Название параметра")]
        [DefaultValue("")]
        public string Parameter { get; }
        
        [JsonProperty("Исходный столбец")]
        [DefaultValue("")]
        public int SourceColumn { get; }
        
        [JsonProperty("Значение")]
        [DefaultValue("")]
        public string Value { get; }
    }
}