using System;
using System.Collections.Generic;

namespace NET.Processor.API.Models.DTO
{
    class RelationsGraphDTO
    {
        public Tuple<Int32, string> Nodes { get; set; }
        public Tuple<Int32, Int32> Edges { get; set; }
        public RelationsGraphDTO Type { get; set; }
    }


    public struct NodeData
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public struct Node
    {
        public NodeData data { get; set; }
    }

    public struct EdgeData
    {
        public string source { get; set; }
        public string target { get; set; }
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
