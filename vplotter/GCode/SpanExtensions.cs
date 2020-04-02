using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace VPlotter.GCode
{
  public static class SpanExtensions
  {
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
      int start = index, value = 0;

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

      return start == index ? -1 : value;
    }

    public static int TryScanGCodeDecimalFractionUnsignedInt32(
      this ReadOnlySpan<char> span, ref int index, int scale, int integralValue = 0)
    {
      int start = index, value = integralValue;

      // scan required scale
      for (; scale > 0 && index < span.Length; scale--, index++)
      {
        var ch = span[index];
        if (ch < '0' | ch > '9') break;

        var digit = ch - '0';
        var newValue = value * 10 + digit;

        if (value > 214748364 | newValue < 0)
        {
          if (scale > int.MaxValue / 2) return value; // detect max scale

          value = -2; // overflow
          goto SkipTrailing;
        }

        value = newValue;
      }

      if (start != index)
      {
        if (scale > int.MaxValue / 2) return value; // detect max scale

        for (; scale > 0; scale--)
        {
          if (value > 214748364)
          {
            value = -2; // overflow
            goto SkipTrailing;
          }

          value *= 10;
        }
      }

      // scan trailing digits
      SkipTrailing:
      for (; index < span.Length; index++)
      {
        var ch = span[index];
        if (ch < '0' | ch > '9') break;
      }

      return start == index ? -1 : value;
    }

    public static int TryScanGCodeDoubleQuotedStringLiteral(
      this ReadOnlySpan<char> span, ref int index, bool singleQuoteEscape)
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
            return index; // unfinished, "aaa'

          index++; // just skip the next character
        }
        else if (ch == '"')
        {
          if (index < span.Length && span[index] == '"')
            index++; // skip ""
          else
            return length;
        }
      }

      return length;
    }
  }
}