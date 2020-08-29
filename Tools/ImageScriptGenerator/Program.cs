using System;
using System.IO;

namespace ImageScriptGenerator
{
    class Program
    {
        static void GenerateScript(string sourceFolder, string destinationFolder)
        {
            var sourceFiles = Directory.GetFiles(sourceFolder);
            foreach (var source in sourceFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(source);
                var destination = Path.Combine(destinationFolder, fileName + ".png");
                Console.WriteLine($"convert \"{source}\" -fuzz 75% -fill none -draw 'matte 0,0 floodfill' \"{destination}\"");
            }
        }
        static int Main(string[] args)
        {
            try
            {
                GenerateScript(args[0], args[1]);
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }
        }
    }
}
