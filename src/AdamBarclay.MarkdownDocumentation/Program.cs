using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AdamBarclay.MarkdownDocumentation
{
	internal static class Program
	{
		private static async Task Main(string[] arguments)
		{
			string assemblyPath;
			string xmlFilePath;
			string outputPath;
			string? assemblyDirectoryName;

			try
			{
				assemblyPath = Path.GetFullPath(arguments.Length > 0 ? arguments[0] : string.Empty);
				xmlFilePath = Path.ChangeExtension(assemblyPath, "xml");
				outputPath = Path.GetFullPath(arguments.Length > 1 ? arguments[1] : string.Empty);

				assemblyDirectoryName = Path.GetDirectoryName(assemblyPath);

				if (assemblyDirectoryName == null)
				{
					throw new Exception("Invalid assembly path.");
				}
			}
			catch
			{
				Console.WriteLine($"Usage: {Assembly.GetExecutingAssembly().GetName().Name} assembly-path output-path");

				return;
			}

			var assemblies = new List<string>();

			assemblies.AddRange(Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll"));
			assemblies.AddRange(Directory.GetFiles(assemblyDirectoryName, "*.dll"));

			using (var context = new MetadataLoadContext(new PathAssemblyResolver(assemblies)))
			{
				Assembly assembly;

				try
				{
					assembly = context.LoadFromAssemblyPath(assemblyPath);
				}
				catch
				{
					Console.WriteLine($"Unable to load assembly \"{assemblyPath}\".");

					return;
				}

				XDocument xmlComments;

				try
				{
					using (var file = File.OpenText(xmlFilePath))
					{
						xmlComments = await XDocument.LoadAsync(file, LoadOptions.None, CancellationToken.None);
					}
				}
				catch
				{
					xmlComments = new XDocument();
				}

				try
				{
					Directory.CreateDirectory(outputPath);
				}
				catch
				{
					Console.WriteLine($"Unable to create output directory \"{outputPath}\".");

					return;
				}

				await DocumentBuilder.Build(assembly, xmlComments, outputPath);
			}
		}
	}
}
