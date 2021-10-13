using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DougMurphy.SourceGenerators.ArgumentNullException {
	[Generator]
	public class ThrowArgumentNullExceptionGenerator : ISourceGenerator {
		public void Initialize(GeneratorInitializationContext context) {
#if DEBUG
			if (!Debugger.IsAttached) {
				Debugger.Launch();
			}
#endif
		}

		public void Execute(GeneratorExecutionContext context) {
			//find usages of our attribute
			IEnumerable<SyntaxTree> syntaxTrees = context.Compilation.SyntaxTrees;

			foreach (SyntaxTree syntaxTree in syntaxTrees) {
				var methodDeclarations = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().ToImmutableArray();

				foreach (MethodDeclarationSyntax methodDeclaration in methodDeclarations) {
					var parameterNamesToGenerateThrowFor = new List<string>();
					SeparatedSyntaxList<ParameterSyntax> parameters = methodDeclaration.ParameterList.Parameters;
					foreach (ParameterSyntax parameter in parameters.Where(p => p.AttributeLists.Count > 0)) {
						ISymbol parameterSymbol = context.Compilation.GetSemanticModel(parameter.SyntaxTree).GetDeclaredSymbol(parameter);
						if (parameterSymbol.GetAttributes().Any(a => a.AttributeClass.Name == "GenerateThrowException")) {
							parameterNamesToGenerateThrowFor.Add(parameterSymbol.Name);
						}
					}

					foreach (string parameterNameToGenerateThrowFor in parameterNamesToGenerateThrowFor) {
						//generate code
						var generatedCode = $@"
if ({parameterNameToGenerateThrowFor} is null) {{
	throw new System.ArgumentNullException(nameof({parameterNameToGenerateThrowFor}));
}}";
						context.AddSource("ArgumentNullExceptionGenerator", SourceText.From(generatedCode, Encoding.UTF8));
					}
				}
			}
		}
	}
}
