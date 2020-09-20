using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using AdamBarclay.MarkdownDocumentation.Helpers;

namespace AdamBarclay.MarkdownDocumentation.Documents
{
	internal static class PropertyDocument
	{
		internal static async Task Build(StreamWriter writer, PropertyInfo property, XDocument xmlComments)
		{
			var type = property.DeclaringType;

			await DocumentHelpers.PageTitle(
				writer,
				async writer =>
				{
					await TypeHelper.FullName(writer, type, t => t.Name, "&lt;", "&gt;");
					await writer.WriteAsync(".");
					await writer.WriteAsync(property.Name);
				},
				"Property");

			await DocumentHelpers.PageHeader(writer, type, xmlComments);
		}
	}
}
