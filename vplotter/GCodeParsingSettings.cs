using System;

namespace VPlotter
{
  public sealed class GCodeParsingSettings
  {
    public static readonly GCodeParsingSettings Default = new GCodeParsingSettings();

    public GCodeParsingSettings(
      GCodeCaseNormalization caseNormalization = GCodeCaseNormalization.ToUppercase,
      int integerArgumentsScale = 0,
      bool enableSingleQuoteEscapingInStringLiterals = false)
    {
      if (integerArgumentsScale < 0 || integerArgumentsScale > 5)
        throw new ArgumentOutOfRangeException(nameof(integerArgumentsScale));

      CaseNormalization = caseNormalization;
      EnableSingleQuoteEscapingInStringLiterals = enableSingleQuoteEscapingInStringLiterals;
      IntegerArgumentsScale = integerArgumentsScale;

      IntegerArgumentScaleFactor = (int) Math.Pow(10, integerArgumentsScale);
    }

    public GCodeCaseNormalization CaseNormalization { get; }

    /// <summary>
    /// Multiply all the numbers in GCode by the 10 to the specified power.
    /// For X123.45 and the scale factor is 4, the .IntArgumentScaled is equal to 1234560.
    /// For X12.3456 and the scale factor is 2, the .IntArgumentScaled is equal to 1234.
    /// Allowed values: 0 - 5.
    /// </summary>
    public int IntegerArgumentsScale { get; }

    internal int IntegerArgumentScaleFactor { get; }

    /// <summary>
    /// When 'false', for S"Alex's printer" the string argument content is "Alex's printer"
    /// When 'true', for S"A'L'E'X'''S PRINTER" the string argument content is "Alex's PRINTER"
    /// </summary>
    public bool EnableSingleQuoteEscapingInStringLiterals { get; }
  }

  public enum GCodeCaseNormalization
  {
    DoNotTouch,
    ToUppercase,
    ToLowercase
  }
}