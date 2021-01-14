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
        public string currentNamespaceName { get; set; }
        private NamespaceDeclarationSyntax currentNamespace = null;

        // Methods
        private List<Method> methodsList = new List<Method>();
        // Classes
        private List<Class> classList = new List<Class>();
        // Interfaces
        private List<Interface> interfaceList = new List<Interface>();
        private List<string> attachedInterfaces = new List<string>();

        private SyntaxNode root;
        private Guid projectId;
        private string fileName = string.Empty;
        private Guid fileId;
        private string language = string.Empty;

        public DocumentWalker(SyntaxNode root, List<Method> methodsList, List<Class> classList, 
            List<Interface> interfaceList, Guid projectId, string fileName, Guid fileId, string language) 
            : base(SyntaxWalkerDepth.Token)
        {
            this.root = root;
            this.methodsList = methodsList;
            this.classList = classList;
            this.interfaceList = interfaceList;
            this.projectId = projectId;
            this.fileName = fileName;
            this.fileId = fileId;
            this.language = language;
        }

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            // Only print namespace if it is a new one
            if (currentNamespaceName == null || !currentNamespaceName.Equals(node.Name.ToString()))
            {
                currentNamespace = node;
                currentNamespaceName = node.Name.ToString();
                Console.WriteLine("Namespace of file: " + currentNamespaceName);
            }
            base.VisitNamespaceDeclaration(node);
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            string interfaceName = node.Identifier.ToString();
            Console.WriteLine("Interface name in file: " + interfaceName);

            documentWalkerFunctions.AddInterface(interfaceName, projectId.ToString(), interfaceList,
                currentNamespaceName, fileId.ToString(), fileName, language);

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

            documentWalkerFunctions.AddClass(root, null, classList, currentClass, currentClassName, currentNamespace,
                currentNamespaceName, projectId.ToString(), fileId, fileName, language, attachedInterfaces);

            Console.WriteLine("Class in file: " + currentClassName);
            base.VisitClassDeclaration(node);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            // Get Parent of Invocation Expression and then assign the invocation expression
            // to that method

           
            // Methods relations towards child methods
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

            // Add method to method list
            Method method = documentWalkerFunctions.AddClassMethod(root, node, methodsList, projectId, fileId, fileName,
                language, currentClass, currentClassName);

            // Check if it is a Class Method or an Interface Method 
            if(currentClassName == null || currentClassName.Equals(String.Empty))
            {
                // It is an interface method
            }
            else
            {
                // It is a class method
                Class c = classList.Single(c => c.Name == currentClassName);
                // Add method to class
                c.AddChild(method);
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