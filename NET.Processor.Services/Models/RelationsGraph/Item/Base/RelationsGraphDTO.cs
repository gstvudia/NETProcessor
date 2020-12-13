using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        public NodeRoot Data { get; set; }
    }

    public class NodeRoot
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Body { get; set; }
        public string NodeType { get; set; }
        public int Weight { get; set; }
        public string ColorCode { get; set; }
        public string ShapeType { get; set; }
        public NodeData NodeData { get; set; } = new NodeData();
    }

    public class NodeData
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public string ClassName { get; set; }
        public string Language { get; set; }

        /* The following values belonging to this class are pulled in dynamically by trigger through frontend
        // method body
        // sectionTags
        // tickets
        // history of changes
        // documentation
        // goToMethod
        */
    }

    public class EdgeData
    {
        public string Source { get; set; }
        public string Target { get; set; }
        public string ColorCode { get; set; }
        public int Strength { get; set; }
    }

    public class Edge
    {
        public EdgeData Data { get; set; }
    }

    public class ProjectRelationsGraph
    {
        public ObjectId Id { get; set; }
        public string SolutionName { get; set; }
        public ProjectRelationsGraphRoot graphData = new ProjectRelationsGraphRoot();
    }

    public class ProjectRelationsGraphRoot
    {
        public List<Node> Nodes { get; set; }
        public List<Edge> Edges { get; set; }
    }

}
