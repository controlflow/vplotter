using System;
using NUnit.Framework;
using VPlotter.GCode;

namespace VMotion.Tests.GCode
{
  [TestFixture]
  public class GCodeCommandParsingTest
  {
    [Test]
    public void Empty()
    {
      var command = GCodeCommand.TryParse("".AsSpan(), new GCodeParsingSettings());

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
      var command = GCodeCommand.TryParse("G0 X0 Y1".AsSpan(), new GCodeParsingSettings());

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
  }
}