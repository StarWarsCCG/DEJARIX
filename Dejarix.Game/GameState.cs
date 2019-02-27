using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Dejarix
{
    static class CardExtensions
    {
        public static void Shuffle<T>(
            this List<T> list,
            Func<int, int> rng)
        {
            for (int i = list.Count - 1; i > 0; --i)
            {
                int swapIndex = rng(i);
                var swapValue = list[i];
                list[i] = list[swapIndex];
                list[swapIndex] = swapValue;
            }
        }
    }

    class GameState
    {
        public List<ParsecBin> Locations { get; } = new List<ParsecBin>();
        public PlayerState DarkSide { get; } = new PlayerState();
        public PlayerState LightSide { get; } = new PlayerState();

        public int CountCards()
        {
            return
                Locations.Sum(bin => bin.CountCards()) +
                DarkSide.CountCards() +
                LightSide.CountCards();
        }

        public GameState DeepClone()
        {
            var result = new GameState();
            result.DarkSide.AddAll(DarkSide);
            result.LightSide.AddAll(LightSide);

            return result;
        }
    }
}