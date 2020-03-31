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

    private const NumberStyles RealNumber = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

    private GCodeField(char word, ReadOnlySpan<char> rawField, KindInternal argumentKind, int payload = -1)
    {
      Word = word;
      Raw = rawField;
      myArgumentKind = argumentKind;
      myPayload = payload;
    }

    public bool IsValid => Raw.Length > 0;

    // todo: do we really need it? S123 Some message
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
          case KindInternal.IntegerNoSpaceNoScaling:
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
          case KindInternal.IntegerNoSpaceNoScaling:
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
          case KindInternal.IntegerNoSpaceNoScaling:
          case KindInternal.Integer:
            return myPayload;
          case KindInternal.RealNoSpace:
            return float.Parse(RawArgument, style: RealNumber);
          case KindInternal.Real:
            return (float) ParseDecimalIgnoreSpacing();
          default:
            throw new ArgumentOutOfRangeException(message: "Float argument missing", null);
        }
      }
    }

    public double DoubleArgument
    {
      get
      {
        switch (myArgumentKind)
        {
          case KindInternal.IntegerNoSpaceNoScaling:
          case KindInternal.Integer:
            return myPayload;
          case KindInternal.RealNoSpace:
            return double.Parse(RawArgument, style: RealNumber);
          case KindInternal.Real:
            return (double) ParseDecimalIgnoreSpacing();
          default:
            throw new ArgumentOutOfRangeException(message: "No argument", null);
        }
      }
    }

    public decimal DecimalArgument
    {
      get
      {
        switch (myArgumentKind)
        {
          case KindInternal.IntegerNoSpaceNoScaling:
          case KindInternal.Integer:
            return myPayload;
          case KindInternal.RealNoSpace:
            return decimal.Parse(RawArgument, style: RealNumber);
          case KindInternal.Real:
            return ParseDecimalIgnoreSpacing();
          default:
            throw new ArgumentOutOfRangeException(message: "No argument", null);
        }
      }
    }

    private decimal ParseDecimalIgnoreSpacing()
    {
      var index = 1;
      Raw.SkipWhitespace(ref index);

      var first = Raw[index];
      if (first == '-' || first == '+')
      {
        index++;
        Raw.SkipWhitespace(ref index);
      }

      var integer = Raw.TryScanGCodeUnsignedInt32(ref index);
      if (integer < 0)
      {
        integer = 0;
      }
      else if (first == '-')
      {
        integer = -integer;
      }

      // skip dot
      Raw.SkipWhitespace(ref index);
      index++;
      Raw.SkipWhitespace(ref index);

      var fraction = Raw.TryScanGCodeDecimalFractionUnsignedInt32(ref index);
      if (fraction < 0)
      {
        return integer;
      }

      var log10 = (byte) Math.Log10(fraction);

      var fractionDec = new decimal(
        lo: fraction, mid: 0, hi: 0, isNegative: false, scale: log10) - 1;

      return first == '-' ? integer - fractionDec : integer + fractionDec;
    }

    public int IntArgumentScaled
    {
      get
      {
        switch (myArgumentKind)
        {
          case KindInternal.NoArgument:
            break;
          case KindInternal.IntegerNoSpaceNoScaling:
            break;
          case KindInternal.RealNoSpace:
            break;
          case KindInternal.Integer:
            break;
          case KindInternal.Real:
            break;
          case KindInternal.ParsingError:
            break;
          case KindInternal.StringNoEscapeNoSpace:
            break;
          case KindInternal.StringNoEscapeMaybeUnfinished:
            break;
          case KindInternal.StringEscaped:
            break;
          case KindInternal.StringEscapedWithSingleQuote:
            break;
          default:
            throw new ArgumentOutOfRangeException();
        }

        return 0;
      }
    }

    public ReadOnlySpan<char> StringArgument
    {
      get
      {
        switch (myArgumentKind)
        {
          case KindInternal.NoArgument:
          case KindInternal.IntegerNoSpaceNoScaling:
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

      var integerValue = line.TryScanGCodeUnsignedInt32(ref index);
      if (integerValue < 0) // "X" or "X-" or "X +"
      {
        if (index < line.Length && line[index] == '.')
        {
          integerValue = 0;
          first = '.'; // re-use variable as a flag
        }
        else
        {
          tail = line.Slice(start: 1);
          return new GCodeField(word, line.Slice(start: 0, length: 1), KindInternal.ParsingError);
        }
      }
      else
      {
        if (first == '-') integerValue = -integerValue;
      }

      var indexAfterDigit = index;
      space += line.SkipWhitespace(ref indexAfterDigit);
      if (indexAfterDigit < line.Length && line[indexAfterDigit] == '.') // found 123.
      {
        var indexAfterDot = indexAfterDigit + 1;
        space += line.SkipWhitespace(ref indexAfterDot);
        var argumentKind = space == 0 ? KindInternal.RealNoSpace : KindInternal.Real;

        var fractional = line.TryScanGCodeDecimalFractionUnsignedInt32(ref indexAfterDot);
        if (fractional < 0) // X. or X-. or X123. or X-123.
        {
          if (first == '.') // X.
          {
            tail = line.Slice(start: 1);
            return new GCodeField(word, line.Slice(start: 0, length: 1), KindInternal.ParsingError);
          }
          else
          {
            tail = line.Slice(start: indexAfterDigit + 1);
            return new GCodeField(word, line.Slice(start: 0, length: indexAfterDigit + 1), argumentKind, payload: integerValue);
          }
        }
        else // 123.456
        {
          tail = line.Slice(start: indexAfterDot);
          return new GCodeField(word, line.Slice(start: 0, length: indexAfterDot), argumentKind, payload: integerValue);
        }
      }

      {
        tail = line.Slice(start: index);

        var argumentKind = space == 0 ? KindInternal.IntegerNoSpaceNoScaling : KindInternal.Integer;
        return new GCodeField(word, line.Slice(start: 0, length: index), argumentKind, payload: integerValue);
      }
    }

    // todo: evaluate expression + .NET Core

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

    private enum KindInternal : short
    {
      //                          sample    payload
      NoArgument,              // X         -1

      IntegerNoSpaceNoScaling, // X123      123
      IntegerNoSpace,          // X123      123000
      IntegerNoScaling,        // X 123     123
      Integer,                 // X 123     123000

      RealNoSpace,    // X123.45   123 / 123450
                      // X123.     123 / 123000


      Real,           // X - 1 .
                      // X.123     0
      ParsingError,   // X-        -1

      StringNoEscapeNoSpace,         // X"abc"
      StringNoEscapeMaybeUnfinished, // X "abc"
                                     // X"abc
      StringEscaped,                 // X"ab""c"
      StringEscapedWithSingleQuote,  // X"'A'B"

      //A = 10
    }
  }
}