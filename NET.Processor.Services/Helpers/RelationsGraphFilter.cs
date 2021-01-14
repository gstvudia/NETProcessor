using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using NET.Processor.Core.Models;
using NET.Processor.Core.Models.RelationsGraph.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using Project = Microsoft.CodeAnalysis.Project;

namespace NET.Processor.Core.Helpers
{
    public class RelationsGraphFilter
    {
        public static IEnumerable<Project> FilterSolutions(Solution solution, Filter filter)
        {
            if (filter.Projects.Count == 0) return solution.Projects;

            return (from existingProjects in solution.Projects
                   join selectedProjects in filter.Projects on existingProjects.Name equals selectedProjects
                   select (existingProjects)).Distinct();
        }

        public static IEnumerable<Document> FilterDocuments(Project project, Filter filter)
        {
            if (filter.Documents.Count == 0) return project.Documents;

            return (from existingDocuments in project.Documents
                   join selectedDocuments in filter.Documents on existingDocuments.Name.Split(".")[0] equals selectedDocuments
                   select (existingDocuments)).Distinct();
        }

        public static IEnumerable<Method> FilterMethods(List<Method> methods, Filter filter)
        {
            if(filter.Methods.Count == 0) return methods;

            return (from existingMethods in methods
                   join selectedMethods in filter.Methods on existingMethods.Name equals selectedMethods
                   select (existingMethods)).Distinct();
        }
    }
}
