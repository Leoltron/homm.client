using HoMM;
using HoMM.Robot;
using HoMM.Robot.HexagonalMovement;

namespace Homm.Client
{
    public static class CommandGenerator
    {
        public static HommCommand GetMoveCommand(Direction direction)
        {
            return new HommCommand
            {
                Movement = new HexMovement(direction)
            };
        }
    }
}