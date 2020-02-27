using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VSimulator
{
  public class PlotterDrawingHost : FrameworkElement
  {
    private readonly DrawingVisual myDrawingVisual;
    private RenderTargetBitmap myRenderTargetBitmap;

    public PlotterDrawingHost()
    {
      myDrawingVisual = new DrawingVisual();
      
      Loaded += Load;
    }

    void Load(object o, EventArgs e)
    {
      var renderSize = RenderSize;

      var source = PresentationSource.FromVisual(this);
      if (source == null) throw new InvalidOperationException();

      var xScale = source.CompositionTarget.TransformToDevice.M11;
      var yScale = source.CompositionTarget.TransformToDevice.M22;

      myRenderTargetBitmap = new RenderTargetBitmap(
        pixelWidth: (int) (renderSize.Width * xScale),
        pixelHeight: (int) (renderSize.Height * yScale),
        dpiX: 96.0 * xScale,
        dpiY: 96.0 * yScale,
        pixelFormat: PixelFormats.Pbgra32);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
      if (myRenderTargetBitmap == null) return;

      drawingContext.DrawImage(
        myRenderTargetBitmap, new Rect(0, 0, myRenderTargetBitmap.Width, myRenderTargetBitmap.Height));
      //base.OnRender(drawingContext);
    }

    public void DrawLine(Point from, Point to)
    {
      using var drawingContext = myDrawingVisual.RenderOpen();
      drawingContext.DrawLine(new Pen(Brushes.Brown, thickness: 1), from, to);
      drawingContext.Close();

      myRenderTargetBitmap.Render(myDrawingVisual);

      InvalidateVisual();
    }

    protected override int VisualChildrenCount => 0;
    //protected override Visual GetVisualChild(int index) => myChildren[index];
  }
}