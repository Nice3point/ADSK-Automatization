using System;
using System.Collections.ObjectModel;
using AdskTemplateMepTools.Commands.CopyADSK.Operations;
using AdskTemplateMepTools.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdskTemplateMepTools.Commands.CopyADSK.JsonConverters
{
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
                if (!AttributeUtils.TryGetJsonPropertyValue(typeof(IOperation), nameof(IOperation.Name),out var iOperationName)) continue;
                if (!jObject.ContainsKey(iOperationName)) continue;
                if (!jObject.TryGetValue(iOperationName, out var nameToken)) continue;
                var nameValue = nameToken.Value<string>();
                if (nameValue == null) continue;
                if (!AttributeUtils.TryGetEnumMemberField<Operation>(nameValue, out var command))
                    if (!Enum.TryParse(nameValue, out command))
                        continue;

                IOperation operation = command switch
                {
                    Operation.CopyString => jToken.ToObject<CopyStringOperation>(),
                    Operation.CopyInteger => jToken.ToObject<CopyIntegerOperation>(),
                    Operation.CopyLength => jToken.ToObject<CopyLengthOperation>(),
                    Operation.CopyArea => jToken.ToObject<CopyAreaOperation>(),
                    Operation.CopyVolume => jToken.ToObject<CopyVolumeOperation>(),
                    Operation.CopyTemperature => jToken.ToObject<CopyTemperatureOperation>(),
                    Operation.CopyMass => jToken.ToObject<CopyMassOperation>(),
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
    }
}