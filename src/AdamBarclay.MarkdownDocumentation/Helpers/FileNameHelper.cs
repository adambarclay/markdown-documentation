using System;
using System.IO;
using System.Reflection;

namespace AdamBarclay.MarkdownDocumentation.Helpers
{
	internal static class FileNameHelper
	{
		internal static string AssemblyFileName(string outputPath, AssemblyName assemblyName)
		{
			var name = assemblyName.Name ?? "unknown-assembly";

			return Path.Combine(
				outputPath,
				$"{name.ToLowerInvariant()}-{assemblyName.Version?.ToString() ?? "0.0.0.0"}.md");
		}

		internal static string ConstructorFileName(string outputPath, Type type)
		{
			return Path.Combine(outputPath, $"{FileNameHelper.NormaliseType(type)}.-ctor.md");
		}

		internal static string MethodFileName(string outputPath, MethodBase method)
		{
			return Path.Combine(
				outputPath,
				$"{FileNameHelper.NormaliseType(method.DeclaringType)}.{method.Name.ToLowerInvariant()}.md");
		}

		internal static string NamespaceFileName(string outputPath, string? typeNamespace)
		{
			return Path.Combine(outputPath, $"{typeNamespace?.ToLowerInvariant() ?? "empty-namespace"}.md");
		}

		internal static string PropertyFileName(string outputPath, PropertyInfo property)
		{
			return Path.Combine(
				outputPath,
				$"{FileNameHelper.NormaliseType(property.DeclaringType)}.{property.Name.Replace('.', '-').ToLowerInvariant()}.md");
		}

		internal static string TypeFileName(string outputPath, Type type)
		{
			return Path.Combine(outputPath, $"{FileNameHelper.NormaliseType(type)}.md");
		}

		private static string NormaliseType(Type type)
		{
			return type.FullName?.Replace('+', '-').Replace('`', '-').ToLowerInvariant() ?? string.Empty;
		}
	}
}
