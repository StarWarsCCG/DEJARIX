using System;

namespace Dejarix.Swccg
{
    public readonly struct GameDelta
    {
        public readonly GameState FinalState { get; }
        public readonly TimeSpan TimeOffset { get; }
        public readonly IStateChange ForwardStateChange { get; }
        public readonly IStateChange ReverseStateChange { get; }
        public readonly int Ordinal { get; }
        public readonly int ResponsibleParty { get; }
    }
}