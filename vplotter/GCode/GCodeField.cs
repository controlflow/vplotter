using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace VPlotter.GCode
{
  [StructLayout(LayoutKind.Auto)]
  public readonly ref struct GCodeField
  {
    public readonly char Word; // 2 bytes
    public readonly ReadOnlySpan<char> Raw; // 4/8 bytes + 4 byte

    private readonly KindInternal myArgumentKind; // 2 byte (1?)
    private readonly int myPayload; // 4 byte

    private GCodeField(char word, ReadOnlySpan<char> rawField, KindInternal argumentKind, int payload = -1)
    {
      Word = word;
      Raw = rawField;
      myArgumentKind = argumentKind;
      myPayload = payload;
    }

    public bool IsValid => Word != default;

    // todo: computable when edited
    private ReadOnlySpan<char> RawArgument => Raw.Slice(start: 1);

    private ReadOnlySpan<char> RawArgumentNoHeadSpace
    {
      get
      {
        var index = 1;
        Raw.SkipWhitespace(ref index);
        return Raw.Slice(start: index);
      }
    }

    public bool HasArgument => Word != '\0' & myArgumentKind != KindInternal.NoArgument;

    public int IntArgument
    {
      get
      {
        switch (myArgumentKind)
        {
          case KindInternal.IntegerNoScaling:
          case KindInternal.IntegerScaledOverflow:
            return myPayload;
          default:
            return GetIntArgumentSlow();
        }
      }
    }

    [Pure]
    private int GetIntArgumentSlow()
    {
      switch (myArgumentKind)
      {
        case KindInternal.IntegerScale1:
          return myPayload / 10;
        case KindInternal.IntegerScale2:
          return myPayload / 100;
        case KindInternal.IntegerScale3:
          return myPayload / 1000;
        case KindInternal.IntegerScale4:
          return myPayload / 10000;
        case KindInternal.IntegerScale5:
          return myPayload / 100000;
        case KindInternal.IntegerScaledOverflow:
          throw new OverflowException(message: "Scaled integer is too big");
        case KindInternal.RealNoSpace:
        case KindInternal.Real:
          return myPayload;
        default:
          throw new ArgumentOutOfRangeException(
            message: "Integer argument missing", innerException: null);
      }
    }

    public int ScaledIntArgument
    {
      get
      {
        switch (myArgumentKind)
        {
          case KindInternal.IntegerNoScaling:
          case KindInternal.IntegerScale1:
          case KindInternal.IntegerScale2:
          case KindInternal.IntegerScale3:
          case KindInternal.IntegerScale4:
          case KindInternal.IntegerScale5:
          case KindInternal.RealNoSpace:
          case KindInternal.Real:
            return myPayload;
          default:
            return GetIntArgumentSlow();
        }
      }
    }

    private const NumberStyles RealNumber = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

    public float FloatArgument
    {
      get
      {
        switch (myArgumentKind)
        {
          case KindInternal.IntegerNoScaling:
          case KindInternal.IntegerScale1:
          case KindInternal.IntegerScale2:
          case KindInternal.IntegerScale3:
          case KindInternal.IntegerScale4:
          case KindInternal.IntegerScale5:
          case KindInternal.IntegerScaledOverflow:
            return IntArgument;
          case KindInternal.RealNoSpace:
            return float.Parse(RawArgument, style: RealNumber);
          case KindInternal.Real:
            return (float) ParseDecimalIgnoreSpacing();
          default:
            return FloatArgumentMissing();
        }
      }
    }

    private static float FloatArgumentMissing()
    {
      throw new ArgumentOutOfRangeException(
        message: "Float argument missing", innerException: null);
    }

    public double DoubleArgument
    {
      get
      {
        switch (myArgumentKind)
        {
          case KindInternal.IntegerNoScaling:
          case KindInternal.IntegerScale1:
          case KindInternal.IntegerScale2:
          case KindInternal.IntegerScale3:
          case KindInternal.IntegerScale4:
          case KindInternal.IntegerScale5:
          case KindInternal.IntegerScaledOverflow:
            return IntArgument;
          case KindInternal.RealNoSpace:
            return double.Parse(RawArgument, style: RealNumber);
          case KindInternal.Real:
            return (double) ParseDecimalIgnoreSpacing();
          default:
            return DoubleArgumentMissing();
        }
      }
    }

    private static double DoubleArgumentMissing()
    {
      throw new ArgumentOutOfRangeException(
        message: "Double argument missing", innerException: null);
    }

    public decimal DecimalArgument
    {
      get
      {
        switch (myArgumentKind)
        {
          case KindInternal.IntegerNoScaling:
          case KindInternal.IntegerScale1:
          case KindInternal.IntegerScale2:
          case KindInternal.IntegerScale3:
          case KindInternal.IntegerScale4:
          case KindInternal.IntegerScale5:
          case KindInternal.IntegerScaledOverflow:
            return IntArgument;
          case KindInternal.RealNoSpace:
            return decimal.Parse(RawArgument, style: RealNumber);
          case KindInternal.Real:
            return ParseDecimalIgnoreSpacing();
          default:
            return DecimalArgumentMissing();
        }
      }
    }

    private static decimal DecimalArgumentMissing()
    {
      throw new ArgumentOutOfRangeException(
        message: "Decimal argument missing", innerException: null);
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

    public ReadOnlySpan<char> StringArgument
    {
      get
      {
        switch (myArgumentKind)
        {
          case KindInternal.NoArgument:
          case KindInternal.IntegerNoScaling:
          case KindInternal.IntegerScale1:
          case KindInternal.IntegerScale2:
          case KindInternal.IntegerScale3:
          case KindInternal.IntegerScale4:
          case KindInternal.IntegerScale5:
          case KindInternal.IntegerScaledOverflow:
          case KindInternal.RealNoSpace:
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
            throw new ArgumentOutOfRangeException(
              message: "String argument missing", innerException: null);
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
      else if (word != '*') // checksum
      {
        tail = line;
        return default; // unknown word
      }

      var index = 1;
      var space = line.SkipWhitespace(ref index);

      if (index == line.Length) goto NoArgument; // "X"

      var firstChar = line[index];
      if (firstChar == '"')
      {
        var literalKind = ParseStringLiteral(line, ref index, settings, out var length);
        return new GCodeField(word, line.SplitAt(index, out tail), literalKind, payload: length);
      }

      if (firstChar == '-' || firstChar == '+')
      {
        index++;
        space += line.SkipWhitespace(ref index); // space after sign
      }

      var integerValue = line.TryScanGCodeUnsignedInt32(ref index);
      if (integerValue < 0) // "X" or "X-" or "X +", no digits scanned, or overflow
      {
        // still can be "X-.1" or "X.1"
        if (index >= line.Length || line[index] != '.') goto NoArgumentIfSpaced; // overflow

        integerValue = 0;
        firstChar = '.'; // re-use variable as a flag
      }
      else // integral number skipped
      {
        if (firstChar == '-') integerValue = -integerValue;
      }

      // "X-123" or "X-" (with "." after) parsed
      var indexAfterDigit = index;
      space += line.SkipWhitespace(ref indexAfterDigit);

      if (indexAfterDigit >= line.Length || line[indexAfterDigit] != '.') // "X123"
      {
        KindInternal integerKind;

        if (settings.IntegerArgumentsScale == 0)
        {
          integerKind = KindInternal.IntegerNoScaling;
        }
        else if (integerValue > settings.IntegerArgumentMaxIntegralPart)
        {
          integerKind = KindInternal.IntegerScaledOverflow;
        }
        else
        {
          integerValue *= settings.IntegerArgumentScaleFactor;
          integerKind = KindInternal.IntegerNoScaling + (ushort) settings.IntegerArgumentsScale;
        }

        var rawField = line.SplitAt(index, out tail);
        return new GCodeField(word, rawField, integerKind, payload: integerValue);
      }

      var indexAfterDot = indexAfterDigit + 1;
      space += line.SkipWhitespace(ref indexAfterDot);
      var realKind = space == 0 ? KindInternal.RealNoSpace : KindInternal.Real;

      var fractional = line.TryScanGCodeDecimalFractionUnsignedInt32(ref indexAfterDot);
      if (fractional < 0) // "X." or "X-." or "X123." or "X-123."
      {
        if (firstChar == '.') goto NoArgumentIfSpaced; // "X."

        // "X123."
        return new GCodeField(word, line.SplitAt(indexAfterDigit + 1, out tail), realKind, payload: integerValue);
      }

      // 123.456
      return new GCodeField(word, line.SplitAt(indexAfterDot, out tail), realKind, payload: integerValue);

      // some common code merged:
      NoArgumentIfSpaced:

      index = 1;
      if (line.SkipWhitespace(ref index) == 0)
      {
        tail = line;
        return default;
      }

      NoArgument:
      return new GCodeField(word, line.SplitAt(index: 1, out tail), KindInternal.NoArgument);
    }

    private static KindInternal ParseStringLiteral(ReadOnlySpan<char> line, ref int index, GCodeParsingSettings settings, out int stringLength)
    {
      var startIndex = index;
      var singleQuoteEscape = settings.EnableSingleQuoteEscapingInStringLiterals;
      stringLength = line.TryScanGCodeDoubleQuotedStringLiteral(ref index, singleQuoteEscape);
      Debug.Assert(stringLength >= 0, "stringLength >= 0");

      var borders = (line[index - 1] == '"') ? 2 : 1;

      if (index - startIndex == stringLength + borders) // no escaping
      {
        if (startIndex == 1 && line[index - 1] == '"')
          return KindInternal.StringNoEscapeNoSpace;

        return KindInternal.StringNoEscapeMaybeUnfinished;
      }

      return singleQuoteEscape ? KindInternal.StringEscapedWithSingleQuote : KindInternal.StringEscaped;
    }

    // note: do not reorder
    private enum KindInternal : ushort
    {
      //                                sample    payload
      IntegerNoScaling,              // X 123     123
      IntegerScale1,                 // X 123     1230
      IntegerScale2,                 // X 123     12300
      IntegerScale3,                 // X 123     123000
      IntegerScale4,                 // X 123     1230000
      IntegerScale5,                 // X 123     12300000
      IntegerScaledOverflow,         // X 123456  123456
      RealNoSpace,                   // X123.45   123 / 123450
                                     // X123.     123 / 123000
      Real,                          // X - 1 .   -1  / -1000
                                     // X .123    0   / 123

      StringNoEscapeNoSpace,         // X"ab"     2
      StringNoEscapeMaybeUnfinished, // X "abc"   3
                                     // X"abc     3
      StringEscaped,                 // X"ab""c"  3
      StringEscapedWithSingleQuote,  // X"'A'B"   2

      NoArgument,                    // X         0
    }
  }
}