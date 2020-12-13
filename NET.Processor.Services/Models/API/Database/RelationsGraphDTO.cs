using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace NET.Processor.Core.Models.RelationsGraph.Item.Base
{
    public class RelationsGraphDTO
    {
        public Tuple<Int32, string> Nodes { get; set; }
        public Tuple<Int32, Int32> Edges { get; set; }
        public RelationsGraphDTO Type { get; set; }
    }

    public class Node
    {
        public NodeRoot data { get; set; }
    }

    public class NodeRoot
    {
        public string Id { get; set; }
        public string name { get; set; }
        public string nodeType { get; set; }
        public int weight { get; set; }
        public string colorCode { get; set; }
        public string shapeType { get; set; }

        public NodeData nodeData = new NodeData();
    }

    public class NodeData
    {
        public string name { get; set; }
        public string fileName { get; set; }
        public string className { get; set; }
        public string language { get; set; }
        public string repositoryLinkToMethod { get; set; }

        /* The following values belonging to this class are pulled in dynamically by trigger through frontend
        // sectionTags
        // tickets
        // history of changes
        // documentation
        // goToMethod
        */
    }

    public class EdgeData
    {
        public string source { get; set; }
        public string target { get; set; }
        public string colorCode { get; set; }
        public int strength { get; set; }
    }

    public class Edge
    {
        public EdgeData data { get; set; }
    }

    public class ProjectRelationsGraph
    {
        public ObjectId Id { get; set; }
        public string SolutionName { get; set; }
        public ProjectRelationsGraphRoot graphData = new ProjectRelationsGraphRoot();
    }

    public class ProjectRelationsGraphRoot
    {
        public List<Node> nodes { get; set; }
        public List<Edge> edges { get; set; }
    }
}
