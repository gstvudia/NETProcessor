using System;
using System.Collections.Generic;
using System.Text;

namespace NET.Processor.Core.Models.RelationsGraph.Item
{
    public class NodeDocument : Item
    {
        public Guid Guid { get; set; }

        public NodeDocument(Guid guid, string name)
        {
            Guid = guid;
            Name = name;
        }
    }
}
