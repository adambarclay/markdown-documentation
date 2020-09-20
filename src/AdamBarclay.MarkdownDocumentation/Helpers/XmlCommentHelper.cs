using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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
			var nameAttributeValue = XmlCommentHelper.MemberNameAttribute(methodInfo);

			return xmlComments.Root?.Descendants("member")
				.FirstOrDefault(o => o.Attribute("name")?.Value == nameAttributeValue);
		}

		internal static string Parameter(XDocument xmlComments, MethodBase method, ParameterInfo parameter)
		{
			return XmlCommentHelper.MethodElement(xmlComments, method)
					?.Descendants("param")
					.FirstOrDefault(el => el.Attribute("name")?.Value == parameter.Name)
					?.Value ??
				string.Empty;
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

		internal static string Returns(XDocument xmlComments, MethodInfo method)
		{
			return XmlCommentHelper.MethodElement(xmlComments, method)
					?.Descendants("returns")
					.FirstOrDefault()
					?.Value ??
				string.Empty;
		}

		internal static string Summary(XElement? node)
		{
			if (node != null)
			{
				var summary = node.Element("summary");

				if (summary != null)
				{
					return summary.Value;
				}

				var inherit = node.Element("inheritdoc");

				if (inherit != null)
				{
					return "inheritdoc";
				}
			}

			return string.Empty;
		}

		internal static XElement? TypeElement(XDocument xmlComments, Type type)
		{
			return xmlComments.Root?.Descendants("member")
				.FirstOrDefault(o => o.Attribute("name")?.Value == $"T:{type.FullName}");
		}

		internal static string TypeParameter(XDocument xmlComments, MethodInfo method, Type genericArgument)
		{
			return XmlCommentHelper.MethodElement(xmlComments, method)
					?.Descendants("typeparam")
					.FirstOrDefault(el => el.Attribute("name")?.Value == genericArgument.Name)
					?.Value ??
				string.Empty;
		}

		internal static async Task WriteValue(StreamWriter writer, XElement value)
		{
			foreach (var node in value.Nodes())
			{
				if (node.NodeType == XmlNodeType.Element)
				{
					var element = (XElement)node;

					if (element.Name == "paramref" || element.Name == "typeparamref")
					{
						await writer.WriteAsync("`");
						await writer.WriteAsync(element.Attributes("name").FirstOrDefault()?.Value);
						await writer.WriteAsync("`");
					}
					else if (element.Name == "see")
					{
						await writer.WriteAsync("`");
						await writer.WriteAsync(element.Attributes("cref").FirstOrDefault()?.Value);
						await writer.WriteAsync(element.Attributes("langword").FirstOrDefault()?.Value);
						await writer.WriteAsync("`");
					}
				}
				else
				{
					await writer.WriteAsync(node.ToString());
				}
			}
		}

		private static string MemberNameAttribute(MethodBase methodInfo)
		{
			var stringBuilder = new StringBuilder(256);

			stringBuilder.Append("M:");
			stringBuilder.Append(methodInfo.DeclaringType?.FullName);
			stringBuilder.Append('.');
			stringBuilder.Append(methodInfo.Name.Replace('.', '#'));

			Type[]? genericTypeArguments = null;

			if (methodInfo.DeclaringType != null && methodInfo.DeclaringType.IsGenericTypeDefinition)
			{
				genericTypeArguments = methodInfo.DeclaringType.GetGenericArguments();
			}

			Type[]? genericMethodArguments = null;

			if (methodInfo.IsGenericMethodDefinition)
			{
				genericMethodArguments = methodInfo.GetGenericArguments();

				stringBuilder.Append("``");
				stringBuilder.Append(methodInfo.GetGenericArguments().Length);
			}

			var parameters = methodInfo.GetParameters();

			if (parameters.Length > 0)
			{
				stringBuilder.Append('(');

				stringBuilder.Append(
					XmlCommentHelper.ReplaceGenericMethodParameter(
						genericTypeArguments,
						genericMethodArguments,
						parameters[0].ParameterType));

				for (var i = 1; i < parameters.Length; ++i)
				{
					stringBuilder.Append(',');

					stringBuilder.Append(
						XmlCommentHelper.ReplaceGenericMethodParameter(
							genericTypeArguments,
							genericMethodArguments,
							parameters[i].ParameterType));
				}

				stringBuilder.Append(')');
			}

			return stringBuilder.ToString();
		}

		private static string? ReplaceGenericMethodParameter(
			Type[]? genericTypeArguments,
			Type[]? genericMethodArguments,
			Type parameterType)
		{
			if (parameterType.IsGenericType)
			{
				var parameterGenericArguments = parameterType.GetGenericArguments();

				var stringBuilder = new StringBuilder(256);

				stringBuilder.Append(parameterType.Namespace);

				if (!string.IsNullOrEmpty(parameterType.Namespace))
				{
					stringBuilder.Append('.');
				}

				stringBuilder.Append(parameterType.Name.Split('`')[0]);
				stringBuilder.Append('{');

				stringBuilder.Append(
					XmlCommentHelper.ReplaceGenericMethodParameter(
						genericTypeArguments,
						genericMethodArguments,
						parameterGenericArguments[0]));

				for (var i = 1; i < parameterGenericArguments.Length; ++i)
				{
					stringBuilder.Append(',');

					stringBuilder.Append(
						XmlCommentHelper.ReplaceGenericMethodParameter(
							genericTypeArguments,
							genericMethodArguments,
							parameterGenericArguments[i]));
				}

				stringBuilder.Append('}');

				return stringBuilder.ToString();
			}

			if (parameterType.FullName == null)
			{
				if (genericTypeArguments != null)
				{
					for (var i = 0; i < genericTypeArguments.Length; ++i)
					{
						if (genericTypeArguments[i].Name == parameterType.Name)
						{
							return "`" + i;
						}
					}
				}

				if (genericMethodArguments != null)
				{
					for (var i = 0; i < genericMethodArguments.Length; ++i)
					{
						if (genericMethodArguments[i].Name == parameterType.Name)
						{
							return "``" + i;
						}
					}
				}
			}

			return parameterType.FullName;
		}
	}
}
