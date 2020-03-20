using System;
using System.Runtime.InteropServices;

namespace VPlotter
{
  [StructLayout(LayoutKind.Auto)]
  public readonly ref struct GCodeComment
  {
    public readonly ReadOnlySpan<char> Raw;

    public GCodeCommentKind Kind
    {
      get
      {
        switch (Raw.FirstOrDefault())
        {
          case ';':
            return GCodeCommentKind.EndOfLine;
          case '(' when Raw.EndsWith(')'):
            return GCodeCommentKind.Inline;
          case '(':
            return GCodeCommentKind.InlineUnfinished;
          default:
            throw new InvalidOperationException();
        }
      }
    }

    public ReadOnlySpan<char> Content
    {
      get
      {
        switch (Kind)
        {
          case GCodeCommentKind.Inline:
            return Raw[1..^1];
          default:
            return Raw[1..];
        }
      }
    }

    public bool IsValid => Raw.Length > 0;

    private GCodeComment(ReadOnlySpan<char> comment)
    {
      Raw = comment;
    }

    public static GCodeComment TryParse(ReadOnlySpan<char> line, out ReadOnlySpan<char> tail)
    {
      switch (line.FirstOrDefault())
      {
        case ';':
        {
          tail = ReadOnlySpan<char>.Empty;
          return new GCodeComment(line);
        }

        case '(':
        {
          for (var index = 1; index < line.Length; index++)
          {
            if (line[index] == ')')
            {
              tail = line[(index + 1)..];
              return new GCodeComment(line[..(index + 1)]);
            }
          }

          // unfinished comment
          tail = ReadOnlySpan<char>.Empty;
          return new GCodeComment(line);
        }

        default:
        {
          tail = line;
          return default;
        }
      }
    }

    public override string ToString()
    {
      return IsValid ? Content.ToString() : "<Invalid>";
    }
  }

  public enum GCodeCommentKind
  {
    EndOfLine,       // G1 X10 ; comment
    Inline,          // G1 (comment) X10
    InlineUnfinished // G1 (comment
  }
}