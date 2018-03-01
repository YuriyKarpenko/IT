using System;
using System.Collections.Generic;
using System.ComponentModel;
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
	public abstract class CsvBase : ILog
	{
		/// <summary>
		/// Дефолтный разделитель полей
		/// </summary>
		public static string DEFAULT_SEPARATOR { get; set; } = ";";

		/// <summary>
		/// Разделитель полей
		/// </summary>
		[DefaultValue(";")]
		public string Separator { get; set; }

		/// <summary>
		/// ctor
		/// </summary>
		public CsvBase() => Separator = DEFAULT_SEPARATOR;
	}

	/// <summary>
	/// Класс для работы с основными операциями формата Csv. 
	/// </summary>
	/// <typeparam name="T">Тип обрабатываемых данных</typeparam>
	public class CsvReader<T> : CsvBase
	{
		private readonly Func<string[], T> _Creator;

		/// <summary>
		/// Запускается сразу после прочтения файла
		/// </summary>
		public Func<IEnumerable<string>, IEnumerable<string>> FilterSource { get; set; }

		/// <summary>
		/// Прочитанные сущности
		/// </summary>
		public T[] Items { get; set; }

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="creator"></param>
		public CsvReader(Func<string[], T> creator)
		{
			Contract.NotNull(creator, nameof(creator));
			_Creator = creator;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="splitCount"></param>
		/// <param name="creator"></param>
		/// <returns></returns>
		public IEnumerable<T> Load(string fileName, int splitCount, Func<string[], T> creator = null)
		{
			try
			{
				creator = creator ?? _Creator;
				Items = null;
				if (File.Exists(fileName))
				{
					var lines = File.ReadAllLines(fileName);
					Items = (FilterSource?.Invoke(lines) ?? lines)
						.Select(i => creator(i.Split(Separator.ToCharArray(), splitCount, StringSplitOptions.RemoveEmptyEntries)))
						.ToArray();
				}
			}
			catch (Exception ex)
			{
				this.Error(ex);
			}

			return Items;
		}
	}

	/// <summary>
	/// Упрощенный класс для работы с основными операциями формата Csv. 
	/// </summary>
	public class CsvReader : CsvReader<string[]>
	{
		/// <summary>
		/// ctor
		/// </summary>
		public CsvReader() : base(i => i) { }
	}

	/// <summary>
	/// Класс для работы с основными операциями формата Csv. Понимает атрибуты DisplayNameAttribute и DescriptionAttribute
	/// </summary>
	/// <typeparam name="T">Тип обрабатываемых данных</typeparam>
	public class CsvWriter<T> : CsvBase
	{
		#region static

		/// <summary>
		/// Создает Csv с указанными полями (пересечение) и порядком следования
		/// </summary>
		/// <param name="fields">Позволяет ограничить/фильтровать состав полей, указанными</param>
		/// <returns></returns>
		public static CsvWriter<T> Create(string[] fields)
		{
			var pis = fields
				.Join(typeof(T).GetProperties(), i => i, i => i.Name, (s, p) => p)
				.ToArray();
			var csv = new CsvWriter<T>(pis);
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
		public static void UsingCsv(Action<CsvWriter<T>> act, CsvWriter<T> existingItem = null)
		{
			Logger.ToLogFmt(null, TraceLevel.Verbose, null, "()");
			try
			{
				var csv = existingItem ?? new CsvWriter<T>();
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

		#region Properties

		/// <summary>
		/// Набор свойств указанного типа
		/// </summary>
		public PropertyInfo[] ItemPropertyes { get; set; }

		#endregion

		#region ctors

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="itemPropertyes">Набор свойств указанного типа</param>
		public CsvWriter(PropertyInfo[] itemPropertyes)
		{
			Contract.NotNull(itemPropertyes, "itemPropertyes");

			this.Separator = DEFAULT_SEPARATOR;

			this.ItemPropertyes = itemPropertyes;
		}
		/// <summary>
		/// Конструктор
		/// </summary>
		public CsvWriter() : this(typeof(T).GetProperties()) { }
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="pis">Набор свойств указанного типа</param>
		/// <param name="getRow">Метод получения строки данных указанного объекта</param>
		public CsvWriter(PropertyInfo[] pis, Func<T, string> getRow = null) : this(pis)
		{
			this.actGetRow = getRow;
		}
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="getRow">Метод получения строки данных указанного объекта</param>
		public CsvWriter(Func<T, string> getRow) : this()
		{
			this.actGetRow = getRow;
		}

		#endregion


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
		/// Метод получения заголовка
		/// </summary>
		/// <returns></returns>
		protected string GetHeaderCore()
		{
			if (this.ItemPropertyes.Length == 0)
				return "value";
			else
				return this.UsingProps(p => p.GetNameFromAttributes(p.Name));
		}

		/// <summary>
		/// Метод получения строки данных указанного объекта
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		protected string GetRowCore(T v)
		{
			if (this.actGetRow != null)
				return this.actGetRow(v);

			if (this.ItemPropertyes.Length == 0)
				return v.ToString();
			else
				return this.UsingProps(p => p.GetValue(v, null)?.ToString() ?? string.Empty);
		}

		/// <summary>
		/// Вспомогательный метод получения строки из предоставляемых полей
		/// </summary>
		/// <param name="getPropertyValue"></param>
		/// <returns></returns>
		protected virtual string UsingProps(Func<PropertyInfo, string> getPropertyValue)
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
	}
}
