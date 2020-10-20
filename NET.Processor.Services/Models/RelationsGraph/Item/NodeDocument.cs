using System;

namespace NET.Processor.Core.Models.RelationsGraph.Item
{
    public class NodeDocument : Item
    {
        public new Guid Id { get; set; }

        public NodeDocument(Guid guid, string name)
        {
            Id = guid;
            Name = name;
        }
    }
}
