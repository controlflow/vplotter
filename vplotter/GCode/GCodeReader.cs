using System;
using System.IO;

namespace VPlotter.GCode
{
  // https://duet3d.dozuki.com/Wiki/Gcode
  // https://github.com/synthetos/g2/wiki/GCode-Parsing
  // http://linuxcnc.org/docs/html/gcode/overview.html#_g_code_overview

  public class GCodeReader
  {
    private readonly StreamReader myReader;

    void M()
    {




    }

  }

  public readonly ref struct GCodeLine
  {
    // can be comment
    // can be command
    // can be blank
    // can be / block
  }

  public readonly ref struct GCodeCommand2
  {
    public readonly ReadOnlySpan<char> RawLine;

    // can start with the line number


    public GCodeCommand2(ReadOnlySpan<char> rawLine)
    {
      RawLine = rawLine;
    }
  }
}