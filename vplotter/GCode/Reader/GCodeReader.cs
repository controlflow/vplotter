using System;
using System.IO;
using System.Xml;

namespace VPlotter.GCode.Reader
{
  // https://duet3d.dozuki.com/Wiki/Gcode
  // https://github.com/synthetos/g2/wiki/GCode-Parsing
  // http://linuxcnc.org/docs/html/gcode/overview.html#_g_code_overview

// todo: ISpanFormattable

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

  public class GCodeReader
  {
    private readonly LineByLineStreamReader myReader;

    void M()
    {
      //var reader = XmlReader.Create(null);



      //myReader.ReadNextLine()
    }

    public readonly struct GCodesEnumerable
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
}