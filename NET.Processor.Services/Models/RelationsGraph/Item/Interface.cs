using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace NET.Processor.Core.Models.RelationsGraph.Item
{
    public class Interface : Item
    {
        public string NamespaceName { get; set; }

        [BsonIgnore]
        public List<Class> ChildList { get; set; } = new List<Class>();
        public Namespace Parent { get; set; }
        public string Language { get; set; }

        [BsonElement("FileName")]
        public string FileName { get; set; }
        public string FileId { get; set; }

        public Interface(string id, string name, string ProjectId, string NamespaceName,
            string FileId, string FileName, string Language) : base(id, name, ProjectId)
        {
            this.NamespaceName = NamespaceName;
            this.FileName = FileName;
            this.FileId = FileId;
            this.Language = Language;
            TypeHierarchy = 3;
        }

        public void AddRangeChild(List<Class> childList)
        {
            ChildList.AddRange(childList);
        }
    }
}