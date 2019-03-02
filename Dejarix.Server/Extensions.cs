using System;
using System.Collections.Generic;

namespace Dejarix.Server
{
    static class Extensions
    {
        private const string HexDigits = "0123456789abcdef";
        public static string ToHex(this byte[] bytes)
        {
            var result = new char[bytes.Length * 2];

            int index = 0;
            foreach (var b in bytes)
            {
                int firstDigit = (b >> 4) & 0x0f;
                int secondDigit = b & 0x0f;

                result[index++] = HexDigits[firstDigit];
                result[index++] = HexDigits[secondDigit];
            }

            return new string(result);
        }

        private static int FromHex(char c)
        {
            if ('0' <= c && c <= '9')
                return c - '0';
            else if ('a' <= c && c <= 'f')
                return c - 'a' + 10;
            else if ('A' <= c && c <= 'F')
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
    }
}