using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Linq;
using System.Collections.Immutable;

namespace SwIpExporter
{
    class Program
    {
        static readonly ImmutableDictionary<string, string> GempExpansions = new Dictionary<string, string>
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
            ["Virtual Card Set #11"] = "211"
        }.ToImmutableDictionary();

        static readonly string[] IconSeparators = new string[] { ", ", "," };

        private static bool InRange(char c, char low, char high) => low <= c && c <= high;

        static void MaybeAddInt32(Dictionary<string, object> dictionary, JsonElement element, string key)
        {
            var value = MaybeGetInt32(element, key);

            if (value.HasValue)
                dictionary.Add(key, value);
        }
        
        static int? MaybeGetInt32(JsonElement element, string key)
        {
            if (element.TryGetProperty(key, out var property) &&
                property.ValueKind == JsonValueKind.Number)
            {
                return property.GetInt32();
            }
            else
            {
                return null;
            }
        }

        static void MaybeAddString(Dictionary<string, object> dictionary, JsonElement element, string key)
        {
            var value = MaybeGetString(element, key);

            if (value != null)
                dictionary.Add(key, value);
        }
        
        static string MaybeGetString(JsonElement element, string key)
        {
            if (element.TryGetProperty(key, out var property) &&
                property.ValueKind == JsonValueKind.String)
            {
                return property.GetString();
            }
            else
            {
                return null;
            }
        }

        static string FilterString(string s)
        {
            return s
                .Replace((char)65533, 'é')
                .Replace("1/2", "½")
                .Replace("1/4", "¼")
                .Replace("<>", "♢")
                .Replace("(*]", "•")
                .Replace("(*)", "(•)")
                .Replace("(**)", "(••)")
                .Replace("(***)", "(•••)")
                .Replace("\\par ", "\n")
                .Replace("\\b0 ", "")
                .Replace("\\b0", "")
                .Replace("\\b ", "")
                .Replace("\\ul0 ", "")
                .Replace("\\ul0", "")
                .Replace("\\ul ", "")
                .Replace("  ", " ")
                .Replace(" \n", "\n")
                ;
        }

        static string SearchNormalized(string text)
        {
            if (text == null)
                return null;
            
            var buffer = new char[text.Length];
            int n = 0;

            foreach (var c in text)
            {
                if (InRange(c, 'a', 'z') || InRange(c, '0', '9'))
                {
                    buffer[n++] = c;
                }
                else if ('A' <= c && c <= 'Z')
                {
                    buffer[n++] = (char)(c + 32);
                }
            }

            return new string(buffer, 0, n);
        }

        static readonly string[] Suffixes = new string[]
        {
            " (V)",
            " (EP1)",
            " (CC)",
            " (Frozen)",
            " (Starship)",
            " (Vehicle)"
        };

        static string WithoutSuffix(string cardName)
        {
            foreach (var suffix in Suffixes)
            {
                if (cardName.EndsWith(suffix))
                    cardName = cardName[..^suffix.Length];
            }
            
            return cardName;
        }

        static async Task<JsonDocument> ParseJsonFileAsync(string file)
        {
            using (var stream = File.OpenRead(file))
                return await JsonDocument.ParseAsync(stream);
        }

