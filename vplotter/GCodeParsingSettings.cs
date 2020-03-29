namespace VPlotter
{
  public sealed class GCodeParsingSettings
  {
    public static readonly GCodeParsingSettings Default = new GCodeParsingSettings();

    public GCodeParsingSettings(
      GCodeCaseNormalization caseNormalization = GCodeCaseNormalization.ToUppercase,
      bool enableSingleQuoteEscapingInStringLiterals = false)
    {
      CaseNormalization = caseNormalization;
      EnableSingleQuoteEscapingInStringLiterals = enableSingleQuoteEscapingInStringLiterals;
    }

    public GCodeCaseNormalization CaseNormalization { get; }

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