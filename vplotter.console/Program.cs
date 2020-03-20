using System;
using System.IO;

namespace VPlotter.Console
{
  public static class Program
  {
    private static void Main()
    {
      var field = GCodeField.TryParse("X2147483647.2147483647".AsSpan(), out _, new GCodeParsingSettings());




      //System.Console.WriteLine("2147483647.2147483647");

      //float d = (float) 2147483647.2147483647;
      //System.Console.WriteLine(d.ToString("R"));

      var aa = 21474836470000000000M + 2147483647M;
      System.Console.WriteLine(aa);



      //((double)2147483647_2147483647)

      decimal dd = 2147483647.2147483647M;
      System.Console.WriteLine(dd);



      //var big = 2147483642147483647L;


      //decimal dd2 = new decimal(lo: (int) big, mid: (int) (big >> 32), hi: 0, isNegative: false, scale: 10);
      //decimal dd2 = new decimal(lo: 1, mid: 1, hi: 0, isNegative: false, scale: 0);
      //System.Console.WriteLine(dd2);

      var a = Convert.ToDouble(dd);
      System.Console.WriteLine(a);

      System.Console.WriteLine(Double.Parse("2147483647.2147483647"));

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