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

        public List<Edge> MapItemsToEdges(List<Item> listItems)
        {
            foreach (var listItem in listItems)
            {
                if (listItem is Method)
                {
                    Method item = (Method)listItem;
                    if (item.ChildList.Count > 0)
                    {
                        foreach (var child in item.ChildList)
                        {
                            MapToEdge(item, child);
                        }
                    }
                }
                else if (listItem is Class)
                {
                    Class item = (Class)listItem;
                    if (item.ChildList.Count > 0)
                    {
                        foreach (var child in item.ChildList)
                        {
                            MapToEdge(item, child);
                        }
                    }
                }
                else if (listItem is Namespace)
                {
                    Namespace item = (Namespace)listItem;
                    if (item.ChildList.Count > 0)
                    {
                        foreach (var child in item.ChildList)
                        {
                            MapToEdge(item, child);
                        }
                    }
                }
                else if (listItem is Project)
                {
                    Project item = (Project)listItem;
                    if (item.ChildList.Count > 0)
                    {
                        foreach (var child in item.ChildList)
                        {
                            MapToEdge(item, child);
                        }
                    }
                }
                else if (listItem is File)
                {
                    File item = (File)listItem;
                    if (item.ChildList.Count > 0)
                    {
                        foreach (var child in item.ChildList)
                        {
                            MapToEdge(item, child);
                        }
                    }
                }
                else
                {
                    throw new Exception("The Item found for mapping Edges is not handled by any NodeType! Aborting program");
                }
            }

            //{
            //    Console.WriteLine(item);
            /*
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
            */
            // }

            /*
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
            */

            return graphEdges;
        }

        private void MapToEdge(Item item, Item child)
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
