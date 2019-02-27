using System.Collections.Generic;

namespace Dejarix
{
    struct CardStackAddress
    {

    }

    class GameDelta
    {
        // create/remove LocationBin at index
        // insert/remove Card at index in collection
    }

    interface ICardCommand
    {
        void PerformAction(GameState gameState);
        // string Serialize();
    }

    class GameEngine
    {
        private readonly List<GameState> _gameStates = new List<GameState>();

        public GameEngine()
        {
        }
    }
}