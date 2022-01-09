using System;
using NUnit.Framework;
using VPlotter.GCode;
using VPlotter.GCode.Reader;
// ReSharper disable AssignmentIsFullyDiscarded
// ReSharper disable StringLiteralTypo

namespace VMotion.Tests.GCode
{
  [TestFixture]
  public class GCodeFieldParsingTest
  {
    private static GCodeFieldSpan Parse(string line, out ReadOnlySpan<char> tail)
    {
      return GCodeFieldSpan.TryParse(line, out tail, GCodeParsingSettings.Default);
    }

    [Test]
    public void Invalid01()
    {
      var field = Parse(" ", out var tail);

      Assert.IsFalse(field.IsValid);
      Assert.IsFalse(field.HasArgument);
      Assert.AreEqual('\0', field.Word);
      Assert.AreEqual(Code.Invalid, field.CodeArgument);
      Assert.AreEqual(" ", tail.ToString());
    }

    [Test]
    public void Invalid02()
    {
      var field = Parse("X.tail", out var tail);

      Assert.IsFalse(field.IsValid);
      Assert.IsFalse(field.HasArgument);
      Assert.AreEqual("X.tail", tail.ToString());
    }

    [Test]
    public void Invalid04()
    {
      var field = Parse("X-.tail", out var tail);

      Assert.IsFalse(field.IsValid);
      Assert.IsFalse(field.HasArgument);
      Assert.AreEqual("X-.tail", tail.ToString());
    }

    [Test]
    public void Invalid05()
    {
      var field = Parse("X-tail", out var tail);

      Assert.IsFalse(field.IsValid);
      Assert.IsFalse(field.HasArgument);
      Assert.AreEqual("X-tail", tail.ToString());
    }

    [Test]
    public void Invalid06()
    {
      var field = Parse("endsub", out var tail);

      Assert.IsFalse(field.IsValid);
      Assert.IsFalse(field.HasArgument);
      Assert.AreEqual("endsub", tail.ToString());
    }

    [Test]
    public void NoArgument01()
    {
      var trivial = Parse("X", out var tail);

      Assert.IsTrue(trivial.IsValid);
      Assert.IsFalse(trivial.HasArgument);
      Assert.AreEqual('X', trivial.Word);
      Assert.AreEqual("X", trivial.Raw.ToString());
      Assert.AreEqual("", trivial.StringArgument.ToString());
      Assert.AreEqual("", tail.ToString());
    }

    [Test]
    public void NoArgument02()
    {
      var trivial = Parse("X \t", out var tail);

      Assert.IsTrue(trivial.IsValid);
      Assert.IsFalse(trivial.HasArgument);
      Assert.AreEqual('X', trivial.Word);
      Assert.AreEqual("X", trivial.Raw.ToString());
      Assert.AreEqual("", trivial.StringArgument.ToString());
      Assert.AreEqual(" \t", tail.ToString());
    }

    [Test]
    public void NoArgument03()
    {
      var trivial = Parse("X\t Y", out var tail);

      Assert.IsTrue(trivial.IsValid);
      Assert.IsFalse(trivial.HasArgument);
      Assert.AreEqual('X', trivial.Word);
      Assert.AreEqual("X", trivial.Raw.ToString());
      Assert.AreEqual("", trivial.StringArgument.ToString());
      Assert.AreEqual("\t Y", tail.ToString());
    }

    [Test]
    public void NoArgument04()
    {
      var field = Parse("X\t -Y", out var tail);

      Assert.IsTrue(field.IsValid);
      Assert.IsFalse(field.HasArgument);
      Assert.AreEqual('X', field.Word);
      Assert.AreEqual("X", field.Raw.ToString());
      Assert.AreEqual("", field.StringArgument.ToString());
      Assert.AreEqual("\t -Y", tail.ToString());
    }

    [Test]
    public void NoArgument05()
    {
      var field = Parse("X 9876543210", out var tail);

      Assert.IsTrue(field.IsValid);
      Assert.IsFalse(field.HasArgument);
      Assert.AreEqual('X', field.Word);
      Assert.AreEqual("X", field.Raw.ToString());
      Assert.AreEqual("", field.StringArgument.ToString());
      Assert.AreEqual(" 9876543210", tail.ToString());
    }

