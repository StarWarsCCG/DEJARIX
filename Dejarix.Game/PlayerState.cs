using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Dejarix
{
    public readonly struct PlayerState
    {
        public static readonly PlayerState Empty = new PlayerState(
            ImmutableArray<Card>.Empty,
            ImmutableArray<Card>.Empty,
            ImmutableArray<Card>.Empty,
            ImmutableArray<Card>.Empty,
            ImmutableArray<Card>.Empty,
            ImmutableArray<Card>.Empty,
            ImmutableArray<Card>.Empty,
            ImmutableArray<Card>.Empty);

        public ImmutableArray<Card> ReserveDeck { get; }
        public ImmutableArray<Card> ForcePile { get; }
        public ImmutableArray<Card> UsedPile { get; }
        public ImmutableArray<Card> LostPile { get; }
        public ImmutableArray<Card> OutOfPlay { get; }
        public ImmutableArray<Card> Table { get; }
        public ImmutableArray<Card> Hand { get; }
        public ImmutableArray<Card> Play { get; }

        public int Count =>
            ReserveDeck.Length +
            ForcePile.Length +
            UsedPile.Length +
            LostPile.Length +
            OutOfPlay.Length +
            Table.Length +
            Hand.Length +
            Play.Length;

        public PlayerState(
            ImmutableArray<Card> reserveDeck,
            ImmutableArray<Card> forcePile,
            ImmutableArray<Card> usedPile,
            ImmutableArray<Card> lostPile,
            ImmutableArray<Card> outOfPlay,
            ImmutableArray<Card> table,
            ImmutableArray<Card> hand,
            ImmutableArray<Card> play)
        {
            ReserveDeck = reserveDeck;
            ForcePile = forcePile;
            UsedPile = usedPile;
            LostPile = lostPile;
            OutOfPlay = outOfPlay;
            Table = table;
            Hand = hand;
            Play = play;
        }

        public PlayerState WithReserveDeck(ImmutableArray<Card> reserveDeck)
        {
            return new PlayerState(
                reserveDeck,
                ForcePile,
                UsedPile,
                LostPile,
                OutOfPlay,
                Table,
                Hand,
                Play);
        }
    }
}