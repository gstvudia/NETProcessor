using NET.Processor.API.Helpers.Interfaces;
using NET.Processor.API.Models.DTO;
using NET.Processor.Core.Models;
using NET.Processor.Core.Models.RelationsGraph.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NET.Processor.API.Helpers.Mappers
{
    public class RelationsGraphMapper : IRelationsGraphMapper
    {

        public List<Edge> MapItemsToEdges(List<Item> items)
        {
            List<Edge> graphEdges = new List<Edge>();
            foreach (var item in items.Where(item => item.ChildList.Count > 0))
            {
                foreach (var child in item.ChildList)
                {
                    graphEdges.Add(new Edge
                        {
                            data = new EdgeData
                            {
                                source = Convert.ToString(item.Id),
                                target = Convert.ToString(child.Id),
                                colorCode = "blue",
                                strength = 5
                            }
                        }
                    );
                }                
            }

            return graphEdges;
        }
    }
}
