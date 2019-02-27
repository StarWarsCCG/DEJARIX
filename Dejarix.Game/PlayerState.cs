using System.Collections.Generic;
using System.Linq;

namespace Dejarix
{
    class PlayerState
    {
        private readonly List<Card>[] _lists;
        public List<Card> ReserveDeck { get; } = new List<Card>();
        public List<Card> ForcePile { get; } = new List<Card>();
        public List<Card> UsedPile { get; } = new List<Card>();
        public List<Card> LostPile { get; } = new List<Card>();
        public List<Card> OutOfPlay { get; } = new List<Card>();
        public List<Card> Table { get; } = new List<Card>();
        public List<Card> Hand { get; } = new List<Card>();
        public List<Card> Play { get; } = new List<Card>();

        public PlayerState()
        {
            _lists = new[]
            {
                ReserveDeck,
                ForcePile,
                UsedPile,
                LostPile,
                OutOfPlay,
                Table,
                Hand,
                Play
            };
        }

        public void AddAll(PlayerState other)
        {
            for (int i = 0; i < _lists.Length; ++i)
                _lists[i].AddRange(other._lists[i]);
        }

        public int CountCards()
        {
            return _lists.Sum(list => list.Count);
        }
    }
}