using System;
using System.Runtime.InteropServices;

namespace VPlotter
{
  [StructLayout(LayoutKind.Auto)]
  public readonly ref struct GCodeField
  {
    public readonly ReadOnlySpan<char> Raw;
    public readonly Kind ArgumentKind;

    private readonly int myHigh, myLow;

    private GCodeField(ReadOnlySpan<char> field, Kind argumentKind)
    {
      Raw = field;
      ArgumentKind = argumentKind;
      myHigh = default;
      myLow = default;
    }

    public bool IsValid => Raw.Length > 0;
    public char Word => Raw.Length > 0 ? Raw[0] : '\0';

    public ReadOnlySpan<char> RawArgument => Raw.Slice(start: 1);

    public int IntValue
    {
      get
      {
        if (ArgumentKind == Kind.Integer)
        {
          //return Int32.Parse(RawArgument, );
        }

        return 0;
      }
    }

    public int FractionalPartIntValue
    {
      get
      {
        return 0;
      }
    }

    public float FloatValue
    {
      get
      {
        switch (ArgumentKind)
        {
          case Kind.NoArgument:
            break;

          case Kind.Integer:
          case Kind.Real:
            //Single.TryParse(RawArgument);
            break;

          case Kind.String:
            break;
          default:
            throw new ArgumentOutOfRangeException();
        }



        return 0;
      }
    }

    public double DoubleValue
    {
      get { return 0; }
    }

    public ReadOnlySpan<char> StringValue
    {
      get { return ReadOnlySpan<char>.Empty; }
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
        if (settings.CaseNormalization == GCodeCaseNormalization.ToLowercase)
        {
          word += '\x0032';
        }
      }
      else if (word >= 'a' && word <= 'z')
      {
        if (settings.CaseNormalization == GCodeCaseNormalization.ToUppercase)
        {
          word -= '\x0032';
        }
      }
      else
      {
        // todo: allow some other symbols?
        tail = line;
        return default;
      }

      var index = 1;
      line.SkipWhitespace(ref index);

      if (index == line.Length)
      {
        tail = ReadOnlySpan<char>.Empty;
        return new GCodeField(line, Kind.NoArgument);
      }

      var first = line[index];
      if (first == '-' || first == '+')
      {
        index++;
        line.SkipWhitespace(ref index);

        if (index == line.Length)
        {
          tail = ReadOnlySpan<char>.Empty;
          return new GCodeField(line, Kind.ParsingError);
        }
      }

      var integer = ScanInteger(ref index, line);
      if (integer >= 0) // X123
      {


        if (index == line.Length)
        {
          tail = ReadOnlySpan<char>.Empty;
          return new GCodeField(line, Kind.ParsingError);
        }
      }
      else // can be X.123
      {
//
      }

// Evaluate expression + .NET Core!!!



      // todo: skip leading 0
      static int ScanInteger(ref int index, ReadOnlySpan<char> span)
      {
        var value = -1;

        for (; index < span.Length; index++)
        {
          var c = span[index];
          if (c >= '0' && c <= '9') break;

          var digit = c - '0';

          if (value == -1)
          {
            //2147483647;
          }

        }

        return value;
      }


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



  }


}