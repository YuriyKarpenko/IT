using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using IT.Log;

namespace IT
{
	/// <summary>
	/// Базовый класс для работы с основными операциями формата Csv
	/// </summary>
	/// <typeparam name="TItemProperty">Тип свойств полей, как правило <see cref="PropertyInfo"/> или <see cref="PropertyDescriptor"/> </typeparam>
	/// <typeparam name="T">Тип обрабатываемых данных</typeparam>
	public abstract class CsvBase<TItemProperty, T> : ILog
	{
		/// <summary>
		/// Дефолтный разделитель полей
		/// </summary>
		public const string DEFAULT_SEPARATOR = ";";

		#region Properties

		/// <summary>
		/// Разделитель полей
		/// </summary>
		[DefaultValue(";")]
		public string Separator { get; set; }

		/// <summary>
		/// Набор свойств указанного типа
		/// </summary>
		public TItemProperty[] ItemPropertyes { get; set; }

		#endregion

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="itemPropertyes">Набор свойств указанного типа</param>
		public CsvBase(TItemProperty[] itemPropertyes)
		{
			Contract.NotNull(itemPropertyes, "itemPropertyes");

			this.Separator = DEFAULT_SEPARATOR;

			this.ItemPropertyes = itemPropertyes;
		}


		/// <summary>
		/// Метод получения заголовка
		/// </summary>
		/// <returns></returns>
		public string GetHeader()
		{
			this.Debug("()");
			try
			{
				return this.GetHeaderCore();
			}
			catch (Exception ex)
			{
				this.Error(ex, "()");
				throw;
			}
		}

		/// <summary>
		/// Метод получения строки данных указанного объекта
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public string GetRow(T v)
		{
			try
			{
				return this.GetRowCore(v);
			}
			catch (Exception ex)
			{
				this.Error(ex, "()");
				throw;
			}
		}

		/// <summary>
		/// Метод запиви набора данных в выходной потокововый объект
		/// </summary>
		/// <param name="list">Исходный набор данных</param>
		/// <param name="tw"></param>
		/// <param name="includeHeader">Записывать ли заголовок</param>
		public void Write(IEnumerable<T> list, TextWriter tw, bool includeHeader)
		{
			Contract.NotNull(list, "list");
			Contract.NotNull(tw, "tw");

			this.Debug("()");
			try
			{
				if (includeHeader)
					tw.WriteLine(this.GetHeader());

				foreach (T v in list)
					tw.WriteLine(this.GetRow(v));

			}
			catch (Exception ex)
			{
				this.Error(ex, "()");
				throw;
			}
			finally
			{
				tw.Flush();
			}
		}


		/// <summary>
		/// Вспомогательный метод получения строки из предоставляемых полей
		/// </summary>
		/// <param name="getPropertyValue"></param>
		/// <returns></returns>
		protected virtual string UsingProps(Func<TItemProperty, string> getPropertyValue)
		{
			try
			{
				var ss = this.ItemPropertyes
					.Select(i => getPropertyValue(i))
					.ToArray();
				return string.Join(this.Separator, ss);
			}
			catch (Exception ex)
			{
				this.Error(ex, "()");
				throw;
			}
		}

		/// <summary>
		/// Метод получения заголовка
		/// </summary>
		/// <returns></returns>
		protected abstract string GetHeaderCore();

		/// <summary>
		/// Метод получения строки данных указанного объекта
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		protected abstract string GetRowCore(T v);
	}

	/// <summary>
	/// Класс для работы с основными операциями формата Csv. Понимает атрибуты DisplayNameAttribute и DescriptionAttribute
	/// </summary>
	/// <typeparam name="T">Тип обрабатываемых данных</typeparam>
	public class Csv<T> : CsvBase<PropertyInfo, T>
	{
		#region static

		/// <summary>
		/// Создает Csv с указанными полями (пересечение) и порядком следования
		/// </summary>
		/// <param name="fields"></param>
		/// <returns></returns>
		public static Csv<T> Create(string[] fields)
		{
			var pis = fields
				.Join(typeof(T).GetProperties(), i => i, i => i.Name, (s, p) => p)
				.ToArray();
			var csv = new Csv<T>(pis);
			return csv;
		}

