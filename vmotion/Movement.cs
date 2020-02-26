using System;

namespace VMotion
{
  /// <summary>
  /// Represents single stepper movement, possibly accelerated
  /// </summary>
  public readonly struct Movement
  {
    public readonly int StepsCount;
    public readonly int StartDelay;
    public readonly int EndDelay;

    public bool IsAccelerated => Acceleration != 0;

    public int Acceleration => (EndDelay - StartDelay) / StepsCount;

    public int TotalDelay
    {
      get
      {
        /*
         *  ----
         * |    |----
         * |____|____|
         */

        return 1;
      }
    }

    public Movement(int stepsCount, int delay)
    {
      if (stepsCount < 0) throw new ArgumentOutOfRangeException(nameof(stepsCount));
      if (delay <= 0) throw new ArgumentOutOfRangeException(nameof(delay));

      StepsCount = stepsCount;
      StartDelay = delay;
      EndDelay = delay;
    }

    public Movement(int stepsCount, int startDelay, int endDelay)
    {
      if (stepsCount < 0) throw new ArgumentOutOfRangeException(nameof(stepsCount));
      if (startDelay <= 0) throw new ArgumentOutOfRangeException(nameof(startDelay));
      if (endDelay <= 0) throw new ArgumentOutOfRangeException(nameof(endDelay));

      StepsCount = stepsCount;
      StartDelay = startDelay;
      EndDelay = endDelay;
    }
  }
}