using System;
using NUnit.Framework;
using VPlotter;

namespace VMotion.Tests
{
  [TestFixture]
  public class GCodeFieldParsingTest
  {
    [Test]
    public void Invalid()
    {
      var invalid = GCodeField.TryParse(
        "".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsFalse(invalid.IsValid);
      Assert.AreEqual(invalid.Word, '\0');
      Assert.AreEqual(tail.ToString(), "");
    }

    [Test]
    public void Trivial()
    {
      var trivial = GCodeField.TryParse(
        "X".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(trivial.IsValid);
      Assert.AreEqual(trivial.Word, 'X');
      Assert.AreEqual(tail.ToString(), "");
    }

    [Test]
    public void CaseToNormalization()
    {
      var toUpper = GCodeField.TryParse(
        "x".AsSpan(), out _,
        new GCodeParsingSettings(caseNormalization: GCodeCaseNormalization.ToUppercase));

      Assert.AreEqual(toUpper.Word, 'X');

      var toLower = GCodeField.TryParse(
        "X".AsSpan(), out _,
        new GCodeParsingSettings(caseNormalization: GCodeCaseNormalization.ToLowercase));

      Assert.AreEqual(toLower.Word, 'x');
    }
  }
}