    [Test]
    public void NoArgument06()
    {
      var field = Parse("X\t .Y", out var tail);

      Assert.IsTrue(field.IsValid);
      Assert.IsFalse(field.HasArgument);
      Assert.AreEqual('X', field.Word);
      Assert.AreEqual("X", field.Raw.ToString());
      Assert.AreEqual("", field.StringArgument.ToString());
      Assert.AreEqual("\t .Y", tail.ToString());
    }

    [Test]
    public void CaseToNormalization()
    {
      var toUpper = GCodeFieldSpan.TryParse(
        "x", out _, new GCodeParsingSettings(caseNormalization: GCodeCaseNormalization.ToUppercase));

      Assert.AreEqual('X', toUpper.Word);

      var toLower = GCodeFieldSpan.TryParse(
        "X", out _, new GCodeParsingSettings(caseNormalization: GCodeCaseNormalization.ToLowercase));

      Assert.AreEqual(toLower.Word, 'x');

      var doNotTouch = GCodeFieldSpan.TryParse(
        "x", out _, new GCodeParsingSettings(caseNormalization: GCodeCaseNormalization.DoNotTouch));

      Assert.AreEqual(doNotTouch.Word, 'x');
    }

    [Test]
    public void IntArgument01([Range(0, 5)] int scale)
    {
      var field = GCodeFieldSpan.TryParse(
        "*123y1", out var tail,
        new GCodeParsingSettings(integerArgumentScale: scale));

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('*', field.Word);
      Assert.AreEqual(123, field.IntArgument);
      Assert.AreEqual(123.0f, field.FloatArgument);
      Assert.AreEqual(123.0d, field.DoubleArgument);
      Assert.AreEqual(123.0m, field.DecimalArgument);
      Assert.AreEqual("123", field.StringArgument.ToString());
      Assert.AreEqual("y1", tail.ToString());

      // todo: do not insert pair "" after 123 literal
    }

    [Test]
    public void IntArgument02([Range(0, 5)] int scale)
    {
      var field = GCodeFieldSpan.TryParse(
        "Y-42tail", out var tail,
        new GCodeParsingSettings(integerArgumentScale: scale));

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Y', field.Word);
      Assert.AreEqual("Y-42", field.Raw.ToString());
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
      var field = GCodeFieldSpan.TryParse(
        "Y + 2147483647 tail", out var tail,
        new GCodeParsingSettings(integerArgumentScale: 0));

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Y', field.Word);
      Assert.AreEqual("Y + 2147483647", field.Raw.ToString());
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
      var field = Parse("Y    -\t2147483647 tail", out var tail);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Y', field.Word);
      Assert.AreEqual("Y    -\t2147483647", field.Raw.ToString());
      Assert.AreEqual(-2147483647, field.IntArgument);
      Assert.AreEqual(-2147483647f, field.FloatArgument);
      Assert.AreEqual(-2147483647d, field.DoubleArgument);
      Assert.AreEqual(-2147483647m, field.DecimalArgument);
      Assert.AreEqual("-\t2147483647", field.StringArgument.ToString());
      Assert.AreEqual(" tail", tail.ToString());
    }

    [Test]
    public void IntArgument05()
    {
      var field = Parse("X9876543210tail", out var tail);

      Assert.IsFalse(field.IsValid); // overflow
      Assert.AreEqual("X9876543210tail", tail.ToString());
    }

    [Test]
    public void RealArgument01([Range(0, 5)] int scale)
    {
      var field = GCodeFieldSpan.TryParse(
        "Z1.2tail", out var tail,
        new GCodeParsingSettings(integerArgumentScale: scale));

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Z', field.Word);
      Assert.AreEqual("Z1.2", field.Raw.ToString());
      Assert.AreEqual(1, field.IntArgument);
      Assert.AreEqual(1.2f, field.FloatArgument);
      Assert.AreEqual(1.2d, field.DoubleArgument);
      Assert.AreEqual(1.2m, field.DecimalArgument);
      Assert.AreEqual("1.2", field.StringArgument.ToString());
      Assert.AreEqual("tail", tail.ToString());
    }

    [Test]
    public void RealArgument02([Range(0, 5)] int scale)
    {
      var field = GCodeFieldSpan.TryParse(
        "Z42.tail", out var tail,
        new GCodeParsingSettings(integerArgumentScale: scale));

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Z', field.Word);
      Assert.AreEqual("Z42.", field.Raw.ToString());
      Assert.AreEqual(42, field.IntArgument);
      Assert.AreEqual(42.0f, field.FloatArgument);
      Assert.AreEqual(42.0d, field.DoubleArgument);
      Assert.AreEqual(42.0m, field.DecimalArgument);
      Assert.AreEqual("42.", field.StringArgument.ToString());
      Assert.AreEqual("tail", tail.ToString());
    }

