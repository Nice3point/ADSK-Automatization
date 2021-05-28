using AdskTemplateMepTools.Commands.CopyADSK.Operations;
using Newtonsoft.Json;

namespace AdskTemplateMepTools.Commands.CopyADSK
{
    public class CopyDoubleOperation : IOperation
    {
        public CopyDoubleOperation(int sourceColumn, double reserveLength, string reserveParameter)
        {
            SourceColumn = sourceColumn;
            ReserveLength = reserveLength;
            ReserveParameter = reserveParameter;
        }

        public Operation Name => Operation.CopyDouble;

        [JsonProperty("Исходный столбец")]
        public int SourceColumn { get; }
        
        [JsonProperty("Коэфициент запаса")]
        public double ReserveLength { get; }

        [JsonProperty("Название параметра запаса")]
        public string ReserveParameter { get; }
    }
    
    public class CopyAreaOperation : IOperation
    {
        public Operation Name => Operation.CopyArea;
    }
    public class CopyVolumeOperation : IOperation
    {
        public Operation Name => Operation.CopyVolume;
    }
    
    public class CopyTemperatureOperation : IOperation
    {
        public Operation Name => Operation.CopyTemperature;
    }
    public class CopyMassOperation : IOperation
    {
        public Operation Name => Operation.CopyMass;
    }
}