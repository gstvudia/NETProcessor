using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace NET.Processor.Core.Models.RelationsGraph.Item.Base
{
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

    public class Node
    {
        public NodeRoot data { get; set; }
    }

    public class NodeRoot
    {
        public string Id { get; set; }
        public string projectId { get; set; }
        public string name { get; set; }
        public string nodeType { get; set; }

        public NodeData nodeData = new NodeData();
    }

    public class NodeData
    {
        public string name { get; set; }
        public string nodeType { get; set; }
        public string fileName { get; set; }
        public string className { get; set; }
        public string language { get; set; }
        public List<Comment> comments { get; set; }
        public string repositoryLinkOfMethod { get; set; }
        public string repositoryCommitLinkOfMethod { get; set; }

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
        public string sourceName { get; set; }
        public string target { get; set; }
        public string targetName { get; set; }
        public string targetNodeType { get; set; }
    }

    public class Edge
    {
        public EdgeData data { get; set; }
    }
}
