using System.Collections.Generic;
using System.Linq;

namespace Dejarix
{
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
}