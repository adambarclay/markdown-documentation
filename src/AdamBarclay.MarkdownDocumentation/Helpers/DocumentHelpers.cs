using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AdamBarclay.MarkdownDocumentation.Helpers
{
	internal static class DocumentHelpers
	{
		internal static async Task PageHeader(StreamWriter writer, Type type, XDocument xmlComments)
		{
			await writer.WriteAsync("Namespace: [");
			await writer.WriteAsync(type.Namespace ?? "&lt;empty&gt;");
			await writer.WriteAsync("](");
			await writer.WriteAsync(FileNameHelper.NamespaceFileName(string.Empty, type.Namespace));
			await writer.WriteLineAsync(")  ");

			await writer.WriteLineAsync($"Assembly: {Path.GetFileName(type.Assembly.Location)}");
			await writer.WriteLineAsync();
		}

		internal static async Task PageTitle(StreamWriter writer, Func<StreamWriter, Task> name, string type)
		{
			await writer.WriteAsync("# ");
			await name(writer);
			await writer.WriteAsync(" ");
			await writer.WriteLineAsync(type);
			await writer.WriteLineAsync();
		}
	}
}
