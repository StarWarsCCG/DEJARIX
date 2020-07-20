using System.Text.Json;

namespace Dejarix.App
{
    public static class JsonExtensions
    {
        public static string MaybeGetString(this JsonElement element)
        {
            return element.ValueKind == JsonValueKind.String ? element.GetString() : null;
        }

        public static int? MaybeGetInt32(this JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Number ? element.GetInt32() : default;
        }
    }
}