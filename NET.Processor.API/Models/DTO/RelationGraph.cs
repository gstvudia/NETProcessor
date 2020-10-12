using System;

namespace NET.Processor.API.Models.DTO
{
    class RelationGraph
    {
        public Tuple<Int32, string> Nodes { get; set; }
        public Tuple<Int32, Int32> Edges { get; set; }
        public RelationGraph Type { get; set; }
    }


    public struct Data
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public struct Root
    {
        public Data data { get; set; }
    }

}
