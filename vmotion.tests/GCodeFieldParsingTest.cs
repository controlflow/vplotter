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
      Assert.AreEqual('-', field.RawArgument.ToString());
      Assert.AreEqual("Y", tail.ToString());
    }

    [Test]
    public void Trivial()
    {
      var trivial = GCodeField.TryParse(
        "X \t".AsSpan(), out var tail, GCodeParsingSettings.Default);

      Assert.IsTrue(trivial.IsValid);
      Assert.IsFalse(trivial.HasArgument);
      Assert.AreEqual(trivial.Word, 'X');
      Assert.AreEqual(tail.ToString(), " \t");
    }

    [Test]
    public void CaseToNormalization()
    {
      var toUpper = GCodeField.TryParse(
        "x".AsSpan(), out _,
        new GCodeParsingSettings(caseNormalization: GCodeCaseNormalization.ToUppercase));

      Assert.AreEqual('X', toUpper.Word);

      var toLower = GCodeField.TryParse(
        "X".AsSpan(), out _,
        new GCodeParsingSettings(caseNormalization: GCodeCaseNormalization.ToLowercase));

      Assert.AreEqual(toLower.Word, 'x');
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
      Assert.AreEqual(123.0d, field.DoubleValue);
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
      Assert.AreEqual("-42", field.RawArgument.ToString());
      Assert.AreEqual(-42, field.IntArgument);
      Assert.AreEqual(-42f, field.FloatArgument);
      Assert.AreEqual(-42d, field.DoubleValue);
      Assert.AreEqual("-42", field.StringArgument.ToString());
      Assert.AreEqual("tail", tail.ToString());
    }

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