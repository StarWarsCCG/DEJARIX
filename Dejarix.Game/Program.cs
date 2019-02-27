using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace Dejarix
{
    class Program
    {
        static void SampleGameChanges()
        {
            var state = new GameState();
            state.DarkSide.ReserveDeck.AddRange(Card.Make(5, 9, 11));
            state.LightSide.ReserveDeck.AddRange(Card.Make(6, 12, 18));

            Console.WriteLine("Starting card count: " + state.CountCards());
            Console.WriteLine(
                JsonConvert.SerializeObject(state, Formatting.Indented));
        }

        static void Main(string[] args)
        {
            // var test = default(ImmutableArray<Card>);
            // test = test.Add(default);
            // Console.WriteLine(test.Length);

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
