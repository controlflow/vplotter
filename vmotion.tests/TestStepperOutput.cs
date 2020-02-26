using System.Text;
using NUnit.Framework;

namespace VMotion.Tests
{
  public class TestStepperOutput : IStepperOutput
  {
    private readonly StringBuilder myLog = new StringBuilder();

    private bool myLastState;

    public void Write(bool state)
    {
      if (myLastState == state)
        throw new AssertionException("State is the same as the last state");

      myLastState = state;
      myLog.Append(state ? "<step>" : "</step>");
    }

    public void Delay(int interval)
    {
      myLog.Append($"<delay {interval}>");
    }

    public string ToDebugString()
    {
      if (myLastState)
        throw new AssertionException("Pulse in progress");

      return myLog.ToString();
    }
  }
}