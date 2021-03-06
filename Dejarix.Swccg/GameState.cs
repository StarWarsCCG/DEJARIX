using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Dejarix.Swccg
{
    public readonly struct GameState
    {
        public static readonly GameState Empty = new GameState(
            ImmutableArray<SystemState>.Empty,
            PlayerState.Empty,
            PlayerState.Empty);
        
        public readonly ImmutableArray<SystemState> Systems { get; }
        public readonly PlayerState DarkSide { get; }
        public readonly PlayerState LightSide { get; }

        public readonly int Count
        {
            get
            {
                int sum = DarkSide.Count + LightSide.Count;

                foreach (var bin in Systems)
                    sum += bin.Count;
                
                return sum;
            }
        }

        public GameState(
            ImmutableArray<SystemState> systems,
            PlayerState darkSide,
            PlayerState lightSide)
        {
            Systems = systems;
            DarkSide = darkSide;
            LightSide = lightSide;
        }
    }
}