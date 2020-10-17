using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace NET.Processor.Core.Models.RelationsGraph.Item
{
    class Region : Item
    {
        public Region(string name, TextSpan span)
        {
            Name = name;
            Span = span;
        }
    }
}