    [Test]
    public void RealArgument03([Range(0, 5)] int scale)
    {
      var field = GCodeFieldSpan.TryParse(
        "Z.42tail", out var tail,
        new GCodeParsingSettings(integerArgumentScale: scale));

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Z', field.Word);
      Assert.AreEqual("Z.42", field.Raw.ToString());
      Assert.AreEqual(0, field.IntArgument);
      Assert.AreEqual(.42f, field.FloatArgument);
      Assert.AreEqual(.42d, field.DoubleArgument);
      Assert.AreEqual(.42m, field.DecimalArgument);
      Assert.AreEqual(".42", field.StringArgument.ToString());
      Assert.AreEqual("tail", tail.ToString());
    }

    [Test]
    public void RealArgument04([Range(0, 5)] int scale)
    {
      var field = GCodeFieldSpan.TryParse(
        "Z-42.tail", out var tail,
        new GCodeParsingSettings(integerArgumentScale: scale));

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Z', field.Word);
      Assert.AreEqual("Z-42.", field.Raw.ToString());
      Assert.AreEqual(-42, field.IntArgument);
      Assert.AreEqual(-42.0f, field.FloatArgument);
      Assert.AreEqual(-42.0d, field.DoubleArgument);
      Assert.AreEqual(-42.0m, field.DecimalArgument);
      Assert.AreEqual("-42.", field.StringArgument.ToString());
      Assert.AreEqual("tail", tail.ToString());
    }

    [Test]
    public void RealArgument05([Range(0, 5)] int scale)
    {
      var field = GCodeFieldSpan.TryParse(
        "Z-.42tail", out var tail,
        new GCodeParsingSettings(integerArgumentScale: scale));

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Z', field.Word);
      Assert.AreEqual("Z-.42", field.Raw.ToString());
      Assert.AreEqual(0, field.IntArgument);
      Assert.AreEqual(-.42f, field.FloatArgument);
      Assert.AreEqual(-.42d, field.DoubleArgument);
      Assert.AreEqual(-.42m, field.DecimalArgument);
      Assert.AreEqual("-.42", field.StringArgument.ToString());
      Assert.AreEqual("tail", tail.ToString());
    }

    [Test]
    public void RealArgument06([Range(0, 5)] int scale)
    {
      var field = GCodeFieldSpan.TryParse(
        "Z-123.42tail", out var tail,
        new GCodeParsingSettings(integerArgumentScale: scale));

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Z', field.Word);
      Assert.AreEqual("Z-123.42", field.Raw.ToString());
      Assert.AreEqual(-123, field.IntArgument);
      Assert.AreEqual(-123.42f, field.FloatArgument);
      Assert.AreEqual(-123.42d, field.DoubleArgument);
      Assert.AreEqual(-123.42m, field.DecimalArgument);
      Assert.AreEqual("-123.42", field.StringArgument.ToString());
      Assert.AreEqual("tail", tail.ToString());
    }

    [Test]
    public void RealArgument07([Range(0, 5)] int scale)
    {
      var field = GCodeFieldSpan.TryParse(
        "Z - 12 . 3tail", out var tail,
        new GCodeParsingSettings(integerArgumentScale: scale));

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Z', field.Word);
      Assert.AreEqual("Z - 12 . 3", field.Raw.ToString());
      Assert.AreEqual(-12, field.IntArgument);
      Assert.AreEqual(-12.3f, field.FloatArgument);
      Assert.AreEqual(-12.3d, field.DoubleArgument);
      Assert.AreEqual(-12.3m, field.DecimalArgument);
      Assert.AreEqual("- 12 . 3", field.StringArgument.ToString());
      Assert.AreEqual("tail", tail.ToString());
    }

    [Test]
    public void RealArgument08([Range(0, 5)] int scale)
    {
      var field = GCodeFieldSpan.TryParse(
        "Z - 12 .tail", out var tail,
        new GCodeParsingSettings(integerArgumentScale: scale));

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Z', field.Word);
      Assert.AreEqual("Z - 12 .", field.Raw.ToString());
      Assert.AreEqual(-12, field.IntArgument);
      Assert.AreEqual(-12f, field.FloatArgument);
      Assert.AreEqual(-12d, field.DoubleArgument);
      Assert.AreEqual(-12m, field.DecimalArgument);
      Assert.AreEqual("- 12 .", field.StringArgument.ToString());
      Assert.AreEqual("tail", tail.ToString());
    }

