using System;
using NUnit.Framework;
using VPlotter.GCode;

namespace VMotion.Tests.GCode
{
  [TestFixture]
  public class GCodeCommentParsingTest
  {
    [Test]
    public void NotComment()
    {
      var comment = GCodeComment.TryParse("hello".AsSpan(), out var tail);

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
      var comment = GCodeComment.TryParse("; comment".AsSpan(), out var tail);

      Assert.That(comment.IsValid, Is.True);
      Assert.That(comment.Kind, Is.EqualTo(GCodeCommentKind.EndOfLine));

      Assert.That(comment.Content.ToString(), Is.EqualTo(" comment"));
      Assert.That(tail.ToString(), Is.EqualTo(""));
    }

    [Test]
    public void InlineComment()
    {
      var comment = GCodeComment.TryParse("( comment ) bb".AsSpan(), out var tail);

      Assert.That(comment.IsValid, Is.True);
      Assert.That(comment.Kind, Is.EqualTo(GCodeCommentKind.Inline));

      Assert.That(comment.Content.ToString(), Is.EqualTo(" comment "));
      Assert.That(tail.ToString(), Is.EqualTo(" bb"));
    }

    [Test]
    public void UnfinishedInlineComment()
    {
      var comment = GCodeComment.TryParse("( comment  ".AsSpan(), out var tail);

      Assert.That(comment.IsValid, Is.True);
      Assert.That(comment.Kind, Is.EqualTo(GCodeCommentKind.InlineUnfinished));

      Assert.That(comment.Content.ToString(), Is.EqualTo(" comment  "));
      Assert.That(tail.ToString(), Is.EqualTo(""));
    }
  }
}