using System;

namespace VPlotter
{
  public readonly ref struct GCodeCommand
  {
    // context here

    // [8 bits] command, [8 bits] subcode, [16 bits] code
    public readonly GCode GCode;

    public char Word => (char) ((uint) GCode >> 24);
    public int Code => (ushort) GCode;
    public int Subcode => (byte) ((uint) GCode >> 16);

    public GCodeField TryGetField(char word)
    {
      // parse forward
      return default;
    }

    // todo: command.Read('X', 'Y', 'Z', ref tuple)?


    public FieldsEnumerable Fields => default;



    public readonly ref struct FieldsEnumerable
    {
      public ref struct FieldsEnumerator
      {
        private ReadOnlySpan<char> myTail;

        public bool MoveNext()
        {


          // skip ws
          // skip comments

          return false;
        }

        public GCodeField Current
        {
          get { return default; }
        }
      }
    }

    public void Deconstruct(out char word, out int code)
    {
      var gCode = (uint) GCode;
      word = (char) (gCode >> 24);
      code = (ushort) gCode;
    }

    public void Deconstruct(out char word)
    {
      word = (char) ((uint) GCode >> 24);
    }
  }

  // todo: encode as  code
  public enum GCode
  {
    G0_RapidPositioning = ('G' << 24) | 0,
    G1_LinearInterpolation = ('G' << 24) | 1,

  }
}