    [Test]
    public void RealArgument09([Range(0, 5)] int scale)
    {
      var field = GCodeFieldSpan.TryParse(
        "Z - . 34tail", out var tail,
        new GCodeParsingSettings(integerArgumentScale: scale));

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Z', field.Word);
      Assert.AreEqual("Z - . 34", field.Raw.ToString());
      Assert.AreEqual(0, field.IntArgument);
      Assert.AreEqual(-0.34f, field.FloatArgument);
      Assert.AreEqual(-0.34d, field.DoubleArgument);
      Assert.AreEqual(-0.34m, field.DecimalArgument);
      Assert.AreEqual("- . 34", field.StringArgument.ToString());
      Assert.AreEqual("tail", tail.ToString());
    }

    [Test]
    public void RealArgument10([Range(0, 5)] int scale)
    {
      var field = GCodeFieldSpan.TryParse(
        "Z . 34tail", out var tail,
        new GCodeParsingSettings(integerArgumentScale: scale));

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('Z', field.Word);
      Assert.AreEqual("Z . 34", field.Raw.ToString());
      Assert.AreEqual(0, field.IntArgument);
      Assert.AreEqual(.34f, field.FloatArgument);
      Assert.AreEqual(.34d, field.DoubleArgument);
      Assert.AreEqual(.34m, field.DecimalArgument);
      Assert.AreEqual(". 34", field.StringArgument.ToString());
      Assert.AreEqual("tail", tail.ToString());
    }

    [Test]
    public void RealArgument11()
    {
      var field = GCodeFieldSpan.TryParse(
        "X-.1tail", out var tail, new GCodeParsingSettings(integerArgumentScale: 2));

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('X', field.Word);
      Assert.AreEqual("X-.1", field.Raw.ToString());
      Assert.AreEqual(0, field.IntArgument);
      Assert.AreEqual(-10, field.ScaledIntArgument);
      Assert.AreEqual(-.1f, field.FloatArgument);
      Assert.AreEqual(-.1d, field.DoubleArgument);
      Assert.AreEqual(-.1m, field.DecimalArgument);
      Assert.AreEqual("-.1", field.StringArgument.ToString());
      Assert.AreEqual("tail", tail.ToString());
    }

    [Test]
    public void StringArgument01()
    {
      var field = Parse("S\"abc\" Y", out var tail);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('S', field.Word);
      Assert.AreEqual("S\"abc\"", field.Raw.ToString());
      Assert.AreEqual("abc", field.StringArgument.ToString());
      Assert.AreEqual(" Y", tail.ToString());
    }

    [Test]
    public void StringArgument02()
    {
      var field = Parse("S \"def\"aa", out var tail);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('S', field.Word);
      Assert.AreEqual("S \"def\"", field.Raw.ToString());
      Assert.AreEqual("def", field.StringArgument.ToString());
      Assert.AreEqual("aa", tail.ToString());
    }

    [Test]
    public void StringArgument03()
    {
      var field = Parse("S \"def", out var tail);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('S', field.Word);
      Assert.AreEqual("S \"def", field.Raw.ToString());
      Assert.AreEqual("def", field.StringArgument.ToString());
      Assert.AreEqual("", tail.ToString());
    }

    [Test]
    public void StringArgument04()
    {
      var field = Parse("S\"foo\"\"bar\"t", out var tail);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('S', field.Word);
      Assert.AreEqual("S\"foo\"\"bar\"", field.Raw.ToString());
      Assert.AreEqual("foo\"bar", field.StringArgument.ToString());
      Assert.AreEqual("t", tail.ToString());
    }

    [Test]
    public void StringArgument05()
    {
      var field = Parse("S\"Foo'Bar\"t", out var tail);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('S', field.Word);
      Assert.AreEqual("S\"Foo'Bar\"", field.Raw.ToString());
      Assert.AreEqual("Foo'Bar", field.StringArgument.ToString());
      Assert.AreEqual("t", tail.ToString());
    }

