using System;
using System.Device.Gpio;

namespace VPlotterCore
{
  public static class Program
  {
    public static void Main()
    {
      Console.WriteLine("Hello, .NET on PI!");

      return;

      using var controller = new GpioController(PinNumberingScheme.Board);


      const int kotiPin = 15;

      using var pwmChannel = System.Device.Pwm.PwmChannel.Create(chip: 0, channel:0, frequency: 50);
      pwmChannel.Start();

      while (true)
      {
        Console.Write("value=");
        var line = Console.ReadLine();
        if (!int.TryParse(line, out var value)) continue;

        if (value < 0 || value > 100) continue;

        pwmChannel.DutyCycle = (double) value / 100;
      }

      //controller.OpenPin(kotiPin, PinMode.Output);

      // for (var index = 0;; index++)
      // {
      //   var pinValue = index % 2 == 0 ? PinValue.High : PinValue.Low;
      //   Console.WriteLine(pinValue);
      //   controller.Write(kotiPin, pinValue);
      //   Thread.Sleep(TimeSpan.FromSeconds(1));
      // }
    }
  }
}