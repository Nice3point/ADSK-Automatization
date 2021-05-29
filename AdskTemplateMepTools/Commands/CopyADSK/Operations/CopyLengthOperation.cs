using System.ComponentModel;
using Newtonsoft.Json;

namespace AdskTemplateMepTools.Commands.CopyADSK.Operations
{
    public class CopyLengthOperation : IOperation
    {
       public CopyLengthOperation(string parameter, string reserveParameter)
        {
            Parameter = parameter;
            ReserveParameter = reserveParameter;
            Reserve = 1;
        }

        [JsonConstructor]
        public CopyLengthOperation(string parameter, double reserve, string reserveParameter)
        {
            Parameter = parameter;
            Reserve = reserve;
            ReserveParameter = reserveParameter;
        }

        public Operation Name => Operation.CopyLength;
        
        [JsonProperty("Название параметра")] public string Parameter { get; }
        
        [JsonProperty("Множитель")]
        [DefaultValue(1)]
        public double Reserve { get; }
        
        [JsonProperty("Глобальный параметр множителя")]
        [DefaultValue("")]
        public string ReserveParameter { get; }
    }
}