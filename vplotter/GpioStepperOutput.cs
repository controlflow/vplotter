using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;
using VMotion;

namespace VPlotterCore
{
  public class GpioStepperOutput : IStepperOutput
  {
    private readonly Stopwatch myStopwatch = Stopwatch.StartNew();
    private readonly GpioController myController;
    private readonly int myPin;

    public GpioStepperOutput(GpioController controller, int pin)
    {
      myController = controller;
      myPin = pin;

      controller.OpenPin(pin);
    }

    public void Write(bool state)
    {
      myController.Write(myPin, state);
    }

    public void Delay(int interval)
    {
      var atDelayStart = myStopwatch.ElapsedTicks;

      while (myStopwatch.ElapsedTicks - atDelayStart < interval)
      {
        // todo: tune this value
        Thread.SpinWait(iterations: 100);
      }
    }
  }
}