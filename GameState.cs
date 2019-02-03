using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DejarixGame
{
    static class CardExtensions
    {
        public static ImmutableArray<T> Shuffled<T>(
            this ImmutableArray<T> array,
            Func<int, int> rng)
        {
            var builder = array.ToBuilder();
            int last = array.Length - 1;
            for (int i = 0; i < last; ++i)
            {
                int swapIndex = rng(array.Length - i) + i;
                var swapValue = builder[i];
                builder[i] = builder[swapIndex];
                builder[swapIndex] = swapValue;
            }

            return builder.MoveToImmutable();
        }

        public static ImmutableArray<T> Pop<T>(
            this ImmutableArray<T> array)
        {
            return array.RemoveAt(array.Length - 1);
        }

        public static ImmutableArray<T> Pop<T>(
            this ImmutableArray<T> array,
            out T value)
        {
            int index = array.Length - 1;
            value = array[index];
            return array.RemoveAt(index);
        }
    }

    struct Card
    {
        private const int IndexMask = 0x000000ff;
        private const int IndentMask = 0x0000ff00;
        private const int RotateMask = 3 << 16;
        private const int FaceMask = 1 << 18;

        public static IEnumerable<Card> Make(params int[] indices)
        {
            return indices.Select(i => new Card(i));
        }

        private int _data;

        public int Index => _data & IndexMask;
        public int Indent => (_data & IndentMask) >> 8;
        public int Rotate => (_data & RotateMask) >> 16;
        public bool IsFaceUp => (_data & FaceMask) == FaceMask;

        public Card(
            int index,
            int indent = 0,
            int rotate = 0,
            bool isFaceUp = false)
        {
            _data =
                (index & IndexMask) |
                ((indent << 8) & IndentMask) |
                ((rotate << 16) & RotateMask) |
                (isFaceUp ? FaceMask : 0);
        }

        public override string ToString()
        {
            string face = IsFaceUp ? "up" : "down";
            return $"card {Index} indent {Indent} rotate {Rotate} face {face}";
        }

        public Card Rotated(int rotate)
        {
            var result = this;
            result._data &= ~RotateMask;
            result._data |= (rotate << 16) & RotateMask;
            return result;
        }
    }

    class Location
    {
        public List<Card> LocationStack { get; } = new List<Card>();
        public List<Card> DarkSide { get; } = new List<Card>();
        public List<Card> LightSide { get; } = new List<Card>();
        public List<Card> Attach { get; } = new List<Card>();

        public Location DeepClone()
        {
            var result = new Location();
            result.LocationStack.AddRange(LocationStack);
            result.DarkSide.AddRange(DarkSide);
            result.LightSide.AddRange(LightSide);
            result.Attach.AddRange(Attach);
            return result;
        }

        public int CountCards()
        {
            return
                LocationStack.Count +
                DarkSide.Count +
                LightSide.Count +
                Attach.Count;
        }
    }

    class LocationBin
    {
        public Location System { get; set; }
        public List<Location> Sectors { get; } = new List<Location>();
        public List<Location> Sites { get; } = new List<Location>();

        public LocationBin DeepClone()
        {
            var result = new LocationBin();
            result.System = System?.DeepClone();
            result.Sectors.AddRange(Sectors);
            result.Sites.AddRange(Sites);
            return result;
        }

        public int CountCards()
        {
            int systemCount = System == null ? 0 : System.CountCards();

            return
                systemCount +
                Sectors.Sum(sector => sector.CountCards()) +
                Sites.Sum(site => site.CountCards());
        }
    }

    class PlayerState
    {
        public List<Card> ReserveDeck { get; } = new List<Card>();
        public List<Card> ForcePile { get; } = new List<Card>();
        public List<Card> UsedPile { get; } = new List<Card>();
        public List<Card> LostPile { get; } = new List<Card>();
        public List<Card> OutOfPlay { get; } = new List<Card>();
        public List<Card> Table { get; } = new List<Card>();
        public List<Card> Hand { get; } = new List<Card>();
        public List<Card> Play { get; } = new List<Card>();

        public void AddAll(PlayerState other)
        {
            ReserveDeck.AddRange(other.ReserveDeck);
            ForcePile.AddRange(other.ForcePile);
            UsedPile.AddRange(other.UsedPile);
            LostPile.AddRange(other.LostPile);
            OutOfPlay.AddRange(other.OutOfPlay);
            Table.AddRange(other.Table);
            Hand.AddRange(other.Hand);
            Play.AddRange(other.Play);
        }

        public int CountCards()
        {
            return
                ReserveDeck.Count +
                ForcePile.Count +
                UsedPile.Count +
                LostPile.Count +
                OutOfPlay.Count +
                Table.Count +
                Hand.Count +
                Play.Count;
        }
    }

    class GameState
    {
        public List<LocationBin> Locations { get; } = new List<LocationBin>();
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