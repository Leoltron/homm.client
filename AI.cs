using System;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public class AI
    {
        private HommSensorData Data;

        public AI(HommSensorData data)
        {
            Data = data;
        }

        private double GetPileValue(ResourcePile pile)
        {
            throw new NotImplementedException();
        }
    }
}