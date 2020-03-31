using System;
using NUnit.Framework;
using VPlotter;
// ReSharper disable StringLiteralTypo

namespace VMotion.Tests
{
  [TestFixture]
  public class GCodeFieldParsingTest
  {
    [Test]
    public void Invalid01()
    {
      var field = GCodeField.TryParse(" ".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsFalse(field.IsValid);
      Assert.IsFalse(field.HasArgument);
      Assert.AreEqual('\0', field.Word);
      Assert.AreEqual(" ", tail.ToString());
    }

    [Test]
    public void Invalid02()
    {
      var field = GCodeField.TryParse("X  -Y".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(field.IsValid);
      Assert.IsFalse(field.HasArgument);
      Assert.AreEqual('X', field.Word);
      Assert.AreEqual("", field.RawArgument.ToString());
      Assert.AreEqual("  -Y", tail.ToString());
    }

    [Test]
    public void Invalid03()
    {
      var field = GCodeField.TryParse("X.tail".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(field.IsValid);
      Assert.IsFalse(field.HasArgument);
      Assert.AreEqual('X', field.Word);
      Assert.AreEqual("", field.RawArgument.ToString());
      Assert.AreEqual(".tail", tail.ToString());
    }

    [Test]
    public void Invalid04()
    {
      var field = GCodeField.TryParse("X-.tail".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(field.IsValid);
      Assert.IsFalse(field.HasArgument);
      Assert.AreEqual('X', field.Word);
      Assert.AreEqual("", field.RawArgument.ToString());
      Assert.AreEqual("-.tail", tail.ToString());
    }

    [Test]
    public void Invalid05()
    {
      var field = GCodeField.TryParse("X-tail".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(field.IsValid);
      Assert.IsFalse(field.HasArgument);
      Assert.AreEqual('X', field.Word);
      Assert.AreEqual("", field.RawArgument.ToString());
      Assert.AreEqual("-tail", tail.ToString());
    }

    [Test]
    public void Trivial()
    {
      var trivial = GCodeField.TryParse(
        "X \t".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(trivial.IsValid);
      Assert.IsFalse(trivial.HasArgument);
      Assert.AreEqual('X', trivial.Word);
      Assert.AreEqual("X", trivial.Raw.ToString());
      Assert.AreEqual("", trivial.RawArgument.ToString());
      Assert.AreEqual("", trivial.StringArgument.ToString());
      Assert.AreEqual(" \t", tail.ToString());
    }

    [Test]
    public void CaseToNormalization()
    {
      var toUpper = GCodeField.TryParse(
        "x".AsSpan(), out _, new GCodeParsingSettings(caseNormalization: GCodeCaseNormalization.ToUppercase));

      Assert.AreEqual('X', toUpper.Word);

      var toLower = GCodeField.TryParse(
        "X".AsSpan(), out _, new GCodeParsingSettings(caseNormalization: GCodeCaseNormalization.ToLowercase));

      Assert.AreEqual(toLower.Word, 'x');

      var doNotTouch = GCodeField.TryParse(
        "x".AsSpan(), out _, new GCodeParsingSettings(caseNormalization: GCodeCaseNormalization.DoNotTouch));

      Assert.AreEqual(doNotTouch.Word, 'x');
    }

    [Test]
    public void IntArgument01()
    {
      var field = GCodeField.TryParse("x123y1".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('X', field.Word);
      Assert.AreEqual("123", field.RawArgument.ToString());
      Assert.AreEqual(123, field.IntArgument);
      Assert.AreEqual(123.0f, field.FloatArgument);
      Assert.AreEqual(123.0d, field.DoubleArgument);
      Assert.AreEqual(123.0m, field.DecimalArgument);
      Assert.AreEqual("123", field.StringArgument.ToString());
      Assert.AreEqual("y1", tail.ToString());

      // todo: do not insert pair "" after 123 literal
    }

    [Test]
    public void IntArgument02()
    {
      var field = GCodeField.TryParse("Y-42tail".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Y', field.Word);
      Assert.AreEqual("Y-42", field.Raw.ToString());
      Assert.AreEqual("-42", field.RawArgument.ToString());
      Assert.AreEqual(-42, field.IntArgument);
      Assert.AreEqual(-42f, field.FloatArgument);
      Assert.AreEqual(-42d, field.DoubleArgument);
      Assert.AreEqual(-42m, field.DecimalArgument);
      Assert.AreEqual("-42", field.StringArgument.ToString());
      Assert.AreEqual("tail", tail.ToString());
    }

    [Test]
    public void IntArgument03()
    {
      var field = GCodeField.TryParse("Y + 2147483647 tail".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Y', field.Word);
      Assert.AreEqual("Y + 2147483647", field.Raw.ToString());
      Assert.AreEqual(" + 2147483647", field.RawArgument.ToString());
      Assert.AreEqual(2147483647, field.IntArgument);
      Assert.AreEqual(2147483647f, field.FloatArgument);
      Assert.AreEqual(2147483647d, field.DoubleArgument);
      Assert.AreEqual(2147483647m, field.DecimalArgument);
      Assert.AreEqual("+ 2147483647", field.StringArgument.ToString());
      Assert.AreEqual(" tail", tail.ToString());
    }

    [Test]
    public void IntArgument04()
    {
      var field = GCodeField.TryParse("Y    -\t2147483647 tail".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Y', field.Word);
      Assert.AreEqual("Y    -\t2147483647", field.Raw.ToString());
      Assert.AreEqual("    -\t2147483647", field.RawArgument.ToString());
      Assert.AreEqual(-2147483647, field.IntArgument);
      Assert.AreEqual(-2147483647f, field.FloatArgument);
      Assert.AreEqual(-2147483647d, field.DoubleArgument);
      Assert.AreEqual(-2147483647m, field.DecimalArgument);
      Assert.AreEqual("-\t2147483647", field.StringArgument.ToString());
      Assert.AreEqual(" tail", tail.ToString());
    }

    [Test]
    public void RealArgument01()
    {
      var field = GCodeField.TryParse("Z1.2tail".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Z', field.Word);
      Assert.AreEqual("Z1.2", field.Raw.ToString());
      Assert.AreEqual("1.2", field.RawArgument.ToString());
      Assert.AreEqual(1, field.IntArgument);
      Assert.AreEqual(1.2f, field.FloatArgument);
      Assert.AreEqual(1.2d, field.DoubleArgument);
      Assert.AreEqual(1.2m, field.DecimalArgument);
      Assert.AreEqual("1.2", field.StringArgument.ToString());
      Assert.AreEqual("tail", tail.ToString());
    }

    [Test]
    public void RealArgument02()
    {
      var field = GCodeField.TryParse("Z42.tail".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Z', field.Word);
      Assert.AreEqual("Z42.", field.Raw.ToString());
      Assert.AreEqual("42.", field.RawArgument.ToString());
      Assert.AreEqual(42, field.IntArgument);
      Assert.AreEqual(42.0f, field.FloatArgument);
      Assert.AreEqual(42.0d, field.DoubleArgument);
      Assert.AreEqual(42.0m, field.DecimalArgument);
      Assert.AreEqual("42.", field.StringArgument.ToString());
      Assert.AreEqual("tail", tail.ToString());
    }

    [Test]
    public void RealArgument03()
    {
      var field = GCodeField.TryParse("Z.42tail".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Z', field.Word);
      Assert.AreEqual("Z.42", field.Raw.ToString());
      Assert.AreEqual(".42", field.RawArgument.ToString());
      Assert.AreEqual(0, field.IntArgument);
      Assert.AreEqual(.42f, field.FloatArgument);
      Assert.AreEqual(.42d, field.DoubleArgument);
      Assert.AreEqual(.42m, field.DecimalArgument);
      Assert.AreEqual(".42", field.StringArgument.ToString());
      Assert.AreEqual("tail", tail.ToString());
    }

    [Test]
    public void RealArgument04()
    {
      var field = GCodeField.TryParse("Z-42.tail".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Z', field.Word);
      Assert.AreEqual("Z-42.", field.Raw.ToString());
      Assert.AreEqual("-42.", field.RawArgument.ToString());
      Assert.AreEqual(-42, field.IntArgument);
      Assert.AreEqual(-42.0f, field.FloatArgument);
      Assert.AreEqual(-42.0d, field.DoubleArgument);
      Assert.AreEqual(-42.0m, field.DecimalArgument);
      Assert.AreEqual("-42.", field.StringArgument.ToString());
      Assert.AreEqual("tail", tail.ToString());
    }

    [Test]
    public void RealArgument05()
    {
      var field = GCodeField.TryParse("Z-.42tail".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Z', field.Word);
      Assert.AreEqual("Z-.42", field.Raw.ToString());
      Assert.AreEqual("-.42", field.RawArgument.ToString());
      Assert.AreEqual(0, field.IntArgument);
      Assert.AreEqual(-.42f, field.FloatArgument);
      Assert.AreEqual(-.42d, field.DoubleArgument);
      Assert.AreEqual(-.42m, field.DecimalArgument);
      Assert.AreEqual("-.42", field.StringArgument.ToString());
      Assert.AreEqual("tail", tail.ToString());
    }

    [Test]
    public void RealArgument06()
    {
      var field = GCodeField.TryParse("Z-123.42tail".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Z', field.Word);
      Assert.AreEqual("Z-123.42", field.Raw.ToString());
      Assert.AreEqual("-123.42", field.RawArgument.ToString());
      Assert.AreEqual(-123, field.IntArgument);
      Assert.AreEqual(-123.42f, field.FloatArgument);
      Assert.AreEqual(-123.42d, field.DoubleArgument);
      Assert.AreEqual(-123.42m, field.DecimalArgument);
      Assert.AreEqual("-123.42", field.StringArgument.ToString());
      Assert.AreEqual("tail", tail.ToString());
    }

    [Test]
    public void RealArgument07()
    {
      var field = GCodeField.TryParse("Z - 12 . 3tail".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Z', field.Word);
      Assert.AreEqual("Z - 12 . 3", field.Raw.ToString());
      Assert.AreEqual(" - 12 . 3", field.RawArgument.ToString());
      Assert.AreEqual(-12, field.IntArgument);
      Assert.AreEqual(-12.3f, field.FloatArgument);
      Assert.AreEqual(-12.3d, field.DoubleArgument);
      Assert.AreEqual(-12.3m, field.DecimalArgument);
      Assert.AreEqual("- 12 . 3", field.StringArgument.ToString());
      Assert.AreEqual("tail", tail.ToString());
    }

    [Test]
    public void RealArgument08()
    {
      var field = GCodeField.TryParse("Z - 12 .tail".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Z', field.Word);
      Assert.AreEqual("Z - 12 .", field.Raw.ToString());
      Assert.AreEqual(" - 12 .", field.RawArgument.ToString());
      Assert.AreEqual(-12, field.IntArgument);
      Assert.AreEqual(-12f, field.FloatArgument);
      Assert.AreEqual(-12d, field.DoubleArgument);
      Assert.AreEqual(-12m, field.DecimalArgument);
      Assert.AreEqual("- 12 .", field.StringArgument.ToString());
      Assert.AreEqual("tail", tail.ToString());
    }

    [Test]
    public void RealArgument09()
    {
      var field = GCodeField.TryParse("Z - . 34tail".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Z', field.Word);
      Assert.AreEqual("Z - . 34", field.Raw.ToString());
      Assert.AreEqual(" - . 34", field.RawArgument.ToString());
      Assert.AreEqual(0, field.IntArgument);
      Assert.AreEqual(-0.34f, field.FloatArgument);
      Assert.AreEqual(-0.34d, field.DoubleArgument);
      Assert.AreEqual(-0.34m, field.DecimalArgument);
      Assert.AreEqual("- . 34", field.StringArgument.ToString());
      Assert.AreEqual("tail", tail.ToString());
    }

    [Test]
    public void RealArgument10()
    {
      var field = GCodeField.TryParse("Z . 34tail".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Z', field.Word);
      Assert.AreEqual("Z . 34", field.Raw.ToString());
      Assert.AreEqual(" . 34", field.RawArgument.ToString());
      Assert.AreEqual(0, field.IntArgument);
      Assert.AreEqual(.34f, field.FloatArgument);
      Assert.AreEqual(.34d, field.DoubleArgument);
      Assert.AreEqual(.34m, field.DecimalArgument);
      Assert.AreEqual(". 34", field.StringArgument.ToString());
      Assert.AreEqual("tail", tail.ToString());
    }

    // todo: multiplication

    [Test]
    public void StringArgument01()
    {
      var field = GCodeField.TryParse("S\"abc\" Y".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('S', field.Word);
      Assert.AreEqual("S\"abc\"", field.Raw.ToString());
      Assert.AreEqual("\"abc\"", field.RawArgument.ToString());
      Assert.AreEqual("abc", field.StringArgument.ToString());
      Assert.AreEqual(" Y", tail.ToString());
    }

    [Test]
    public void StringArgument02()
    {
      var field = GCodeField.TryParse("S \"def\"aa".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('S', field.Word);
      Assert.AreEqual("S \"def\"", field.Raw.ToString());
      Assert.AreEqual(" \"def\"", field.RawArgument.ToString());
      Assert.AreEqual("def", field.StringArgument.ToString());
      Assert.AreEqual("aa", tail.ToString());
    }

    [Test]
    public void StringArgument03()
    {
      var field = GCodeField.TryParse("S \"def".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('S', field.Word);
      Assert.AreEqual("S \"def", field.Raw.ToString());
      Assert.AreEqual(" \"def", field.RawArgument.ToString());
      Assert.AreEqual("def", field.StringArgument.ToString());
      Assert.AreEqual("", tail.ToString());
    }

    [Test]
    public void StringArgument04()
    {
      var field = GCodeField.TryParse("S\"foo\"\"bar\"t".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('S', field.Word);
      Assert.AreEqual("S\"foo\"\"bar\"", field.Raw.ToString());
      Assert.AreEqual("\"foo\"\"bar\"", field.RawArgument.ToString());
      Assert.AreEqual("foo\"bar", field.StringArgument.ToString());
      Assert.AreEqual("t", tail.ToString());
    }

    [Test]
    public void StringArgument05()
    {
      var field = GCodeField.TryParse("S\"Foo'Bar\"t".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('S', field.Word);
      Assert.AreEqual("S\"Foo'Bar\"", field.Raw.ToString());
      Assert.AreEqual("\"Foo'Bar\"", field.RawArgument.ToString());
      Assert.AreEqual("Foo'Bar", field.StringArgument.ToString());
      Assert.AreEqual("t", tail.ToString());
    }

    [Test]
    public void StringArgument06()
    {
      var field = GCodeField.TryParse("S\"Foo'Bar'".AsSpan(), out var tail,
        new GCodeParsingSettings(enableSingleQuoteEscapingInStringLiterals: true));

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('S', field.Word);
      Assert.AreEqual("S\"Foo'Bar'", field.Raw.ToString());
      Assert.AreEqual("\"Foo'Bar'", field.RawArgument.ToString());
      Assert.AreEqual("Foobar", field.StringArgument.ToString());
      Assert.AreEqual("", tail.ToString());
    }

    [Test]
    public void StringArgument07()
    {
      var field = GCodeField.TryParse("S\"'Foo''Bar".AsSpan(), out var tail,
        new GCodeParsingSettings(enableSingleQuoteEscapingInStringLiterals: true));

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('S', field.Word);
      Assert.AreEqual("S\"'Foo''Bar", field.Raw.ToString());
      Assert.AreEqual("\"'Foo''Bar", field.RawArgument.ToString());
      Assert.AreEqual("foo'Bar", field.StringArgument.ToString());
      Assert.AreEqual("", tail.ToString());
    }
  }
}