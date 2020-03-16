using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VPlotter;

namespace VMotion.Tests
{
  [TestFixture]
  public sealed class LineByLineStreamReaderTests
  {
    private static readonly string[] PossibleLineBreaks = {"\r", "\n", "\r\n"};
    private static readonly string[] PossibleLines =
      Enumerable.Range(0, 26).Select(c => new string((char) ('a' + c), count: c)).ToArray();

    [Test, Repeat(1000)]
    public void ReadLines()
    {
      var random = new Random();
      var builder = new StringBuilder();
      var addedLines = new List<string>();

      for (var line = random.Next(0, 100); line >= 0; line--)
      {
        var newLine = PossibleLines[random.Next(0, PossibleLines.Length)];
        var newBreak = PossibleLineBreaks[random.Next(0, PossibleLineBreaks.Length)];
        if (newLine == "" && newBreak == "\n") continue; // to avoid confusion with \r\n

        builder.Append(newLine).Append(newBreak);
        addedLines.Add(newLine);
      }

      if (random.Next(0, 100) % 2 == 0)
      {
        var lastLine = PossibleLines[random.Next(0, PossibleLines.Length)];
        builder.Append(lastLine);
        addedLines.Add(lastLine);
      }
      else
      {
        addedLines.Add(string.Empty);
      }

      using var stringReader = new StringReader(builder.ToString());
      using var reader = new LineByLineStreamReader(stringReader);

      var index = 0;
      while (reader.ReadNextLine(out var span))
      {
        var contents = span.ToString();
        Assert.That(contents, Is.EqualTo(addedLines[index]));
        index++;

        Console.WriteLine(contents);
      }

      Assert.AreEqual(addedLines.Count, index);
    }
  }
}