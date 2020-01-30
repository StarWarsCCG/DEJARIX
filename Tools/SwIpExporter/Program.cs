using System;
using System.Text.Json;
using System.Data.SQLite;
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

        private static bool InRange(char c, char low, char high) => low <= c && c <= high;

        static string GetString(string s)
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
            " (Frozen)"
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

        static async Task Main(string[] args)
        {
            try
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

                var data = new List<Dictionary<string, object>>();

                Console.WriteLine("Converting data -- " + DateTime.Now.ToString("s"));
                using (var connection = new SQLiteConnection("Data Source=swccg_db.sqlite;Version=3"))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM SWD";
                        command.Prepare();

                        using (var reader = command.ExecuteReader())
                        {
                            int idOrdinal = reader.GetOrdinal("id");
                            int uniquenessOrdinal = reader.GetOrdinal("Uniqueness");
                            int groupingOrdinal = reader.GetOrdinal("Grouping");
                            int cardTypeOrdinal = reader.GetOrdinal("CardType");
                            int cardNameOrdinal = reader.GetOrdinal("CardName");
                            int subtypeOrdinal = reader.GetOrdinal("Subtype");
                            int expansionOrdinal = reader.GetOrdinal("Expansion");

                            var darkGempLookup = GempTitles.Organized(gempTitles.DarkSide);
                            var lightGempLookup = GempTitles.Organized(gempTitles.LightSide);

                            while (reader.Read())
                            {
                                ++rowCount;
                                var fields = new Dictionary<string, object>();
                                var swIpId = reader[idOrdinal].ToString();
                                var frontId = Guid.NewGuid().ToString();
                                var grouping = reader.GetString(groupingOrdinal);
                                var isLightSide = grouping == "Light";
                                var cardType = reader.GetString(cardTypeOrdinal);
                                var cardName = WithoutSuffix(reader.GetString(cardNameOrdinal));
                                var expansion = reader.GetString(expansionOrdinal);
                                var gempLookup = isLightSide ? lightGempLookup : darkGempLookup;
                                fields.Add("ImageId", frontId.ToString());
                                fields.Add("OtherImageId", isLightSide ? lightBackId : darkBackId);
                                fields.Add("IsLightSide", isLightSide);
                                fields.Add("IsFront", true);

                                var gempExpansionId = GempExpansions[expansion];
                                var gempIdByTitle = gempLookup[gempExpansionId];
                                
                                if (gempIdByTitle.TryGetValue(cardName, out var gempId) ||
                                    (gempExpansionId == "200" && gempLookup["301"].TryGetValue(cardName, out gempId)))
                                {
                                    fields.Add("GempId", gempId);
                                }
                                else
                                {
                                    fields.Add("NoGemp", null);
                                }

                                for (int i = 0; i < reader.FieldCount; ++i)
                                {
                                    string name = i == idOrdinal ? "SwIpId" : reader.GetName(i);
                                    var value = GetString(reader[i].ToString());

                                    if (i == uniquenessOrdinal)
                                    {
                                        value = value.Replace('*', '•');
                                    }
                                    else if (i == cardNameOrdinal)
                                    {
                                        value = WithoutSuffix(value);
                                    }
                                    else if (i == subtypeOrdinal)
                                    {
                                        const string Or = " Or ";
                                        var subtype = reader[i].ToString();
                                        var option = StringSplitOptions.RemoveEmptyEntries;
                                        var subtypes = subtype.Contains(Or) ? subtype.Split(Or, option) : subtype.Split('/', option);

                                        if (swIpId == "832")
                                        {
                                            for (int j = 0; j < subtypes.Length; ++j)
                                            {
                                                // Need to cleanup sw-ip. :(
                                                if (subtypes[j] == "Jedi Master")
                                                    subtypes[j] = "Dark Jedi Master";
                                            }
                                        }
                                        fields["Subtypes"] = subtypes;
                                    }

                                    if (!string.IsNullOrWhiteSpace(value))
                                        fields.Add(name, value);
                                }

                                fields["CardNameNormalized"] = SearchNormalized(cardName);

                                data.Add(fields);

                                if (cardType == "Objective")
                                {
                                    cardName = fields["ObjectiveFrontName"].ToString();
                                    
                                    if (gempIdByTitle.TryGetValue(cardName, out gempId))
                                    {
                                        fields.Add("GempId", gempId);
                                        fields.Remove("NoGemp");
                                    }
                                    
                                    var backFields = new Dictionary<string, object>(fields);
                                    var backId = Guid.NewGuid().ToString();
                                    var backCardName = fields["ObjectiveBackName"].ToString();
                                    
                                    fields["OtherImageId"] = backId;
                                    fields["CardName"] = cardName;
                                    fields["Gametext"] = fields["ObjectiveFront"];
                                    fields["CardNameNormalized"] = SearchNormalized(cardName);
                                    fields["Destiny"] = "0";
                                    
                                    backFields["ImageId"] = backId;
                                    backFields["OtherImageId"] = frontId;
                                    backFields["IsFront"] = false;
                                    backFields["CardName"] = fields["ObjectiveBackName"];
                                    backFields["Gametext"] = fields["ObjectiveBack"];
                                    backFields["CardNameNormalized"] = SearchNormalized(backCardName);
                                    backFields["Destiny"] = "7";

                                    data.Add(backFields);
                                }
                                else if (swIpId == "5055")
                                {
                                    fields["ImageId"] = falconFront;
                                    fields["OtherImageId"] = falconBack;
                                }
                                else if (swIpId == "5056")
                                {
                                    fields["ImageId"] = falconBack;
                                    fields["OtherImageId"] = falconFront;
                                    fields["IsFront"] = false;
                                }
                                else if (swIpId == "5117")
                                {
                                    fields["ImageId"] = frozenLukeFront;
                                    fields["OtherImageId"] = frozenLukeBack;
                                }
                                else if (swIpId == "5118")
                                {
                                    fields["ImageId"] = frozenLukeBack;
                                    fields["OtherImageId"] = frozenLukeFront;
                                    fields["IsFront"] = false;
                                }
                                else if (swIpId == "1361" || swIpId == "3208")
                                {
                                    fields["OtherImageId"] = frozenHan;
                                }
                            }
                        }
                    }
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                
                using (var stream = File.Create("swccg.json"))
                    await JsonSerializer.SerializeAsync(stream, data, options);
                
                foreach (var d in data)
                {
                    if (d.TryGetValue("GempId", out var obj))
                    {
                        var gempId = obj.ToString();
                        gempTitles.DarkSide.Remove(gempId);
                        gempTitles.LightSide.Remove(gempId);
                    }
                }

                using (var stream = File.Create("gemp-missing.json"))
                    await JsonSerializer.SerializeAsync(stream, gempTitles, options);
                
                var cardCount = data.Count(d => (bool)d["IsFront"]);
                Console.WriteLine($"Converted {cardCount} cards ({rowCount} rows) to {data.Count} faces in {stopwatch.Elapsed}");

                // foreach (var cardName in data.Select(d => d["CardName"].ToString()).Where(cn => cn.EndsWith(")")))
                //     Console.WriteLine(cardName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
