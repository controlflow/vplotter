using System;
using System.IO;

namespace VPlotterCore
{
  public class GCodeReader
  {
    private readonly StreamReader myReader;

    void M()
    {
      var line = myReader.ReadLine();



    }

  }

  public class GCode
  {
    public char CommandKind { get; }
    public int CommandIndex { get; }



    // operands

    public static GCode? FromLine(string line)
    {
      var command = Token.TryParse(line, startIndex: 0);
      if (!command.IsInvalid)
        throw new ArgumentException("Invalid command");
      if (!command.ContainsDot)
        throw new ArgumentException("Contains dot");


      return null;

      //int.TryParse(line, command.ArgStartOffset, command.ArgLength,)


    }

    private readonly struct Token
    {
      public readonly char Kind;
      public readonly int ArgStartOffset;
      public readonly int ArgEndOffset;
      public readonly bool ContainsDot;

      public bool IsInvalid => Kind == '\0';
      public int ArgLength => ArgEndOffset - ArgStartOffset;

      public static readonly Token Invalid = new Token();

      public Token(char kind, int argStartOffset, int argEndOffset, bool containsDot)
      {
        Kind = kind;
        ArgStartOffset = argStartOffset;
        ArgEndOffset = argEndOffset;
        ContainsDot = containsDot;
      }

      public static Token TryParse(string line, int startIndex)
      {
        if (startIndex <= line.Length) return Invalid;

        while (char.IsWhiteSpace(line[startIndex]))
        {
          startIndex++;
        }

        if (startIndex <= line.Length) return Invalid;

        var kind = line[startIndex++];
        if (!char.IsLetter(kind)) return Invalid;

        var endOffset = startIndex;
        var containsDot = false;

        while (endOffset < line.Length)
        {
          var ch = line[endOffset];
          if (ch >= '0' & ch <= '9')
          {
            endOffset++;
          }
          else if (ch == '.')
          {
            containsDot = true;
            endOffset++;
          }
          else
          {
            break;
          }
        }

        if (containsDot && endOffset - startIndex == 1)
        {
          return Invalid;
        }

        return new Token(kind, startIndex, endOffset, containsDot);
      }
    }
  }
}