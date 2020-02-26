using System;
using System.Windows;

namespace VSimulator
{
  public sealed class VPlotterModel
  {
    public Size PlottingArea { get; }

    public Point GondolaPosition { get; }
    public event EventHandler PositionChanged;

    public VPlotterModel(Size area)
    {
      if (area.Height <= 0) throw new ArgumentException(nameof(area));
      if (area.Width <= 0) throw new ArgumentException(nameof(area));

      PlottingArea = area;
      GondolaPosition = new Point(x: area.Width / 2, y: area.Height / 2);
    }

    public void DoStep(Stepper stepper, Direction direction)
    {
      switch (stepper)
      {
        case Stepper.Left:

          break;

        case Stepper.Right:
          break;

        default:
          throw new ArgumentOutOfRangeException(nameof(stepper), stepper, null);
      }
    }
  }

  public enum Stepper
  {
    Left,
    Right
  }

  public enum Direction
  {
    Forward,
    Backward
  }
}