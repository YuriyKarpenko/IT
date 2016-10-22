using System;
using System.Linq;

namespace IT
{
	/// <summary>
	/// Расширения для string
	/// </summary>
	public static class String_Extentions
	{
		/// <summary>
		/// Варианты строк в нижнем регистре, обозначающие позитивное значение bool
		/// </summary>
		public static string[] TRUE_WORDS = { "1", "+", "y", "yes", "true" };

		/// <summary>
		/// Разделитель целой и дробной части, используемые в системе
		/// </summary>
		public static readonly string DecimalSeparator = System.Globalization
			//.CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator;
			.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
		//.NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator;
		//.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
		//.TextElementEnumerator.CurrentInfo.CurrencyDecimalSeparator;


		/// <summary>
		/// Замена символов '.' и ',' на  System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator
		/// </summary>
		/// <param name="s">Расширяемый экземпляр</param>
		/// <returns></returns>
		public static string ReplaceDecimalSeparator(this string s)
		{
			return s
				.Replace(",", DecimalSeparator)
				.Replace(".", DecimalSeparator);
		}


		#region parsing extentions

		/// <summary>
		/// Применяет "TryParse" для указанного типа
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="s"></param>
		/// <returns></returns>
		public static Nullable<T> To<T>(this string s) where T : struct
		{
			if (!string.IsNullOrEmpty(s))
			{
				var args = new object[] { s.ReplaceDecimalSeparator(), default(T) };
				if (UtilsReflection.ExecStaticMethod<bool>(typeof(T), "TryParse", args))
					return (T)args[1];
			}
			return null;
		}

		/// <summary>
		/// Пытаерся применить typeof(T).TryParse()
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="s"></param>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static bool TryParse<T>(string s, out T obj)
		{
			obj = default(T);
			var res = false;
			if (!string.IsNullOrEmpty(s)) //&& typeof(T).IsValueType)
			{
				var t = typeof(T).FromNullable();

				if (t == typeof(Guid) && !s.StartsWith("{"))
				{
					s = "{" + s + "}";
				}

				var args = new object[] { s.ReplaceDecimalSeparator(), obj };
				res = UtilsReflection.ExecStaticMethod<bool>(t, "TryParse", args);
				obj = (T)args[1];
			}
			return res;
		}


		/// <summary>
		/// Применяет "TryParse" (если есть) для указанного типа
		/// инче возвращает default(T)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="s"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public static T To<T>(this string s, T defaultValue)
		{
			T res;
			if (TryParse<T>(s, out res))
				return res;
			return defaultValue;
		}

		/// <summary>
		/// Применяет "TryParse" (если есть) для указанного типа
		/// инче возвращает default(T)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="s"></param>
		/// <returns></returns>
		public static T ToDef<T>(this string s)
		{
			T res;
			TryParse<T>(s, out res);
			return res;
		}

		/// <summary>
		/// Как правило актуально для импорта
		/// </summary>
		/// <param name="s"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public static bool ToBool(this string s, bool defaultValue = false)
		{
			if (!string.IsNullOrWhiteSpace(s))
			{
				s = s.Trim().ToLowerInvariant();
				return TRUE_WORDS.Contains(s);
			}
			return defaultValue;
		}


		#endregion


		/// <summary>
		/// Возвращает строковый массив, содержащий подстроки данной строки, разделенные
		/// элементами заданного массива знаков Юникода.Параметр указывает, следует ли
		/// возвращать пустые элементы массива.
		/// </summary>
		/// <param name="s">Расширяемый экземпляр</param>
		/// <param name="options">System.StringSplitOptions.RemoveEmptyEntries, чтобы исключить пустые элементы из возвращаемого массива; или System.StringSplitOptions.None для включения пустых элементов в возвращаемый массив.</param>
		/// <param name="separator">Массив знаков Юникода, разделяющих подстроки в данной строке, пустой массив, не содержащий разделителей, или null.</param>
		/// <returns>Массив, элементы которого содержат подстроки данной строки, разделенные одним или более знаками из separator.Дополнительные сведения см. в разделе "Примечания".</returns>
		public static string[] Split(this string s, StringSplitOptions options, params char[] separator)
		{
			return s.Split(separator, options);
		}

		/// <summary>
		/// Перевод русского текста в латиницу + ToUpper()
		/// </summary>
		/// <param name="s">Исходная строка</param>
		/// <returns></returns>
		public static string ToTranslit(this string s)
		{
			s = s.ToUpper();
			string result = "";
			string l;
			for (int i = 0; i < s.Length; ++i)
			{
				var c = s[i];
				if ('A' <= c && c <= 'Z' || 'a' <= c && c <= 'z')
				{
					result += c;
					continue;
				}

				try
				{
					if (c == 'Ы' && s[i + 1] == 'Й')
					{
						result += "YI";
						++i;
						continue;
					}
				}
				catch (IndexOutOfRangeException) { }

				switch (c)
				{
					case 'А':
						l = "A";
						break;
					case 'Б':
						l = "B";
						break;
					case 'В':
						l = "V";
						break;
					case 'Г':
						l = "G";
						break;
					case 'Д':
						l = "D";
						break;
					case 'Е':
						l = "E";
						break;
					case 'Ё':
						l = "YO";
						break;
					case 'Ж':
						l = "ZH";
						break;
					case 'З':
						l = "Z";
						break;
					case 'И':
						l = "I";
						break;
					case 'Й':
						l = "Y";
						break;
					case 'К':
						l = "K";
						break;
					case 'Л':
						l = "L";
						break;
					case 'М':
						l = "M";
						break;
					case 'Н':
						l = "N";
						break;
					case 'О':
						l = "O";
						break;
					case 'П':
						l = "P";
						break;
					case 'Р':
						l = "R";
						break;
					case 'С':
						l = "S";
						break;
					case 'Т':
						l = "T";
						break;
					case 'У':
						l = "U";
						break;
					case 'Ф':
						l = "F";
						break;
					case 'Х':
						l = "KH";
						break;
					case 'Ц':
						l = "TS";
						break;
					case 'Ч':
						l = "CH";
						break;
					case 'Ш':
						l = "SH";
						break;
					case 'Щ':
						l = "SHCH";
						break;
					case 'Ъ':
						l = "";
						break;
					case 'Ы':
						l = "Y";
						break;
					case 'Ь':
						l = "";
						break;
					case 'Э':
						l = "E";
						break;
					case 'Ю':
						l = "YU";
						break;
					case 'Я':
						l = "YA";
						break;
					case '-':
						l = "-";
						break;
					default:
						l = "";
						break;
				}
				result += l;
			}
			return result;
		}


	}
}
