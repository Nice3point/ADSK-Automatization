using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace AdskTemplateMepTools.Utils
{
    public static class AttributeUtils
    {
        public static bool TryGetEnumMemberField<T>(string value, out T @enum) where T : Enum
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
        
        public static bool GetAttributeOfType<T>(this Enum enumVal, out T attribute) where T:Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            if (attributes.Length > 0)
            {
                attribute = (T) attributes[0];
                return true;
            }
            
            attribute = null;
            return false;
        }
        
        public static bool TryGetJsonPropertyValue(Type type, string field, out string propertyName)
        {
            var property = TypeDescriptor.GetProperties(type)[field];
            foreach (var attribute in property.Attributes)
            {
                if (attribute is not JsonPropertyAttribute jsonProperty) continue;
                propertyName = jsonProperty.PropertyName;
                return true;
            }

            propertyName = default;
            return false;
        }
    }
}