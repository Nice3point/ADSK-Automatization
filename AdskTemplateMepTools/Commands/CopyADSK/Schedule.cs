using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace AdskTemplateMepTools.Commands.CopyADSK
{
    public class Schedule
    {
        private ObservableCollection<Operation> _operations;

        public Schedule(string name) => Name = name;

        [JsonConstructor]
        public Schedule(string name, params Operation[] operations)
        {
            Name = name;
            foreach (var operation in operations) Operations.Add(operation);
        }

        [JsonProperty("Schedule name")] public string Name { get; }

        public ObservableCollection<Operation> Operations => _operations ??= new ObservableCollection<Operation>();
    }
}