using System.ComponentModel;
using Newtonsoft.Json;

namespace AdskTemplateMepTools.Commands.CopyADSK.Operations
{
    public class CopyMassOperation : IOperation
    {
        public CopyMassOperation(string parameter, int sourceColumn)
        {
            Parameter = parameter;
            SourceColumn = sourceColumn;
            Reserve = 1;
        }

        [JsonConstructor]
        public CopyMassOperation(string parameter, double reserve, string reserveParameter)
        {
            Parameter = parameter;
            Reserve = reserve;
            ReserveParameter = reserveParameter;
        }

        public Operation Name => Operation.CopyMass;
        
        [JsonProperty("Название параметра")] public string Parameter { get; }
        
        [JsonProperty("Исходный столбец")]
        public int SourceColumn { get; }
        
        [JsonProperty("Коэффициент запаса")]
        [DefaultValue(1.0)]
        public double Reserve { get; }
        
        [JsonProperty("Глобальный параметр коэффициента запаса")]
        [DefaultValue("")]
        public string ReserveParameter { get; }
    }
}