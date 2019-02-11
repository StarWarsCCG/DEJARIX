using System.Collections.Generic;
using System.Linq;

namespace Dejarix
{
    class PlayerState
    {
        private readonly List<Card>[] _lists = new List<Card>[8];
        public List<Card> ReserveDeck => _lists[0];
        public List<Card> ForcePile => _lists[1];
        public List<Card> UsedPile => _lists[2];
        public List<Card> LostPile => _lists[3];
        public List<Card> OutOfPlay => _lists[4];
        public List<Card> Table => _lists[5];
        public List<Card> Hand => _lists[6];
        public List<Card> Play => _lists[7];

        public PlayerState()
        {
            for (int i = 0; i < _lists.Length; ++i)
                _lists[i] = new List<Card>();
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