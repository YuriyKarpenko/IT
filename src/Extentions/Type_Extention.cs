using System;
using System.Collections.Generic;
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
		/// <param name="t"></param>
		/// <returns></returns>
		public static PropertyInfo[] GetPropertiesI(this Type t)
		{
			var res = t.GetProperties();
			if(t.IsInterface)
			{
				var ii = t.GetInterfaces();
				res = res
					.Union(ii.SelectMany(i => i.GetProperties()))
					.Distinct()
					.ToArray();
			}
			return res;
		}
	}
}
