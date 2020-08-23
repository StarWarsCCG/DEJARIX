using System.Text.Json;

namespace Dejarix.Swccg
{
    public interface IStateChange
    {
        GameState Transform(GameState gameState);
        void Serialize(Utf8JsonWriter writer, JsonSerializerOptions options);
    }
}