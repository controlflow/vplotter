using System;
using System.Buffers;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;

namespace VPlotter.GCode
{
  [PublicAPI]
  [DebuggerDisplay("{ToString(),raw}")]
  [StructLayout(LayoutKind.Auto)]
  public readonly ref struct GCodeCommand
  {
    // [8 bits] command word, [8 bits] subcode number, [16 bits] code number
    public readonly Code Code;

    private readonly GCodeParsingSettings myParsingSettings;
    private readonly ReadOnlySpan<char> myRawFields;

    public bool IsValid => Word != 0;

    /// <summary>
    /// Returns 'G' for 'G1 X0 Y0' g-code command
    /// </summary>
    public char Word => (char) ((uint) Code >> 24);

    /// <summary>
    /// Return 1 for 'G1 X0 Y0' g-code command
    /// </summary>
    public int CodeNumber => (ushort) Code;

    /// <summary>
    /// Return 42 for 'M1.42 A0' g-code command
    /// </summary>
    public int SubCodeNumber => (byte) ((uint) Code >> 16);

    private GCodeCommand(Code code, ReadOnlySpan<char> rawFields, GCodeParsingSettings settings)
    {
      Code = code;
      myRawFields = rawFields;
      myParsingSettings = settings;
    }

    [Pure]
    public GCodeField TryGetField(char word)
    {
      for (var fields = myRawFields; ; )
      {
        var field = GCodeField.TryParse(fields, out fields, myParsingSettings);
        if (field.Word == default) break;
        if (field.Word == word) return field;
      }

      return default;
    }

    // todo: command.Read('X', 'Y', 'Z', ref tuple)?
    // todo: ReadOnlySpan<char> NotParsedRawTail { get; }

    public FieldsEnumerable Fields => new(in this);

    // todo: debug view
    public readonly ref struct FieldsEnumerable
    {
      private readonly GCodeParsingSettings mySettings;
      private readonly ReadOnlySpan<char> myRawFields;

      public FieldsEnumerable(in GCodeCommand command)
      {
        mySettings = command.myParsingSettings;
        myRawFields = command.myRawFields;
      }

      public FieldsEnumerator GetEnumerator() => new FieldsEnumerator(in this);

      public ref struct FieldsEnumerator
      {
        private readonly GCodeParsingSettings mySettings;
        private ReadOnlySpan<char> myTail;
        private GCodeField myCurrent;

        public FieldsEnumerator(in FieldsEnumerable enumerable)
        {
          mySettings = enumerable.mySettings;
          myTail = enumerable.myRawFields;
          myCurrent = default;
        }

        public bool MoveNext()
        {
          // todo: skip comments?
          myCurrent = GCodeField.TryParse(myTail.SkipWhitespace(), out myTail, mySettings);
          return myCurrent.IsValid;
        }

        public readonly GCodeField Current => myCurrent;
      }
    }

    public void Deconstruct(out char word, out int code)
    {
      word = Word;
      code = CodeNumber;
    }

    public void Deconstruct(out char word)
    {
      word = Word;
    }

    [Pure]
    public static GCodeCommand TryParse(ReadOnlySpan<char> line, GCodeParsingSettings settings)
    {
      if (line.Length == 0) return default;

      GCodeComment.TryParse(line.SkipWhitespace(), out line);

      var firstField = GCodeField.TryParse(line, out var rawArguments, settings);
      if (!firstField.IsValid) return default;

      var code = firstField.CodeArgument;
      if (code == Code.Invalid) return default;

      return new GCodeCommand(code, rawArguments.SkipWhitespace(), settings);
    }

    public override string ToString()
    {
      if (!IsValid) return "<Invalid G-Code>";

      var capacity = 1 + DigitsCount(CodeNumber);
      if (SubCodeNumber > 0)
        capacity += 1 + DigitsCount(SubCodeNumber);
      if (myRawFields.Length > 0)
        capacity += 1 + myRawFields.Length;

      var sb = new StringBuilder(capacity);
      sb.Append(Word);
      sb.Append(CodeNumber);

      if (SubCodeNumber > 0)
      {
        sb.Append('.');
        sb.Append(SubCodeNumber);
      }

      if (myRawFields.Length > 0)
      {
        sb.Append(' ');
        sb.Append(myRawFields);
      }

      return sb.ToString();

      static int DigitsCount(int x)
      {
        return x switch
        {
          < 10    => 1,
          < 100   => 2,
          < 1000  => 3,
          < 10000 => 4,
          _       => 5
        };
      }
    }
  }
}