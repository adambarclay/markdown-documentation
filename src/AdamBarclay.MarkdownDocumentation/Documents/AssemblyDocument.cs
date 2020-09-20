using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace AdamBarclay.MarkdownDocumentation.Documents
{
	internal static class AssemblyDocument
	{
		internal static async Task Build(StreamWriter writer, Assembly assembly, IEnumerable<string> typeNamespaces)
		{
			await writer.WriteAsync("# ");
			await writer.WriteAsync(assembly.GetName().Name);
			await writer.WriteLineAsync(".dll");
			await writer.WriteLineAsync();

			await writer.WriteLineAsync("| |");
			await writer.WriteLineAsync("|_|");

			foreach (var typeNamespace in typeNamespaces)
			{
				await writer.WriteAsync("|[");
				await writer.WriteAsync(typeNamespace);
				await writer.WriteAsync("](");
				await writer.WriteLineAsync(")|");
			}
		}
	}
}
