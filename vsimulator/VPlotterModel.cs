using System;
using System.Windows;
using static System.Math;

namespace VSimulator
{
  public sealed class VPlotterModel
  {
    public Size DrawingArea { get; }

    public Point GondolaPosition { get; private set; }
    public event EventHandler GondolaPositionChanged;

    public double StepLength { get; set; } = 10;

    public Point LeftHookLocation => new Point(x: 0, y: 0);
    public Point RightHookLocation => new Point(x: DrawingArea.Width, y: 0);

    public VPlotterModel(Size area)
    {
      if (area.Height <= 0) throw new ArgumentException(nameof(area));
      if (area.Width <= 0) throw new ArgumentException(nameof(area));

      DrawingArea = area;
      GondolaPosition = new Point(x: area.Width / 2, y: area.Height / 2);
    }

    private static double Square(double value) => value * value;

    public double LeftHypo
    {
      get => Sqrt(Square(GondolaPosition.X) + Square(GondolaPosition.Y));
      private set => UpdateGondolaPosition(value, RightHypo);
    }

    public double RightHypo
    {
      get => Sqrt(Square(DrawingArea.Width - GondolaPosition.X) + Square(GondolaPosition.Y));
      private set => UpdateGondolaPosition(LeftHypo, value);
    }

    private void UpdateGondolaPosition(double leftHypo, double rightHypo)
    {
      if (leftHypo <= 0) throw new ArgumentOutOfRangeException(nameof(leftHypo));
      if (rightHypo <= 0) throw new ArgumentOutOfRangeException(nameof(rightHypo));

      var widthSquared = Square(DrawingArea.Width);
      var leftHypoSquared = Square(leftHypo);
      var rightHypoSquared = Square(rightHypo);

      var ySquared = 0.5 * (leftHypoSquared + rightHypoSquared - 0.5 * (widthSquared + Square(leftHypoSquared - rightHypoSquared) / widthSquared));
      var xSquared = leftHypoSquared - ySquared;
      var newPosition = new Point(x: Sqrt(xSquared), y: Sqrt(ySquared));

      GondolaPosition = newPosition;
      GondolaPositionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void DoStep(Stepper stepper, Direction direction)
    {
      var delta = direction == Direction.Forward ? +StepLength : -StepLength;

      switch (stepper)
      {
        case Stepper.Left: LeftHypo += delta; break;
        case Stepper.Right: RightHypo += delta; break;
        default: throw new ArgumentOutOfRangeException(nameof(stepper));
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