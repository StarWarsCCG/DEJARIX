using System.Collections.Generic;
using System.Collections.Immutable;

namespace Dejarix
{
    public readonly struct LocationState
    {
        public static readonly LocationState Empty = new LocationState(
            ImmutableArray<Card>.Empty,
            ImmutableArray<Card>.Empty,
            ImmutableArray<Card>.Empty,
            ImmutableArray<Card>.Empty);
        
        public ImmutableArray<Card> LocationStack { get; }
        public ImmutableArray<Card> DarkSide { get; }
        public ImmutableArray<Card> LightSide { get; }
        public ImmutableArray<Card> Attach { get; }

        public int Count =>
            LocationStack.Length +
            DarkSide.Length +
            LightSide.Length +
            Attach.Length;

        public LocationState(
            ImmutableArray<Card> locationStack,
            ImmutableArray<Card> darkSide,
            ImmutableArray<Card> lightSide,
            ImmutableArray<Card> attach)
        {
            LocationStack = locationStack;
            DarkSide = darkSide;
            LightSide = lightSide;
            Attach = attach;
        }
    }
}