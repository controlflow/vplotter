using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;

namespace VPlotterCore
{
  public static class Program
  {
    public static void Main()
    {
      //var gcode = "  G1    X5453  Y3423.423";

      //var code = GCode.FromLine(gcode);




      //Console.WriteLine("Hello, .NET on PI!");

      //return;


      var controller = new GpioController(PinNumberingScheme.Board);
      var stopwatch = Stopwatch.StartNew();

      //Thread.SpinWait();

      var thread = new Thread(() => { });
      thread.Priority = ThreadPriority.Highest;
      thread.Name = "Stepper Worker";


      const int dirPin = 15;
      const int dirPin2 = 18;
      const int stepPin = 13;
      const int stepPin2 = 16;

      controller.OpenPin(dirPin, PinMode.Output);
      controller.OpenPin(dirPin2, PinMode.Output);
      controller.OpenPin(stepPin, PinMode.Output);
      controller.OpenPin(stepPin2, PinMode.Output);

      controller.Write(dirPin, PinValue.High);
      controller.Write(dirPin2, PinValue.High);
      controller.Write(stepPin, PinValue.Low);
      controller.Write(stepPin2, PinValue.Low);

      while (true)
      {

        Console.Write("high=");
        var delay1 = int.Parse(Console.ReadLine()!) * 1000;
        Console.Write("dir?");
        var dir = string.IsNullOrEmpty(Console.ReadLine());

        controller.Write(dirPin, dir);
        controller.Write(dirPin2, dir);

        for (var i = 0; i < 400 * 20; i++)
        {
          controller.Write(stepPin, PinValue.High);
          controller.Write(stepPin2, PinValue.High);
          Sleep(delay1);
          controller.Write(stepPin, PinValue.Low);
          controller.Write(stepPin2, PinValue.Low);
          Sleep(delay1);



          //Console.Write('.');
        }

        Console.WriteLine();
      }


      controller.ClosePin(dirPin);
      controller.ClosePin(stepPin);

      //controller.OpenPin(kotiPin, PinMode.Output);

      // for (var index = 0;; index++)
      // {
      //   var pinValue = index % 2 == 0 ? PinValue.High : PinValue.Low;
      //   Console.WriteLine(pinValue);
      //   controller.Write(kotiPin, pinValue);
      //   Thread.Sleep(TimeSpan.FromSeconds(1));
      // }

      void Sleep(int delayTicks)
      {
        var now = stopwatch.ElapsedTicks;

        while (stopwatch.ElapsedTicks - now < delayTicks)
        {
          GC.KeepAlive(stopwatch);
        }
      }
    }



    public static void HardwarePwmExample()
    {
      using var pwmChannel = System.Device.Pwm.PwmChannel.Create(chip: 0, channel:0, frequency: 50, dutyCyclePercentage: 0.05);
      pwmChannel.Start();

      while (true)
      {
        Console.Write("value=");
        var line = Console.ReadLine();
        if (!int.TryParse(line, out var value)) continue;

        if (value < 0 || value > 100) continue;

        pwmChannel.DutyCycle = (double) value / 100;
      }
    }
  }
}