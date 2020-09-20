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
			if (arguments.Length != 2 ||
				arguments[0].AsSpan().Trim().Length == 0 ||
				arguments[1].AsSpan().Trim().Length == 0)
			{
				Program.Usage();

				return;
			}

			string assemblyPath;
			string xmlFilePath;

			try
			{
				assemblyPath = Path.GetFullPath(arguments[0]);
				xmlFilePath = Path.ChangeExtension(assemblyPath, "xml");
			}
			catch
			{
				Console.WriteLine("assembly-path \"" + arguments[0] + "\" is not valid.");
				Console.WriteLine();

				Program.Usage();

				return;
			}

			var assemblyDirectoryName = Path.GetDirectoryName(assemblyPath);

			if (string.IsNullOrEmpty(assemblyDirectoryName))
			{
				Console.WriteLine("assembly-path \"" + arguments[0] + "\" is not valid.");
				Console.WriteLine();

				Program.Usage();

				return;
			}

			string outputPath;

			try
			{
				outputPath = Path.GetFullPath(arguments[1]);
			}
			catch
			{
				Console.WriteLine("output-path \"" + arguments[1] + "\" is not valid.");
				Console.WriteLine();

				Program.Usage();

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

					foreach (var childAssembly in assembly.GetReferencedAssemblies())
					{
						context.LoadFromAssemblyName(childAssembly);
					}
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
						xmlComments = await XDocument.LoadAsync(
							file,
							LoadOptions.PreserveWhitespace,
							CancellationToken.None);
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

		private static void Usage()
		{
			Console.WriteLine($"Usage: {Assembly.GetExecutingAssembly().GetName().Name} assembly-path output-path");
			Console.WriteLine();

			Console.WriteLine(
				"assembly-path\tThe path to the assembly being documented, e.g. my-project\\bin\\Release\\net5.0\\MyProject.dll");

			Console.WriteLine();

			Console.WriteLine(
				"output-path\tThe path to the directory where the generated markdown files will be written, e.g. ../../docs");
		}
	}
}