        static async Task Main(string[] args)
        {
            try
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("Must specify card JSON folder and output file.");
                }
                else
                {
                    await ExportAsync(args[0], args[1]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static async Task ExportAsync(string cardFolder, string cardFile)
        {
            var stopwatch = Stopwatch.StartNew();
            var darkBackId = "60cca2dd-a989-4c55-8a7b-cd6b86a95ce5";
            var lightBackId = "14c10fe6-199e-4b7d-9cea-1c7247e42d3e";
            
            var falconFront = Guid.NewGuid().ToString();
            var falconBack = Guid.NewGuid().ToString();
            var frozenLukeFront = Guid.NewGuid().ToString();
            var frozenLukeBack = Guid.NewGuid().ToString();
            var frozenHan = Guid.NewGuid().ToString();
            int rowCount = 0;

            GempTitles gempTitles;

            using (var stream = File.OpenRead("gemp-titles.json"))
                gempTitles = await JsonSerializer.DeserializeAsync<GempTitles>(stream);

            var cardData = new List<Dictionary<string, object>>();
            var cardsWithoutGemp = new Dictionary<string, string>();

            Console.WriteLine("Converting data -- " + DateTime.Now.ToString("s"));
            var darkGempLookup = GempTitles.Organized(gempTitles.DarkSide);
            var lightGempLookup = GempTitles.Organized(gempTitles.LightSide);
            
            foreach (var file in Directory.GetFiles(cardFolder))
            {
                using (var document = await ParseJsonFileAsync(file))
                {
                    foreach (var element in document.RootElement.EnumerateArray())
                    {
                        ++rowCount;
                        var swIpId = element.GetProperty("id").GetInt32();
                        var frontId = Guid.NewGuid().ToString();
                        var grouping = element.GetProperty("Grouping").GetString();
                        var isLightSide = grouping == "Light";
                        var cardType = element.GetProperty("CardType").GetString();
                        var expansion = element.GetProperty("Expansion").GetString();
                        var gempLookup = isLightSide ? lightGempLookup : darkGempLookup;
                        var isObjective = cardType == "Objective";
                        var isLocation = cardType == "Location";
                        var backId = isObjective ? Guid.NewGuid().ToString() : (isLightSide ? lightBackId : darkBackId);
                        var cardNameField = isObjective ? "ObjectiveFrontName" : "CardName";
                        var cardName = WithoutSuffix(element.GetProperty(cardNameField).GetString());
                        var destiny = "0";
                        
                        var fields = new Dictionary<string, object>();
                        fields.Add("ImageId", frontId.ToString());
                        fields.Add("OtherImageId", backId);
                        fields.Add("Title", cardName);
                        fields.Add("TitleNormalized", SearchNormalized(cardName));
                        fields.Add("Expansion", expansion);
                        MaybeAddString(fields, element, "Rarity");

                        if (element.TryGetProperty("Destiny", out var destinyProperty))
                        {
                            if (destinyProperty.ValueKind == JsonValueKind.Number)
                                destiny = destinyProperty.GetDouble().ToString();
                            else
                                destiny = destinyProperty.GetString().Replace("pi", "π");
                        }

                        fields.Add("Destiny", destiny);
                        
                        if (element.TryGetProperty("Uniqueness", out var uniquenessElement))
                        {
                            var uniqueness = uniquenessElement.GetString();
                            fields.Add("Uniqueness", uniqueness.Replace('*', '•').Replace("<>", "♢"));
                        }

                        fields.Add("PrimaryType", cardType);

                        var secondaryTypes = Array.Empty<string>();
                        if (element.TryGetProperty("Subtype", out var subtypeElement))
                        {
                            const string Or = " Or ";
                            var subtype = subtypeElement.GetString();
                            secondaryTypes = subtype.Contains(Or) ?
                                subtype.Split(Or, StringSplitOptions.RemoveEmptyEntries) :
                                subtype.Split('/', StringSplitOptions.RemoveEmptyEntries);
                        }

                        fields.Add("SecondaryTypes", secondaryTypes);
                        fields.Add("IsLightSide", isLightSide);
                        fields.Add("IsFront", true);

                        var gempExpansionId = GempExpansions[expansion];
                        var gempIdByTitle = gempLookup[gempExpansionId];
                        var gempCardName = cardName.Replace('é', 'e');
                        
                        if (gempIdByTitle.TryGetValue(gempCardName, out var gempId) ||
                            (gempExpansionId == "200" && gempLookup["301"].TryGetValue(gempCardName, out gempId)))
                        {
                            fields.Add("GempId", gempId);
                        }
                        else
                        {
                            fields.Add("GempId", null);
                            cardsWithoutGemp.Add(swIpId.ToString(), cardName);
                        }

                        if (isLocation)
                        {
                            MaybeAddString(fields, element, "DarkSideText");
                            MaybeAddString(fields, element, "LightSideText");
                            MaybeAddInt32(fields, element, "DarkSideIcons");
                            MaybeAddInt32(fields, element, "LightSideIcons");
                        }
                        else
                        {
                            var gametextField = isObjective ? "ObjectiveFront" : "Gametext";

                            if (element.TryGetProperty(gametextField, out var property))
                            {
                                var gametext = FilterString(property.GetString());
                                fields.Add("Gametext", gametext);
                            }
                        }

                        var icons = Array.Empty<string>();
                        if (element.TryGetProperty("Icons", out var iconsElement) && iconsElement.ValueKind == JsonValueKind.String)
                        {
                            var iconsText = iconsElement.GetString();
                            icons = iconsText.Split(IconSeparators, StringSplitOptions.RemoveEmptyEntries);
                        }

                        fields.Add("Icons", icons);
                        fields.Add("SwIp", element.Clone());
                        cardData.Add(fields);

                        if (isObjective)
                        {
                            var backFields = new Dictionary<string, object>(fields);

                            backFields["ImageId"] = backId;
                            backFields["OtherImageId"] = frontId;
                            backFields["IsFront"] = false;

                            var title = element.GetProperty("ObjectiveBackName").GetString();
                            backFields["Title"] = title;
                            backFields["TitleNormalized"] = SearchNormalized(title);
                            backFields["Destiny"] = "7";

                            var gametext = FilterString(element.GetProperty("ObjectiveBack").GetString());
                            fields["Gametext"] = gametext;

                            cardData.Add(backFields);
                        }
                        else if (swIpId == 5055)
                        {
                            fields["ImageId"] = falconFront;
                            fields["OtherImageId"] = falconBack;
                        }
                        else if (swIpId == 5056)
                        {
                            fields["ImageId"] = falconBack;
                            fields["OtherImageId"] = falconFront;
                            fields["IsFront"] = false;
                        }
                        else if (swIpId == 5117)
                        {
                            fields["ImageId"] = frozenLukeFront;
                            fields["OtherImageId"] = frozenLukeBack;
                        }
                        else if (swIpId == 5118)
                        {
                            fields["ImageId"] = frozenLukeBack;
                            fields["OtherImageId"] = frozenLukeFront;
                            fields["IsFront"] = false;
                        }
                        else if (swIpId == 1361 || swIpId == 3208)
                        {
                            fields["OtherImageId"] = frozenHan;
                        }
                    }
                }
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            
            using (var stream = File.Create("swccg.json"))
                await JsonSerializer.SerializeAsync(stream, cardData, options);
            
            foreach (var d in cardData)
            {
                if (d.TryGetValue("GempId", out var obj) && obj != null)
                {
                    var gempId = obj.ToString();
                    gempTitles.DarkSide.Remove(gempId);
                    gempTitles.LightSide.Remove(gempId);
                }
            }

            using (var stream = File.Create("gemp-missing.json"))
                await JsonSerializer.SerializeAsync(stream, gempTitles, options);
            
            using (var stream = File.Create("sw-ip-missing.json"))
                await JsonSerializer.SerializeAsync(stream, cardsWithoutGemp, options);
            
            var cardCount = cardData.Count(d => (bool)d["IsFront"]);
            Console.WriteLine($"Converted {cardCount} cards ({rowCount} rows) to {cardData.Count} faces in {stopwatch.Elapsed}");

            // foreach (var cardName in data.Select(d => d["CardName"].ToString()).Where(cn => cn.EndsWith(")")))
            //     Console.WriteLine(cardName);
        }
    }
}