		///// <summary>
		///// Метод запиви набора данных в выходной потокововый объект
		///// </summary>
		///// <param name="list">Исходный набор данных</param>
		///// <param name="outStream"></param>
		///// <param name="includeHeader"></param>
		//public static void ToStream(IEnumerable<T> list, Stream outStream, bool includeHeader = true)
		//{
		//	Csv<T>.UsingCsv(csv => csv.Serialize(list, outStream, includeHeader));
		//}

		///// <summary>
		///// Метод запиви набора данных в выходной потокововый объект
		///// </summary>
		///// <param name="list">Исходный набор данных</param>
		//public static Stream ToStream(IEnumerable<T> list)
		//{
		//	Stream str = null;
		//	Csv<T>.UsingCsv(csv => str = csv.ToStream(list, true));
		//	return str;
		//}

		/// <summary>
		/// Предоставляет готовый экземпляр для удобной работы с ним
		/// </summary>
		/// <param name="act"></param>
		/// <param name="existingItem"></param>
		public static void UsingCsv(Action<Csv<T>> act, Csv<T> existingItem = null)
		{
			Logger.ToLogFmt(null, TraceLevel.Verbose, null, "()");
			try
			{
				var csv = existingItem ?? new Csv<T>();
				act(csv);
			}
			catch (Exception ex)
			{
				Logger.ToLogFmt(null, TraceLevel.Error, ex, "()");
				throw;
			}
		}

		#endregion


		private Func<T, string> actGetRow;

		#region ctors

		/// <summary>
		/// Конструктор
		/// </summary>
		public Csv() : base(typeof(T).GetProperties()) { }
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="pis">Набор свойств указанного типа</param>
		/// <param name="getRow">Метод получения строки данных указанного объекта</param>
		public Csv(PropertyInfo[] pis, Func<T, string> getRow = null) : base(pis) 
		{
			this.actGetRow = getRow;
		}
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="getRow">Метод получения строки данных указанного объекта</param>
		public Csv(Func<T, string> getRow) : this() 
		{
			this.actGetRow = getRow;
		}

		#endregion


		/// <summary>
		/// Метод запиви набора данных в выходной потокововый объект
		/// </summary>
		/// <param name="list">Исходный набор данных</param>
		/// <param name="outStream"></param>
		/// <param name="includeHeader"></param>
		public void Serialize(IEnumerable<T> list, Stream outStream, bool includeHeader = true)
		{
			Contract.NotNull(outStream, "outStream");
			var sw = new StreamWriter(outStream, Encoding.UTF8);
			this.Write(list, sw, includeHeader);
		}

		/// <summary>
		/// Создает и заполняет MemoryStream
		/// </summary>
		/// <param name="list"></param>
		/// <param name="includeHeader"></param>
		/// <returns></returns>
		public Stream ToStream(IEnumerable<T> list, bool includeHeader = true)
		{
			var ms = new MemoryStream();
			this.Serialize(list, ms, includeHeader);
			ms.Position = 0;
			return ms;
		}

		/// <summary>
		/// Метод получения заголовка
		/// </summary>
		/// <returns></returns>
		protected override string GetHeaderCore()
		{
			if (this.ItemPropertyes.Length == 0)
				return "value";
			else return this.UsingProps(p =>
				{
					var o = null
						//?? p.GetAttributeValue<DisplayAttribute, string>(a => a.ShortName)
						?? p.GetAttributeValue<DisplayAttribute, string>(a => a.Name)
						?? p.GetAttributeValue<DisplayNameAttribute, string>(a => a.DisplayName)
						?? p.GetAttributeValue<DescriptionAttribute, string>(a => a.Description);
					return string.IsNullOrEmpty(o) ? p.Name : o;
				}
			);
		}

		/// <summary>
		/// Метод получения строки данных указанного объекта
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		protected override string GetRowCore(T v)
		{
			if (this.actGetRow != null)
				return this.actGetRow(v);

			if (this.ItemPropertyes.Length == 0)
				return v.ToString();
			else
				return this.UsingProps(p => p.GetValue(v, null)?.ToString() ?? "");
		}
	}
}
