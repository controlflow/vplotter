using System.IO;

namespace VPlotter.Console
{
  public static class Program
  {
    private static void Main()
    {
      using var stream = new FileStream(
        path: "sample.gcode", FileMode.Open, FileAccess.Read, FileShare.Read);

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