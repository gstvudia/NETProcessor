using Microsoft.CodeAnalysis.Text;

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
