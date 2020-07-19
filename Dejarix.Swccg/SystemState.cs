using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Dejarix.Swccg
{
    public readonly struct SystemState
    {
        public static readonly SystemState Empty = new SystemState(
            LocationState.Empty,
            ImmutableArray<LocationState>.Empty,
            ImmutableArray<LocationState>.Empty);

        public readonly LocationState System { get; }
        public readonly ImmutableArray<LocationState> Sectors { get; }
        public readonly ImmutableArray<LocationState> Sites { get; }

        public readonly int Count
        {
            get
            {
                var sum = System.Count;

                foreach (var ls in Sectors)
                    sum += ls.Count;
                
                foreach (var ls in Sites)
                    sum += ls.Count;
                
                return sum;
            }
        }

        public SystemState(
            LocationState system,
            ImmutableArray<LocationState> sectors,
            ImmutableArray<LocationState> sites)
        {
            System = system;
            Sectors = sectors;
            Sites = sites;
        }
    }
}