using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace NET.Processor.Core.Models
{
    public abstract class CodeItem
    {
        private readonly TextSelection selection;

        private Location location;
        private IEnumerable<string> modifiers;
        private string name;


        private CodeItem()
            : this(null, null)
        { }

        protected CodeItem(MemberDeclarationSyntax memberDeclarationSyntax, TextSelection selection)
        {
            if (memberDeclarationSyntax == null)
                return;

            this.selection = selection;


            this.location = memberDeclarationSyntax.GetLocation();
            this.modifiers = memberDeclarationSyntax.Modifiers.Select(m => m.ValueText);
            this.name = GetNameFromDeclarationSyntaxCore(memberDeclarationSyntax);
        }

        public Location Location { get => location; protected set => location = value; }

        public IEnumerable<string> Modifiers { get => modifiers; protected set => modifiers = value; }

        public string Name { get => name; protected set => name = value; }


        public override string ToString()
        {
            return $"{string.Join(" ", modifiers)} {name}";
        }

        public string GetNameFromDeclarationSyntax(MemberDeclarationSyntax memberDeclarationSyntax)
        {
            return GetNameFromDeclarationSyntaxCore(memberDeclarationSyntax);
        }

        protected abstract string GetNameFromDeclarationSyntaxCore(MemberDeclarationSyntax memberDeclarationSyntax);

        public TreeViewItem ToUIControl()
        {
            return ToUIControlCore();
        }

        protected virtual TreeViewItem ToUIControlCore()
        {
            var treeViewItem = new TreeViewItem();
            treeViewItem.HeaderTemplate = CreateDataTemplate();
            treeViewItem.Tag = location.SourceSpan;
            treeViewItem.IsExpanded = true;


            return treeViewItem;
        }

        internal DataTemplate CreateDataTemplate()
        {
            return CreateDataTemplateCore();
        }

        protected virtual DataTemplate CreateDataTemplateCore()
        {
            var stackPanel = new FrameworkElementFactory(typeof(StackPanel));




            var text = new FrameworkElementFactory(typeof(TextBlock));
           stackPanel.AppendChild(text);

            var dataTemplate = new DataTemplate();
            dataTemplate.VisualTree = stackPanel;

            return dataTemplate;
        }

        internal string GetCodeType()
        {
            return GetCodeTypeCore();
        }

        protected abstract string GetCodeTypeCore();

        internal string MappingDeclarationSyntax()
        {
            return MappingDeclarationSyntaxCore();
        }

        protected abstract string MappingDeclarationSyntaxCore();




    }
}