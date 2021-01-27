using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NET.Processor.Core.Models;
using NET.Processor.Core.Models.RelationsGraph.Item;
using NET.Processor.Core.Services.Project.Walkers;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace NET.Processor.Core.Services.Project
{
    public class DocumentWalker : CSharpSyntaxWalker
    {
        private DocumentWalkerMethods documentWalkerFunctions = new DocumentWalkerMethods();
        private string currentClassName = string.Empty;
        private  ClassDeclarationSyntax currentClass = null;
        private string currentNamespaceName { get; set; }
        private NamespaceDeclarationSyntax currentNamespaceNode = null;
        private List<string> attachedInterfaces = new List<string>();
        private SyntaxNode root;
        private Guid projectId;
        private string fileName = string.Empty;
        private Guid fileId;
        private string language = string.Empty;

        private static readonly string INTERFACEDECLARATION = "InterfaceDeclaration";

        // Methods
        public List<Method> methodsList = new List<Method>();
        // Classes
        public List<Class> classList = new List<Class>();
        // Namespaces 
        public Namespace containingNamespace = null;
        // Interfaces
        public List<Interface> interfaceList = new List<Interface>();
        // Namespaces
        List<Namespace> namespacesList = new List<Namespace>();

        public DocumentWalker(SyntaxNode root, Guid projectId, string fileName, Guid fileId, string language, List<Namespace> namespacesList) 
            : base(SyntaxWalkerDepth.Token)
        {
            this.root = root;
            this.projectId = projectId;
            this.fileName = fileName;
            this.fileId = fileId;
            this.language = language;
            this.namespacesList = namespacesList;
        }

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            currentNamespaceNode = node;
            currentNamespaceName = node.Name.ToString();
            Console.WriteLine("Namespace of file: " + currentNamespaceName);

            // This is needed to ensure that the id of the namespace remains the same if identical node,
            // otherwise the system might not be able to find the correct namespace edge id for different file nodes!
            Namespace n = namespacesList.Where(i => i.Name.Equals(currentNamespaceName)).FirstOrDefault();
            if (n != null)
            {
                containingNamespace = n;
            } else
            {
                // Adding class relations towards Namespace
                containingNamespace = new Namespace(Guid.NewGuid().ToString(),
                currentNamespaceName, projectId.ToString(),
                fileId.ToString(), fileName);
            }
                
            base.VisitNamespaceDeclaration(node);
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            string interfaceName = node.Identifier.ToString();
            Console.WriteLine("Interface name in file: " + interfaceName);

            documentWalkerFunctions.AddInterface(interfaceName, projectId.ToString(), interfaceList,
                containingNamespace, currentNamespaceName, fileId.ToString(), fileName, language);

            base.VisitInterfaceDeclaration(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            currentClassName = node.Identifier.ToString();
            currentClass = node;
            if(node.BaseList != null && node.BaseList.Types != null)
            {
                // Get all interface names attached to class
                foreach(var interfaceName in node.BaseList.Types)
                {
                    attachedInterfaces.Add(interfaceName.GetFirstToken().ToString());
                }
            }

            documentWalkerFunctions.AddClass(root, null, classList, currentClassName, currentNamespaceNode,
                currentNamespaceName, containingNamespace, projectId.ToString(), fileId, fileName, language, attachedInterfaces);

            Console.WriteLine("Class in file: " + currentClassName);
            base.VisitClassDeclaration(node);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            // Get Parent of Invocation Expression and then assign the invocation expression
            // to that method

           
            // Method relations towards child methods
            /*
            if (!methodsList.Any(x => x.Name == method.Name))
            {
                method.ChildList.AddRange(documentWalkerFunctions.GetChilds(method));
                // Remove Third Party Methods 
                // method = RemoveThirdPartyMethods(method);
                methodsList.Add(method);
            }
            */
            
            base.VisitInvocationExpression(node);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            string methodName = node.Identifier.ToString();
            Console.WriteLine("Class and Method in file: " + currentClassName + '.' + methodName);

            // Check if it is an interface or class method
            if (node.Parent.Kind().ToString().Equals(INTERFACEDECLARATION))
            {
                // Add interface method

            } else {
                // Add class method to method list
                Method method = documentWalkerFunctions.AddClassMethod(root, node, methodsList, projectId, fileId, fileName,
                    language, currentClass, currentClassName);

                // Add method to class
                Class c = classList.Single(c => c.Name.Equals(currentClassName));
                c.AddChild(method);

                // Add parent to method 
                method.Parent = c;
            }

            base.VisitMethodDeclaration(node);
        }

        // This is to visit each node and each token separately (might be slower)
        /*
        public override void Visit(SyntaxNode node)
        {
            Console.WriteLine(node);
            base.Visit(node);
        }
        */

        /*
        public override void VisitToken(SyntaxToken token)
        {
            Console.WriteLine(token);
            base.VisitToken(token);
        }
        */
    }
}