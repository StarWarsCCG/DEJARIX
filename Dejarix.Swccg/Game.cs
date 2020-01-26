using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Dejarix.Swccg
{
    public class Game
    {
        private readonly List<GameState> _gameStates = new List<GameState>();
        private readonly ImmutableArray<Guid> _cardPalette;

        public Game(ImmutableArray<Guid> cardPalette)
        {
            _cardPalette = cardPalette;
        }
    }
}