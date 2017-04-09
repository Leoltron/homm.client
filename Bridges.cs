using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    class Node
    {
        public bool used { get; set; }
        public int timeIN { get; set; }
        public int queryTime { get; set; }

        public Node(bool used, int timeIN, int queryTime)
        {
            this.used = used;
            this.timeIN = timeIN;
            this.queryTime = queryTime;
        }
    }
    public static class Bridges
    {
        private static HashSet<Location> bridgesNodes = new HashSet<Location>();
        private static Dictionary<Location, Node> nodes = new Dictionary<Location, Node>();
        private static int timer;

        public static void RefreshBridges(Dictionary<Location, List<Location>> graph)
        {
            bridgesNodes.Clear();
            nodes.Clear();
            timer = 0;
            foreach (var key in graph.Keys)
                nodes.Add(key, new Node(false, 0, 0));
            foreach (var key in graph.Keys)
                if (!nodes[key].used)
                    DFS(key, new Location(-1, -1), graph);
            ;
        }

        public static bool IsBridge(Location location) => bridgesNodes.Contains(location);

        private static void DFS(Location vertex, Location parent, Dictionary<Location, List<Location>> graph)
        {
            nodes[vertex].used = true;
            nodes[vertex].timeIN = timer;
            nodes[vertex].queryTime = timer;
            timer++;
            foreach (var location in graph[vertex])
            {
                if (location == parent) continue;
                if (nodes[location].used)
                    nodes[vertex].queryTime = Math.Min(nodes[vertex].queryTime, nodes[location].timeIN);
                else
                {
                    DFS(location, vertex, graph);
                    nodes[vertex].queryTime = Math.Min(nodes[vertex].queryTime, nodes[location].queryTime);
                    if (nodes[location].queryTime > nodes[vertex].timeIN)
                    {
                        bridgesNodes.Add(vertex);
                        bridgesNodes.Add(location);
                    }
                }
            }
        }

        public static Dictionary<Location, List<Location>> ToGraph(Dictionary<Location, MapObjectData>[] levels, MapData map)
        {
            var vertexes = new HashSet<Location>();
            levels
                .SelectMany(level => level)
                .Select(pair => vertexes.Add(pair.Key))
                .ToArray();
            var graph = new Dictionary<Location, List<Location>>();
            foreach (var vertex in vertexes)
            {
                var neighbs = vertex.Neighborhood
                                    .Where(neighb => vertexes.Contains(neighb) && LocationHelper.CanStandThere(map, neighb))
                                    .ToList();
                graph.Add(vertex, neighbs);
            }
            return graph;
        }
    }
}
