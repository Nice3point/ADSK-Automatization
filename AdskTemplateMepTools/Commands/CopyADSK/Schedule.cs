using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using AdskTemplateMepTools.Commands.CopyADSK.Operations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        [JsonProperty("Название спецификации")]
        public string Name { get; }

        [JsonProperty("Список операций")]
        [JsonConverter(typeof(CommandJsonConverter))]
        public ObservableCollection<IOperation> Operations => _operations ??= new ObservableCollection<IOperation>();
    }

    public class CommandJsonConverter : JsonConverter
    {
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Only read");
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var operations = new ObservableCollection<IOperation>();
            var jsonArray = JArray.Load(reader);
            foreach (var jToken in jsonArray)
            {
                if (jToken is not JObject jObject) continue;
                if (!jObject.ContainsKey(nameof(IOperation.Name))) continue;
                if (!jObject.TryGetValue(nameof(IOperation.Name), out var nameToken)) continue;
                var nameValue = nameToken.Value<string>();
                if (nameValue == null) continue;
                if (!TryGetEnumByEnumMember<Command>(nameValue, out var command))
                    if (!Enum.TryParse(nameValue, out command))
                        continue;

                IOperation operation = command switch
                {
                    Command.CopyString => jToken.ToObject<CopyStringOperation>(),
                    Command.CopyInteger => jToken.ToObject<CopyIntegerOperation>(),
                    Command.CopyDouble => jToken.ToObject<CopyDoubleOperation>(),
                    Command.CopyArea => jToken.ToObject<CopyAreaOperation>(),
                    Command.CopyVolume => jToken.ToObject<CopyVolumeOperation>(),
                    Command.CopyTemperature => jToken.ToObject<CopyTemperatureOperation>(),
                    Command.CopyMass => jToken.ToObject<CopyMassOperation>(),
                    _ => throw new ArgumentOutOfRangeException()
                };

                if (operation == null) continue;
                serializer.Populate(jObject.CreateReader(), operation);
                operations.Add(operation);
            }

            return operations;
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        private static bool TryGetEnumByEnumMember<T>(string value, out T @enum) where T : Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(EnumMemberAttribute)) is not EnumMemberAttribute attribute) continue;
                if (attribute.Value != value) continue;
                @enum = (T) field.GetValue(null);
                return true;
            }

            @enum = default;
            return false;
        }
    }
}