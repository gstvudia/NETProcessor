using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using NET.Processor.Core.Models;
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
            return from existingProjects in solution.Projects
                   join selectedProjects in filter.Projects on existingProjects.Name equals selectedProjects
                   select (existingProjects);
        }

        public static IEnumerable<Document> FilterDocuments(Project project, Filter filter)
        {
            return from existingDocuments in project.Documents
                   join selectedDocuments in filter.Documents on existingDocuments.Name.Split(".")[0] equals selectedDocuments
                   select (existingDocuments);
        }

        public static IEnumerable<Item> FilterMethods(List<Item> methods, Filter filter)
        {
            return from existingMethods in methods
                   join selectedMethods in filter.Methods on existingMethods.Name equals selectedMethods
                   select (existingMethods);
        }
    }
}
