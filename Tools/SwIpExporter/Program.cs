using System;
using System.Text.Json;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text;

namespace SwIpExporter
{
    class Program
    {
        static string GetString(string s)
        {
            // if (s.StartsWith("Tatooine: Watto"))
            // {
            //     var array = s.ToCharArray();
            //     var ints = Array.ConvertAll(array, c => (int)c);
            //     int debug = 1337;
            // }
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
                .Replace("\\b ", "")
                .Replace("\\b0 ", "")
                .Replace("\\b0", "");
        }

        static async Task Main(string[] args)
        {
            try
            {
                var darkBackId = "60cca2dd-a989-4c55-8a7b-cd6b86a95ce5";
                var lightBackId = "14c10fe6-199e-4b7d-9cea-1c7247e42d3e";
                var data = new List<Dictionary<string, object>>();

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

                            while (reader.Read())
                            {
                                var fields = new Dictionary<string, object>();
                                var id = Guid.NewGuid();
                                var grouping = reader.GetString(groupingOrdinal);
                                var isLightSide = grouping == "Light";
                                fields.Add("ImageId", id.ToString());
                                fields.Add("OtherImageId", isLightSide ? lightBackId : darkBackId);
                                fields.Add("IsLightSide", isLightSide);
                                fields.Add("IsFront", true);

                                for (int i = 0; i < reader.FieldCount; ++i)
                                {
                                    // if (i == idOrdinal && reader.GetInt32(i) == 2146)
                                    // {
                                    //     await Task.Yield();
                                    // }
                                    string name = i == idOrdinal ? "SwIpId" : reader.GetName(i);
                                    var value = GetString(reader[i].ToString());

                                    if (i == uniquenessOrdinal)
                                        value = value.Replace('*', '•');

                                    if (!string.IsNullOrWhiteSpace(value))
                                        fields.Add(name, value);
                                }

                                data.Add(fields);
                                // Console.WriteLine(reader[0]);
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
