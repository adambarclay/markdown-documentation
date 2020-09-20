using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace AdamBarclay.MarkdownDocumentation.Helpers
{
	internal static class MethodHelper
	{
		internal static async Task MethodName(
			StreamWriter writer,
			MethodBase method,
			Func<Type, string> typeAlias,
			string openAngle,
			string closeAngle)
		{
			if (method.IsConstructor)
			{
				if (method.DeclaringType.IsGenericType)
				{
					await writer.WriteAsync(method.DeclaringType.Name.Split('`')[0]);
				}
				else
				{
					await writer.WriteAsync(method.DeclaringType.Name);
				}
			}
			else
			{
				await writer.WriteAsync(method.Name);
			}

			if (method.IsGenericMethodDefinition)
			{
				var genericArguments = method.GetGenericArguments();

				await writer.WriteAsync(openAngle);
				await writer.WriteAsync(typeAlias(genericArguments[0]));

				for (var i = 1; i < genericArguments.Length; ++i)
				{
					await writer.WriteAsync(",");
					await writer.WriteAsync(typeAlias(genericArguments[i]));
				}

				await writer.WriteAsync(closeAngle);
			}
		}

		internal static async Task MethodParameters(
			StreamWriter writer,
			MethodBase method,
			Func<Type, string> typeAlias,
			string openAngle,
			string closeAngle)
		{
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

				await TypeHelper.FullName(writer, parameters[0].ParameterType, typeAlias, openAngle, closeAngle);
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

					await TypeHelper.FullName(writer, parameters[i].ParameterType, typeAlias, openAngle, closeAngle);
					await writer.WriteAsync(" ");
					await writer.WriteAsync(parameters[i].Name);
				}
			}

			await writer.WriteAsync(")");
		}

		internal static async Task MethodParameterTypes(
			StreamWriter writer,
			MethodBase method,
			Func<Type, string> typeAlias,
			string openAngle,
			string closeAngle)
		{
			await writer.WriteAsync("(");

			var parameters = method.GetParameters();

			if (parameters.Length > 0)
			{
				await TypeHelper.FullName(writer, parameters[0].ParameterType, typeAlias, openAngle, closeAngle);

				for (var i = 1; i < parameters.Length; i++)
				{
					await writer.WriteAsync(", ");

					await TypeHelper.FullName(writer, parameters[i].ParameterType, typeAlias, openAngle, closeAngle);
				}
			}

			await writer.WriteAsync(")");
		}
	}
}
