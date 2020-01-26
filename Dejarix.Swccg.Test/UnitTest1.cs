using System;
using Xunit;

namespace Dejarix.Swccg.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var ps = PlayerState.Empty;
            Assert.True(ps.ReserveDeck.IsDefaultOrEmpty);
        }
    }
}
