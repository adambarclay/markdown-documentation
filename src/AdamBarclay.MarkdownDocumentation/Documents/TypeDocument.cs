using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using AdamBarclay.MarkdownDocumentation.Helpers;

namespace AdamBarclay.MarkdownDocumentation.Documents
{
	internal static class TypeDocument
	{
		internal static async Task Build(StreamWriter writer, Type type, XDocument xmlComments)
		{
			var typeName = TypeHelper.FullName(type);
			var typeNameEncoded = TypeHelper.FullNameEncoded(type);

			await TypeDocument.PageTitle(writer, typeNameEncoded, TypeHelper.TypeTypeTitle(type));
			await TypeDocument.Namespace(writer, type);
			await TypeDocument.Assembly(writer, type);
			await TypeDocument.Description(writer, type, xmlComments);
			await TypeDocument.Signature(writer, type, typeName);
			await TypeDocument.TypeParameters(writer, type, xmlComments);
			await TypeDocument.Inheritance(writer, type, typeName);

			if (TypeHelper.TypeIsADelegate(type))
			{
			}
			else
			{
				await TypeDocument.Implements(writer, type);

				// TODO - Derived
				// TODO - Attributes
				await TypeDocument.Constructors(writer, type, typeName, xmlComments);
				await TypeDocument.Properties(writer, type, xmlComments);
				await TypeDocument.Methods(writer, type, xmlComments);

				// TODO - Operators
				// TODO - Explicit Interface Implementations
				// TODO - Extension Methods
			}
		}

		private static async Task Assembly(StreamWriter writer, Type type)
		{
			await writer.WriteLineAsync($"Assembly: {Path.GetFileName(type.Assembly.Location)}");
			await writer.WriteLineAsync();
		}

		private static async Task Constructors(StreamWriter writer, Type type, string typeName, XDocument xmlComments)
		{
			var constructors = type.GetConstructors();

			if (constructors.Length > 0)
			{
				await writer.WriteLineAsync("### Constructors");
				await writer.WriteLineAsync("| | |");
				await writer.WriteLineAsync("|_|_|");

				foreach (var constructor in constructors)
				{
					await writer.WriteAsync(typeName);
					await writer.WriteAsync("(");

					var parameters = constructor.GetParameters();

					if (parameters.Length > 0)
					{
						await writer.WriteAsync(TypeHelper.FullNameEncoded(parameters[0].ParameterType));

						for (var i = 1; i < parameters.Length; i++)
						{
							await writer.WriteAsync(", ");
							await writer.WriteAsync(TypeHelper.FullNameEncoded(parameters[i].ParameterType));
						}
					}

					await writer.WriteAsync(")|");

					await writer.WriteLineAsync(
						XmlCommentHelper.MethodElement(xmlComments, constructor)?.Element("summary")?.Value);
				}
			}
		}

		private static async Task Description(StreamWriter writer, Type type, XDocument xmlComments)
		{
			await writer.WriteLineAsync(XmlCommentHelper.TypeElement(xmlComments, type)?.Element("summary")?.Value);
			await writer.WriteLineAsync();
		}

		private static async Task Implements(StreamWriter writer, Type type)
		{
			var interfaces = type.GetInterfaces();

			if (interfaces.Length > 0)
			{
				await writer.WriteLineAsync("#### Implements");
				await writer.WriteLineAsync(string.Join(", ", interfaces.Select(TypeHelper.FullNameEncoded)));
				await writer.WriteLineAsync();
			}
		}

		private static async Task Inheritance(StreamWriter writer, Type type, string typeName)
		{
			await writer.WriteLineAsync("#### Inheritance");

			var path = typeName;

			while (type.BaseType != null)
			{
				type = type.BaseType;

				path = TypeHelper.FullName(type) + " &rarr; " + path;
			}

			await writer.WriteLineAsync(path);
			await writer.WriteLineAsync();
		}

