using System.ComponentModel;
using Newtonsoft.Json;

namespace AdskTemplateMepTools.Commands.CopyADSK.Operations
{
    public class CopyMassOperation : IOperation
    {
        public CopyMassOperation(string parameter, int sourceColumn)
        {
            Parameter    = parameter;
            SourceColumn = sourceColumn;
            Reserve      = 1;
        }

        [JsonConstructor]
        public CopyMassOperation(string parameter, int sourceColumn, double reserve, string reserveParameter)
        {
            Parameter    = parameter;
            SourceColumn = sourceColumn;
            Reserve      = reserve == 0 ? 1 : reserve;

            ReserveParameter = reserveParameter;
        }

        public Operation Name => Operation.CopyMass;

        [JsonProperty("Название параметра")] public string Parameter { get; }

        [JsonProperty("Исходный столбец")] public int SourceColumn { get; }

        [JsonProperty("Коэффициент запаса")]
        [DefaultValue(1d)]
        public double Reserve { get; }

        [JsonProperty("Глобальный параметр коэффициента запаса")]
        [DefaultValue("")]
        public string ReserveParameter { get; }
    }
}