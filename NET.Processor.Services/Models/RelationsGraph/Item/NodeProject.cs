using System;

namespace NET.Processor.Core.Models.RelationsGraph.Item
{
    public class NodeProject : Item
    {
        public new Guid Id { get; set; }

        public NodeProject(Guid guid, string name)
        {
            Id = guid;
            Name = name;
        }
    }
}
