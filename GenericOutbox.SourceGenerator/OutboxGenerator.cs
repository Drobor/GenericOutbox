using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GenericOutbox.SourceGenerator
{
    [Generator]
    public class OutboxGenerator : ISourceGenerator
    {
        private static readonly SymbolDisplayFormat s_fullyQualifiedTypeDisplayFormat = new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            memberOptions: SymbolDisplayMemberOptions.IncludeParameters,
            parameterOptions: SymbolDisplayParameterOptions.IncludeType,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new OutboxInterfacesSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var synRes = (OutboxInterfacesSyntaxReceiver)context.SyntaxReceiver;

            foreach (var connectionType in synRes.OutboxInterfaces)
            {
                try
                {
                    var fullName = $"{((BaseNamespaceDeclarationSyntax)connectionType.Parent).Name}.{connectionType.Identifier.Text}";
                    var outboxInterface = context.Compilation.GetTypeByMetadataName(fullName);

                    try
                    {
                        context.AddSource($"{outboxInterface.Name}.g.cs", GenerateInterfaceWrapper(outboxInterface));
                    }
                    catch (Exception ex)
                    {
                        context.AddSource($"{outboxInterface.Name}{DateTime.Now:HH-mm-ss}.Error.g.cs", "//" + DumpException(ex).Replace("\n", "\n//"));
                    }
                }
                catch (Exception ex)
                {
                    context.AddSource($"GenericOutbox.{DateTime.Now:HH-mm-ss}.Error.g.cs", $"{ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        private string GenerateInterfaceWrapper(INamedTypeSymbol outboxInterface)
        {
            if (outboxInterface.Interfaces.Length != 1)
            {
                throw new Exception($"Wrong implemented interfaces count for type {outboxInterface.Name}\n" +
                                    "Interfaces marked by OutboxInterfaceAttribute must have exactly one interface they implement\n" +
                                    "e.g. public interface IOutboxStorageApiClient : IStorageApiClient { }");
            }

            var baseInterface = outboxInterface.Interfaces.Single();
            var interfaceName = outboxInterface.Name;
            var className = interfaceName.Substring(1);

            var methods = baseInterface
                .GetMembers()
                .OfType<IMethodSymbol>()
                .ToArray();

            var cb = new CodeBuilder();

            cb.WriteCode(
                "using GenericOutbox;",
                "",
                "using System;",
                "using System.Collections.Generic;",
                "using System.IO;",
                "using System.Threading;",
                "using System.Threading.Tasks;",
                "",
                $"namespace {outboxInterface.ContainingNamespace}",
                "{",
                $"public partial class {className} : {outboxInterface.ToDisplayString(s_fullyQualifiedTypeDisplayFormat)}",
                "{",
                "public static Dictionary<string, Type> ActionHandlerMap = new Dictionary<string, Type>",
                "{");

            foreach (var method in methods)
                cb.WriteCode($"[\"{GetOutboxActionName(baseInterface, method)}\"] = typeof({GetMethodHandlerClassName(method)}),");

            cb.WriteCode(
                "};",
                "",
                "private readonly OutboxOptions _options;",
                "private readonly IOutboxCreatorContext _outboxCreatorContext;",
                $"private readonly {baseInterface.ToDisplayString(s_fullyQualifiedTypeDisplayFormat)} _baseInterface;",
                "",
                $"public {className}(OutboxOptions options, {baseInterface.ToDisplayString(s_fullyQualifiedTypeDisplayFormat)} baseInterface, IOutboxCreatorContext outboxCreatorContext)",
                "{",
                "_options = options;",
                "_outboxCreatorContext = outboxCreatorContext;",
                "_baseInterface = baseInterface;",
                "}"
            );

            foreach (var method in methods)
            {
                var methodIsAsync = IsTask(method.ReturnType);

                var methodReturnType = methodIsAsync
                    ? method.AsyncReturnType()
                    : method.ReturnType;

                var asyncIfNeeded = methodIsAsync
                    ? "async "
                    : "";

                var awaitIfNeeded = methodIsAsync
                    ? "await "
                    : "";

                cb.WriteCode(
                    $"public {asyncIfNeeded}{method.ReturnType} {method.Name}({string.Join(", ", method.Parameters.Select(x => $"{x.Type.ToDisplayString(s_fullyQualifiedTypeDisplayFormat)} {x.Name}"))})",
                    "{",
                    "if (_options.DirectPassthrough)",
                    "{");

                if (methodReturnType != null && methodReturnType.ToDisplayString(s_fullyQualifiedTypeDisplayFormat) != "System.Void")
                {
                    cb.WriteCode($"return {awaitIfNeeded}_baseInterface.{method.Name}({string.Join(", ", method.Parameters.Select(x => x.Name))});");
                }
                else
                {
                    cb.WriteCode(
                        $"{awaitIfNeeded}_baseInterface.{method.Name}({string.Join(", ", method.Parameters.Select(x => x.Name))});",
                        "return;");
                }

                cb.WriteCode(
                    "}",
                    "",
                    $"var payload = new {GetMethodArgumentsWrapperModelName(method)}",
                    "{");

                foreach (var argument in method.Parameters.Where(x => x.Type.Name != "CancellationToken").OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase))
                    cb.WriteCode($"{argument.Name} = {argument.Name},");

                cb.WriteCode(
                    "};",
                    "");

                if (methodIsAsync)
                {
                    cb.WriteCode($"await _outboxCreatorContext.CreateOutboxRecordAsync(\"{GetOutboxActionName(baseInterface, method)}\", payload);");
                }
                else
                {
                    cb.WriteCode($"_outboxCreatorContext.CreateOutboxRecord(\"{GetOutboxActionName(baseInterface, method)}\", payload);");
                }

                if (methodReturnType != null && methodReturnType.ToDisplayString(s_fullyQualifiedTypeDisplayFormat) != "System.Void")
                    cb.WriteCode("return default;");

                cb.WriteCode("}");
            }

            foreach (var method in methods)
                GenerateHandlerClass(cb, baseInterface, method);

            foreach (var method in methods)
                GenerateArgumentsWrapperModel(cb, method);

            cb.WriteCode(
                "}",
                "}");

            return cb.ToString();
        }

        private void GenerateArgumentsWrapperModel(CodeBuilder cb, IMethodSymbol method)
        {
            cb.WriteCode(
                $"public class {GetMethodArgumentsWrapperModelName(method)}",
                "{");

            foreach (var argument in method.Parameters.Where(x => x.Type.Name != "CancellationToken").OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase))
                cb.WriteCode($"public {argument.Type.ToDisplayString(s_fullyQualifiedTypeDisplayFormat)} {argument.Name} {{ get; set; }}");

            cb.WriteCode("}");
        }

        private void GenerateHandlerClass(CodeBuilder cb, ITypeSymbol type, IMethodSymbol method)
        {
            var originalInterfaceType = type.ToDisplayString(s_fullyQualifiedTypeDisplayFormat);
            var className = GetMethodHandlerClassName(method);
            var argumentWrapperModelName = GetMethodArgumentsWrapperModelName(method);

            var methodIsAsync = IsTask(method.ReturnType);

            var awaitIfNeeded = methodIsAsync
                ? "await "
                : "";

            var methodReturnType = methodIsAsync
                ? method.AsyncReturnType()
                : method.ReturnType;

            if (methodReturnType != null && methodReturnType.ToDisplayString(s_fullyQualifiedTypeDisplayFormat) != "System.Void")
            {
                cb.WriteCode(
                    $"public class {className} : OutboxActionHandlerBase<{argumentWrapperModelName}, {methodReturnType.ToDisplayString(s_fullyQualifiedTypeDisplayFormat)}>",
                    "{",
                    $"private readonly {originalInterfaceType} _originalInterface;",
                    "",
                    $"public {className} (ISerializer serializer, IRetryStrategy retryStrategy, IOutboxDataStorageService outboxDataStorageService, {originalInterfaceType} originalInterface) : base(serializer, retryStrategy, outboxDataStorageService)",
                    "{",
                    "_originalInterface = originalInterface;",
                    "}",
                    "",
                    $"protected override async Task<{methodReturnType.ToDisplayString(s_fullyQualifiedTypeDisplayFormat)}> Handle({argumentWrapperModelName} payload)",
                    "{",
                    $"return {awaitIfNeeded}_originalInterface.{method.Name}({string.Join(", ", method.Parameters.Select(x => x.Type.Name == "CancellationToken" ? "default" : $"payload.{x.Name}"))});",
                    "}",
                    "}");
            }
            else
            {
                cb.WriteCode(
                    $"public class {className} : OutboxActionHandlerBase<{argumentWrapperModelName}>",
                    "{",
                    $"private readonly {originalInterfaceType} _originalInterface;",
                    "",
                    $"public {className} (ISerializer serializer, IRetryStrategy retryStrategy, IOutboxDataStorageService outboxDataStorageService, {originalInterfaceType} originalInterface) : base(serializer, retryStrategy, outboxDataStorageService)",
                    "{",
                    "_originalInterface = originalInterface;",
                    "}",
                    "",
                    $"protected override async Task Handle({argumentWrapperModelName} payload)",
                    "{",
                    $"{awaitIfNeeded}_originalInterface.{method.Name}({string.Join(", ", method.Parameters.Select(x => x.Type.Name == "CancellationToken" ? "default" : $"payload.{x.Name}"))});",
                    "}",
                    "}");
            }
        }

        private string GetMethodArgumentsWrapperModelName(IMethodSymbol method)
        {
            return $"{method.Name}ArgumentsWrapperModel";
        }

        private string GetMethodHandlerClassName(IMethodSymbol method)
        {
            return $"{method.Name}Handler";
        }

        private string GetOutboxActionName(ITypeSymbol type, IMethodSymbol method)
        {
            return $"{type.Name}.{method.Name}";
        }

        private string DumpException(Exception ex)
        {
            var sb = new StringBuilder();

            while (ex != null)
            {
                sb.AppendLine($"========{ex.GetType()}");
                sb.AppendLine(ex.Message);
                sb.AppendLine(ex.StackTrace);
                ex = ex.InnerException;
            }

            return sb.ToString();
        }

        private bool IsTask(ITypeSymbol type)
        {
            return type.ToDisplayString(s_fullyQualifiedTypeDisplayFormat).StartsWith("System.Threading.Tasks.Task");
        }
    }
}