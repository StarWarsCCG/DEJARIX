using System;
using System.Collections.Generic;

namespace Dejarix.App
{
    static class Extensions
    {
        private const string HexDigits = "0123456789abcdef";

        private static bool InRange(char c, char low, char high) => low <= c && c <= high;

        public static string ToHex(this byte[] bytes)
        {
            return string.Create(bytes.Length * 2, bytes, (span, state) =>
            {
                int index = 0;
                foreach (var b in state)
                {
                    int firstDigit = (b >> 4) & 0x0f;
                    int secondDigit = b & 0x0f;

                    span[index++] = HexDigits[firstDigit];
                    span[index++] = HexDigits[secondDigit];
                }
            });
        }

        private static int FromHex(char c)
        {
            if (InRange(c, '0', '9'))
                return c - '0';
            else if (InRange(c, 'a', 'f'))
                return c - 'a' + 10;
            else if (InRange(c, 'A', 'F'))
                return c - 'A' + 10;
            else
                throw new ArgumentException("Invalid hex digit: " + c);
        }

        public static byte[] AsHex(this string text)
        {
            var result = new byte[text.Length / 2];

            for (int i = 0; i < result.Length; ++i)
            {
                int textIndex = i * 2;
                int a = FromHex(text[textIndex]);
                int b = FromHex(text[textIndex + 1]);
                int digit = (a << 4) | b;
                result[i] = (byte)digit;
            }

            return result;
        }

        public static IEnumerable<T> Yield<T>(this T value)
        {
            yield return value;
        }

        public static string NormalizedForSearch(this string text)
        {
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
    }
}