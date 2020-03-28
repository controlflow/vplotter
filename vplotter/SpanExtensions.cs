using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;

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

    public static void SkipWhitespace(this ReadOnlySpan<char> span, ref int index)
    {
      for (; index < span.Length; index++)
      {
        var ch = span[index];
        if (ch != ' ' && ch != '\t') break;
      }
    }

    public static int TryScanUnsignedInt32(this ReadOnlySpan<char> span, ref int index)
    {
      var start = index;
      var value = 0;

      for (; index < span.Length; index++)
      {
        var ch = span[index];
        if (ch < '0' || ch > '9') break;

        if (value >= 1000000000)
        {
          index = start;
          return -1; // mul overflow
        }

        var digit = ch - '0';

        var newValue = value * 10 + digit;
        if (newValue < 0)
        {
          index = start;
          return -1; // add overflow
        }

        value = newValue;
      }

      return start != index ? value : -1;
    }

    public static int TryScanDecimalFractionUnsignedInt32(this ReadOnlySpan<char> span, ref int index)
    {
      var start = index;
      var value = 1;
      var valueNoTrailingZeroes = 1;

      for (; index < span.Length; index++)
      {
        var ch = span[index];
        if (ch < '0' || ch > '9') break;

        /*if (valueNoTrailingZeroes >= 1000000000)
        {
          index = start;
          return -1; // mul overflow
        }*/

        var digit = ch - '0';

        if (value < 0 || value >= 1000000000)
        {
          value = -1;

          if (digit != 0)
          {
            index = start;
            return -1;
          }
        }
        else
        {
          value = value * 10 + digit;

          if (digit != 0)
          {
            valueNoTrailingZeroes = value;
          }
        }
      }

      return start == index ? -1 : valueNoTrailingZeroes;
    }
  }
}