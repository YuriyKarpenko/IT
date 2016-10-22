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
	}


	/// <summary>
	/// расширения для перечислений
	/// </summary>
	public static class EnumExtention
	{
//#if !SILVERLIGHT
//		/// <summary>
//		/// Получение значения атрибута 
//		/// </summary>
//		/// <typeparam name="Tattr">Тип атрибута</typeparam>
//		/// <typeparam name="R">Тип результата</typeparam>
//		/// <param name="pd">Расширяемый экземпляр</param>
//		/// <param name="expression">Функция получения результата</param>
//		/// <param name="defaultValue">Значение по умолчанию</param>
//		/// <returns></returns>
//		public static R GetAttributeValue<Tattr, R>(this MemberDescriptor pd, Func<Tattr, R> expression, R defaultValue = default(R))
//			where Tattr : Attribute
//		{
//			var attr = pd.Attributes[typeof(Tattr)] as Tattr;
//			return attr == null ? defaultValue : expression(attr);
//		}
//		/// <summary>
//		/// Получение строкового значения атрибута указанного типа
//		/// </summary>
//		/// <typeparam name="Tattr">Тип атрибута</typeparam>
//		/// <param name="pd">Расширяемый экземпляр</param>
//		/// <param name="expression">Функция получения результата</param>
//		/// <param name="defaultValue">Значение по умолчанию</param>
//		/// <returns></returns>
//		public static string GetAttributeValueStr<Tattr>(this MemberDescriptor pd, Func<Tattr, string> expression, string defaultValue = null)
//			where Tattr : Attribute
//		{
//			var attr = pd.Attributes[typeof(Tattr)] as Tattr;
//			var res = attr == null ? defaultValue : expression(attr);
//			return (string.IsNullOrEmpty(res) ? defaultValue : res);
//		}
//#endif

		/// <summary>
		/// Получение значения атрибута указанного типа
		/// </summary>
		/// <typeparam name="Tattr">Тип атрибута</typeparam>
		/// <typeparam name="R">Тип результата</typeparam>
		/// <param name="aPr">Расширяемый экземпляр</param>
		/// <param name="expression">Функция получения результата</param>
		/// <param name="defaultValue">Значение по умолчанию</param>
		/// <returns></returns>
		public static R GetAttributeValue<Tattr, R>(this ICustomAttributeProvider aPr, Func<Tattr, R> expression, R defaultValue = default(R))
			where Tattr : Attribute
		{
			var attribute = aPr.GetCustomAttributes(typeof(Tattr), true)?.OfType<Tattr>()?.FirstOrDefault();
			return attribute == null ? defaultValue : expression(attribute);
		}

		/// <summary>
		/// Получение строкового значения атрибута указанного типа
		/// </summary>
		/// <typeparam name="Tattr">Тип атрибута</typeparam>
		/// <param name="aPr">Расширяемый экземпляр</param>
		/// <param name="expression">Функция получения результата</param>
		/// <param name="defaultValue">Значение по умолчанию</param>
		/// <returns></returns>
		public static string GetAttributeValueStr<Tattr>(this ICustomAttributeProvider aPr, Func<Tattr, string> expression, string defaultValue = null)
			where Tattr : Attribute
		{
			var res = GetAttributeValue<Tattr, string>(aPr, expression, defaultValue);
			return (string.IsNullOrEmpty(res) ? defaultValue : res);
		}

		/// <summary>
		/// Попытка получить значения атрибутов : Display.Name, Display.ShortName, Display.Description, DisplayName.DisplayName, Description.Description
		/// </summary>
		/// <param name="aPr"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public static string GetNameFromAttributes(this ICustomAttributeProvider aPr, string defaultValue = null)
		{
			return null
				?? aPr.GetAttributeValueStr<System.ComponentModel.DataAnnotations.DisplayAttribute>(a => a.Name)
				?? aPr.GetAttributeValueStr<System.ComponentModel.DataAnnotations.DisplayAttribute>(a => a.ShortName)
				?? aPr.GetAttributeValueStr<System.ComponentModel.DataAnnotations.DisplayAttribute>(a => a.Description)
				?? aPr.GetAttributeValueStr<DisplayNameAttribute>(a => a.DisplayName)
				?? aPr.GetAttributeValueStr<DescriptionAttribute>(a => a.Description)
				?? defaultValue;
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
		/// <typeparam name="Tattr">Тип атрибута</typeparam>
		/// <typeparam name="R">Тип результата</typeparam>
		/// <param name="enumValue">Расширяемый экземпляр</param>
		/// <param name="expression">Функция получения результата</param>
		/// <param name="defaultValue">Функция получения результата</param>
		/// <returns></returns>
		public static R GetAttributeValue<Tattr, R>(this Enum enumValue, Func<Tattr, R> expression, R defaultValue = default(R))
			where Tattr : Attribute
		{
			var mi = enumValue.GetType()
				.GetMember(enumValue.ToString())?
				.FirstOrDefault();
			if (mi != null)
			{
				return mi.GetAttributeValue(expression, defaultValue);
			}

			return defaultValue;
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

	}
}
