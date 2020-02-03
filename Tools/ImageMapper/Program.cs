using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace ImageMapper
{
    static class Program
    {
        static readonly char[] BadChars = new char[] { ',', '\'', '!', ':', '(', ')', '?', '.', '"' };

        static string Cleaned(this string s)
        {
            int index = s.IndexOf(" (");
            if (0 <= index)
                s = s.Substring(0, index);
            
            var array = s.ToCharArray();
            int n = 0;

            foreach (var c in s)
            {
                if (Array.IndexOf(BadChars, c) == -1)
                    array[n++] = c;
            }

            return new string(array, 0, n);
        }
        
        static async Task MapAsync(string cardFile, string sourceFolder, string destinationFolder)
        {
            JsonDocument document;
            using (var stream = File.OpenRead(cardFile))
                document = await JsonDocument.ParseAsync(stream);
            
            var missing = new List<string>();
            
            using (document)
            using (var renameWriter = File.CreateText("rename.sh"))
            using (var convertWriter = File.CreateText("convert.sh"))
            {
                foreach (var element in document.RootElement.EnumerateArray())
                {
                    var imageId = element.GetProperty("ImageId").GetString();
                    var title = element.GetProperty("Title").GetString().Cleaned();
                    var expansion = element.GetProperty("Expansion").GetString().Cleaned();
                    var isLightSide = element.GetProperty("IsLightSide").GetBoolean();
                    var grouping = isLightSide ? "Light" : "Dark";
                    // var cardType = element.GetProperty("CardType").GetString();
                    var file = title + ".tif";
                    var folder = $"{expansion}-{grouping}";
                    var fullPath = Path.Combine(sourceFolder, folder, file);

                    if (File.Exists(fullPath))
                    {
                        var newFile = imageId + ".tif";
                        var destination = Path.Combine(destinationFolder, newFile);
                        var withoutExtension = Path.GetFileNameWithoutExtension(destination);
                        var converted = withoutExtension + ".png";
                        await Task.WhenAll(
                            renameWriter.WriteLineAsync($"cp -v -n \"{fullPath}\" \"{destination}\""),
                            convertWriter.WriteLineAsync($"convert \"{destination}\" -resize 370x512 \"{converted}\""));
                    }
                    else
                    {
                        missing.Add(fullPath);
                        Console.WriteLine("Missing file: " + fullPath);
                    }
                }
            }
            
            await File.WriteAllLinesAsync("missing.txt", missing);
        }

        static async Task Main(string[] args)
        {
            try
            {
                await MapAsync(args[0], args[1], args[2]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
