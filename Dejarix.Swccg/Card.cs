using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Dejarix
{
    public readonly struct Card
    {
        private const int IndexMask = 0x000000ff;
        private const int IndentMask = 0x0000ff00;
        private const int RotateMask = 3 << 16;
        private const int FaceMask = 1 << 18;

        public static Card Create(
            int index = 0,
            int indent = 0,
            int rotate = 0,
            bool isFaceUp = false)
        {
            return new Card(index, indent, rotate, isFaceUp);
        }

        public static ImmutableArray<Card> MakeArray(params int[] indices) => indices.MakeImmutable(i => Create(index: i));


        private readonly int _data;

        public int Index => _data & IndexMask;
        public int Indent => (_data & IndentMask) >> 8;
        public int Rotate => (_data & RotateMask) >> 16;
        public bool IsFaceUp => (_data & FaceMask) == FaceMask;

        private Card(int data) => _data = data;

        public Card(
            int index,
            int indent,
            int rotate,
            bool isFaceUp)
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
            var data = _data;
            data &= ~RotateMask;
            data |= (rotate << 16) & RotateMask;
            return new Card(data);
        }
    }
}