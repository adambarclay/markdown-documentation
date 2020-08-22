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

		internal static string NamespaceFileName(string outputPath, string? typeNamespace)
		{
			return Path.Combine(outputPath, $"{typeNamespace?.ToLowerInvariant() ?? "empty-namespace"}.md");
		}

		internal static string TypeFileName(string outputPath, Type type)
		{
			return Path.Combine(outputPath, $"{type.FullName?.Replace('`', '-').ToLowerInvariant()}.md");
		}
	}
}
