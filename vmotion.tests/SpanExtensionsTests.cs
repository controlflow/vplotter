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
      "aaa".AsSpan().SkipWhitespace(ref index1);
      Assert.AreEqual(index1, 0);

      var index2 = 1;
      "b \t  aaa".AsSpan().SkipWhitespace(ref index2);
      Assert.AreEqual(index2, 5);

      var index3 = 0;
      "".AsSpan().SkipWhitespace(ref index3);
      Assert.AreEqual(index3, 0);
    }

    [Test]
    public void TryScanUnsignedInteger()
    {
      AssertScan(text: "X", startIndex: 0, length: 0, value: -1);
      AssertScan(text: "0", startIndex: 0, length: 1, value: 0);
      AssertScan(text: "1bb", startIndex: 0, length: 1, value: 1);
      AssertScan(text: "9d", startIndex: 0, length: 1, value: 9);
      AssertScan(text: "42", startIndex: 0, length: 2, value: 42);
      AssertScan(text: "a000000000000000001bb", startIndex: 1, length: 18, value: 1);
      AssertScan(text: "X1234", startIndex: 1, length: 4, value: 1234);
      AssertScan(text: "X12345678", startIndex: 1, length: 8, value: 12345678);
      AssertScan(text: "X-2147483647", startIndex: 2, length: 10, value: 2147483647); // max value
      AssertScan(text: "X-2147483648", startIndex: 2, length: 0, value: -1); // overflow
      AssertScan(text: "Y100000000000", startIndex: 1, length: 0, value: -1); // overflow

      static void AssertScan(string text, int startIndex, int length, int value)
      {
        var index = startIndex;
        var actualValue = text.AsSpan().TryScanUnsignedInt32(ref index);
        Assert.AreEqual(length, index - startIndex);
        Assert.AreEqual(value, actualValue);
      }
    }

    [Test]
    public void TryScanUnsignedDecimalFraction()
    {
      AssertScan(text: "X", startIndex: 0, length: 0, value: -1);
      AssertScan(text: "0", startIndex: 0, length: 1, value: 1);
      AssertScan(text: "1", startIndex: 0, length: 1, value: 11);
      AssertScan(text: "2", startIndex: 0, length: 1, value: 12);
      AssertScan(text: "9", startIndex: 0, length: 1, value: 19);
      AssertScan(text: "09", startIndex: 0, length: 2, value: 109);
      AssertScan(text: "90b", startIndex: 0, length: 2, value: 19);
      AssertScan(text: "b90000", startIndex: 1, length: 5, value: 19);
      AssertScan(text: "90000000000000000000", startIndex: 0, length: 20, value: 19);
      AssertScan(text: "1000000000000000000000000000000000000000", startIndex: 0, length: 40, value: 11);
      AssertScan(text: "0000000000000000000000000000000000000000", startIndex: 0, length: 40, value: 1);
      AssertScan(text: "999999999", startIndex: 0, length: 9, value: 1999999999);
      AssertScan(text: "1000000000", startIndex: 0, length: 10, value: 11);
      AssertScan(text: "0000000001", startIndex: 0, length: 0, value: -1);
      AssertScan(text: "1000000001", startIndex: 0, length: 0, value: -1);

      static void AssertScan(string text, int startIndex, int length, int value)
      {
        var index = startIndex;
        var actualValue = text.AsSpan().TryScanDecimalFractionUnsignedInt32(ref index);
        Assert.AreEqual(length, index - startIndex);
        Assert.AreEqual(value, actualValue);
      }
    }
  }
}