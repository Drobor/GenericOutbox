using System.Linq;
using Microsoft.CodeAnalysis;

namespace GenericOutbox.SourceGenerator
{
    public static class SyntaxExtensions
    {
        public static ITypeSymbol AsyncReturnType(this IMethodSymbol method)
            => (method?.ReturnType as INamedTypeSymbol)?.TypeArguments.FirstOrDefault();
        
        private static INamespaceSymbol GetNamespace(this SyntaxNode node, SemanticModel semanticModel) =>
            semanticModel.GetTypeInfo(node).Type?.ContainingNamespace;
    }
}
