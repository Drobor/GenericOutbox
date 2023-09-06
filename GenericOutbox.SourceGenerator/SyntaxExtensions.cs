using System.Collections.Generic;
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

        public static List<ISymbol> GetAllMembers(this INamedTypeSymbol type)
        {
            var result = type.GetMembers().ToList();

            foreach (var baseInterface in type.Interfaces)
                result.AddRange(baseInterface.GetAllMembers());

            return result;
        }
    }
}
