using NUnit.Framework;

namespace Homm.Client.Tests
{
    [TestFixture]
    internal class CommandGeneratorTests
    {
        [Test]
        public void TestCommandGenerator()
        {
            foreach (var direction in Constants.Directions)
                Assert.AreEqual(CommandGenerator.GetMoveCommand(direction).Movement.MovementDirection, direction);
        }
    }
}