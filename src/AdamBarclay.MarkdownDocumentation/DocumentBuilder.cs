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
					if (!TypeHelper.TypeIsADelegate(type))
					{
						var constructors = type
							.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
							.Where(o => o.IsPublic || o.IsFamily)
							.ToList();

						if (constructors.Count > 0)
						{
							await using (var writer = File.CreateText(
								FileNameHelper.ConstructorFileName(outputPath, type)))
							{
								if (constructors.Count > 1)
								{
									await ConstructorDocument.BuildMultiple(writer, type, constructors, xmlComments);
								}
								else
								{
									await ConstructorDocument.BuildSingle(writer, type, constructors[0], xmlComments);
								}
							}
						}

						foreach (var property in type.GetProperties().Where(o => o.DeclaringType == type))
						{
							await using (var writer =
								File.CreateText(FileNameHelper.PropertyFileName(outputPath, property)))
							{
								await PropertyDocument.Build(writer, property, xmlComments);
							}
						}

						foreach (var methods in type
							.GetMethods(
								BindingFlags.Instance |
								BindingFlags.Static |
								BindingFlags.Public |
								BindingFlags.NonPublic)
							.Where(
								o => o.DeclaringType == type &&
									!o.IsSpecialName &&
									(o.IsPublic || o.IsFamily) &&
									(!o.IsVirtual || (o.Attributes & MethodAttributes.NewSlot) != 0))
							.GroupBy(o => o.Name)
							.Select(o => o.ToList()))
						{
							await using (var writer =
								File.CreateText(FileNameHelper.MethodFileName(outputPath, methods[0])))
							{
								if (methods.Count > 1)
								{
									await MethodDocument.BuildMultiple(writer, assembly, type, methods, xmlComments);
								}
								else
								{
									await MethodDocument.BuildSingle(writer, assembly, type, methods[0], xmlComments);
								}
							}
						}
					}

					await using (var writer = File.CreateText(FileNameHelper.TypeFileName(outputPath, type)))
					{
						if (TypeHelper.TypeIsADelegate(type))
						{
							await DelegateDocument.Build(writer, type, xmlComments);
						}
						else
						{
							await TypeDocument.Build(writer, type, xmlComments);
						}
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
