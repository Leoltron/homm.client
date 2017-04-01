using System;
using System.Collections.Generic;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    class BackBrain
    {
        public static Dictionary<Tuple<int, int>, Direction> Compas = new Dictionary<Tuple<int, int>, Direction>
        {
            {Tuple.Create(-1, -1), Direction.LeftDown },
            {Tuple.Create(-1, 1), Direction.LeftUp },
            {Tuple.Create(0, -1), Direction.Down },
            {Tuple.Create(0, 1), Direction.Up },
            {Tuple.Create(1, -1), Direction.RightDown },
            {Tuple.Create(1, 1), Direction.RightUp }
        };

        public static Dictionary<Tuple<int, int>, MapObjectData>[] DivideByFar(int radius, HommSensorData data)
        {
            var levels = new Dictionary<int, List<MapObjectData>>();
            foreach (var mapObject in data.Map.Objects)
            {
                var dx = mapObject.Location.X - data.Location.X;
                var dy = mapObject.Location.Y - data.Location.Y;
                var distance = dx * dx + dy * dy;
                if (!levels.ContainsKey(distance))
                    levels.Add(distance, new List<MapObjectData>());
                levels[distance].Add(mapObject);
            }
            return levels
                    .OrderBy(record => record.Key)
                    .Select(record => record.Value)
                    .Select(collection =>
                            collection.ToDictionary(point => Tuple.Create(point.Location.X, point.Location.Y)))
                    .ToArray();
        }

        public static double GetWeight(MapObjectData mapObject)
        {
            var weight = 0.0;
            weight += AI.GetPileValue(mapObject.ResourcePile);
            weight += AI.GetMineValue(mapObject.Mine);
            weight += AI.GetDwellingValue(mapObject.Dwelling);
            //... и тут видимо для каждого поля нужно так сделать(
            return weight;
        }

        public static double AddNeighboursWeight(Dictionary<Tuple<int, int>, double> previousLevel,
            MapObjectData current)
        {
            var sum = 0.0;
            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                {
                    var x = current.Location.X + i;
                    var y = current.Location.Y + j;
                    var point = Tuple.Create(x, y);
                    if (i != 0 || j != 0 && previousLevel.ContainsKey(point))
                        sum += previousLevel[point];
                }
            return sum;
        }
    }
}
