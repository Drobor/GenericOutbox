using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GenericOutbox.SourceGenerator
{
    public class OutboxInterfacesSyntaxReceiver : ISyntaxReceiver
    {
        public List<InterfaceDeclarationSyntax> OutboxInterfaces = new List<InterfaceDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InterfaceDeclarationSyntax ids && ids.AttributeLists.SelectMany(x=>x.Attributes).Any(x=>x.Name.ToString().Contains("OutboxInterface")))
            {
                OutboxInterfaces.Add(ids);
            }
        }
    }
}
