using Newtonsoft.Json;

namespace AdskTemplateMepTools.Commands.CopyADSK.Operations
{
    public class CopyTemperatureOperation : IOperation
    {
        [JsonConstructor]
        public CopyTemperatureOperation(string parameter)
        {
            Parameter = parameter;
        }

        public Operation Name => Operation.CopyTemperature;
        
        [JsonProperty("Название параметра")] public string Parameter { get; }
    }
}