using CVARC.V2;
using HoMM;
using HoMM.Robot;
using HoMM.Robot.HexagonalMovement;

namespace Homm.Client
{
    public class CommandGenerator
    {
        public static HommCommand GetMoveCommand(Direction direction)
        {
            return new HommCommand()
            {
                Movement = new HexMovement(direction)
            };
        }
    }
}