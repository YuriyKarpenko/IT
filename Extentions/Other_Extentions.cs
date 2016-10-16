using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using IT.Log;

namespace IT
{
	/// <summary>
	/// Расширения для простых типов
	/// </summary>
	public static class Other_Extentions
	{
		/// <summary>
		/// No coment
		/// </summary>
		/// <param name="collection"></param>
		/// <param name="value"></param>
		public static void AddRange(this IList collection, IEnumerable value)
		{
			Contract.Requires<ArgumentException>(collection != null, "collection");
			if (value != null)
				foreach (var v in value)
					collection.Add(v);
		}

		/// <summary>
		/// No coment
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection"></param>
		/// <param name="value"></param>
		public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> value)
		{
			Contract.Requires<ArgumentException>(collection != null, "collection");
			if (value != null)
				foreach (T v in value)
					collection.Add(v);
		}



		/// <summary>
		/// При отсутствии значения возвращает def
		/// </summary>
		/// <typeparam name="T">Тип расширения</typeparam>
		/// <param name="obj">Расширяемый объект</param>
		/// <param name="def">Значение по умолчанию</param>
		/// <returns></returns>
		public static T GetValue<T>(this Nullable<T> obj, T def) where T : struct
		{
			return obj.HasValue ? obj.Value : def;
		}


