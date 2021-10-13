using DougMurphy.SourceGenerators.ArgumentNullException.Attributes;
using System;

namespace SourceGeneratorDemonstrator {
	internal static class Program {
		private static void Main(string[] args) {
			DemonstrateArgumentNullExceptionSourceGeneration("Foo", "Bar");
			DemonstrateArgumentNullExceptionSourceGeneration(null, null);
		}

		private static void DemonstrateArgumentNullExceptionSourceGeneration([GenerateThrowException] string parameter1, string parameter2) {
			Console.WriteLine($"The value specified for {nameof(parameter1)} is {parameter1} and {nameof(parameter2)} is {parameter2}");
		}
	}
}
