using Microsoft.Extensions.Hosting;
using NET.Processor.Core.Models;
using NET.Processor.Core.Models.RelationsGraph.Item;
using NET.Processor.Core.Models.RelationsGraph.Item.Base;
using System.Collections.Generic;

namespace NET.Processor.Core.Helpers.Interfaces
{
    public interface IRelationsGraphMapper
    {
        List<Edge> MapItemsToEdges(List<Item> items);
    }
}
