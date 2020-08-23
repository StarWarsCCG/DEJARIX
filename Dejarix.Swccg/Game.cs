using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Dejarix.Swccg
{
    public class Game
    {
        private readonly List<GameDelta> _gameStates = new List<GameDelta>();
        private readonly ImmutableArray<TwoSidedCard> _cardPalette;

        public Game(ImmutableArray<TwoSidedCard> cardPalette)
        {
            _cardPalette = cardPalette;
        }

        public void Push(in GameDelta gameState)
        {
            _gameStates.Add(gameState);
        }
    }
}