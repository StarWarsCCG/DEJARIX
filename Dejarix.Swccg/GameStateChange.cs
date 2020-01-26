namespace Dejarix.Swccg
{
    public abstract class GameStateChange
    {
        public string CommandString { get; }

        public GameStateChange(string commandString)
        {
            CommandString = commandString;
        }

        public abstract GameState ChangeState(GameState gameState);
    }
}