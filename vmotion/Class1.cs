namespace VMotion
{
  public interface IStepperOutput
  {
    void Write(bool state);
    void Delay(int interval);
  }

  public class MovementExecutor
  {
    private readonly IStepperOutput myOutput;

    public MovementExecutor(IStepperOutput output)
    {
      myOutput = output;
    }

    public void Run(Movement movement)
    {
      var stepsLeft = movement.StepsCount;
      var delay = movement.StartDelay;
      var acceleration = movement.Acceleration;

      while (stepsLeft > 0)
      {
        myOutput.Write(state: true);
        myOutput.Delay(delay / 2);
        myOutput.Write(state: false);
        myOutput.Delay(delay / 2);

        stepsLeft--;
        delay += acceleration;
      }

      //movement.Delay;
    }
  }


  /*
   *  motor1   motor2
   *  |          |    ____
   *  |         /     ____
   *   \       |
   *    \      |      ____
   *     |     |      ____
   *     |      \
   *     |       \    ____
   *     |        |   ____
   *    /        /    ____
   *   /         \    ____
   *  |           |   ____
   *  |          /
   *  |         /     ____
   *   \       |      ____
   */
}