using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Dejarix
{
    public readonly struct SystemState
    {
        public static readonly SystemState Empty = new SystemState(
            LocationState.Empty,
            ImmutableArray<LocationState>.Empty,
            ImmutableArray<LocationState>.Empty);

        public LocationState System { get; }
        public ImmutableArray<LocationState> Sectors { get; }
        public ImmutableArray<LocationState> Sites { get; }

        public int Count
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