using System;
using NUnit.Framework;
using VPlotter.GCode;
using VPlotter.GCode.Reader;

namespace VMotion.Tests.GCode
{
  [TestFixture]
  public class GCodeCommandParsingTest
  {
    private static GCodeCommandSpan Parse(string line)
    {
      return GCodeCommandSpan.TryParse(line, GCodeParsingSettings.Default);
    }

    [Test]
    public void Empty()
    {
      var command = Parse("");

      Assert.IsFalse(command.IsValid);
      Assert.AreEqual(Code.Invalid, command.Code);
      Assert.AreEqual(0, command.CodeNumber);
      Assert.AreEqual(0, command.SubCodeNumber);

      Assert.IsFalse(command is ('G') _);

      Assert.AreEqual("<Invalid G-Code>", command.ToString());
    }

    [Test]
    public void Simple()
    {
      var command = Parse("G0 X0 Y1");

      Assert.IsTrue(command.IsValid);
      Assert.AreEqual(Code.G0_RapidMove, command.Code);
      Assert.AreEqual(0, command.CodeNumber);
      Assert.AreEqual(0, command.SubCodeNumber);

      var enumerator = command.Fields.GetEnumerator();

      Assert.IsTrue(enumerator.MoveNext());
      var firstField = enumerator.Current;
      Assert.AreEqual('X', firstField.Word);
      Assert.AreEqual(0, firstField.IntArgument);

      Assert.IsTrue(enumerator.MoveNext());
      var secondField = enumerator.Current;
      Assert.AreEqual('Y', secondField.Word);
      Assert.AreEqual(1, secondField.IntArgument);

      Assert.IsFalse(enumerator.MoveNext());

      Assert.IsTrue(command is ('G') _);
      Assert.IsTrue(command is ('G', 0));
      Assert.IsFalse(command is ('G', 1));
      Assert.IsFalse(command is ('M') _);

      Assert.AreEqual("G0 X0 Y1", command.ToString());
    }

    [Test]
    [TestCase("G1 X100 Y-5 Z42")]
    [TestCase("G1 Y-5")]
    [TestCase("G1 X100 Y-5")]
    [TestCase("G1")]
    public void OptionalFields01(string commandText)
    {
      var command = Parse("G1 X100 Y-5");

      Assert.IsTrue(command.IsValid);
      Assert.AreEqual(Code.G1_LinearMove, command.Code);
      Assert.AreEqual(1, command.CodeNumber);

      var optionalX = command.GetOptionalField('X', "X100");
      Assert.IsTrue(optionalX.IsValid);
      Assert.AreEqual('X', optionalX.Word);
      Assert.IsTrue(optionalX.HasArgument);
      Assert.AreEqual(100, optionalX.IntArgument);
      Assert.AreEqual(100M, optionalX.DecimalArgument);
      Assert.AreEqual(100.0, optionalX.DoubleArgument);

      var optionalY = command.GetOptionalField('Y', "Y-5");
      Assert.IsTrue(optionalY.IsValid);
      Assert.AreEqual('Y', optionalY.Word);
      Assert.IsTrue(optionalY.HasArgument);
      Assert.AreEqual(-5, optionalY.IntArgument);
      Assert.AreEqual(-5M, optionalY.DecimalArgument);
      Assert.AreEqual(-5.0, optionalY.DoubleArgument);

      var optionalZ = command.GetOptionalField('Z', "Z42");
      Assert.IsTrue(optionalZ.IsValid);
      Assert.AreEqual('Z', optionalZ.Word);
      Assert.IsTrue(optionalZ.HasArgument);
      Assert.AreEqual(42, optionalZ.IntArgument);
      Assert.AreEqual(42M, optionalZ.DecimalArgument);
      Assert.AreEqual(42.0, optionalZ.DoubleArgument);

      try
      {
        _  = command.GetOptionalField('U', "");
        Assert.Fail("Must be unreachable");
      }
      catch (ArgumentException) { }

      try
      {
        _  = command.GetOptionalField('U', "X100");
        Assert.Fail("Must be unreachable");
      }
      catch (ArgumentException) { }

      try
      {
        _  = command.GetOptionalField('U', "U100  AA");
        Assert.Fail("Must be unreachable");
      }
      catch (ArgumentException) { }
    }

    [Test]
    [TestCase("g1 x100 y45")]
    [TestCase("G1 Y45")]
    public void OptionalFields02(string commandText)
    {
      var command = GCodeCommandSpan.TryParse(
        commandText,
        new GCodeParsingSettings(integerArgumentScale: 3, caseNormalization: GCodeCaseNormalization.ToUppercase));

      Assert.AreEqual(-1, command.GetIntFieldOrDefault('x', -1));
      Assert.AreEqual(100, command.GetIntFieldOrDefault('X', 100));
      Assert.AreEqual(100000, command.GetScaledIntFieldOrDefault('X', 100000));
      Assert.AreEqual(45, command.GetIntFieldOrDefault('Y', 1000));
      Assert.AreEqual(45000, command.GetScaledIntFieldOrDefault('Y', 45000));
    }
  }
}