using NET.Processor.API.Models.DTO;
using NET.Processor.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NET.Processor.API.Helpers.Interfaces
{
    public interface IRelationsGraphMapper
    {
        List<Edge> MapItemsToEdges(List<Item> items);
    }
}