		private static async Task Methods(StreamWriter writer, Type type, XDocument xmlComments)
		{
			var methods = type.GetMethods().Where(o => !o.IsSpecialName).Where(TypeHelper.IgnoreDeclaringType).ToList();

			if (methods.Count > 0)
			{
				await writer.WriteLineAsync("### Methods");
				await writer.WriteLineAsync("| | |");
				await writer.WriteLineAsync("|_|_|");

				foreach (var method in methods)
				{
					await writer.WriteAsync(method.Name);

					if (method.IsGenericMethodDefinition)
					{
						var genericArguments = method.GetGenericArguments();

						await writer.WriteAsync("&lt;");
						await writer.WriteAsync(TypeHelper.FullNameEncoded(genericArguments[0]));

						for (var i = 1; i < genericArguments.Length; ++i)
						{
							await writer.WriteAsync(",");
							await writer.WriteAsync(TypeHelper.FullNameEncoded(genericArguments[i]));
						}

						await writer.WriteAsync("&gt;");
					}

					await writer.WriteAsync("(");

					var parameters = method.GetParameters();

					if (parameters.Length > 0)
					{
						await writer.WriteAsync(TypeHelper.FullNameEncoded(parameters[0].ParameterType));

						for (var i = 1; i < parameters.Length; i++)
						{
							await writer.WriteAsync(", ");
							await writer.WriteAsync(TypeHelper.FullNameEncoded(parameters[i].ParameterType));
						}
					}

					await writer.WriteAsync(")|");

					var summary = XmlCommentHelper.MethodElement(xmlComments, method)?.Element("summary")?.Value ??
						string.Empty;

					await writer.WriteAsync(summary);

					if (method.DeclaringType != type)
					{
						if (summary.Length > 0)
						{
							await writer.WriteAsync("<br/>");
						}

						await writer.WriteAsync("(Inherited from ");
						await writer.WriteAsync(method.DeclaringType?.Name);
						await writer.WriteAsync(")");
					}

					await writer.WriteLineAsync();
				}
			}
		}

		private static async Task Namespace(StreamWriter writer, Type type)
		{
			await writer.WriteAsync("Namespace: [");
			await writer.WriteAsync(type.Namespace ?? "&lt;empty&gt;");
			await writer.WriteAsync("](");
			await writer.WriteAsync(FileNameHelper.NamespaceFileName(string.Empty, type.Namespace));
			await writer.WriteLineAsync(")  ");
		}

		private static async Task PageTitle(StreamWriter writer, string typeName, string typeType)
		{
			await writer.WriteAsync("# ");
			await writer.WriteAsync(typeName);
			await writer.WriteAsync(" ");
			await writer.WriteLineAsync(typeType);
			await writer.WriteLineAsync();
		}

		private static async Task Properties(StreamWriter writer, Type type, XDocument xmlComments)
		{
			var properties = type.GetProperties().Where(TypeHelper.IgnoreDeclaringType).ToList();

			if (properties.Count > 0)
			{
				await writer.WriteLineAsync("### Properties");
				await writer.WriteLineAsync("| | |");
				await writer.WriteLineAsync("|_|_|");

				foreach (var property in properties.OrderBy(o => o.Name))
				{
					await writer.WriteAsync(property.Name);
					await writer.WriteAsync("|");

					var summary = XmlCommentHelper.Property(xmlComments, property);

					await writer.WriteAsync(summary);

					if (property.DeclaringType != type)
					{
						if (summary.Length > 0)
						{
							await writer.WriteAsync("<br/>");
						}

						await writer.WriteAsync("(Inherited from ");
						await writer.WriteAsync(property.DeclaringType?.Name);
						await writer.WriteAsync(")");
					}

					await writer.WriteLineAsync();
				}
			}

			await writer.WriteLineAsync();
		}

		private static async Task Signature(StreamWriter writer, Type type, string typeName)
		{
			await writer.WriteLineAsync("```c#");
			await writer.WriteAsync("    public");
			await writer.WriteAsync(TypeHelper.TypeModifiers(type));
			await writer.WriteAsync(" ");
			await writer.WriteAsync(TypeHelper.TypeType(type));
			await writer.WriteAsync(" ");
			await writer.WriteAsync(typeName);
			await writer.WriteLineAsync(TypeHelper.BaseClasses(type));
			await writer.WriteLineAsync("```");
			await writer.WriteLineAsync();
		}

		private static async Task TypeParameters(StreamWriter writer, Type type, XDocument xmlComments)
		{
			if (type.IsGenericType)
			{
				await writer.WriteLineAsync("#### Type Parameters");
				await writer.WriteLineAsync("| | |");
				await writer.WriteLineAsync("|_|_|");

				foreach (var genericArgument in type.GetGenericArguments())
				{
					await writer.WriteAsync("`");
					await writer.WriteAsync(genericArgument.Name);
					await writer.WriteAsync("`");
					await writer.WriteAsync("|");
					await writer.WriteLineAsync(XmlCommentHelper.GenericArgument(xmlComments, genericArgument));
				}

				await writer.WriteLineAsync();
			}
		}
	}
}
