using Microsoft.Extensions.Hosting;
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
                            MapToEdge(item, child, child.GetType().ToString());
                        }
                    }

                    // Method item = (Method)listItem;
                    // MapMethodToEdge(item);
                }
                else if (listItem is Class)
                {
                    Class item = (Class)listItem;
                    if (item.ChildList.Count > 0)
                    {
                        foreach (var child in item.ChildList)
                        {
                            MapToEdge(item, child, child.GetType().ToString());
                        }
                    }
                }
                else if(listItem is Interface)
                {
                    Interface item = (Interface)listItem;
                    if (item.ChildList.Count > 0)
                    {
                        foreach (var child in item.ChildList)
                        {
                            MapToEdge(item, child, child.GetType().ToString());
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
                            MapToEdge(item, child, child.GetType().ToString());
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
                            MapToEdge(item, child, child.GetType().ToString());
                        }
                    }
                }
                else if (listItem is File)
                {
                    File item = (File)listItem;
                    if (item.Child != null)
                    {
                        MapToEdge(item, item.Child, item.Child.GetType().ToString());
                    }
                }
                else
                {
                    throw new Exception("The Item found for mapping Edges is not handled by any NodeType! Aborting program");
                }
            }

            return graphEdges;
        }

        private void MapToEdge(Item item, Item child, string childType)
        {
            graphEdges.Add(new Edge
            {
                data = new EdgeData
                {
                    source = Convert.ToString(item.Id),
                    sourceName = item.Name,
                    target = Convert.ToString(child.Id),
                    targetName = child.Name,
                    targetNodeType = childType.Split(".").Last()
                }
            }
            );
        }

        /*
        private void MapMethodToEdge(Method method)
        {
            if(method.ChildList == null)
                return;

            // Go through all childs of this method, after all methods have been run through
            // Proceed to the next method with childs
            foreach(var child in method.ChildList)
            {
                if (method.ChildList.Count > 0)
                {
                    graphEdges.Add(new Edge
                    {
                        data = new EdgeData
                        {
                            source = Convert.ToString(method.Id),
                            sourceName = method.Name,
                            target = Convert.ToString(child.Id),
                            targetName = child.Name,
                            targetNodeType = child.GetType().ToString().Split(".").Last()
                        }
                    }
                    );
                }
            }
        }
        */
    }
}
