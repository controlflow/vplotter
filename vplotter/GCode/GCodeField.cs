using JetBrains.Annotations;

namespace VPlotter.GCode
{
  [PublicAPI]
  public struct GCodeField
  {
    private byte myWord;
    private long myPayload;
    private object myBox;

    [ValueRange('a', 'z')]
    [ValueRange('A', 'Z')]
    [ValueRange('*')]
    public char Word => (char) myWord;
  }
}