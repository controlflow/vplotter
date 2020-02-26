using NUnit.Framework;

namespace VMotion.Tests
{
  [TestFixture]
  public class MovementExecutorTests
  {
    private TestStepperOutput myTestOutput;
    private MovementExecutor myMovementExecutor;

    [SetUp]
    public void SetUp()
    {
      myTestOutput = new TestStepperOutput();
      myMovementExecutor = new MovementExecutor(myTestOutput);
    }

    [Test]
    public void SimpleStep()
    {
      myMovementExecutor.Run(new Movement(stepsCount: 1, delay: 2));

      Assert.AreEqual(
        "<step><delay 1></step><delay 1>",
        myTestOutput.ToDebugString());
    }

    [Test]
    public void MultipleSteps()
    {
      myMovementExecutor.Run(new Movement(stepsCount: 3, delay: 2));

      Assert.AreEqual(
        "<step><delay 1></step><delay 1>" +
        "<step><delay 1></step><delay 1>" +
        "<step><delay 1></step><delay 1>",
        myTestOutput.ToDebugString());
    }

    [Test]
    public void WithAcceleration()
    {
      var movement = new Movement(stepsCount: 10, startDelay: 10, endDelay: 110);
      Assert.AreEqual(10, movement.Acceleration);

      myMovementExecutor.Run(movement);

      Assert.AreEqual(
        "<step><delay 5></step><delay 5>" +
        "<step><delay 10></step><delay 10>" +
        "<step><delay 15></step><delay 15>" +
        "<step><delay 20></step><delay 20>" +
        "<step><delay 25></step><delay 25>" +
        "<step><delay 30></step><delay 30>" +
        "<step><delay 35></step><delay 35>" +
        "<step><delay 40></step><delay 40>" +
        "<step><delay 45></step><delay 45>" +
        "<step><delay 50></step><delay 50>",
        myTestOutput.ToDebugString());
    }

    [Test]
    public void WithNegativeAcceleration()
    {
      var movement = new Movement(stepsCount: 20, startDelay: 110, endDelay: 10);
      Assert.AreEqual(-5, movement.Acceleration);

      myMovementExecutor.Run(movement);

      Assert.AreEqual(
        "<step><delay 55></step><delay 55>" +
        "<step><delay 52></step><delay 52>" +
        "<step><delay 50></step><delay 50>" +
        "<step><delay 47></step><delay 47>" +
        "<step><delay 45></step><delay 45>" +
        "<step><delay 40></step><delay 40>" +
        "<step><delay 35></step><delay 35>" +
        "<step><delay 30></step><delay 30>" +
        "<step><delay 25></step><delay 25>" +
        "<step><delay 20></step><delay 20>" +
        "<step><delay 15></step><delay 15>" +
        "<step><delay 10></step><delay 10>" +
        "<step><delay 5></step><delay 5>",
        myTestOutput.ToDebugString());
    }
  }
}