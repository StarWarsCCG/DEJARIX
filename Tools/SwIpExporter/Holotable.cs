using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace SwIpExporter
{
    public static class Holotable
    {
        public static readonly ImmutableDictionary<string, string> Expansions = new Dictionary<string, string>
        {
            ["Hoth"] = "Hoth",
            ["Premiere"] = "Premiere",
            ["Special Edition"] = "SpecialEdition",
            ["Dagobah"] = "Dagobah",
            ["Theed Palace"] = "TheedPalace",
            ["Enhanced Cloud City"] = "EnhancedCloudCity",
            ["Jabba's Palace"] = "JabbasPalace",
            ["Reflections III"] = "ReflectionsIII",
            ["Tatooine"] = "Tatooine",
            ["Third Anthology"] = "ThirdAnthology",
            ["Coruscant"] = "Coruscant",
            ["Endor"] = "Endor",
            ["Cloud City"] = "CloudCity",
            ["Virtual Card Set #0"] = "Virtual0",
            ["Reflections II"] = "ReflectionsII",
            ["Death Star II"] = "DeathStarII",
            ["A New Hope"] = "ANewHope",
            ["Virtual Defensive Shields"] = "ResetDS",
            ["Jabba's Palace Sealed Deck"] = "JabbasPalaceSealedDeck",
            ["Official Tournament Sealed Deck"] = "OfficialTournamentSealedDeck",
            ["Enhanced Premiere Pack"] = "EnhancedPremiere",
            ["Enhanced Jabba's Palace"] = "EnhancedJabbasPalace",
            ["Hoth 2 Player"] = "EmpireStrikesBackIntroductoryTwoPlayerGame",
            ["Virtual Card Set #1"] = "Virtual1",
            ["Jedi Pack"] = "JediPack",
            ["Premiere 2 Player"] = "PremiereIntroductoryTwoPlayerGame",
            ["Rebel Leader Cards"] = "RebelLeader",
            ["Virtual Card Set #2"] = "Virtual2",
            ["Virtual Card Set #3"] = "Virtual3",
            ["Virtual Card Set #4"] = "Virtual4",
            ["Virtual Card Set #5"] = "Virtual5",
            ["Demonstration Deck Premium Card Set"] = "DemoDeck",
            ["Virtual Card Set #6"] = "Virtual6",
            ["Virtual Card Set #7"] = "Virtual7",
            ["Virtual Card Set #8"] = "Virtual8",
            ["Virtual Card Set #9"] = "Virtual9",
            ["Virtual Card Set #10"] = "Virtual10",
            ["Virtual Card Set #11"] = "Virtual11"
        }.ToImmutableDictionary();

        public static Dictionary<string, Dictionary<string, string[]>> Organized(
            Dictionary<string, string> dictionary)
        {
            var result = new Dictionary<string, Dictionary<string, string[]>>();

            foreach (var pair in dictionary)
            {
                var path = pair.Key.Split(
                    '/', StringSplitOptions.RemoveEmptyEntries);
                
                var index = Array.FindIndex(path, p => p == "starwars");

                if (0 <= index)
                {
                    var title = Suffix.Removed(pair.Value).ToLowerInvariant();
                    var slashIndex = title.IndexOf('/');
                    
                    if (0 < slashIndex)
                        title = title.Substring(0, slashIndex).Trim();
                    
                    var expansion = path[index + 1]
                        .Split('-', StringSplitOptions.RemoveEmptyEntries);
                    
                    var isLightSide = expansion[1] == "Light";
                    if (!result.TryGetValue(expansion[0], out var idByTitle))
                    {
                        idByTitle = new Dictionary<string, string[]>();
                        result.Add(expansion[0], idByTitle);
                    }

                    if (idByTitle.TryGetValue(title, out var ids))
                    {
                        var n = ids.Length;
                        Array.Resize(ref ids, n + 1);
                        ids[n] = pair.Key;
                        idByTitle[title] = ids;
                        // Console.WriteLine(pair.Value + " : " + JsonSerializer.Serialize(ids));
                    }
                    else
                    {
                        ids = new string[] { pair.Key };
                        idByTitle.Add(title, ids);
                    }
                }
            }

            return result;
        }

        public static async Task<CardTitles> LoadAsync()
        {
            Dictionary<string, string> allTitles;
            using (var stream = File.OpenRead("holotable-titles.json"))
                allTitles = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream);
            
            var cardTitles = new CardTitles
            {
                DarkSide = new Dictionary<string, string>(),
                LightSide = new Dictionary<string, string>()
            };

            foreach (var pair in allTitles)
            {
                var isLightSide = pair.Key.Contains("-Light/");
                var destination = isLightSide ? cardTitles.LightSide : cardTitles.DarkSide;
                destination.Add(pair.Key, pair.Value);
            }

            return cardTitles;
        }
    }
}