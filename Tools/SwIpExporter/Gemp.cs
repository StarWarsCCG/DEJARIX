using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace SwIpExporter
{
    public class Gemp
    {
        public static readonly ImmutableDictionary<string, string> Expansions = new Dictionary<string, string>
        {
            ["Hoth"] = "3",
            ["Premiere"] = "1",
            ["Special Edition"] = "7",
            ["Dagobah"] = "4",
            ["Theed Palace"] = "14",
            ["Enhanced Cloud City"] = "109",
            ["Jabba's Palace"] = "6",
            ["Reflections III"] = "13",
            ["Tatooine"] = "11",
            ["Third Anthology"] = "111",
            ["Coruscant"] = "12",
            ["Endor"] = "8",
            ["Cloud City"] = "5",
            ["Virtual Card Set #0"] = "200",
            ["Reflections II"] = "10",
            ["Death Star II"] = "9",
            ["A New Hope"] = "2",
            ["Virtual Defensive Shields"] = "200",
            ["Jabba's Palace Sealed Deck"] = "112",
            ["Official Tournament Sealed Deck"] = "106",
            ["Enhanced Premiere Pack"] = "108",
            ["Enhanced Jabba's Palace"] = "110",
            ["Hoth 2 Player"] = "104",
            ["Virtual Card Set #1"] = "201",
            ["Jedi Pack"] = "102",
            ["Premiere 2 Player"] = "101",
            ["Rebel Leader Cards"] = "103",
            ["Virtual Card Set #2"] = "202",
            ["Virtual Card Set #3"] = "203",
            ["Virtual Card Set #4"] = "204",
            ["Virtual Card Set #5"] = "205",
            ["Demonstration Deck Premium Card Set"] = "301",
            ["Virtual Card Set #6"] = "206",
            ["Virtual Card Set #7"] = "207",
            ["Virtual Card Set #8"] = "208",
            ["Virtual Card Set #9"] = "209",
            ["Virtual Card Set #10"] = "210",
            ["Virtual Card Set #11"] = "211",
            ["Virtual Premium Set"] = "301"
        }.ToImmutableDictionary();

        public static Dictionary<string, Dictionary<string, string[]>> Organized(
            Dictionary<string, string> dictionary)
        {
            var result = new Dictionary<string, Dictionary<string, string[]>>();

            foreach (var pair in dictionary)
            {
                int n = pair.Key.IndexOf('_');
                var expansionId = pair.Key.Substring(0, n);

                if (!result.TryGetValue(expansionId, out var idByTitle))
                {
                    idByTitle = new Dictionary<string, string[]>();
                    result.Add(expansionId, idByTitle);
                }

                if (idByTitle.TryGetValue(pair.Value, out var ids))
                {
                    var nn = ids.Length;
                    Array.Resize(ref ids, nn + 1);
                    ids[nn] = pair.Key;
                    idByTitle[pair.Value] = ids;
                }
                else
                {
                    idByTitle.Add(pair.Value, new string[] { pair.Key });
                }
            }

            foreach (var value in Expansions.Values)
            {
                if (!result.ContainsKey(value))
                    result.Add(value, new Dictionary<string, string[]>());
            }

            return result;
        }

        public static async Task<CardTitles> LoadAsync()
        {
            using (var stream = File.OpenRead("gemp-titles.json"))
                return await JsonSerializer.DeserializeAsync<CardTitles>(stream);
        }
    }
}