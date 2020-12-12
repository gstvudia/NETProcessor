using NET.Processor.Core.Helpers.Interfaces;
using NET.Processor.Core.Models;
using NET.Processor.Core.Models.RelationsGraph.Item;
using NET.Processor.Core.Models.RelationsGraph.Item.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NET.Processor.Core.Helpers.Mappers
{
    public class RelationsGraphMapper : IRelationsGraphMapper
    {

        public List<Edge> MapItemsToEdges(List<Method> items)
        {
            List<Edge> graphEdges = new List<Edge>();
            foreach (var item in items.Where(item => item.ChildList.Count > 0))
            {
                foreach (var child in item.ChildList)
                {
                    graphEdges.Add(new Edge
                    {
                        Data = new EdgeData
                        {
                            Source = Convert.ToString(item.Id),
                            Target = Convert.ToString(child.Id),
                            ColorCode = "white",
                            Strength = 5
                        }
                    }
                    );
                }
            }

            return graphEdges;
        }
    }
}
