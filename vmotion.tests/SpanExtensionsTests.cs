using System;
using NUnit.Framework;
using VPlotter;

namespace VMotion.Tests
{
  [TestFixture]
  public class SpanExtensionsTests
  {
    [Test]
    public void SkipWhitespace()
    {
      var index1 = 0;
      var skipped1 = "aaa".AsSpan().SkipWhitespace(ref index1);
      Assert.AreEqual(0, index1);
      Assert.AreEqual(0, skipped1);

      var index2 = 1;
      var skipped2 = "b \t  aaa".AsSpan().SkipWhitespace(ref index2);
      Assert.AreEqual(5, index2);
      Assert.AreEqual(4, skipped2);

      var index3 = 0;
      var skipped3 = "".AsSpan().SkipWhitespace(ref index3);
      Assert.AreEqual(0, index3);
      Assert.AreEqual(0, skipped3);
    }

    [Test]
    public void TryScanUnsignedInteger()
    {
      AssertScan(text: "X", length: 0, value: -1);
      AssertScan(text: "0", length: 1, value: 0);
      AssertScan(text: "1bb", length: 1, value: 1);
      AssertScan(text: "9d", length: 1, value: 9);
      AssertScan(text: "42", length: 2, value: 42);
      AssertScan(text: "a000000000000000001bb", length: 18, value: 1, startIndex: 1);
      AssertScan(text: "X1234", length: 4, value: 1234, startIndex: 1);
      AssertScan(text: "X12345678", length: 8, value: 12345678, startIndex: 1);
      AssertScan(text: "X-2147483647", length: 10, value: 2147483647, startIndex: 2); // max value
      AssertScan(text: "X-2147483648", length: 0, value: -1, startIndex: 2); // overflow
      AssertScan(text: "Y100000000000", length: 0, value: -1, startIndex: 1); // overflow

      static void AssertScan(string text, int length, int value, int startIndex = 0)
      {
        var index = startIndex;
        var actualValue = text.AsSpan().TryScanGCodeUnsignedInt32(ref index);
        Assert.AreEqual(length, index - startIndex);
        Assert.AreEqual(value, actualValue);
      }
    }

    [Test]
    public void TryScanUnsignedDecimalFraction()
    {
      AssertScan(text: "X", length: 0, value: -1);
      AssertScan(text: "0", length: 1, value: 1);
      AssertScan(text: "1", length: 1, value: 11);
      AssertScan(text: "2", length: 1, value: 12);
      AssertScan(text: "9", length: 1, value: 19);
      AssertScan(text: "09", length: 2, value: 109);
      AssertScan(text: "90b", length: 2, value: 19);
      AssertScan(text: "b90000", startIndex: 1, length: 5, value: 19);
      AssertScan(text: "90000000000000000000", length: 20, value: 19);
      AssertScan(text: "1000000000000000000000000000000000000000b", length: 40, value: 11);
      AssertScan(text: "0000000000000000000000000000000000000000", length: 40, value: 1);
      AssertScan(text: "999999999", length: 9, value: 1999999999);
      AssertScan(text: "1000000000", length: 10, value: 11);
      AssertScan(text: "0000000001", length: 0, value: -1);
      AssertScan(text: "0000000000000000001", length: 0, value: -1);
      AssertScan(text: "1000000001", length: 0, value: -1);

      // todo: default value inspection not working

      static void AssertScan(string text, int length, int value, int startIndex = 0)
      {
        var index = startIndex;
        var actualValue = text.AsSpan().TryScanGCodeDecimalFractionUnsignedInt32(ref index);
        Assert.AreEqual(length, index - startIndex);
        Assert.AreEqual(value, actualValue);
        // todo: return and assert scale?
      }
    }

    [Test]
    public void TryScanDoubleQuotedStringLiteral()
    {
      AssertScan(text: "", length: 0, contentLength: -1);
      AssertScan(text: "\"", length: 1, contentLength: 0);
      AssertScan(text: "\"abc", length: 4, contentLength: 3);
      AssertScan(text: "a\"abc\"", length: 5, contentLength: 3, startIndex: 1);
      AssertScan(text: "a\"abc\"", length: 0, contentLength: -1, startIndex: 0);
      AssertScan(text: "a\"abc", length: 0, contentLength: -1, startIndex: 0);
      AssertScan(text: "\"abc def\"bbb", length: 9, contentLength: 7);
      AssertScan(text: "\"\"", length: 2, contentLength: 0);
      AssertScan(text: "\"aa\"\"\"\"bb\"", length: 10, contentLength: 6);
      AssertScan(text: "\"aa''bb\"", length: 8, contentLength: 6);
      AssertScan(text: "\"aa''bb\"", length: 8, contentLength: 5, singleQuoteEscape: true);

      static void AssertScan(string text, int length, int contentLength, int startIndex = 0, bool singleQuoteEscape = false)
      {
        var index = startIndex;
        var content = text.AsSpan().TryScanGCodeDoubleQuotedStringLiteral(ref index, singleQuoteEscape);
        Assert.AreEqual(length, index - startIndex);
        Assert.AreEqual(contentLength, content);
      }
    }
  }
}