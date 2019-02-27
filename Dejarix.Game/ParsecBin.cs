using System.Collections.Generic;
using System.Linq;

namespace Dejarix
{
    class ParsecBin
    {
        public Location System { get; set; }
        public List<Location> Sectors { get; } = new List<Location>();
        public List<Location> Sites { get; } = new List<Location>();

        public ParsecBin DeepClone()
        {
            var result = new ParsecBin();
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
}