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
        static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        
        static readonly string[] IconSeparators = new string[] { ", ", "," };

        static readonly ImmutableHashSet<int> CardsWithAlt = ImmutableHashSet.Create(
            172,629,1806,1888,
            1981,1984,2194,2583,
            2746,5338,631,1614,
            1651,1659,1974,2213,
            2381,2535,2870,4048,
            4074,4075,4082,138,
            307,633,1023,1616,
            1801,1891,1973,2140,
            5042,1615);

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

        static async Task WriteToFileAsync<T>(string file, T item)
        {
            using (var stream = File.Create(file))
                await JsonSerializer.SerializeAsync(stream, item, SerializerOptions);
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

            var gempTitles = await Gemp.LoadAsync();
            var holotableTitles = await Holotable.LoadAsync();
            
            var cardData = new List<Dictionary<string, object>>();
            var cardsWithoutGemp = new Dictionary<string, string>();
            var cardsWithoutHolotable = new Dictionary<string, string>();
            var cardsWithAltImage = new List<int>();

            Console.WriteLine("Converting data -- " + DateTime.Now.ToString("s"));
            var darkGempLookup = Gemp.Organized(gempTitles.DarkSide);
            var lightGempLookup = Gemp.Organized(gempTitles.LightSide);
            var darkHolotableLookup = Holotable.Organized(holotableTitles.DarkSide);
            var lightHolotableLookup = Holotable.Organized(holotableTitles.LightSide);

            // await WriteToFileAsync("debug-ht-dark.json", darkHolotableLookup);
            
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
                        var holotableLookup = isLightSide ? lightHolotableLookup : darkHolotableLookup;
                        var isObjective = cardType == "Objective";
                        var isLocation = cardType == "Location";
                        var backId = isObjective ? Guid.NewGuid().ToString() : (isLightSide ? lightBackId : darkBackId);
                        var cardNameField = isObjective ? "ObjectiveFrontName" : "CardName";
                        var cardName = Suffix.Removed(element.GetProperty(cardNameField).GetString());
                        var destiny = "0";
                        
                        var fields = new Dictionary<string, object>();
                        fields.Add("ImageId", frontId.ToString());
                        fields.Add("OtherImageId", backId);
                        fields.Add("Title", cardName);
                        fields.Add("TitleNormalized", Strings.Normalized(cardName));
                        fields.Add("Expansion", expansion);
                        Maybe.AddString(fields, element, "Rarity");

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

                        fields.Add("SwIpId", swIpId);

                        var gempExpansionId = Gemp.GempExpansions[expansion];
                        var gempIdByTitle = gempLookup[gempExpansionId];
                        var gempCardName = cardName.Replace('é', 'e');
                        
                        if (gempIdByTitle.TryGetValue(gempCardName, out var gempIds) ||
                            (gempExpansionId == "200" && gempLookup["301"].TryGetValue(gempCardName, out gempIds)))
                        {
                            fields.Add("GempId", gempIds[0]);

                            if (gempIds.Length > 1)
                            {
                                Console.WriteLine($"AI : {swIpId} : {cardName}");
                                cardsWithAltImage.Add(swIpId);
                            }
                        }
                        else
                        {
                            fields.Add("GempId", null);
                            cardsWithoutGemp.Add(swIpId.ToString(), cardName);
                        }

                        var holotableExpansionId = Holotable.Expansions[expansion];
                        var holotableIdByTitle = holotableLookup[holotableExpansionId];
                        var holotableCardName = gempCardName
                            .Replace('"', '\'')
                            .ToLowerInvariant();
                        
                        if (holotableIdByTitle.TryGetValue(holotableCardName, out var holotableIds))
                        {
                            fields.Add("HolotableId", holotableIds[0]);
                        }
                        else
                        {
                            fields.Add("HolotableId", null);
                        }

                        if (isLocation)
                        {
                            Maybe.AddString(fields, element, "DarkSideText");
                            Maybe.AddString(fields, element, "LightSideText");
                            Maybe.AddInt32(fields, element, "DarkSideIcons");
                            Maybe.AddInt32(fields, element, "LightSideIcons");
                        }
                        else
                        {
                            var gametextField = isObjective ? "ObjectiveFront" : "Gametext";

                            if (element.TryGetProperty(gametextField, out var property))
                            {
                                var gametext = Strings.Filtered(property.GetString());
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
                        // fields.Add("SwIp", element.Clone());
                        cardData.Add(fields);

                        if (isObjective)
                        {
                            var backFields = new Dictionary<string, object>(fields);

                            backFields["ImageId"] = backId;
                            backFields["OtherImageId"] = frontId;
                            backFields["IsFront"] = false;

                            var title = element.GetProperty("ObjectiveBackName").GetString();
                            backFields["Title"] = title;
                            backFields["TitleNormalized"] = Strings.Normalized(title);
                            backFields["Destiny"] = "7";

                            var gametext = Strings.Filtered(element.GetProperty("ObjectiveBack").GetString());
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

                        if (CardsWithAlt.Contains(swIpId))
                        {
                            var altFields = new Dictionary<string, object>(fields);
                            altFields["ImageId"] = Guid.NewGuid().ToString();
                            altFields["GempId"] = gempIds[1];
                            altFields["HolotableId"] = holotableIds[1];
                            altFields["AlternateImageOf"] = fields["ImageId"];
                            cardData.Add(altFields);
                        }
                    }
                }
            }
            
            await WriteToFileAsync("swccg.json", cardData);
            
            foreach (var d in cardData)
            {
                if (d.TryGetValue("GempId", out var obj) && obj != null)
                {
                    var gempId = obj.ToString();
                    gempTitles.DarkSide.Remove(gempId);
                    gempTitles.LightSide.Remove(gempId);
                }
            }

            await WriteToFileAsync("gemp-missing.json", gempTitles);
            await WriteToFileAsync("sw-ip-missing.json", cardsWithoutGemp);
            
            var cardCount = cardData.Count(d => (bool)d["IsFront"]);
            Console.WriteLine(string.Join(',', cardsWithAltImage));
            Console.WriteLine($"Converted {cardCount} cards ({rowCount} rows) to {cardData.Count} faces in {stopwatch.Elapsed}");

            // foreach (var cardName in data.Select(d => d["CardName"].ToString()).Where(cn => cn.EndsWith(")")))
            //     Console.WriteLine(cardName);
        }
    }
}
