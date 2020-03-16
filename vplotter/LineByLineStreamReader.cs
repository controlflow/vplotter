using System;
using System.IO;
using System.Text;

namespace VPlotter
{
  public sealed class LineByLineStreamReader : IDisposable
  {
    private readonly TextReader myTextReader;
    private Memory<char> myLineBuffer; // can be expanded
    private int myBufferLineStartOffset, myBufferUsedLength;
    private bool mySkipNextLineFeed;

    public LineByLineStreamReader(TextReader textReader)
    {
      myTextReader = textReader;
      myLineBuffer = new Memory<char>(array: new char[1000]);
    }

    public LineByLineStreamReader(Stream source, Encoding encoding)
    {
      myTextReader = new StreamReader(source, encoding);
      myLineBuffer = new Memory<char>(array: new char[1000]);
    }

    public bool ReadNextLine(out ReadOnlySpan<char> line)
    {
      if (myBufferLineStartOffset > myBufferUsedLength)
      {
        line = ReadOnlySpan<char>.Empty;
        return false;
      }

      for (var buffer = myLineBuffer.Span;;)
      {
        if (myBufferLineStartOffset < myBufferUsedLength)
        {
          var bufferTail = buffer[myBufferLineStartOffset..myBufferUsedLength];

          if (ScanToNextLineBreak(bufferTail, out line))
            return true; // line break found in buffer

          // here 'line' contains the last line from buffer, not terminated with line break
          line.CopyTo(buffer); // move it to the beginning of the buffer
          myBufferLineStartOffset = 0;
          myBufferUsedLength = line.Length;

          // read more data from TextReader
        }

        // buffer possibly contains the last line, not terminated with line break + garbage
        // check if buffer is big enough to fit more data from TextReader
        var bufferFree = buffer.Slice(start: myBufferUsedLength);
        if (bufferFree.Length == 0)
        {
          var newLineBuffer = new Memory<char>(new char[buffer.Length * 2]);
          myLineBuffer.CopyTo(newLineBuffer);
          myLineBuffer = newLineBuffer;

          buffer = newLineBuffer.Span;
          bufferFree = buffer.Slice(start: myBufferUsedLength);
        }

        var charsRead = myTextReader.Read(bufferFree);
        if (charsRead > 0) // new characters available, rescan for line breaks
        {
          myBufferUsedLength += charsRead;
          continue;
        }

        // no new characters, flush the last line terminated by EOF
        line = buffer[myBufferLineStartOffset..myBufferUsedLength];
        myBufferLineStartOffset = myBufferUsedLength + 1;
        return true;
      }
    }

    private bool ScanToNextLineBreak(ReadOnlySpan<char> tail, out ReadOnlySpan<char> line)
    {
      var start = 0;

      if (mySkipNextLineFeed && tail.Length > 0 && tail[0] == '\n')
      {
        start = +1;
      }

      for (var index = start; index < tail.Length; index++)
      {
        var ch = tail[index];
        if (ch == '\r' || ch == '\n')
        {
          line = tail[start..index];
          myBufferLineStartOffset += index + 1;
          mySkipNextLineFeed = ch == '\r'; // DOS \r\n
          return true;
        }
      }

      line = tail[start..tail.Length];
      return false;
    }

    public void Dispose()
    {
      myTextReader.Dispose();
    }
  }

  /*
   * GCode command
   *   Can be comment (skip those lines?)
   *   Contains fields
   *     Field has name
   *     Integral/fractional numbers or strings
   *
   *   Can have line number
   *   Can have checksum
   *
   */
}