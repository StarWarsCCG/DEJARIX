using System.Text.Json;

namespace Dejarix.Swccg
{
    public interface IStateChange
    {
        GameState Transform(GameState gameState);
        void Serialize(Utf8JsonWriter writer);
    }

    public readonly struct StateChange
    {
        public IStateChange Forward { get; }
        public IStateChange Backward { get; }

        public StateChange(IStateChange forward, IStateChange backward)
        {
            Forward = forward;
            Backward = backward;
        }
    }
}