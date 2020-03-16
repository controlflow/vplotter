namespace VPlotter
{
  public readonly ref struct GCodeCommand
  {
    // context here

    // [8 bits] command, [8 bits] subcode, [16 bits] code
    public readonly GCode GCode;

    public char Command => (char) ((uint) GCode >> 24);
    public int Code => (ushort) GCode;
    public int Subcode => (byte) ((uint) GCode >> 16);

    public GCodeField GetField(char c)
    {
      return default;
    }


    public FieldsEnumerable Fields => default;



    public readonly ref struct FieldsEnumerable
    {
      public ref struct FieldsEnumerator
      {
        public bool MoveNext()
        {
          return false;
        }

        public GCodeField Current
        {
          get { return default; }
        }
      }
    }

    public void Deconstruct(out char command, out int code)
    {
      var gCode = (uint) GCode;
      command = (char) (gCode >> 24);
      code = (ushort) gCode;
    }

    public void Deconstruct(out char command)
    {
      command = (char) ((uint) GCode >> 24);
    }
  }

  // todo: encode as  code
  public enum GCode
  {
    G0_RapidPositioning = ('G' << 24) | 0,
    G1_LinearInterpolation = ('G' << 24) | 1,

  }
}