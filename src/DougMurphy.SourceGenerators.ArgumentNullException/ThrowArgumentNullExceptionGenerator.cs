using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DougMurphy.SourceGenerators.ArgumentNullException {
	[Generator]
	public class ThrowArgumentNullExceptionGenerator : ISourceGenerator {
		private const string ATTRIBUTE_SOURCE_CODE = @"
using System;

namespace DougMurphy.SourceGenerators.ArgumentNullException.Attributes {
	/// <summary>This attribute indicates to the source generator that the value of the parameter this attribute is placed on should be checked for null, and an ArgumentNullException thrown if so.</summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public class GenerateThrowExceptionAttribute : Attribute {
	}
}
";

		public void Initialize(GeneratorInitializationContext context) {
			context.RegisterForPostInitialization(ctx => ctx.AddSource("GenerateThrowExceptionAttribute.g.cs", SourceText.From(ATTRIBUTE_SOURCE_CODE, Encoding.UTF8)));
#if DEBUG
			if (!Debugger.IsAttached) {
				Debugger.Launch();
			}
#endif
		}

		public void Execute(GeneratorExecutionContext context) {
			//find usages of our attribute
			IEnumerable<SyntaxTree> syntaxTrees = context.Compilation.SyntaxTrees;
			INamedTypeSymbol generateThrowExceptionAttribute = context.Compilation.GetTypeByMetadataName("GenerateThrowExceptionAttribute");
			foreach (SyntaxTree syntaxTree in syntaxTrees) {
				var methodDeclarations = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().ToImmutableArray();

				foreach (MethodDeclarationSyntax methodDeclaration in methodDeclarations) {
					var parameterNamesToGenerateThrowFor = new List<string>();
					SeparatedSyntaxList<ParameterSyntax> parameters = methodDeclaration.ParameterList.Parameters;
					foreach (ParameterSyntax parameter in parameters.Where(p => p.AttributeLists.Count > 0)) {
						ISymbol parameterSymbol = ModelExtensions.GetDeclaredSymbol(context.Compilation.GetSemanticModel(parameter.SyntaxTree), parameter);
						if (parameterSymbol.GetAttributes().Any(a => a.AttributeClass.Name == "GenerateThrowExceptionAttribute")) {
							parameterNamesToGenerateThrowFor.Add(parameterSymbol.Name);
						}
					}

					foreach (string parameterNameToGenerateThrowFor in parameterNamesToGenerateThrowFor) {
						//generate code
						var generatedCode = $@"
if ({parameterNameToGenerateThrowFor} is null) {{
	throw new System.ArgumentNullException(nameof({parameterNameToGenerateThrowFor}));
}}";

						SyntaxNode newRoot = Formatter.Format(syntaxTree.GetRoot().ReplaceNode(methodDeclaration, methodDeclaration.AddBodyStatements(SyntaxFactory.ParseStatement(generatedCode))), new AdhocWorkspace());
						// context.AddSource("ArgumentNullExceptionGenerator", SourceText.From(generatedCode, Encoding.UTF8));
					}
				}
			}
		}
	}
}
