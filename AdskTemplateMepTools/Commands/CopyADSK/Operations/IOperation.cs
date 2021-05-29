using Newtonsoft.Json;

namespace AdskTemplateMepTools.Commands.CopyADSK.Operations
{
    public interface IOperation
    {
        [JsonProperty("Название операции", DefaultValueHandling = DefaultValueHandling.Populate)]
        Operation Name { get; }
    }
}