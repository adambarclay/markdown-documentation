using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using AdamBarclay.MarkdownDocumentation.Helpers;

namespace AdamBarclay.MarkdownDocumentation.Documents
{
	internal static class NamespaceDocument
	{
		internal static async Task Build(
			StreamWriter writer,
			string typeNamespace,
			List<Type> types,
			XDocument xmlComments)
		{
			await writer.WriteAsync("# ");
			await writer.WriteAsync(typeNamespace);
			await writer.WriteLineAsync(" Namespace");
			await writer.WriteLineAsync();

			await writer.WriteLineAsync("| | |");
			await writer.WriteLineAsync("|_|_|");

			foreach (var type in types)
			{
				await writer.WriteAsync("|[");
				await writer.WriteAsync(TypeHelper.FullNameEncoded(type));
				await writer.WriteAsync("](");
				await writer.WriteAsync(FileNameHelper.TypeFileName(string.Empty, type));
				await writer.WriteAsync(")|");
				await writer.WriteAsync(XmlCommentHelper.Summary(XmlCommentHelper.TypeElement(xmlComments, type)));
				await writer.WriteLineAsync("|");
			}

			await writer.WriteLineAsync();
		}
	}
}
