using System.Windows;
using System.Windows.Media;

namespace VSimulator
{
  public class PlotterDrawingHost : FrameworkElement
  {
    private readonly VisualCollection myChildren;

    public PlotterDrawingHost()
    {
      myChildren = new VisualCollection(this);

      myChildren.Add(CreateDrawingVisualRectangle());
    }

    private DrawingVisual CreateDrawingVisualRectangle()
    {
      var drawingVisual = new DrawingVisual();
      var drawingContext = drawingVisual.RenderOpen();

      var rect = new Rect(new Point(160, 100), new Size(320, 80));
      drawingContext.DrawRectangle(Brushes.LightBlue, (Pen)null, rect);

      drawingContext.Close();

      return drawingVisual;
    }

    protected override int VisualChildrenCount => myChildren.Count;
    protected override Visual GetVisualChild(int index) => myChildren[index];
  }
}