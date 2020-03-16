using System;
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
  }
}