using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace VPlotter
{
  public static class SpanExtensions
  {
    // [return: MaybeNull]
    // public static T FirstOrDefault<T>(in this Span<T> span)
    // {
    //   return span.Length > 0 ? span[0] : default;
    // }

    [return: MaybeNull]
    public static T FirstOrDefault<T>(in this ReadOnlySpan<T> span)
    {
      return span.Length > 0 ? span[0] : default;
    }

    [return: MaybeNull]
    public static bool EndsWith<T>(this ReadOnlySpan<T> span, T value)
      where T : IEquatable<T>
    {
      var length = span.Length;
      return length > 0 && span[length - 1].Equals(value);
    }

    [Pure]
    public static ReadOnlySpan<T> SplitAt<T>(this ReadOnlySpan<T> span, int index, out ReadOnlySpan<T> tail)
    {
      tail = span.Slice(start: index);
      return span.Slice(start: 0, length: index);
    }

    public static int SkipWhitespace(this ReadOnlySpan<char> span, ref int index)
    {
      var skipped = 0;

      for (; index < span.Length; index++)
      {
        var ch = span[index];
        if (ch != ' ' && ch != '\t') break;

        skipped++;
      }

      return skipped;
    }

    public static int TryScanGCodeUnsignedInt32(this ReadOnlySpan<char> span, ref int index)
    {
      var start = index;
      var value = 0;

      for (; index < span.Length; index++)
      {
        var ch = span[index];
        if (ch < '0' || ch > '9') break;

        var digit = ch - '0';
        var newValue = value * 10 + digit;

        if (value > 214748364 | newValue < 0)
        {
          index = start;
          return -2; // overflow
        }

        value = newValue;
      }

      return start != index ? value : -1;
    }

    public static int TryScanGCodeDecimalFractionUnsignedInt32(this ReadOnlySpan<char> span, ref int index)
    {
      var start = index;
      var value = 1;
      var valueNoTrailingZeroes = 1;

      for (; index < span.Length; index++)
      {
        var ch = span[index];
        if (ch < '0' || ch > '9') break;

        var digit = ch - '0';

        if (value >= 0 && value < 1000000000)
        {
          value = value * 10 + digit;

          if (digit != 0)
          {
            valueNoTrailingZeroes = value;
          }
        }
        else
        {
          value = -1;

          if (digit != 0)
          {
            index = start;
            return -1;
          }
        }
      }

      return start == index ? -1 : valueNoTrailingZeroes;
    }

    public static int TryScanGCodeDoubleQuotedStringLiteral(this ReadOnlySpan<char> span, ref int index, bool singleQuoteEscape)
    {
      if (span.Length == index) return -1;
      if (span[index] != '"') return -1;

      index++;

      var length = 0;
      for (; index < span.Length; length++)
      {
        var ch = span[index++];
        if (ch == '\'' && singleQuoteEscape)
        {
          if (index == span.Length)
          {
            return index; // unfinished, "aaa'
          }

          index++; // just skip the next character
        }
        else if (ch == '"')
        {
          if (index < span.Length && span[index] == '"')
          {
            index++; // skip ""
          }
          else
          {
            return length;
          }
        }
      }

      return length;
    }
  }
}