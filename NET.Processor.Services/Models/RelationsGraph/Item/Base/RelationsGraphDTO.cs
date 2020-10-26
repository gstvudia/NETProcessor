using System;
using System.Collections.Generic;

namespace NET.Processor.Core.Models.RelationsGraph.Item.Base
{
    public class RelationsGraphDTO
    {
        public Tuple<Int32, string> Nodes { get; set; }
        public Tuple<Int32, Int32> Edges { get; set; }
        public RelationsGraphDTO Type { get; set; }
    }


    public struct NodeData
    {
        public string id { get; set; }
        public string name { get; set; }
        public string nodeType { get; set; }
        public int weight { get; set; }
        public string colorCode { get; set; }
        public string shapeType { get; set; }
    }

    public struct Node
    {
        public NodeData data { get; set; }
    }

    public struct EdgeData
    {
        public string source { get; set; }
        public string target { get; set; }
        public string colorCode { get; set; }
        public int strength { get; set; }
    }

    public struct Edge
    {
        public EdgeData data { get; set; }
    }

    public struct Root
    {
        public List<Node> nodes { get; set; }
        public List<Edge> edges { get; set; }
    }

}
