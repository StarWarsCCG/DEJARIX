using System;

namespace Dejarix.Swccg
{
    public readonly struct TwoSidedCard
    {
        public readonly Guid FrontId { get; }
        public readonly Guid BackId { get; }

        public TwoSidedCard(Guid frontId, Guid backId)
        {
            FrontId = frontId;
            BackId = backId;
        }

        public override string ToString() => $"front {FrontId} back {BackId}";
    }
}