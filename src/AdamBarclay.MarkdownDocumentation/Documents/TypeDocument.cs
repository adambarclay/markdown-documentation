using System;
using System.Collections.Generic;
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
			await DocumentHelpers.PageTitle(
				writer,
				async w => await TypeHelper.FullName(w, type, t => t.Name, "&lt;", "&gt;"),
				TypeHelper.TypeTypeTitle(type));

			await DocumentHelpers.PageHeader(writer, type, xmlComments);

			await writer.WriteLineAsync(XmlCommentHelper.Summary(XmlCommentHelper.TypeElement(xmlComments, type)));
			await writer.WriteLineAsync();

			await TypeDocument.Signature(writer, type);
			await TypeDocument.TypeParameters(writer, type, xmlComments);
			await TypeDocument.Inheritance(writer, type);

			await TypeDocument.Implements(writer, type);

			// TODO - Derived
			// TODO - Attributes
			await TypeDocument.Constructors(writer, type, xmlComments);
			await TypeDocument.Properties(writer, type, xmlComments);
			await TypeDocument.Methods(writer, type, xmlComments);

			// TODO - Operators
			// TODO - Explicit Interface Implementations
			// TODO - Extension Methods
		}

		private static async Task Constructors(StreamWriter writer, Type type, XDocument xmlComments)
		{
			var constructors = type.GetConstructors();

			if (constructors.Length > 0)
			{
				await writer.WriteLineAsync("### Constructors");
				await writer.WriteLineAsync("| | |");
				await writer.WriteLineAsync("|_|_|");

				foreach (var constructor in constructors)
				{
					await writer.WriteAsync("[");

					await TypeHelper.FullName(writer, type, t => t.Name, "&lt;", "&gt;");

					await writer.WriteAsync("(");

					var parameters = constructor.GetParameters();

					if (parameters.Length > 0)
					{
						await TypeHelper.FullName(writer, parameters[0].ParameterType, t => t.Name, "&lt;", "&gt;");

						for (var i = 1; i < parameters.Length; i++)
						{
							await writer.WriteAsync(", ");
							await TypeHelper.FullName(writer, parameters[i].ParameterType, t => t.Name, "&lt;", "&gt;");
						}
					}

					await writer.WriteAsync(")](");
					await writer.WriteAsync(FileNameHelper.ConstructorFileName(string.Empty, type));
					await writer.WriteAsync(")|");

					await writer.WriteLineAsync(
						XmlCommentHelper.Summary(XmlCommentHelper.MethodElement(xmlComments, constructor)));
				}
			}
		}

		private static async Task Implements(StreamWriter writer, Type type)
		{
			var interfaces = type.GetInterfaces();

			if (interfaces.Length > 0)
			{
				await writer.WriteLineAsync("#### Implements");

				await TypeHelper.FullName(writer, interfaces[0], t => t.Name, "&lt;", "&gt;");

				for (var i = 1; i < interfaces.Length; ++i)
				{
					await writer.WriteAsync(", ");
					await TypeHelper.FullName(writer, interfaces[i], t => t.Name, "&lt;", "&gt;");
				}

				await writer.WriteLineAsync();
			}
		}

		private static async Task Inheritance(StreamWriter writer, Type type)
		{
			await writer.WriteLineAsync("#### Inheritance");

			var stack = new Stack<Type>();

			var baseType = type;

			while (baseType.BaseType != null)
			{
				stack.Push(baseType.BaseType);

				baseType = baseType.BaseType;
			}

			while (stack.Count > 0)
			{
				await TypeHelper.FullName(writer, stack.Pop(), t => t.Name, "&lt;", "&gt;");

				await writer.WriteAsync(" &rarr; ");
			}

			await TypeHelper.FullName(writer, type, t => t.Name, "&lt;", "&gt;");

			await writer.WriteLineAsync();
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
					await writer.WriteAsync("[");
					await writer.WriteAsync(method.Name);

					if (method.IsGenericMethodDefinition)
					{
						var genericArguments = method.GetGenericArguments();

						await writer.WriteAsync("&lt;");
						await TypeHelper.FullName(writer, genericArguments[0], t => t.Name, "&lt;", "&gt;");

						for (var i = 1; i < genericArguments.Length; ++i)
						{
							await writer.WriteAsync(",");
							await TypeHelper.FullName(writer, genericArguments[i], t => t.Name, "&lt;", "&gt;");
						}

						await writer.WriteAsync("&gt;");
					}

					await writer.WriteAsync("(");

					var parameters = method.GetParameters();

					if (parameters.Length > 0)
					{
						await TypeHelper.FullName(writer, parameters[0].ParameterType, t => t.Name, "&lt;", "&gt;");

						for (var i = 1; i < parameters.Length; i++)
						{
							await writer.WriteAsync(", ");
							await TypeHelper.FullName(writer, parameters[i].ParameterType, t => t.Name, "&lt;", "&gt;");
						}
					}

					await writer.WriteAsync(")](");
					await writer.WriteAsync(FileNameHelper.MethodFileName(string.Empty, method));
					await writer.WriteAsync(")|");

					var summary = XmlCommentHelper.Summary(XmlCommentHelper.MethodElement(xmlComments, method));

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
					await writer.WriteAsync("[");
					await writer.WriteAsync(property.Name);
					await writer.WriteAsync("](");
					await writer.WriteAsync(FileNameHelper.PropertyFileName(string.Empty, property));
					await writer.WriteAsync(")|");

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

		private static async Task Signature(StreamWriter writer, Type type)
		{
			await writer.WriteLineAsync("```c#");
			await writer.WriteAsync("    public");
			await writer.WriteAsync(TypeHelper.TypeModifiers(type));
			await writer.WriteAsync(" ");
			await writer.WriteAsync(TypeHelper.TypeType(type));
			await writer.WriteAsync(" ");
			await TypeHelper.FullName(writer, type, t => t.Name, "<", ">");
			await TypeHelper.BaseClasses(writer, type);
			await writer.WriteLineAsync();
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
