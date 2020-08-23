using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AdamBarclay.MarkdownDocumentation.Helpers
{
	internal static class TypeHelper
	{
		internal static string BaseClasses(Type type)
		{
			var baseType = string.Empty;

			if (!TypeHelper.TypeIsADelegate(type))
			{
				var interfaces = TypeHelper.Interfaces(type);

				if (type.BaseType != null &&
					type.BaseType.FullName != "System.Object" &&
					type.BaseType.FullName != "System.ValueType")
				{
					baseType = type.BaseType.Name;

					if (interfaces.Length > 0)
					{
						baseType += ", ";
					}
				}

				baseType += interfaces;
			}

			return baseType.Length > 0 ? " : " + baseType : string.Empty;
		}

		internal static string FullName(Type type)
		{
			if (type.IsGenericType)
			{
				var typeName = type.Name.Split('`')[0];

				var genericArguments = type.GetGenericArguments();

				typeName += $"<{TypeHelper.FullName(genericArguments[0])}";

				for (var i = 1; i < genericArguments.Length; ++i)
				{
					typeName += "," + TypeHelper.FullName(genericArguments[i]);
				}

				return typeName + ">";
			}

			return type.Name;
		}

		internal static string FullNameEncoded(Type type)
		{
			if (type.IsGenericType)
			{
				var typeName = type.Name.Split('`')[0];

				var genericArguments = type.GetGenericArguments();

				typeName += $"&lt;{TypeHelper.FullNameEncoded(genericArguments[0])}";

				for (var i = 1; i < genericArguments.Length; ++i)
				{
					typeName += "," + TypeHelper.FullNameEncoded(genericArguments[i]);
				}

				return typeName + "&gt;";
			}

			return type.Name;
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

		private static string Interfaces(Type type)
		{
			var interfaces = type.GetInterfaces();

			return string.Join(
				", ",
				interfaces.Except(
						interfaces.SelectMany(o => o.GetInterfaces())
							.Concat(type.BaseType?.GetInterfaces() ?? Array.Empty<Type>()))
					.Select(TypeHelper.FullName));
		}
	}
}
