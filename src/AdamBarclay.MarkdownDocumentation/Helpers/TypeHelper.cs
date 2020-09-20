using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AdamBarclay.MarkdownDocumentation.Helpers
{
	internal static class TypeHelper
	{
		private static readonly Dictionary<string, string> Aliases = new Dictionary<string, string>
		{
			{ "System.Byte", "byte" },
			{ "System.SByte", "sbyte" },
			{ "System.Int16", "short" },
			{ "System.UInt16", "ushort" },
			{ "System.Int32", "int" },
			{ "System.UInt32", "uint" },
			{ "System.Int64", "long" },
			{ "System.UInt64", "ulong" },
			{ "System.Single", "float" },
			{ "System.Double", "double" },
			{ "System.Decimal", "decimal" },
			{ "System.Object", "object" },
			{ "System.Boolean", "bool" },
			{ "System.Char", "char" },
			{ "System.String", "string" },
			{ "System.Void", "void" }
		};

		internal static async Task BaseClasses(StreamWriter writer, Type type)
		{
			if (!TypeHelper.TypeIsADelegate(type))
			{
				if (type.BaseType != null &&
					type.BaseType.FullName != "System.Object" &&
					type.BaseType.FullName != "System.ValueType")
				{
					await writer.WriteAsync(" : ");
					await writer.WriteAsync(type.BaseType.Name);
					await TypeHelper.Interfaces(writer, type, ", ");
				}
				else
				{
					await TypeHelper.Interfaces(writer, type, " : ");
				}
			}
		}

		internal static async Task FullName(
			StreamWriter writer,
			Type type,
			Func<Type, string> typeName,
			string openAngle,
			string closeAngle)
		{
			if (type.IsByRef)
			{
				type = type.GetElementType()!;
			}

			if (type.IsGenericType)
			{
				await writer.WriteAsync(type.Name.Split('`')[0]);

				var genericArguments = type.GetGenericArguments();

				await writer.WriteAsync(openAngle);

				await TypeHelper.FullName(writer, genericArguments[0], typeName, openAngle, closeAngle);

				for (var i = 1; i < genericArguments.Length; ++i)
				{
					await writer.WriteAsync(",");

					await TypeHelper.FullName(writer, genericArguments[i], typeName, openAngle, closeAngle);
				}

				await writer.WriteAsync(closeAngle);
			}
			else
			{
				await writer.WriteAsync(typeName(type));
			}
		}

		internal static Type? GetType(Assembly assembly, string typeName)
		{
			var type = Type.GetType(typeName);

			if (type != null)
			{
				return type;
			}

			type = assembly.GetType(typeName);

			if (type != null)
			{
				return type;
			}

			return null;
		}

		internal static bool IgnoreDeclaringType(MemberInfo memberInfo)
		{
			var name = memberInfo.DeclaringType?.FullName;

			return name != "System.Exception" &&
				name != "System.Delegate" &&
				name != "System.Object" &&
				name != "System.ValueType";
		}

		internal static bool TypeIsADelegate(Type type)
		{
			while (type.BaseType != null)
			{
				if (type.BaseType.FullName == "System.Delegate")
				{
					return true;
				}

				type = type.BaseType;
			}

			return false;
		}

		internal static string TypeModifiers(Type type)
		{
			if (!TypeHelper.TypeIsADelegate(type))
			{
				if (type.IsClass)
				{
					if (type.IsSealed)
					{
						if (type.IsAbstract)
						{
							return " static";
						}

						return " sealed";
					}

					if (type.IsAbstract)
					{
						return " abstract";
					}
				}
				else if (type.IsValueType &&
					type.CustomAttributes.Any(o => o.AttributeType == typeof(IsReadOnlyAttribute)))
				{
					return " readonly";
				}
			}

			return string.Empty;
		}

		internal static string TypeNameAliased(Type type)
		{
			if (type.FullName != null)
			{
				if (type.BaseType?.FullName == "System.Array")
				{
					var elementType = type.GetElementType()!;

					if (TypeHelper.Aliases.TryGetValue(elementType.FullName ?? elementType.Name, out var typeNameAlias))
					{
						return typeNameAlias + "[]";
					}
				}
				else
				{
					if (TypeHelper.Aliases.TryGetValue(type.FullName, out var typeNameAlias))
					{
						return typeNameAlias;
					}
				}
			}

			return type.Name;
		}

		internal static string TypeType(Type type)
		{
			if (TypeHelper.TypeIsADelegate(type))
			{
				return "delegate";
			}

			if (type.IsClass)
			{
				return "class";
			}

			if (type.IsEnum)
			{
				return "enum";
			}

			if (type.IsInterface)
			{
				return "interface";
			}

			if (type.IsValueType)
			{
				return "struct";
			}

			return string.Empty;
		}

		internal static string TypeTypeTitle(Type type)
		{
			if (TypeHelper.TypeIsADelegate(type))
			{
				return "Delegate";
			}

			if (type.IsClass)
			{
				return "Class";
			}

			if (type.IsEnum)
			{
				return "Enum";
			}

			if (type.IsInterface)
			{
				return "Interface";
			}

			if (type.IsValueType)
			{
				return "Struct";
			}

			return string.Empty;
		}

		private static async Task Interfaces(StreamWriter writer, Type type, string initialSeparator)
		{
			var interfaces = type.GetInterfaces()
				.Except(
					type.GetInterfaces()
						.SelectMany(o => o.GetInterfaces())
						.Concat(type.BaseType?.GetInterfaces() ?? Array.Empty<Type>()))
				.ToList();

			if (interfaces.Count > 0)
			{
				await writer.WriteAsync(initialSeparator);
				await TypeHelper.FullName(writer, interfaces[0], t => t.Name, "<", ">");

				for (var i = 1; i < interfaces.Count; ++i)
				{
					await writer.WriteAsync(", ");
					await TypeHelper.FullName(writer, interfaces[i], t => t.Name, "<", ">");
				}
			}
		}
	}
}
