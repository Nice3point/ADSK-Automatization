using System.Collections.ObjectModel;
using AdskTemplateMepTools.Commands.CopyADSK.Operations;
using Newtonsoft.Json;

namespace AdskTemplateMepTools.Commands.CopyADSK
{
    public class Schedule
    {
        private ObservableCollection<IOperation> _operations;

        public Schedule(string name)
        {
            Name = name;
        }

        public Schedule(string name, params IOperation[] operations)
        {
            Name = name;
            foreach (var operation in operations) Operations.Add(operation);
        }

        [JsonConstructor]
        public Schedule(string name, ObservableCollection<IOperation> operations)
        {
            _operations = operations;
            Name = name;
        }

        [JsonProperty("Название спецификации")] public string Name { get; }

        [JsonProperty("Список операций")]
        [JsonConverter(typeof(CommandJsonConverter))]
        public ObservableCollection<IOperation> Operations => _operations ??= new ObservableCollection<IOperation>();
    }
}