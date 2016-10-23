using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace IT
{
	/// <summary>
	/// Extention methods for Type
	/// </summary>
	public static class Type_Extention
	{
		/// <summary>
		/// Type extracts from Nullable of Type
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public static Type FromNullable(this Type t)
		{
			if (t.IsGenericType && t.Name.StartsWith("Nullable"))
				t = t.GetGenericArguments()[0];

			return t;
		}

		/// <summary>
		/// PropertyInfo extracts from Type (include interaces)
		/// </summary>
		/// <param name="type"></param>
		/// <param name="isFiltered"></param>
		/// <returns></returns>
		public static PropertyInfo[] GetProperties(this Type type, bool isFiltered)
		{
			if (type == null)
				return null;

			var res = type.GetProperties();
			if (type.IsInterface)
			{
				var ii = type.GetInterfaces();
				res = res
					.Union(ii.SelectMany(i => i.GetProperties()))
					.Distinct()
					.ToArray();
			}

			if (isFiltered)
			{
				res = res
					.Where(i => i.GetAttributeValue<DisplayAttribute, bool>(a => a.GetAutoGenerateField() ?? true, true))
					.Where(i => i.GetAttributeValue<BrowsableAttribute, bool>(a => a.Browsable, true))
					.ToArray();
			}
			return res;
		}
	}
}
