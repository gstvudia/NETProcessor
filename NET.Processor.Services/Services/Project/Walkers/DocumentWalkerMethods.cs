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
    public class DocumentWalkerMethods
    {
        public Method AddClassMethod(SyntaxNode root, MethodDeclarationSyntax node, List<Method> methodsList,
            Guid projectId, Guid fileId, string fileName, string language, ClassDeclarationSyntax currentClass,
            string currentClassName)
        {
            string returnType = node.ReturnType.ToString();

            // Methods
            Method method = new Method(Guid.NewGuid().ToString(), node.Identifier.ValueText,
                                        projectId.ToString(), node.Body, node.ParameterList, returnType, fileId.ToString(),
                                        fileName, currentClassName, root.DescendantNodes().IndexOf(currentClass),
                                        language);

            // Methods relations towards child methods
            // Ensure that no methods of the same name are added to methodslist unless they have differing 
            // parameters
            if (!methodsList.Any(x => x.Name == method.Name) || !methodsList.Any(x => x.ParameterList.ToString() == method.ParameterList.ToString()))
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

        public void AddClass(SyntaxNode root, Method method, List<Class> classList,
            string currentClassName, NamespaceDeclarationSyntax currentNamespace, string currentNamespaceName,
            Namespace containingNamespace, string projectId, Guid fileId, string fileName, string language, List<string> attachedInterfaces)
        {
            Class containingClass = new Class(Guid.NewGuid().ToString(), currentClassName,
                projectId.ToString(), root.DescendantNodes().IndexOf(currentNamespace),
                currentNamespaceName, fileId.ToString(), fileName, language, attachedInterfaces);
            containingClass.Parent = containingNamespace;
            classList.Add(containingClass);
            attachedInterfaces.Clear();
        }

        public void AddInterface(string name, string projectId, List<Interface> interfaceList, 
            Namespace containingNamespace, string namespaceName, string fileId, string fileName, string language)
        {
            Interface classInterface = new Interface(Guid.NewGuid().ToString(), name, projectId,
                namespaceName, fileId, fileName, language);
            classInterface.Parent = containingNamespace;
            interfaceList.Add(classInterface);
        }
    }
}
