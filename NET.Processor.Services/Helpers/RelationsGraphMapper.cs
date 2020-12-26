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
        List<Edge> graphEdges = new List<Edge>();

        public List<Edge> MapItemsToEdges(List<Item> items)
        {
            foreach (var item in items
                .Where(item =>
                   item.GetType() == typeof(Class) ||
                   item.GetType() == typeof(Method) ||
                   item.GetType() == typeof(File) ||
                   item.GetType() == typeof(Project) ||
                   item.GetType() == typeof(Namespace))
                .Select(item => item.ChildList.Count > 0));


            foreach (var item in items.Where(item => item.ChildList.Count > 0))
            {
                foreach (var child in item.ChildList)
                {
                    MapToEdge(item, child);
                }
            }

            foreach (var item in items.Where(item => item.ChildList.Count > 0))
            {
                foreach (var child in item.ChildList)
                {
                    graphEdges.Add(new Edge
                    {
                        data = new EdgeData
                        {
                            source = Convert.ToString(item.Id),
                            target = Convert.ToString(child.Id)
                        }
                    }
                    );
                }
            }

            return graphEdges;
        }

        private Edge MapToEdge(Item item)
        {
            graphEdges.Add(new Edge
            {
                data = new EdgeData
                {
                    source = Convert.ToString(item.Id),
                    target = Convert.ToString(child.Id)
                }
            }
            );
        }
    }
}
