using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using AdamBarclay.MarkdownDocumentation.Helpers;

namespace AdamBarclay.MarkdownDocumentation.Documents
{
	internal static class ConstructorDocument
	{
		internal static async Task BuildMultiple(
			StreamWriter writer,
			Type type,
			IEnumerable<ConstructorInfo> constructors,
			XDocument xmlComments)
		{
			await DocumentHelpers.PageTitle(
				writer,
				async w =>
				{
					await TypeHelper.FullName(w, type, t => t.Name, "&lt;", "&gt;");
				},
				"Constructors");

			await DocumentHelpers.PageHeader(writer, type, xmlComments);
		}

		internal static async Task BuildSingle(
			StreamWriter writer,
			Type type,
			ConstructorInfo constructor,
			XDocument xmlComments)
		{
			await DocumentHelpers.PageTitle(
				writer,
				async w =>
				{
					await TypeHelper.FullName(w, type, t => t.Name, "&lt;", "&gt;");
					await w.WriteAsync(".");
					await MethodHelper.MethodName(w, constructor, t => t.Name, "&lt;", "&gt;");
					await MethodHelper.MethodParameterTypes(w, constructor, t => t.Name, "&lt;", "&gt;");
				},
				"Constructor");

			await DocumentHelpers.PageHeader(writer, type, xmlComments);
		}
	}
}
