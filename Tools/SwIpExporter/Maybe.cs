using System.Collections.Generic;
using System.Text.Json;

namespace SwIpExporter
{
    public static class Maybe
    {
        public static void AddInt32(Dictionary<string, object> dictionary, JsonElement element, string key)
        {
            var value = GetInt32(element, key);

            if (value.HasValue)
                dictionary.Add(key, value);
        }
        
        public static int? GetInt32(JsonElement element, string key)
        {
            if (element.TryGetProperty(key, out var property) &&
                property.ValueKind == JsonValueKind.Number)
            {
                return property.GetInt32();
            }
            else
            {
                return null;
            }
        }

        public static void AddString(Dictionary<string, object> dictionary, JsonElement element, string key)
        {
            var value = GetString(element, key);

            if (value != null)
                dictionary.Add(key, value);
        }
        
        static string GetString(JsonElement element, string key)
        {
            if (element.TryGetProperty(key, out var property) &&
                property.ValueKind == JsonValueKind.String)
            {
                return property.GetString();
            }
            else
            {
                return null;
            }
        }
    }
}