using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    class Node
    {
        public bool Used { get; set; }
        public int TimeIn { get; set; }
        public int QueryTime { get; set; }

        public Node(bool used, int timeIn, int queryTime)
        {
            Used = used;
            TimeIn = timeIn;
            QueryTime = queryTime;
        }
    }

    public class BridgeLocator
    {
        private readonly HashSet<Location> bridgesNodes = new HashSet<Location>();
        private readonly Dictionary<Location, Node> nodes = new Dictionary<Location, Node>();
        private int timer;
        private readonly LocationHelper locHelper;

        public BridgeLocator(LocationHelper locHelper)
        {
            this.locHelper = locHelper;
        }

        public void RefreshBridges(Dictionary<Location, List<Location>> graph)
        {
            bridgesNodes.Clear();
            nodes.Clear();
            timer = 0;
            foreach (var key in graph.Keys)
                nodes.Add(key, new Node(false, 0, 0));
            foreach (var key in graph.Keys)
                if (!nodes[key].Used)
                    DFS(key, new Location(-1, -1), graph);
            ;
        }

        public bool IsBridge(Location location) => bridgesNodes.Contains(location);

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private void DFS(Location vertex, Location parent, Dictionary<Location, List<Location>> graph)
        {
            nodes[vertex].Used = true;
            nodes[vertex].TimeIn = timer;
            nodes[vertex].QueryTime = timer;
            timer++;
            foreach (var location in graph[vertex])
            {
                if (location == parent) continue;
                if (nodes[location].Used)
                    nodes[vertex].QueryTime = Math.Min(nodes[vertex].QueryTime, nodes[location].TimeIn);
                else
                {
                    DFS(location, vertex, graph);
                    nodes[vertex].QueryTime = Math.Min(nodes[vertex].QueryTime, nodes[location].QueryTime);
                    if (nodes[location].QueryTime <= nodes[vertex].TimeIn) continue;
                    bridgesNodes.Add(vertex);
                    bridgesNodes.Add(location);
                }
            }
        }

        public Dictionary<Location, List<Location>> ToGraph(Dictionary<Location, MapObjectData>[] levels, MapData map)
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
                                    .Where(neighb => vertexes.Contains(neighb) &&
                                                     locHelper.CanStandThere(neighb))
                                    .ToList();
                graph.Add(vertex, neighbs);
            }
            return graph;
        }
    }
}