    [Test]
    public void StringArgument06()
    {
      var field = GCodeFieldSpan.TryParse("S\"Foo'Bar'", out var tail,
        new GCodeParsingSettings(enableSingleQuoteEscapingInStringLiterals: true));

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('S', field.Word);
      Assert.AreEqual("S\"Foo'Bar'", field.Raw.ToString());
      Assert.AreEqual("Foobar", field.StringArgument.ToString());
      Assert.AreEqual("", tail.ToString());
    }

    [Test]
    public void StringArgument07()
    {
      var field = GCodeFieldSpan.TryParse("S\"'Foo''Bar", out var tail,
        new GCodeParsingSettings(enableSingleQuoteEscapingInStringLiterals: true));

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('S', field.Word);
      Assert.AreEqual("S\"'Foo''Bar", field.Raw.ToString());
      Assert.AreEqual("foo'Bar", field.StringArgument.ToString());
      Assert.AreEqual("", tail.ToString());
    }

    [Test]
    public void CodeArgument01()
    {
      var field = Parse("G0 X0", out var tail);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('G', field.Word);
      Assert.AreEqual("G0", field.Raw.ToString());
      Assert.AreEqual(0, field.IntArgument);
      Assert.AreEqual(Code.G0_RapidMove, field.CodeArgument);
      Assert.AreEqual(" X0", tail.ToString());
    }

    [Test]
    public void CodeArgument02()
    {
      var field = Parse("G1 X0 Y0", out var tail);

      Assert.IsTrue(field.IsValid);
      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual('G', field.Word);
      Assert.AreEqual("G1", field.Raw.ToString());
      Assert.AreEqual(1, field.IntArgument);
      Assert.AreEqual(Code.G1_LinearMove, field.CodeArgument);
      Assert.AreEqual(" X0 Y0", tail.ToString());
    }

    [Test]
    public void CodeArgument03()
    {
      var field = Parse("G1.42", out _);

      Assert.IsTrue(field.IsValid);
      Assert.AreEqual('G', field.Word);
      // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
      Assert.AreEqual(Code.G1_LinearMove | (Code) (42 << 16), field.CodeArgument);
    }

    [Test]
    public void IntScaling01()
    {
      var field = GCodeFieldSpan.TryParse("Y-123 ", out var tail,
        new GCodeParsingSettings(integerArgumentScale: 1));

      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual(-123, field.IntArgument);
      Assert.AreEqual(-1230, field.ScaledIntArgument);
      Assert.AreEqual(-123f, field.FloatArgument);
      Assert.AreEqual(-123d, field.DoubleArgument);
      Assert.AreEqual(-123m, field.DecimalArgument);
      Assert.AreEqual(" ", tail.ToString());
    }

    [Test]
    public void IntScaling02()
    {
      var field = GCodeFieldSpan.TryParse(
        "X214748364", out _, new GCodeParsingSettings(integerArgumentScale: 1));

      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual(214748364, field.IntArgument);
      Assert.AreEqual(2147483640, field.ScaledIntArgument);
      Assert.AreEqual(214748364f, field.FloatArgument);
      Assert.AreEqual(214748364d, field.DoubleArgument);
      Assert.AreEqual(214748364m, field.DecimalArgument);
    }

    [Test]
    public void IntScaling03()
    {
      var field = GCodeFieldSpan.TryParse(
        "X214748365", out _, new GCodeParsingSettings(integerArgumentScale: 1));

      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual(214748365, field.IntArgument);
      Assert.AreEqual(214748365f, field.FloatArgument);
      Assert.AreEqual(214748365d, field.DoubleArgument);
      Assert.AreEqual(214748365m, field.DecimalArgument);

      try
      {
        _ = field.ScaledIntArgument;
        Assert.Fail("Must be unreachable");
      }
      catch (OverflowException) { }
    }

    [Test]
    public void IntScaling04()
    {
      var field = GCodeFieldSpan.TryParse(
        "X21474836", out _, new GCodeParsingSettings(integerArgumentScale: 2));

      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual(21474836, field.IntArgument);
      Assert.AreEqual(2147483600, field.ScaledIntArgument);
      Assert.AreEqual(21474836f, field.FloatArgument);
      Assert.AreEqual(21474836d, field.DoubleArgument);
      Assert.AreEqual(21474836m, field.DecimalArgument);
    }

