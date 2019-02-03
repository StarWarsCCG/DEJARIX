using System.Collections.Generic;

namespace DejarixGame
{
    static class CommandCode
    {
        public const byte Noop = 0x00;
        public const byte SetArray = 0x01;
        public const byte RemoveAtIndex = 0x02;
        public const byte InsertAtIndex = 0x03;
        public const byte Pop = 0x04;
        public const byte Push = 0x05;
    }

    struct CardStackAddress
    {

    }

    class GameDelta
    {
        // create/remove LocationBin at index
        // insert/remove Card at index in collection
    }

    class GameEngine
    {
        private readonly List<GameState> _gameStates = new List<GameState>();

        public GameEngine()
        {
        }
    }
}