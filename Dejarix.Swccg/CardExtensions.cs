using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Dejarix
{
    public static class CardExtensions
    {
        public static ImmutableArray<TOutput> MakeImmutable<TInput, TOutput>(
            this TInput[] array,
            Converter<TInput, TOutput> converter)
        {
            var builder = ImmutableArray.CreateBuilder<TOutput>(array.Length);

            for (int i = 0; i < array.Length; ++i)
                builder.Add(converter(array[i]));

            return builder.MoveToImmutable();
        }

        public static void Shuffle<T>(
            this List<T> list,
            Func<int, int> rng)
        {
            for (int i = list.Count - 1; i > 0; --i)
            {
                int swapIndex = rng(i);
                var swapValue = list[i];
                list[i] = list[swapIndex];
                list[swapIndex] = swapValue;
            }
        }

        public static ImmutableArray<T> Shuffled<T>(
            this ImmutableArray<T> array,
            Func<int, int> rng,
            int iterationCount = 1)
        {
            if (array.IsDefaultOrEmpty)
            {
                return array;
            }
            else
            {
                var builder = ImmutableArray.CreateBuilder<T>(array.Length);
                builder.AddRange(array);

                for (int i = 0; i < iterationCount; ++i)
                {
                    for (int j = builder.Count - 1; j > 0; --j)
                    {
                        int swapIndex = rng(j);
                        var swapValue = builder[j];
                        builder[j] = builder[swapIndex];
                        builder[swapIndex] = swapValue;
                    }
                }

                return builder.MoveToImmutable();
            }
        }
    }
}