    [Test]
    public void IntScaling05()
    {
      var field = GCodeFieldSpan.TryParse(
        "X21474837", out _, new GCodeParsingSettings(integerArgumentScale: 2));

      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual(21474837, field.IntArgument);
      Assert.AreEqual(21474837f, field.FloatArgument);
      Assert.AreEqual(21474837d, field.DoubleArgument);
      Assert.AreEqual(21474837m, field.DecimalArgument);

      try
      {
        _ = field.ScaledIntArgument;
        Assert.Fail("Must be unreachable");
      }
      catch (OverflowException) { }
    }

    [Test]
    public void IntScaling06()
    {
      var field = GCodeFieldSpan.TryParse(
        "X21474", out _, new GCodeParsingSettings(integerArgumentScale: 5));

      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual(21474, field.IntArgument);
      Assert.AreEqual(2147400000, field.ScaledIntArgument);
      Assert.AreEqual(21474f, field.FloatArgument);
      Assert.AreEqual(21474d, field.DoubleArgument);
      Assert.AreEqual(21474m, field.DecimalArgument);
    }

    [Test]
    public void IntScaling07()
    {
      var field = GCodeFieldSpan.TryParse(
        "X21475", out _, new GCodeParsingSettings(integerArgumentScale: 5));

      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual(21475, field.IntArgument);
      Assert.AreEqual(21475f, field.FloatArgument);
      Assert.AreEqual(21475d, field.DoubleArgument);
      Assert.AreEqual(21475m, field.DecimalArgument);

      try
      {
        _ = field.ScaledIntArgument;
        Assert.Fail("Must be unreachable");
      }
      catch (OverflowException) { }
    }

    [Test]
    public void RealScaling01()
    {
      var field = GCodeFieldSpan.TryParse(
        "X+214.74", out _, new GCodeParsingSettings(integerArgumentScale: 5));

      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual(214, field.IntArgument);
      Assert.AreEqual(21474000, field.ScaledIntArgument);
      Assert.AreEqual(214.74f, field.FloatArgument);
      Assert.AreEqual(214.74d, field.DoubleArgument);
      Assert.AreEqual(214.74m, field.DecimalArgument);
    }

    [Test]
    public void RealScaling02()
    {
      var field = GCodeFieldSpan.TryParse(
        "X-214.748364", out _, new GCodeParsingSettings(integerArgumentScale: 5));

      Assert.IsTrue(field.HasArgument);
      Assert.AreEqual(-214, field.IntArgument);
      Assert.AreEqual(-21474836, field.ScaledIntArgument);
      Assert.AreEqual(-214.748364f, field.FloatArgument);
      Assert.AreEqual(-214.748364d, field.DoubleArgument);
      Assert.AreEqual(-214.748364m, field.DecimalArgument);
    }

    [Test]
    public void RealScaling03()
    {
      var field = GCodeFieldSpan.TryParse(
        "X+21474.83647", out _, new GCodeParsingSettings(integerArgumentScale: 5));

      Assert.AreEqual(21474, field.IntArgument);
      Assert.AreEqual(2147483647, field.ScaledIntArgument);
      Assert.AreEqual(21474.83647f, field.FloatArgument);
      Assert.AreEqual(21474.83647d, field.DoubleArgument);
      Assert.AreEqual(21474.83647m, field.DecimalArgument);
    }

    [Test]
    public void RealScaling04()
    {
      var field = GCodeFieldSpan.TryParse(
        "X+21474.83648", out _, new GCodeParsingSettings(integerArgumentScale: 5));

      Assert.AreEqual(21474, field.IntArgument);
      Assert.AreEqual(21474.83648f, field.FloatArgument);
      Assert.AreEqual(21474.83648d, field.DoubleArgument);
      Assert.AreEqual(21474.83648m, field.DecimalArgument);

      try
      {
        _ = field.ScaledIntArgument;
        Assert.Fail("Must be unreachable");
      }
      catch (OverflowException) { }
    }

    [Test]
    public void RealScaling05()
    {
      var field = GCodeFieldSpan.TryParse(
        "X+21474.8364789", out _, new GCodeParsingSettings(integerArgumentScale: 5));

      Assert.AreEqual(21474, field.IntArgument);
      Assert.AreEqual(2147483647, field.ScaledIntArgument);
      Assert.AreEqual(21474.8364789f, field.FloatArgument);
      Assert.AreEqual(21474.8364789d, field.DoubleArgument);
      Assert.AreEqual(21474.8364789m, field.DecimalArgument);
    }
  }
}