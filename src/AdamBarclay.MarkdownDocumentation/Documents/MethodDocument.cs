using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using AdamBarclay.MarkdownDocumentation.Helpers;

namespace AdamBarclay.MarkdownDocumentation.Documents
{
	internal static class MethodDocument
	{
		internal static async Task BuildMultiple(
			StreamWriter writer,
			Assembly assembly,
			Type type,
			List<MethodInfo> methods,
			XDocument xmlComments)
		{
			await DocumentHelpers.PageTitle(
				writer,
				async w =>
				{
					await TypeHelper.FullName(w, type, t => t.Name, "&lt;", "&gt;");
					await w.WriteAsync(".");
					await MethodHelper.MethodName(w, methods.First(), t => t.Name, "&lt;", "&gt;");
				},
				"Methods");

			await DocumentHelpers.PageHeader(writer, type, xmlComments);

			await MethodDocument.Overloads(writer, methods, xmlComments);

			foreach (var method in methods)
			{
				await writer.WriteAsync("## ");
				await MethodHelper.MethodName(writer, methods.First(), t => t.Name, "&lt;", "&gt;");
				await MethodHelper.MethodParameterTypes(writer, method, t => t.Name, "&lt;", "&gt;");
				await writer.WriteLineAsync();

				await MethodDocument.MethodDetails(writer, assembly, type, method, xmlComments);
			}
		}

		internal static async Task BuildSingle(
			StreamWriter writer,
			Assembly assembly,
			Type type,
			MethodInfo method,
			XDocument xmlComments)
		{
			await DocumentHelpers.PageTitle(
				writer,
				async w =>
				{
					await TypeHelper.FullName(w, type, t => t.Name, "&lt;", "&gt;");
					await w.WriteAsync(".");
					await MethodHelper.MethodName(w, method, t => t.Name, "&lt;", "&gt;");
					await MethodHelper.MethodParameterTypes(w, method, t => t.Name, "&lt;", "&gt;");
				},
				"Method");

			await DocumentHelpers.PageHeader(writer, type, xmlComments);
			await MethodDocument.MethodDetails(writer, assembly, type, method, xmlComments);
		}

		private static async Task Exceptions(
			StreamWriter writer,
			Assembly assembly,
			MethodInfo method,
			XDocument xmlComments)
		{
			var exceptions = XmlCommentHelper.MethodElement(xmlComments, method)?.Descendants("exception").ToList();

			if (exceptions != null && exceptions.Count > 0)
			{
				await writer.WriteLineAsync("### Exceptions");

				foreach (var exception in exceptions)
				{
					var exceptionTypeName = exception.Attributes("cref").FirstOrDefault()?.Value;

					if (exceptionTypeName != null)
					{
						var exceptionType = TypeHelper.GetType(
							assembly,
							exceptionTypeName.Substring(exceptionTypeName.IndexOf(':', StringComparison.Ordinal) + 1));

						if (exceptionType != null)
						{
							await TypeHelper.FullName(writer, exceptionType, t => t.Name, "&lt;", "&gt;");
						}
						else
						{
							var start = exceptionTypeName.LastIndexOf('.');

							if (start >= 0)
							{
								exceptionTypeName = exceptionTypeName.Substring(start + 1);
							}

							var end = exceptionTypeName.IndexOf('`', StringComparison.Ordinal);

							if (end >= 0)
							{
								exceptionTypeName = exceptionTypeName.Substring(0, end);
							}

							await writer.WriteAsync(exceptionTypeName);
						}

						await writer.WriteLineAsync("  ");
						await XmlCommentHelper.WriteValue(writer, exception);
						await writer.WriteLineAsync();
						await writer.WriteLineAsync();
					}
				}
			}
		}

		private static async Task MethodDetails(
			StreamWriter writer,
			Assembly assembly,
			Type type,
			MethodInfo method,
			XDocument xmlComments)
		{
			await writer.WriteLineAsync(XmlCommentHelper.Summary(XmlCommentHelper.MethodElement(xmlComments, method)));
			await writer.WriteLineAsync();

			await MethodDocument.Signature(writer, type, method);
			await MethodDocument.TypeParameters(writer, method, xmlComments);
			await MethodDocument.Parameters(writer, method, xmlComments);
			await MethodDocument.Returns(writer, method, xmlComments);
			/* await MethodDocument.Implements(); */
			await MethodDocument.Exceptions(writer, assembly, method, xmlComments);
			/* await MethodDocument.Examples() */
			/* await MethodDocument.Remarks() */
		}

