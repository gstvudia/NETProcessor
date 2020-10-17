using Microsoft.CodeAnalysis.Text;
using NET.Processor.Core.Models.RelationsGraph.Item;

namespace NET.Processor.Core.Models
{
    public class Method : Item
    {
        public Method(int id, string name, TextSpan span) : base(id, name, span)
        {
        }
    }
}