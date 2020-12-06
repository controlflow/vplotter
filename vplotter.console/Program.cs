using System;
using System.IO;
using VPlotter.GCode;

namespace VPlotter.Console
{


  public static class Program
  {
    private static void Main()
    {
      //var layout = ObjectLayoutInspector.TypeLayout.GetLayout(typeof(GCodeField));

      //System.Console.WriteLine(layout);

      return;

      //var log10 = (int) Math.Log10(9234);



      var aaaaaa = new decimal(lo: 10456, mid: 0, hi: 0, isNegative: false, scale: 4) - 1;

      System.Console.WriteLine(aaaaaa);

      var aaaa = decimal.Add(1230M, aaaaaa);


      System.Console.WriteLine(aaaa);


      //var field = GCodeField.TryParse("X2147483647.2147483647".AsSpan(), out _, new GCodeParsingSettings());







      //GCodeField.TryParse("X-10.20")

      return;

      using var stream = new FileStream(
        path: "sample.gcode", FileMode.Open, FileAccess.Read, FileShare.Read);

      var charArray = "aaaaaasdsdsd".ToCharArray();



      using var stringReader = new StringReader("aaaaaaaaaa\r");

      using var lineByLine = new LineByLineStreamReader(stringReader);  //stream, Encoding.ASCII);

      while (lineByLine.ReadNextLine(out var line))
      {
        System.Console.WriteLine(line.ToString());
        System.Console.WriteLine("=========");
      }
    }
  }
}