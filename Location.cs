using System.Collections.Generic;

namespace Dejarix
{
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
}