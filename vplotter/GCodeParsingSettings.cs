namespace VPlotter
{
  public sealed class GCodeParsingSettings
  {
    public static readonly GCodeParsingSettings Default = new GCodeParsingSettings(
      caseNormalization: GCodeCaseNormalization.ToUppercase);

    public GCodeParsingSettings(
      GCodeCaseNormalization caseNormalization = GCodeCaseNormalization.DoNotTouch,
      bool allowWhitespaceInFieldNumbers = false)
    {
      CaseNormalization = caseNormalization;
    }

    public GCodeCaseNormalization CaseNormalization { get; }

    public bool AllowWhitespaceInFieldNumbers { get; }
  }

  public enum GCodeCaseNormalization
  {
    DoNotTouch,
    ToUppercase,
    ToLowercase
  }
}