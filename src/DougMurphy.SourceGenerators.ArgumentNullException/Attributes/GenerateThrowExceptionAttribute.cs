using System;

namespace DougMurphy.SourceGenerators.ArgumentNullException.Attributes {
	/// <summary>This attribute indicates to the source generator that the value of the parameter this attribute is placed on should be checked for null, and an ArgumentNullException thrown if so.</summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public class GenerateThrowExceptionAttribute : Attribute {
	}
}