		/// <summary>
		/// Пытается заполнить свойства переданного объекта собственными значениями соответствующих свойств
		/// используя PropertyInfo
		/// </summary>
		/// <param name="source">Расширяемый экземпляр</param>
		/// <param name="dest">Объект для заполнения соответствующих свойств</param>
		/// <param name="isIcgnoreCase">Параметр сравнения наименованй свойств</param>
		/// <returns>Объект с заполнеными соответствующими свойствами</returns>
		[Obsolete]
		public static object ClonePropertyTo(object source, object dest, bool isIcgnoreCase = false)
		{
#if !SILVERLIGHT
			try
			{
#endif
				var param = BindingFlags.Default // = 0
					//| BindingFlags.DeclaredOnly	//	= 2	Наследуемые члены не учитываются.
					| BindingFlags.Instance	//	= 4
					//| BindingFlags.Static	//	= 8
					| BindingFlags.Public	//	= 16
					//| BindingFlags.NonPublic	//	= 32
					//| BindingFlags.SetField	//	= 2048
					//| BindingFlags.SetProperty	//	= 4096
				;

				if (isIcgnoreCase)
					param |= BindingFlags.IgnoreCase;	//	= 1

				var pis2 = dest.GetType().GetProperties(param);

				foreach (var pi2 in pis2)
				{
					var pi = source.GetType().GetProperty(pi2.Name);

					if (pi != null && pi.CanRead && pi2.CanWrite)
					{
						var o = pi.GetValue(source, null);
						pi2.SetValue(dest, o, null);
					}
				}

				return dest;
#if !SILVERLIGHT
			}
			catch (Exception ex)
			{
				Logger.ToLogFmt(null, TraceLevel.Error, ex, "()");
				throw;
			}
#endif
		}

#if !SILVERLIGHT
		/// <summary>
		/// Пытается заполнить свойства переданного объекта собственными значениями соответствующих свойств
		/// используя PropertyDescription (уиитываются атрибуты : ReadOnly(), ...)
		/// </summary>
		/// <param name="source">Расширяемый экземпляр</param>
		/// <param name="dest">Объект для заполнения соответствующих свойств</param>
		/// <param name="isIcgnoreCase">Параметр сравнения наименованй свойств</param>
		/// <returns>Объект с заполнеными соответствующими свойствами</returns>
		[Obsolete]
		public static object ClonePropertyTo_PD(object source, object dest, bool isIcgnoreCase = false)
		{
			try
			{
				var pds = TypeDescriptor.GetProperties(dest);
				var sourPds = TypeDescriptor.GetProperties(source);

				foreach (PropertyDescriptor pd in pds)
				{
					if (!pd.IsReadOnly)
					{
						var sourPd = sourPds.Find(pd.Name, isIcgnoreCase);

						if (sourPd != null)
						{
							var o = sourPd.GetValue(source);
							pd.SetValue(dest, o);
						}
					}
				}

				return dest;
			}
			catch (Exception ex)
			{
				Logger.ToLogFmt(null, TraceLevel.Error, ex, "()");
				throw;
			}
		}
#endif
	}


	/// <summary>
	/// расширения для перечислений
	/// </summary>
	public static class EnumExtention
	{
#if !SILVERLIGHT
		/// <summary>
		/// Получение значения атрибута 
		/// </summary>
		/// <typeparam name="Tatr">Тип атрибута</typeparam>
		/// <typeparam name="R">Тип результата</typeparam>
		/// <param name="pd">Расширяемый экземпляр</param>
		/// <param name="expression">Функция получения результата</param>
		/// <returns></returns>
		public static R GetAttributeValue<Tatr, R>(this MemberDescriptor pd, Func<Tatr, R> expression, R def = default(R))
			where Tatr : Attribute
		{
			var attr = pd.Attributes[typeof(Tatr)] as Tatr;
			return attr == null ? def : expression(attr);
		}
		public static string GetAttributeValueStr<Tatr>(this MemberDescriptor pd, Func<Tatr, string> expression, string def = null)
			where Tatr : Attribute
		{
			var attr = pd.Attributes[typeof(Tatr)] as Tatr;
			var res = attr == null ? def : expression(attr);
			return (string.IsNullOrEmpty(res) ? def : res);
		}
#endif

		/// <summary>
		/// Получение значения атрибута указанного типа из масива атрибуктов
		/// </summary>
		/// <typeparam name="Tatr">Тип атрибута</typeparam>
		/// <typeparam name="R">Тип результата</typeparam>
		/// <param name="apr">Расширяемый экземпляр</param>
		/// <param name="expression">Функция получения результата</param>
		/// <returns></returns>
		public static R GetAttributeValue<Tatr, R>(this ICustomAttributeProvider apr, Func<Tatr, R> expression, R def = default(R))
			where Tatr : Attribute
		{
			var attribute = apr.GetCustomAttributes(typeof(Tatr), true)?.OfType<Tatr>()?.FirstOrDefault();
			return attribute == null ? def : expression(attribute);
		}
		public static string GetAttributeValueStr<Tatr>(this ICustomAttributeProvider apr, Func<Tatr, string> expression, string def = null)
			where Tatr : Attribute
		{
			var res = GetAttributeValue<Tatr, string>(apr, expression, def);
			return (string.IsNullOrEmpty(res) ? def : res);
		}

		/// <summary>
		/// Привычное преобразование перечисленя
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Array ToArray(this Enum value)
		{
			return Enum.GetValues(value.GetType());
		}

		/// <summary>
		/// Получение значения атрибута перечисления
		/// </summary>
		/// <typeparam name="Tatr">Тип атрибута</typeparam>
		/// <typeparam name="R">Тип результата</typeparam>
		/// <param name="enumValue">Расширяемый экземпляр</param>
		/// <param name="expression">Функция получения результата</param>
		/// <returns></returns>
		public static R GetAttributeValue<Tatr, R>(this Enum enumValue, Func<Tatr, R> expression, R def = default(R))
			where Tatr : Attribute
		{
			var mi = enumValue.GetType()
				.GetMember(enumValue.ToString())?
				.FirstOrDefault();
			if (mi != null)
			{
				return mi.GetAttributeValue(expression, def);
			}

			return def;
		}

		/// <summary>
		/// Получение значения атрибута DescriptionAttribute
		/// </summary>
		/// <param name="enumValue">Расширяемый экземпляр</param>
		/// <returns>Значение атрибута DescriptionAttribute</returns>
		public static string GetDescription(this Enum enumValue)
		{
			return enumValue.GetAttributeValue<DescriptionAttribute, string>(a => a.Description);
		}

		//public static override string ToString(this Enum enumValue)
		//{
		//    return enumValue.GetAttributeValue<DescriptionAttribute, string>(a => a.Description) ?? enumValue.ToString();
		//}

	}
}
