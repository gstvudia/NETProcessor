using DynamicData;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NET.Processor.Core.Models;
using NET.Processor.Core.Models.RelationsGraph.Item;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NET.Processor.Core.Services.Project.Walkers
{
    public class DocumentWalkerFunctions
    {
        public Method AddClassMethod(SyntaxNode root, MethodDeclarationSyntax node, List<Method> methodsList,
            Guid projectId, Guid fileId, string fileName, string language, ClassDeclarationSyntax currentClass,
            string currentClassName)
        {
            // Methods
            var method = new Method(Guid.NewGuid().ToString(), node.Identifier.ValueText,
                                        projectId.ToString(), node.Body, fileId.ToString(),
                                        fileName, currentClassName, root.DescendantNodes().IndexOf(currentClass),
                                        language);

            // Methods relations towards child methods
            if (!methodsList.Any(x => x.Name == method.Name))
            {
                // Remove Third Party Methods 
                // method = RemoveThirdPartyMethods(method);
                methodsList.Add(method);
            }

            return method;
        }

        private List<Method> GetChilds(Method method)
        {
            List<Method> childList = new List<Method>();
            /*

            if (method.Body != null)
            {
                var invokees = method.Body.Statements.Where(x => x.Kind().ToString() == "ExpressionStatement").ToList();
                foreach (var invoked in invokees)
                {
                    var expression = invoked as ExpressionStatementSyntax;
                    var arguments = expression.Expression as InvocationExpressionSyntax;
                    string[] child = null;

                    child = invoked.ToString().Replace(";", string.Empty).Split(".");

                    // TODO: If there is a recursive function, this must be handled here by assigning same function ID!
                    if (child.Length > 1)
                    {
                        childList
                            .Add(new Method(
                                "-1", child[1].Substring(0, child[1].LastIndexOf("(") + 1).Replace("(", string.Empty)));
                    }
                    else
                    {
                        childList
                            .Add(new Method(
                                "-1", child[0].Substring(0, child[0].LastIndexOf("(") + 1).Replace("(", string.Empty)));
                    }
                }
            }
            */

            return childList;
        }

        public void AddClass(SyntaxNode root, Method method, List<Class> classList, ClassDeclarationSyntax currentClass,
            string currentClassName, NamespaceDeclarationSyntax currentNamespace, string currentNamespaceName,
            string projectId, Guid fileId, string fileName, string language, List<string> attachedInterfaces)
        {
            Class containingClass = new Class(Guid.NewGuid().ToString(), currentClassName,
                projectId.ToString(), root.DescendantNodes().IndexOf(currentNamespace),
                currentNamespaceName, fileId.ToString(), fileName, language, attachedInterfaces);
            classList.Add(containingClass);
            attachedInterfaces.Clear();
        }

        public void AddInterface(string name, string projectId, List<Interface> interfaceList, 
            string namespaceName, string fileId, string fileName, string language)
        {
            Interface classInterface = new Interface(Guid.NewGuid().ToString(), name, projectId,
                namespaceName, fileId, fileName, language);
            interfaceList.Add(classInterface);
        }
    }
}
