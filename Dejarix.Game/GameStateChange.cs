namespace Dejarix
{
    public abstract class GameStateChange
    {
        public static GameStateChange FromCommandString(string commandString)
        {
            return null;
        }

        public string CommandString { get; }

        public GameStateChange(string commandString)
        {
            CommandString = commandString;
        }

        public abstract GameState ChangeState(GameState gameState);
    }
}