using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace VSimulator
{
  public partial class MainWindow
  {
    private VPlotterModel myModel;

    public MainWindow()
    {
      InitializeComponent();
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
      myModel = new VPlotterModel(Host.RenderSize);
      myModel.GondolaPositionChanged += delegate
      {
        Ropes.Data = new PathGeometry {
          Figures = {
            new PathFigure {
              StartPoint = new Point(x: 0, y: 0),
              IsFilled = false,
              IsClosed = false,
              Segments = {
                new LineSegment(myModel.GondolaPosition, isStroked: true),
                new LineSegment(myModel.RightHookLocation, isStroked: true)
              }
            }
          }
        };
      };
    }

    private void LeftStepperMinus(object sender, EventArgs e)
    {
      myModel.DoStep(Stepper.Left, Direction.Backward);
    }

    private void LeftStepperPlus(object sender, EventArgs e)
    {
      myModel.DoStep(Stepper.Left, Direction.Forward);
    }

    private void RightStepperMinus(object sender, EventArgs e)
    {
      myModel.DoStep(Stepper.Right, Direction.Backward);
    }

    private void RightStepperPlus(object sender, EventArgs e)
    {
      myModel.DoStep(Stepper.Right, Direction.Forward);
    }
  }
}