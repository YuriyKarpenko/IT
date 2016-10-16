using System;

namespace IT
{
	public static class Type_Extention
	{
		public static Type FromNullable(this Type t)
		{
			if (t.IsGenericType && t.Name.StartsWith("Nullable"))
				t = t.GetGenericArguments()[0];

			return t;
		}
	}
}
