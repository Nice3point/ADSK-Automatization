using Newtonsoft.Json;

namespace AdskTemplateMepTools.Commands.CopyADSK.Operations
{
    public interface IOperation
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        Operation Name { get; }
    }
}