using System;

namespace NET.Processor.Core.Models.RelationsGraph.Item
{
    public class NodeProject : Item
    {
        public Guid Guid { get; set; }

        public NodeProject(Guid guid, string name)
        {
            Guid = guid;
            Name = name;
        }
    }
}
