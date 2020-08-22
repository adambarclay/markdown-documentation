using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using AdamBarclay.MarkdownDocumentation.Documents;
using AdamBarclay.MarkdownDocumentation.Helpers;

namespace AdamBarclay.MarkdownDocumentation
{
	internal static class DocumentBuilder
	{
		internal static async Task Build(Assembly assembly, XDocument xmlComments, string outputPath)
		{
			var exportedTypesByNamespace = DocumentBuilder.ExportedTypesByNamespace(assembly);

			foreach ((var typeNamespace, var types) in exportedTypesByNamespace)
			{
				foreach (var type in types)
				{
					// TODO - members files
					await using (var writer = File.CreateText(FileNameHelper.TypeFileName(outputPath, type)))
					{
						await TypeDocument.Build(writer, type, xmlComments);
					}
				}

				await using (var writer = File.CreateText(FileNameHelper.NamespaceFileName(outputPath, typeNamespace)))
				{
					await NamespaceDocument.Build(writer, typeNamespace, types, xmlComments);
				}
			}

			if (exportedTypesByNamespace.Count > 1)
			{
				await using (var writer =
					File.CreateText(FileNameHelper.AssemblyFileName(outputPath, assembly.GetName())))
				{
					await AssemblyDocument.Build(
						writer,
						assembly,
						exportedTypesByNamespace.Select(o => o.TypeNamespace));
				}
			}
		}

		private static List<(string TypeNamespace, List<Type> Types)> ExportedTypesByNamespace(Assembly assembly)
		{
			var types = new Dictionary<string, List<Type>>();

			foreach (var type in assembly.ExportedTypes)
			{
				var typeNamespace = type.Namespace ?? string.Empty;

				if (!types.TryGetValue(typeNamespace, out var typeList))
				{
					types.Add(typeNamespace, typeList = new List<Type>());
				}

				typeList.Add(type);
			}

			foreach (var typeList in types.Values)
			{
				typeList.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
			}

			return new List<(string Namespace, List<Type> Types)>(
				types.OrderBy(o => o.Key).Select(o => (o.Key, o.Value)));
		}
	}
}
