using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Ara3D.Utils.Roslyn
{
    public static class RoslynAnalysisUtils
    {
        public static IEnumerable<IMethodSymbol> GetMethods(this INamedTypeSymbol type)
            => type.GetMembers().OfType<IMethodSymbol>();

        public static IEnumerable<IMethodSymbol> GetExtensionMethods(this INamedTypeSymbol type)
            => type.GetMethods().Where(m => m.IsExtensionMethod);

        public static INamedTypeSymbol GetTypeByMetaDataName(this Compilation compilation, string name)
            => compilation.Compiler.GetTypeByMetadataName(name);

        public static IEnumerable<ISymbol> GetNamedSymbolDeclarations(this Compilation compilation, string name)
            => compilation.Compiler.GetSymbolsWithName(name);

        public static IEnumerable<INamespaceOrTypeSymbol> GetAllLinkedNamespacesAndTypes(this Compilation compilation)
            => compilation.Compiler.GlobalNamespace.GetAllNested();

        public static IEnumerable<INamespaceOrTypeSymbol> GetAllNested(this INamespaceOrTypeSymbol symbol)
        {
            yield return symbol;
            foreach (var child in symbol
                         .GetMembers()
                         .OfType<INamespaceOrTypeSymbol>()
                         .SelectMany(GetAllNested))
            {
                yield return child;
            }
        }

        public static IEnumerable<INamespaceSymbol> GetAllLinkedNamespaces(this Compilation compilation)
            => compilation.GetAllLinkedNamespacesAndTypes().OfType<INamespaceSymbol>();

        public static IEnumerable<ITypeSymbol> GetAllLinkedTypes(this Compilation compilation)
            => compilation.GetAllLinkedNamespacesAndTypes().OfType<ITypeSymbol>();

        public static IEnumerable<ICompilationUnitSyntax> GetCompilationUnits(this Compilation compilation)
            => compilation.Input.SyntaxTrees.Select(st => st.GetRoot() as CompilationUnitSyntax);
        
        public static IEnumerable<(TypeDeclarationSyntax, INamedTypeSymbol)> GetTypeDeclarationsWithSymbols(
            this Compilation compilation)
        {
            foreach (var st in compilation.Input.SyntaxTrees)
            {
                var model = compilation.Compiler.GetSemanticModel(st);
                foreach (var n in st.GetRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>())
                {
                    yield return (n, model.GetDeclaredSymbol(n));
                }
            }
        }

        public static IEnumerable<ISymbol> GetDeclaredAndBaseMembers(this ITypeSymbol sym)
        {
            foreach (var m in sym.GetMembers())
                yield return m;
            if (sym.BaseType != null)
                foreach (var m in sym.BaseType.GetDeclaredAndBaseMembers())
                    yield return m;
        }

        public static IEnumerable<ITypeSymbol> GetFieldTypes(this ITypeSymbol sym)
            => sym.GetDeclaredAndBaseMembers().OfType<IFieldSymbol>().Select(f => f.Type);

        public static IEnumerable<ISymbol> GetExpressionAndTypeSymbols(this Compilation compilation)
        {
            foreach (var st in compilation.Input.SyntaxTrees)
            {
                var model = compilation.Compiler.GetSemanticModel(st);
                foreach (var node in st.GetRoot().DescendantNodesAndSelf())
                {
                    switch (node)
                    {
                        case StatementSyntax _:
                        case TypeDeclarationSyntax _:
                        case MemberDeclarationSyntax _:
                            continue;
                        default:
                            yield return model.GetSymbolInfo(node).Symbol;
                            break;
                    }
                }
            }
        }

        // https://stackoverflow.com/questions/69636558/determining-if-a-private-field-is-read-using-roslyn
        private static bool IsFieldRead(SyntaxNodeAnalysisContext context, IFieldSymbol fieldSymbol)
        {
            var classDeclarationSyntax = context.Node.Parent;
            while (!(classDeclarationSyntax is ClassDeclarationSyntax))
            {
                classDeclarationSyntax = classDeclarationSyntax.Parent;
                if (classDeclarationSyntax == null)
                {
                    throw new InvalidOperationException("You have somehow traversed up and out of the syntax tree when determining if a private member field is being read.");
                }
            }

            //get all methods in the class
            var methodsInClass = classDeclarationSyntax.DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (var method in methodsInClass)
            {
                //get all member references in those methods
                if (context.SemanticModel.GetOperation(method).Descendants().OfType<IMemberReferenceOperation>().Any(x => x.Member.Equals(fieldSymbol)))
                {
                    return true;
                }
            }

            return false;
        }

        // https://stackoverflow.com/questions/44142421/roslyn-check-if-field-declaration-has-been-assigned-to
        // https://stackoverflow.com/questions/64009302/roslyn-c-how-to-get-all-fields-and-properties-and-their-belonging-class-acce/64014939#64014939
        public static IEnumerable<IMemberReferenceOperation> GetMemberReferenceOperations(SemanticModel model,
            SyntaxNode node)
        {
            return model.GetOperation(node).Descendants().OfType<IMemberReferenceOperation>();
        }

        public static FieldDeclarationSyntax GetSyntax(this IFieldSymbol fieldSymbol)
            => fieldSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().Parent?.Parent as FieldDeclarationSyntax;

        public static PropertyDeclarationSyntax GetSyntax(this IPropertySymbol propertySymbol)
            => propertySymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().Parent?.Parent as PropertyDeclarationSyntax;

        public static MethodDeclarationSyntax GetSyntax(this IMethodSymbol methodSymbol)
            => methodSymbol.DeclaringSyntaxReferences.FirstOrDefault().GetSyntax() as MethodDeclarationSyntax;

        public static bool IsPublicMember(this SyntaxNode node)
            => node is MemberDeclarationSyntax mds
               && mds.Modifiers.Any(st => st.Kind() == SyntaxKind.PublicKeyword);

        // https://stackoverflow.com/questions/30300753/how-to-detect-closures-in-code-with-roslyn
        public static IEnumerable<ISymbol> GetCapturedVariables(this SemanticModel model, AnonymousFunctionExpressionSyntax lambda)
            => model.AnalyzeDataFlow(lambda)?.Captured ?? Enumerable.Empty<ISymbol>();

        public static IEnumerable<AnonymousFunctionExpressionSyntax> GetLambdas(this SyntaxNode root)
            => root.DescendantNodesAndSelf().OfType<AnonymousFunctionExpressionSyntax>();

        // https://www.meziantou.net/checking-if-a-property-is-an-auto-implemented-property-in-roslyn.htm
        public static bool IsAutoProperty(this IPropertySymbol propertySymbol)
        {
            // Get fields declared in the same type as the property
            var fields = propertySymbol.ContainingType.GetMembers().OfType<IFieldSymbol>();

            // Check if one field is associated to
            return fields.Any(field => SymbolEqualityComparer.Default.Equals(field.AssociatedSymbol, propertySymbol));
        }

        public static ITypeSymbol GetTypeSymbol(this SemanticModel model, ISymbol symbol)
        {
            if (symbol is IParameterSymbol ps)
                return ps.Type;
            if (symbol is IFieldSymbol fs)
                return fs.Type;
            if (symbol is ILocalSymbol ls)
                return ls.Type;
            if (symbol is IPropertySymbol prop)
                return prop.Type;
            var syntaxReference = symbol.DeclaringSyntaxReferences.FirstOrDefault();
            var node = syntaxReference.GetSyntax();
            return model.GetTypeInfo(node).Type;
        }

        public static IEnumerable<SyntaxNode> GetAllNodes(this Compilation compilation)
            => compilation.Input.SyntaxTrees.SelectMany(st => st.GetRoot().DescendantNodesAndSelf());

        public static IEnumerable<(SemanticModel, SyntaxTree)> GetModelsAndTrees(this Compilation compilation)
        {
            for (var i = 0; i < compilation.Input.SourceFiles.Count; i++)
            {
                yield return (compilation.SemanticModels[i], compilation.Input.SourceFiles[i].SyntaxTree);
            }
        }

        public static SyntaxNode GetAssociatedStatementOrExpression(this SyntaxNode node)
        {
            if (node is StatementSyntax)
                return node;
            
            if (node is ExpressionSyntax)
                return node;
            
            if (node is ArrowExpressionClauseSyntax aecs)
                return aecs.Expression;

            if (node is PropertyDeclarationSyntax pds)
            {
                if (pds.ExpressionBody != null)
                    return pds.ExpressionBody.GetAssociatedStatementOrExpression();
            }

            if (node is BaseMethodDeclarationSyntax mds)
            {
                if (mds.Body != null)
                    return mds.Body;
                if (mds.ExpressionBody != null)
                    return mds.ExpressionBody.GetAssociatedStatementOrExpression();
            }

            return null;
        }

        public static DataFlowAnalysis GetDataFlowAnalysis(this SemanticModel model, SyntaxNode node)
        {
            if (node is ConstructorInitializerSyntax cis)
                return model.AnalyzeDataFlow(cis);
            if (node is StatementSyntax ss)
                return model.AnalyzeDataFlow(ss);
            if (node is ExpressionSyntax es)
                return model.AnalyzeDataFlow(es);
            if (node is PrimaryConstructorBaseTypeSyntax pcsbts)
                return model.AnalyzeDataFlow(pcsbts);

            if (node is ArrowExpressionClauseSyntax expression)
                return model.GetDataFlowAnalysis(expression.Expression);

            if (node is BaseMethodDeclarationSyntax mds)
            {
                if (mds.Body != null)
                    return model.GetDataFlowAnalysis(mds.Body);
                if (mds.ExpressionBody != null)
                    return model.GetDataFlowAnalysis(mds.ExpressionBody);
            }

            if (node is PropertyDeclarationSyntax pds)
            {
                throw new Exception("Must analyze data flow of property getters and setters separately");
            }
            
            return null;
        }
    }
}