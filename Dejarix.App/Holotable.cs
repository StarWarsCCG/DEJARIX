using System;
using System.Collections.Generic;

namespace Dejarix.App
{
    public static class Holotable
    {
        public static void AddFields(List<string> list, string line)
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
    }
}