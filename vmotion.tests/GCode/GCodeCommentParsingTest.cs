using System;
using NUnit.Framework;
using VPlotter.GCode;
using VPlotter.GCode.Reader;

namespace VMotion.Tests.GCode
{
  [TestFixture]
  public class GCodeCommentParsingTest
  {
    private static GCodeCommentSpan Parse(string text, out ReadOnlySpan<char> tail)
      => GCodeCommentSpan.TryParse(text.AsSpan(), out tail);

    [Test]
    public void NotComment()
    {
      var comment = Parse("hello", out var tail);

      Assert.That(comment.IsValid, Is.False);
      Assert.That(tail.ToString(), Is.EqualTo("hello"));

      try
      {
        _ = comment.Kind;
        Assert.Fail("Should be unreachable");
      }
      catch (InvalidOperationException) { }
    }

    [Test]
    public void EndOfLineComment()
    {
      var comment = Parse("; comment", out var tail);

      Assert.That(comment.IsValid, Is.True);
      Assert.That(comment.Kind, Is.EqualTo(GCodeCommentKind.EndOfLine));

      Assert.That(comment.Content.ToString(), Is.EqualTo(" comment"));
      Assert.That(tail.ToString(), Is.EqualTo(""));
    }

    [Test]
    public void InlineComment()
    {
      var comment = Parse("( comment ) bb", out var tail);

      Assert.That(comment.IsValid, Is.True);
      Assert.That(comment.Kind, Is.EqualTo(GCodeCommentKind.Inline));

      Assert.That(comment.Content.ToString(), Is.EqualTo(" comment "));
      Assert.That(tail.ToString(), Is.EqualTo(" bb"));
    }

    [Test]
    public void UnfinishedInlineComment()
    {
      var comment = Parse("( comment  ", out var tail);

      Assert.That(comment.IsValid, Is.True);
      Assert.That(comment.Kind, Is.EqualTo(GCodeCommentKind.InlineUnfinished));

      Assert.That(comment.Content.ToString(), Is.EqualTo(" comment  "));
      Assert.That(tail.ToString(), Is.EqualTo(""));
    }
  }
}