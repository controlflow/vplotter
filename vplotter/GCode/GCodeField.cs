using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace VPlotter.GCode
{
  [StructLayout(LayoutKind.Auto)]
  public readonly ref struct GCodeField
  {
    private readonly char myWord; // 2 bytes
    private readonly ArgumentKind myKind; // 1 byte
    private readonly byte myScale; // 1 byte
    private readonly int myPayload1; // 4 bytes
    private readonly int myPayload2; // 4 bytes
    private readonly ReadOnlySpan<char> myRaw; // 4/8 bytes + 4 byte + 4 bytes padding


    private GCodeField(
      char word, ReadOnlySpan<char> raw, ArgumentKind kind, byte scale)
    {
      myWord = word;
      myRaw = raw;
      myKind = kind;
      myScale = scale;
      myPayload1 = 0;
      myPayload2 = 0;
    }

    private GCodeField(
      char word, ReadOnlySpan<char> raw, ArgumentKind kind, byte scale, int payload1, int payload2)
    {
      myWord = word;
      myRaw = raw;
      myKind = kind;
      myScale = scale;
      myPayload1 = payload1;
      myPayload2 = payload2;
    }

    public char Word => myWord;

    public bool IsValid => myWord != default;

    public ReadOnlySpan<char> Raw => myRaw;

    // todo: computable when edited
    private ReadOnlySpan<char> RawArgument => myRaw.Slice(start: 1);

    private ReadOnlySpan<char> RawArgumentNoHeadSpace
    {
      get
      {
        var index = 1;
        myRaw.SkipWhitespace(ref index);
        return myRaw.Slice(start: index);
      }
    }

    public bool HasArgument => myWord != '\0' & myKind != ArgumentKind.NoArgument;

    public int IntArgument
    {
      get
      {
        switch (myKind)
        {
          case ArgumentKind.Integer:
          case ArgumentKind.RealNoSpace:
          case ArgumentKind.Real:
            return myPayload1;
          default:
            return IntArgumentMissing();
        }
      }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int IntArgumentMissing()
    {
      throw new ArgumentOutOfRangeException(message: "Integer argument missing", innerException: null);
    }

    public int ScaledIntArgument
    {
      get
      {
        switch (myKind)
        {
          case ArgumentKind.Integer:
          case ArgumentKind.RealNoSpace:
          case ArgumentKind.Real:
            if (myPayload2 == int.MinValue) ScaledIntTooBig();
            return myPayload2;
          default:
            return IntArgumentMissing();
        }
      }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ScaledIntTooBig()
    {
      throw new OverflowException(message: "Scaled integer is too big");
    }

    private const NumberStyles RealNumber = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

    public float FloatArgument
    {
      get
      {
        switch (myKind)
        {
          case ArgumentKind.Integer:
            return myPayload1;
          case ArgumentKind.RealNoSpace:
            return float.Parse(RawArgument, style: RealNumber);
          case ArgumentKind.Real:
            return (float) ParseDecimalIgnoreSpacing();
          default:
            return FloatArgumentMissing();
        }
      }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static float FloatArgumentMissing()
    {
      throw new ArgumentOutOfRangeException(message: "Float argument missing", innerException: null);
    }

    public double DoubleArgument
    {
      get
      {
        switch (myKind)
        {
          case ArgumentKind.Integer:
            return myPayload1;
          case ArgumentKind.RealNoSpace:
            return double.Parse(RawArgument, style: RealNumber);
          case ArgumentKind.Real:
            return (double) ParseDecimalIgnoreSpacing();
          default:
            return DoubleArgumentMissing();
        }
      }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double DoubleArgumentMissing()
    {
      throw new ArgumentOutOfRangeException(message: "Double argument missing", innerException: null);
    }

    public decimal DecimalArgument
    {
      get
      {
        switch (myKind)
        {
          case ArgumentKind.Integer:
            return myPayload1;
          case ArgumentKind.RealNoSpace:
            return decimal.Parse(RawArgument, style: RealNumber);
          case ArgumentKind.Real:
            return ParseDecimalIgnoreSpacing();
          default:
            return DecimalArgumentMissing();
        }
      }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static decimal DecimalArgumentMissing()
    {
      throw new ArgumentOutOfRangeException(
        message: "Decimal argument missing", innerException: null);
    }

    private decimal ParseDecimalIgnoreSpacing()
    {
      int index = 1, sign = 1;
      myRaw.SkipWhitespace(ref index);

      var first = myRaw[index];
      if (first == '-' || first == '+')
      {
        index++;
        myRaw.SkipWhitespace(ref index);
      }

      var integralValue = myRaw.TryScanGCodeUnsignedInt32(ref index);
      if (integralValue < 0)
      {
        integralValue = 0;
      }
      else if (first == '-')
      {
        sign = -1;
      }

      // skip dot
      myRaw.SkipWhitespace(ref index);
      index++;
      myRaw.SkipWhitespace(ref index);

      var indexAfterDot = index;
      var scaledValue = myRaw.TryScanGCodeDecimalFractionUnsignedInt32(ref index, scale: int.MaxValue, integralValue);
      if (scaledValue < 0)
      {
        return integralValue / sign;
      }

      var scale = (byte) (index - indexAfterDot);
      return new decimal(lo: scaledValue, mid: 0, hi: 0, isNegative: first == '-', scale);
    }

    public ReadOnlySpan<char> StringArgument
    {
      get
      {
        switch (myKind)
        {
          case ArgumentKind.NoArgument:
          case ArgumentKind.Integer:
          case ArgumentKind.RealNoSpace:
          case ArgumentKind.Real:
            return RawArgumentNoHeadSpace;
          case ArgumentKind.StringNoEscapeNoSpace:
            return myRaw.Slice(start: 2, length: myRaw.Length - 3);
          case ArgumentKind.StringNoEscapeMaybeUnfinished:
            return DecodeNoEscapeString();
          case ArgumentKind.StringEscaped:
            return DecodeStringLiteral(decodeSingleQuotes: false);
          case ArgumentKind.StringEscapedWithSingleQuote:
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
      var builder = new StringBuilder(capacity: myPayload1);

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

    public override string ToString() => myRaw.ToString();

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
        var raw = line.SplitAt(index, out tail);

        return new GCodeField(word, raw, literalKind, scale: 0, payload1: length, payload2: 0);
      }

      var sign = 1;
      if (firstChar == '-' || firstChar == '+')
      {
        index++;
        space += line.SkipWhitespace(ref index); // space after sign

        if (firstChar == '-') sign = -1;
      }

      var integralValue = line.TryScanGCodeUnsignedInt32(ref index);
      if (integralValue < 0) // "X" or "X-" or "X +", no digits scanned, or overflow
      {
        // still can be "X-.1" or "X.1"
        if (index >= line.Length || line[index] != '.') goto NoArgumentIfSpaced; // overflow

        integralValue = 0;
        firstChar = '.'; // re-use variable as a flag
      }

      // "X-123" or "X-" (with "." after) parsed
      var indexAfterDigit = index;
      space += line.SkipWhitespace(ref indexAfterDigit);

      if (indexAfterDigit >= line.Length || line[indexAfterDigit] != '.') // "X123"
      {
        var scaledInt = settings.ScaleInteger(integralValue, sign);
        var raw = line.SplitAt(index, out tail);

        return new GCodeField(
          word, raw, ArgumentKind.Integer, scale: settings.IntegerArgumentScale,
          payload1: integralValue / sign, payload2: scaledInt);
      }

      var indexAfterDot = indexAfterDigit + 1;
      space += line.SkipWhitespace(ref indexAfterDot);

      var realKind = space == 0 ? ArgumentKind.RealNoSpace : ArgumentKind.Real;

      var scaledValue = line.TryScanGCodeDecimalFractionUnsignedInt32(ref indexAfterDot, settings.IntegerArgumentScale, integralValue);
      if (scaledValue == -1) // "X." or "X-." or "X123." or "X-123."
      {
        if (firstChar == '.') goto NoArgumentIfSpaced; // "X."

        // "X123."
        return new GCodeField(
          word, line.SplitAt(indexAfterDigit + 1, out tail),
          realKind, scale: settings.IntegerArgumentScale,
          payload1: integralValue / sign, payload2: 0);
      }

      // 123.456
      return new GCodeField(
        word, line.SplitAt(indexAfterDot, out tail),
        realKind, scale: settings.IntegerArgumentScale,
        payload1: integralValue / sign,
        payload2: scaledValue > 0 ? (scaledValue * sign) : int.MinValue);

      // some common code merged:
      NoArgumentIfSpaced:

      index = 1;
      if (line.SkipWhitespace(ref index) == 0)
      {
        tail = line;
        return default;
      }

      NoArgument:
      return new GCodeField(
        word, line.SplitAt(index: 1, out tail),
        ArgumentKind.NoArgument, scale: settings.IntegerArgumentScale,
        payload1: 0, payload2: 0);
    }

    private static ArgumentKind ParseStringLiteral(
      ReadOnlySpan<char> line, ref int index, GCodeParsingSettings settings, out int stringLength)
    {
      var startIndex = index;
      var singleQuoteEscape = settings.EnableSingleQuoteEscapingInStringLiterals;
      stringLength = line.TryScanGCodeDoubleQuotedStringLiteral(ref index, singleQuoteEscape);
      Debug.Assert(stringLength >= 0, "stringLength >= 0");

      var borders = (line[index - 1] == '"') ? 2 : 1;

      if (index - startIndex == stringLength + borders) // no escaping
      {
        if (startIndex == 1 && line[index - 1] == '"')
          return ArgumentKind.StringNoEscapeNoSpace;

        return ArgumentKind.StringNoEscapeMaybeUnfinished;
      }

      return singleQuoteEscape ? ArgumentKind.StringEscapedWithSingleQuote : ArgumentKind.StringEscaped;
    }

    // todo: finish
    public static GCodeField Create(char word)
    {
      return new GCodeField(
        word, ReadOnlySpan<char>.Empty, ArgumentKind.NoArgument,
        scale: 0, payload1: 0, payload2: 0);
    }

    public static GCodeField Create(char word, int argument)
    {
      return new GCodeField(
        word, ReadOnlySpan<char>.Empty, ArgumentKind.Integer,
        scale: 0, payload1: argument, payload2: int.MinValue);
    }

    public static GCodeField Create(char word, float argument)
    {
      return Create(word, (double) argument);
    }

    public static GCodeField Create(char word, double argument)
    {
      var bits = BitConverter.DoubleToInt64Bits(argument);

      // todo: float-based field
      return new GCodeField(
        word, ReadOnlySpan<char>.Empty, ArgumentKind.DoubleValue,
        scale: 0,
        payload1: (int) (bits >> 32),
        payload2: (int) bits);
    }

    public static GCodeField Create(char word, decimal argument)
    {
      return Create(word, (double) argument);
    }

    public static GCodeField Create(char word, string argument)
    {
      int doubleQuotes = 0;

      for (var index = 0; index < argument.Length; index++)
      {
        if (argument[index] == '\"')
        {
          doubleQuotes++;
        }
      }

      //argument.CopyTo();

      //var builder = new StringBuilder(capacity: argument.Length + 3);



      // todo: create string span
      // todo: produce ' for lowercase letters?
      return new GCodeField(
        word, ReadOnlySpan<char>.Empty, ArgumentKind.StringNoEscapeNoSpace,
        scale: 0, payload1: 0, payload2: 0);
    }

    // note: do not reorder
    private enum ArgumentKind : byte
    {
      //                                sample    payload2
      Integer,                       // X 123     123

      RealNoSpace,                   // X123.45   123
                                     // X123.     123
      Real,                          // X - 1 .   -1
                                     // X .123    0

      DoubleValue,

      StringNoEscapeNoSpace,         // X"ab"     2
      StringNoEscapeMaybeUnfinished, // X "abc"   3
                                     // X"abc     3
      StringEscaped,                 // X"ab""c"  3
      StringEscapedWithSingleQuote,  // X"'A'B"   2

      NoArgument                     // X
    }
  }
}