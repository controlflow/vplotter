using System;
using JetBrains.Annotations;

namespace VPlotter.GCode
{
  [PublicAPI]
  public sealed class GCodeParsingSettings
  {
    public static readonly GCodeParsingSettings Default = new();

    public GCodeParsingSettings(
      GCodeCaseNormalization caseNormalization = GCodeCaseNormalization.ToUppercase,
      int integerArgumentScale = 0,
      bool enableSingleQuoteEscapingInStringLiterals = false)
    {
      if (integerArgumentScale is < 0 or > 5)
        throw new ArgumentOutOfRangeException(nameof(integerArgumentScale));

      CaseNormalization = caseNormalization;
      EnableSingleQuoteEscapingInStringLiterals = enableSingleQuoteEscapingInStringLiterals;

      IntegerArgumentScale = (byte) integerArgumentScale;
      myIntegerArgumentScaleFactor = (int) Math.Pow(10, integerArgumentScale);
      myIntegerArgumentMaxIntegralPart = int.MaxValue / myIntegerArgumentScaleFactor;
    }

    public GCodeCaseNormalization CaseNormalization { get; }

    /// <summary>
    /// Multiply all the numbers in GCode by the 10 to the specified power.
    /// For X123.45 and the scale factor is 4, the .IntArgumentScaled is equal to 1234560.
    /// For X12.3456 and the scale factor is 2, the .IntArgumentScaled is equal to 1234.
    /// Allowed values: 0 - 5.
    /// </summary>
    public byte IntegerArgumentScale { get; }

    private readonly int myIntegerArgumentScaleFactor;
    private readonly int myIntegerArgumentMaxIntegralPart;

    internal int ScaleInteger(int integralValue, int sign)
    {
      if (IntegerArgumentScale == 0)
        return integralValue / sign;

      if (integralValue > myIntegerArgumentMaxIntegralPart)
        return int.MinValue;

      return integralValue / sign * myIntegerArgumentScaleFactor;
    }

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