		private static async Task Overloads(StreamWriter writer, IEnumerable<MethodBase> methods, XDocument xmlComments)
		{
			await writer.WriteLineAsync("## Overloads");

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
				/*await writer.WriteAsync(FileNameHelper.MethodFileName(string.Empty, method));*/
				await writer.WriteAsync(")|");

				var summary = XmlCommentHelper.Summary(XmlCommentHelper.MethodElement(xmlComments, method));

				await writer.WriteLineAsync(summary);
			}

			await writer.WriteLineAsync();
		}

		private static async Task Parameters(StreamWriter writer, MethodInfo method, XDocument xmlComments)
		{
			var parameters = method.GetParameters();

			if (parameters.Length > 0)
			{
				await writer.WriteLineAsync("### Parameters");

				foreach (var parameter in parameters)
				{
					await writer.WriteAsync("**`");
					await writer.WriteAsync(parameter.Name);
					await writer.WriteAsync("`** ");
					await TypeHelper.FullName(writer, parameter.ParameterType, t => t.Name, "&lt;", "&gt;");
					await writer.WriteLineAsync("  ");
					await writer.WriteLineAsync(XmlCommentHelper.Parameter(xmlComments, method, parameter));
					await writer.WriteLineAsync();
				}
			}
		}

		private static async Task Returns(StreamWriter writer, MethodInfo method, XDocument xmlComments)
		{
			if (method.ReturnType.FullName != "System.Void")
			{
				await writer.WriteLineAsync("### Returns");

				await TypeHelper.FullName(writer, method.ReturnType, t => t.Name, "&lt;", "&gt;");
				await writer.WriteLineAsync("  ");
				await writer.WriteLineAsync(XmlCommentHelper.Returns(xmlComments, method));
			}
		}

		private static async Task Signature(StreamWriter writer, Type type, MethodInfo method)
		{
			await writer.WriteLineAsync("```c#");
			await writer.WriteAsync("    ");

			if (method.IsPublic)
			{
				await writer.WriteAsync("public");
			}
			else if (method.IsFamily)
			{
				await writer.WriteAsync("protected");
			}

			if (!type.IsInterface)
			{
				if (method.IsStatic)
				{
					await writer.WriteAsync(" static");
				}
				else if (method.IsAbstract)
				{
					await writer.WriteAsync(" abstract");
				}
				else if (method.IsVirtual && !method.IsFinal)
				{
					await writer.WriteAsync(" virtual");
				}
			}

			await writer.WriteAsync(" ");
			await TypeHelper.FullName(writer, method.ReturnType, TypeHelper.TypeNameAliased, "<", ">");
			await writer.WriteAsync(" ");

			await MethodHelper.MethodName(writer, method, TypeHelper.TypeNameAliased, "<", ">");

			await writer.WriteAsync("(");

			var parameters = method.GetParameters();

			if (parameters.Length > 0)
			{
				if (parameters[0].ParameterType.IsByRef)
				{
					if (parameters[0].IsIn)
					{
						await writer.WriteAsync("in ");
					}
					else if (parameters[0].IsOut)
					{
						await writer.WriteAsync("out ");
					}
					else
					{
						await writer.WriteAsync("ref ");
					}
				}

				await TypeHelper.FullName(writer, parameters[0].ParameterType, TypeHelper.TypeNameAliased, "<", ">");
				await writer.WriteAsync(" ");
				await writer.WriteAsync(parameters[0].Name);

				for (var i = 1; i < parameters.Length; i++)
				{
					await writer.WriteAsync(", ");

					if (parameters[i].ParameterType.IsByRef)
					{
						if (parameters[i].IsIn)
						{
							await writer.WriteAsync("in ");
						}
						else if (parameters[i].IsOut)
						{
							await writer.WriteAsync("out ");
						}
						else
						{
							await writer.WriteAsync("ref ");
						}
					}

					await TypeHelper.FullName(
						writer,
						parameters[i].ParameterType,
						TypeHelper.TypeNameAliased,
						"<",
						">");

					await writer.WriteAsync(" ");
					await writer.WriteAsync(parameters[i].Name);
				}
			}

			await writer.WriteLineAsync(")");

			await writer.WriteLineAsync("```");
			await writer.WriteLineAsync();
		}

		private static async Task TypeParameters(StreamWriter writer, MethodInfo method, XDocument xmlComments)
		{
			if (method.IsGenericMethod)
			{
				var genericArguments = method.GetGenericArguments();

				await writer.WriteLineAsync("### Type Parameters");

				foreach (var genericArgument in genericArguments)
				{
					await writer.WriteAsync("**`");
					await writer.WriteAsync(genericArgument.Name);
					await writer.WriteLineAsync("`**  ");
					await writer.WriteLineAsync(XmlCommentHelper.TypeParameter(xmlComments, method, genericArgument));
					await writer.WriteLineAsync();
				}
			}
		}
	}
}
