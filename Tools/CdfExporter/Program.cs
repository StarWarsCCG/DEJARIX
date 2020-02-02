using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace CdfExporter
{
    class Program
    {
        const char OldDot = (char)0xfffd; // '�'; // UTF-8 0xef bf bd, UTF-16 0xff fd
        const char NewDot = '•';
        const char DarkDot = ''; // UTF-8 0x95

        static void AddFields(List<string> list, string line)
        {
            var start = 0;

            while (start < line.Length)
            {
                char c = line[start];
                char symbol;

                if (c == ' ')
                {
                    ++start;
                    continue;
                }
                else if (c == '"')
                {
                    symbol = '"';
                    
                    if (++start >= line.Length)
                        break;
                }
                else if (c == '[')
                {
                    symbol = ']';

                    if (++start >= line.Length)
                        break;
                }
                else
                {
                    symbol = ' ';
                }
                
                var next = line.IndexOf(symbol, start);

                if (next == -1)
                {
                    list.Add(line.Substring(start));
                    break;
                }
                else
                {
                    list.Add(line.Substring(start, next - start));
                    start = next + 1 + Convert.ToInt32(symbol != ' ');
                }
            }
        }

        static async Task ReadAsync(string file, Dictionary<string, string> titles)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var writerOptions = new JsonWriterOptions
            {
                Indented = true
            };
            
            using (var stream = File.Create(file + ".json"))
            using (var writer = new Utf8JsonWriter(stream, writerOptions))
            using (var reader = File.OpenText(file))
            {
                writer.WriteStartArray();
                var fields = new List<string>();

                while (true)
                {
                    var line = await reader.ReadLineAsync();

                    if (line is null)
                        break;
                    
                    fields.Clear();
                    AddFields(fields, line);

                    if (fields.Count > 2 && fields[0] == "card")
                    {
                        var id = fields[1];
                        var path = id.Split('/', StringSplitOptions.RemoveEmptyEntries);
                        var data = fields[2].Split("\\n", StringSplitOptions.RemoveEmptyEntries);

                        var cardData = new Dictionary<string, object>
                        {
                            ["id"] = id,
                            ["data"] = data
                        };

                        var rawTitle = data[0]
                            .Replace(OldDot.ToString(), "")
                            .Replace("<", "")
                            .Replace(">", "");
                        var finish = rawTitle.LastIndexOf('(');
                        if (rawTitle[finish - 1] == ' ')
                            --finish;
                        var title = rawTitle.Substring(0, finish);
                        titles.Add(id, title);

                        JsonSerializer.Serialize(writer, cardData);
                    }

                    // if (fields.Count > 0)
                    //     JsonSerializer.Serialize(writer, fields);
                    // Console.WriteLine(JsonSerializer.Serialize(fields, options));
                }

                writer.WriteEndArray();
            }
        }
        
        static async Task Main(string[] args)
        {
            try
            {
                var titles = new Dictionary<string, string>();
                await ReadAsync("darkside.cdf", titles);
                await ReadAsync("lightside.cdf", titles);

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                using (var stream = File.Create("holotable-titles.json"))
                    await JsonSerializer.SerializeAsync(stream, titles, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
