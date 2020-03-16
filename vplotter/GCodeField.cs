using System;
using System.Collections.Generic;

namespace VPlotter
{
  public readonly ref struct GCodeField
  {
    public readonly ReadOnlySpan<char> Raw;
    public readonly Kind ArgumentKind;

    private GCodeField(ReadOnlySpan<char> field, Kind argumentKind)
    {
      Raw = field;
      ArgumentKind = argumentKind;
    }

    public bool IsValid => Raw.Length > 0;
    public char Command => Raw.Length > 0 ? Raw[0] : '\0';

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

    public enum Kind
    {
      NoArgument, // X
      Integer, // X123
      Real, // X123.45
      String // X"aaa"
    }

    public override string ToString()
    {
      return "aaaa";

      // todo:
      //return base.ToString();
    }

    public static GCodeField TryParse(ReadOnlySpan<char> line, out ReadOnlySpan<char> tail, GCodeParsingSettings settings)
    {
      if (line.Length == 0)
      {
        tail = line;
        return default;
      }

      var ch = line[0];
      if (ch >= 'A' && ch <= 'Z')
      {
        if (settings.CaseNormalization == GCodeCaseNormalization.ToLowercase)
        {
          ch += '\x0032';
        }
      }
      else if (ch >= 'a' && ch <= 'z')
      {
        if (settings.CaseNormalization == GCodeCaseNormalization.ToUppercase)
        {
          ch -= '\x0032';
        }
      }
      else
      {
        tail = line;
        return default;
      }

      var i = 1;
      for (; i < line.Length; i++)
      {
        var c = line[i];
        if (c != ' ' && c != '\t') break;
      }



      // skip ws

      // can be + - 0-9
      // can be .
      // can be "

      // 123
      // 456
      // 789


      tail = line;
      return new GCodeField(line.Slice(0, 1));
    }



  }


}