using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace VPlotter
{
  [StructLayout(LayoutKind.Auto)]
  public readonly ref struct GCodeField
  {
    public readonly char Word;
    public readonly ReadOnlySpan<char> Raw;
    private readonly KindInternal myArgumentKind;
    private readonly int myPayload;

    private GCodeField(char word, ReadOnlySpan<char> rawField, KindInternal argumentKind, int payload = -1)
    {
      Word = word;
      Raw = rawField;
      myArgumentKind = argumentKind;
      myPayload = payload;
    }

    public bool IsValid => Raw.Length > 0;

    public ReadOnlySpan<char> RawArgument => Raw.Slice(start: 1);

    private ReadOnlySpan<char> RawArgumentNoHeadSpace
    {
      get
      {
        var index = 1;
        Raw.SkipWhitespace(ref index);
        return Raw.Slice(index);
      }
    }

    public bool HasArgument
    {
      get
      {
        switch (myArgumentKind)
        {
          case KindInternal.IntegerNoSpace:
          case KindInternal.RealNoSpace:
          case KindInternal.Integer:
          case KindInternal.Real:
          case KindInternal.StringNoEscapeNoSpace:
          case KindInternal.StringNoEscapeMaybeUnfinished:
          case KindInternal.StringEscaped:
          case KindInternal.StringEscapedWithSingleQuote:
            return true;

          default:
            return false;
        }
      }
    }

    public int IntArgument
    {
      get
      {
        switch (myArgumentKind)
        {
          case KindInternal.IntegerNoSpace:
          case KindInternal.Integer:
          case KindInternal.RealNoSpace:
          case KindInternal.Real:
            return myPayload;

          default:
            throw new ArgumentOutOfRangeException(message: "Integer argument missing", null);
        }
      }
    }

    public float FloatArgument
    {
      get
      {
        switch (myArgumentKind)
        {
          case KindInternal.IntegerNoSpace:
          case KindInternal.Integer:
            return myPayload;

          case KindInternal.RealNoSpace:
            return float.Parse(RawArgument, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint);

          case KindInternal.Real:
            throw new Exception();

          default:
            throw new ArgumentOutOfRangeException(message: "Float argument missing", null);
        }
      }
    }

    public double DoubleValue
    {
      get
      {
        switch (myArgumentKind)
        {
          case KindInternal.IntegerNoSpace:
          case KindInternal.Integer:
            return myPayload;

          case KindInternal.RealNoSpace:
            return double.Parse(RawArgument, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint);

          case KindInternal.Real:
            throw new Exception();

          default:
            throw new ArgumentOutOfRangeException(message: "No argument", null);
        }
      }
    }

    public ReadOnlySpan<char> StringArgument
    {
      get
      {
        switch (myArgumentKind)
        {
          case KindInternal.NoArgument:
          case KindInternal.IntegerNoSpace:
          case KindInternal.RealNoSpace:
          case KindInternal.ParsingError:
            return RawArgument;
          case KindInternal.Integer:
          case KindInternal.Real:
            return RawArgumentNoHeadSpace;
          case KindInternal.StringNoEscapeNoSpace:
            return Raw.Slice(start: 2, length: Raw.Length - 3);
          case KindInternal.StringNoEscapeMaybeUnfinished:
            return DecodeNoEscapeString();
          case KindInternal.StringEscaped:
            return DecodeStringLiteral(decodeSingleQuotes: false);
          case KindInternal.StringEscapedWithSingleQuote:
            return DecodeStringLiteral(decodeSingleQuotes: true);
          default:
            throw new ArgumentOutOfRangeException();
        }
      }
    }

    private ReadOnlySpan<char> DecodeNoEscapeString()
    {
      var argument = RawArgumentNoHeadSpace;
      return argument[^1] == '"' ? argument[1..^1] : argument[1..];
    }

    private ReadOnlySpan<char> DecodeStringLiteral(bool decodeSingleQuotes)
    {
      var argument = RawArgumentNoHeadSpace;
      var builder = new StringBuilder(capacity: myPayload);

      for (var index = 1; index < argument.Length; index++)
      {
        var ch = argument[index];
        if (ch == '"')
        {
          if (index + 1 >= argument.Length) break;
          index++;
        }
        else if (ch == '\'' && decodeSingleQuotes)
        {
          if (index + 1 >= argument.Length) break;

          index++;
          ch = char.ToLower(argument[index]);
        }

        builder.Append(ch);
      }

      return builder.ToString().AsSpan();
    }

    public enum Kind : byte
    {
      NoArgument, // X
      Integer, // X123
      Real, // X123.45
      String, // X"aaa"
      ParsingError // X-
    }

    public override string ToString() => Raw.ToString();

    public static GCodeField TryParse(ReadOnlySpan<char> line, out ReadOnlySpan<char> tail, GCodeParsingSettings settings)
    {
      if (line.Length == 0)
      {
        tail = line;
        return default;
      }

      var word = line[0];
      if (word >= 'A' && word <= 'Z')
      {
        if (settings.CaseNormalization == GCodeCaseNormalization.ToLowercase) word += '\x0020';
      }
      else if (word >= 'a' && word <= 'z')
      {
        if (settings.CaseNormalization == GCodeCaseNormalization.ToUppercase) word -= '\x0020';
      }
      else if (word == '*') // checksum
      {
        // todo: test
      }
      else
      {
        tail = line;
        return default;
      }

      var index = 1;
      var space = line.SkipWhitespace(ref index);

      if (index == line.Length)
      {
        tail = line.Slice(start: 1);
        return new GCodeField(word, line.Slice(start: 0, length: 1), KindInternal.NoArgument);
      }

      var first = line[index];
      if (first == '-' || first == '+')
      {
        index++;
        space += line.SkipWhitespace(ref index); // space after sign

        // continue number
      }
      else if (first == '"')
      {
        var literalKind = ParseStringLiteral(line, ref index, settings, out var length);
        var rawField = line.Slice(start: 0, length: index);

        tail = line.Slice(start: index);
        return new GCodeField(word, rawField, literalKind, payload: length);
      }

      var integer = line.TryScanGCodeUnsignedInt32(ref index);
      if (integer < 0) // "X-" or "X +"
      {
        tail = line.Slice(start: 1);
        return new GCodeField(word, line.Slice(start: 0, length: 1), KindInternal.ParsingError);
      }

      if (first == '-') integer = -integer;

      tail = line.Slice(start: index);

      var argumentKind = space == 0 ? KindInternal.IntegerNoSpace : KindInternal.Integer;
      return new GCodeField(word, line.Slice(start: 0, length: index), argumentKind, payload: integer);




      // todo: Evaluate expression + .NET Core!!!



      // skip ws

      // can be + - 0-9
      // can be .
      // can be "

      // 123
      // 456
      // 789


      tail = line;
      return default; // new GCodeField(line.Slice(0, 1));
    }

    private static KindInternal ParseStringLiteral(ReadOnlySpan<char> line, ref int index, GCodeParsingSettings settings, out int stringLength)
    {
      var startIndex = index;
      var singleQuoteEscape = settings.EnableSingleQuoteEscapingInStringLiterals;
      stringLength = line.TryScanGCodeDoubleQuotedStringLiteral(ref index, singleQuoteEscape);

      if (stringLength < 0)
      {
        return KindInternal.ParsingError; // must be unreachable
      }

      var borders = (line[index - 1] == '"') ? 2 : 1;

      if (index - startIndex == stringLength + borders) // no escaping
      {
        if (startIndex == 1 && line[index - 1] == '"')
          return KindInternal.StringNoEscapeNoSpace;

        return KindInternal.StringNoEscapeMaybeUnfinished;
      }

      return singleQuoteEscape ? KindInternal.StringEscapedWithSingleQuote : KindInternal.StringEscaped;
    }

    private enum KindInternal
    {
      //                 sample    arg1
      NoArgument,     // X         -1
      IntegerNoSpace, // X123      123
      RealNoSpace,    // X123.45   123
                      // X123.     123
      Integer,        // X 123     123
      Real,           // X - 1 .
                      // X.123     0
      ParsingError,   // X-        -1

      StringNoEscapeNoSpace,         // X"abc"
      StringNoEscapeMaybeUnfinished, // X "abc"
                                     // X"abc
      StringEscaped,                 // X"ab""c"
      StringEscapedWithSingleQuote,  // X"'A'B"
    }

  }


}