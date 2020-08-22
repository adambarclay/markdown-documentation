using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace AdamBarclay.MarkdownDocumentation.Helpers
{
	internal static class XmlCommentHelper
	{
		internal static string GenericArgument(XDocument xmlComments, Type genericArgumentType)
		{
			return xmlComments.Root?.Descendants("member")
					.FirstOrDefault(
						o => o.Attribute("name")?.Value == $"T:{genericArgumentType.DeclaringType?.FullName}")
					?.Descendants("typeparam")
					.FirstOrDefault(o => o.Attribute("name")?.Value == genericArgumentType.Name)
					?.Value ??
				string.Empty;
		}

		internal static XElement? MethodElement(XDocument xmlComments, MethodBase methodInfo)
		{
			return xmlComments.Root?.Descendants("member")
				.FirstOrDefault(o => XmlCommentHelper.MemberAttribute(o, methodInfo));
		}

		internal static string Property(XDocument xmlComments, PropertyInfo property)
		{
			return xmlComments.Root?.Descendants("member")
					.FirstOrDefault(
						o => o.Attribute("name")?.Value == $"P:{property.DeclaringType?.FullName}.{property.Name}")
					?.Element("summary")
					?.Value ??
				string.Empty;
		}

		internal static XElement? TypeElement(XDocument xmlComments, Type type)
		{
			return xmlComments.Root?.Descendants("member")
				.FirstOrDefault(o => o.Attribute("name")?.Value == $"T:{type.FullName}");
		}

		private static bool MemberAttribute(XElement element, MethodBase methodInfo)
		{
			var stringBuilder = new StringBuilder(256);

			stringBuilder.Append("M:");
			stringBuilder.Append(methodInfo.DeclaringType?.FullName);
			stringBuilder.Append('.');
			stringBuilder.Append(methodInfo.Name.Replace('.', '#'));

			var parameters = methodInfo.GetParameters();

			if (parameters.Length > 0)
			{
				// TODO - generic parameters
				stringBuilder.Append('(');
				stringBuilder.Append(parameters[0].ParameterType.FullName);

				for (var i = 1; i < parameters.Length; ++i)
				{
					stringBuilder.Append(',');
					stringBuilder.Append(parameters[i].ParameterType.FullName);
				}

				stringBuilder.Append(')');
			}

			return element.Attribute("name")?.Value == stringBuilder.ToString();
		}
	}
}
