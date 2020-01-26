using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using Dejarix.Swccg;

namespace Dejarix.Experimental
{
    class Program
    {
        static void SampleGameChanges()
        {
            var darkSide = PlayerState.Empty.WithReserveDeck(Card.MakeArray(5, 9, 11));
            var lightSide = PlayerState.Empty.WithReserveDeck(Card.MakeArray(6, 12, 18));
            var state = new GameState(ImmutableArray<SystemState>.Empty, darkSide, lightSide);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            Console.WriteLine("Starting card count: " + state.Count);
            Console.WriteLine(JsonSerializer.Serialize(state, options));
        }

        static void Main(string[] args)
        {
            // var test = default(ImmutableArray<Card>);
            // test = test.Add(default);
            // Console.WriteLine(test.Length);

            // var array = default(ImmutableArray<int>);
            // Console.WriteLine(array.Length);

            var random = new Random();

            for (int i = 0; i < 8; ++i)
            {
                var card = new Card(
                    random.Next(256),
                    random.Next(64),
                    random.Next(4),
                    random.Next(2) == 0);
                
                Console.WriteLine(card);
            }

            var list = new List<int>();
            for (int i = 0; i < 16; ++i)
                list.Add(i);
            
            Console.WriteLine(string.Join(", ", list));
            
            for (int i = 0; i < 8; ++i)
            {
                list.Shuffle(random.Next);
                Console.WriteLine(string.Join(", ", list));
            }
            

            SampleGameChanges();
        }
    }
}
