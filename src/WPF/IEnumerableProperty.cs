using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace IT.WPF
{
	///// <summary>
	///// Базовый класс для получения данных асинхронно
	///// </summary>
	//public class DataProviderBase : NotifyPropertyChangedBase
	//{
	//	private Thread _thread = Thread.CurrentThread;
	//	bool isWorking = false;

	//	/// <summary>
	//	/// Пользовательский метод получения данных
	//	/// </summary>
	//	protected Func<object> GetData { get; set; }

	//	#region Properties

	//	/// <summary>
	//	/// Предположительно поток выполнения UI
	//	/// </summary>
	//	public Thread Thread
	//	{
	//		get { return this._thread; }
	//		set
	//		{
	//			if (this._thread != value)
	//				this._thread = value;
	//		}
	//	}

	//	/// <summary>
	//	/// Режим получения данных
	//	/// </summary>
	//	public bool IsAsynchronous { get; set; }

	//	/// <summary>
	//	/// Данные для View
	//	/// </summary>
	//	public object Data { get { return this._Data ?? this.GetDataCore(); } }
	//	object _Data { get; private set; }

	//	/// <summary>
	//	/// Ошибки последнего сеанса
	//	/// </summary>
	//	public Exception Exception { get; private set; }


	//	/// <summary>
	//	/// return Thread.CurrentThread.ManagedThreadId == this.Thread.ManagedThreadId
	//	/// </summary>
	//	protected bool IsUiThread { get { return Thread.CurrentThread.ManagedThreadId == this.Thread.ManagedThreadId; } }

	//	#endregion

	//	/// <summary>
	//	/// Конструктор
	//	/// </summary>
	//	/// <param name="getData"></param>
	//	public DataProviderBase(Func<object> getData, bool isAsynchronous = true)
	//	{
	//		this.GetData = getData;
	//		this.IsAsynchronous = true;
	//	}

	//	/// <summary>
	//	/// Запускает GetData(), если назначено
	//	/// </summary>
	//	public virtual void BeginGetData()
	//	{
	//		if (this.GetData != null)
	//			if (this.IsAsynchronous)
	//				ThreadPool.QueueUserWorkItem(o => this.SetData(this.GetData()));
	//			else
	//				this.SetData(this.GetData());
	//	}

	//	/// <summary>
	//	/// Установка данных. Можно вызывать из любого потока
	//	/// </summary>
	//	/// <param name="newData"></param>
	//	public void SetData(object newData)
	//	{
	//		this.Debug("()");
	//		try
	//		{
	//			if (this.IsUiThread)
	//				this.SetDataCore(newData);
	//			else
	//				this.Thread.
	//		}
	//		catch (System.Exception ex)
	//		{
	//			this.Error(ex, "()");
	//			throw;
	//		}
	//	}


	//	/// <summary>
	//	/// 1 Точка при запросе данных из View
	//	/// </summary>
	//	/// <returns></returns>
	//	object GetDataCore()
	//	{
	//		this.BeginGetData();
	//		return null;
	//	}

	//	/// <summary>
	//	/// Установка нового значения данных. Должно выполняться в родном потоке
	//	/// </summary>
	//	/// <param name="newData"></param>
	//	void SetDataCore(object newData)
	//	{
	//		this.Debug("()");
	//		try
	//		{
	//			Contract.Requires(this.IsUiThread, "Запуск не из потока UI");
	//			this._Data = newData;
	//			this.OnPropertyChanged("Data");
	//		}
	//		catch (System.Exception ex)
	//		{
	//			this.Error(ex, "()");
	//			throw;
	//		}
	//	}
	//}

	/// <summary>
	/// Класс для удобной работы с выделением элементов списками
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class IEnumerablePropertyReadOnly<T> : NotifyPropertyChangedBase
	{
		/// <summary>
		/// Возникает при изменении свойчтва
		/// </summary>
		public event EventHandler<EventArgs<T>> SelectedChanged;

		#region Properties

		/// <summary>
		/// Список
		/// </summary>
		public virtual IEnumerable<T> List { get; protected set; }

		/// <summary>
		/// Текущее значение
		/// </summary>
		public virtual T SelectedItem
		{
			get { return this._Current; }
			set { this.SetCurrent(value); }
		}
		private T _Current;

		/// <summary>
		/// Выбран ли какой-либо элемент в данный момент
		/// </summary>
		public bool HasSelected { get { return this.SelectedItem != null; } }

		#endregion

		/// <summary>
		/// Конструктор
		/// </summary>
		protected IEnumerablePropertyReadOnly() { }

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="list"></param>
		public IEnumerablePropertyReadOnly(IEnumerable<T> list)
		{
			Contract.Requires<ArgumentException>(list != null, "list");
			this.List = list;
		}

		/// <summary>
		/// Устанавливает значение this._Current
		/// </summary>
		/// <param name="value">Значение</param>
		/// <param name="isRaiseEvent">Следует ли вызывать SelectedChanged</param>
		public virtual void SetCurrent(T value, bool isRaiseEvent = true)
		{
			if (!object.Equals(this._Current, value))
			{
				this._Current = value;
				if (isRaiseEvent)
					this.OnSelectedChanged(value);
				else
				{
					this.OnPropertyChanged("SelectedItem");
				}
				this.OnPropertyChanged("HasSelected");
			}
		}

		/// <summary>
		/// Поиск по списку
		/// </summary>
		/// <param name="pred">Услоаие поиска</param>
		/// <param name="changeCurrent">Следует ли заполнять результатом свойство Current</param>
		/// <param name="isRaiseEvent">Следует ли вызывать SelectedChanged</param>
		/// <returns></returns>
		public virtual T Select(Predicate<T> pred, bool changeCurrent, bool isRaiseEvent)
		{
			T ret = this.List == null ? default(T) : this.List.FirstOrDefault(i => pred(i));
			if (changeCurrent)
			{
				this.SetCurrent(ret, isRaiseEvent);
			}
			return ret;
		}


		/// <summary>
		/// Вызов OnPropertyChanged("Current") и соответствующего события
		/// </summary>
		/// <param name="value">Выбор для передачи в событие</param>
		protected virtual void OnSelectedChanged(T value)
		{
			this.OnPropertyChanged("Current");
			this.OnPropertyChanged("SelectedItem");
			if (this.SelectedChanged != null)
				this.SelectedChanged(this, new EventArgs<T>(value));
		}

		/// <summary>
		/// Привязывает метод к событию
		/// </summary>
		/// <param name="selectedChanged">Метод, запускаемый при возникновении события</param>
		protected virtual void SetSelectedChanged(Action<T> selectedChanged)
		{
			Contract.Requires<ArgumentException>(selectedChanged != null, "selectedChanged");
			this.SelectedChanged += (s, e) => selectedChanged(e.Value);
		}

	}

	/// <summary>
	/// Класс для удобной работы со списками в WPF
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class IEnumerableProperty<T> : IEnumerablePropertyReadOnly<T>
	{
		#region Events

		/// <summary>
		/// Event
		/// </summary>
		public event EventHandler<EventArgs<IEnumerable<T>>> ListChanged;

		/// <summary>
		/// Возникает при смене состояния IsWorking (используется только при асинхронных операциях)
		/// </summary>
		public event EventHandler IsWorkingChanged;

		#endregion


		/// <summary>
		/// Метод получения списка из конструктора
		/// </summary>
		protected Func<IEnumerable<T>> fGetList;

		/// <summary>
		/// Метод заполнения списка из конструктора
		/// </summary>
		protected Action<Action<IEnumerable<T>>> fGetListAsync;

		#region Properties

		/// <summary>
		/// Список
		/// </summary>
		public override IEnumerable<T> List { get { return this._list ?? this.GetList(); } }
		private IEnumerable<T> _list;

		/// <summary>
		/// Текущее значение
		/// </summary>
		[Obsolete("Следует использовать свойство 'SelectedItem'")]
		public virtual T Current
		{
			get { return this.SelectedItem; }
			set { this.SelectedItem = value; }
		}

		/// <summary>
		/// Признак работы в асинхронном режиме
		/// </summary>
		public bool IsWorking { get; private set; }

		#endregion

		/// <summary>
		/// Конструктор
		/// </summary>
		protected IEnumerableProperty(Action<T> selectedChanged = null)
		{
			if (selectedChanged != null)
				this.SetSelectedChanged(selectedChanged);
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="getList">Функсия получения списка</param>
		/// <param name="selectedChanged">Подписчик соответствующего события</param>
		public IEnumerableProperty(Func<IEnumerable<T>> getList, Action<T> selectedChanged = null)
			: this(selectedChanged)
		{
			Contract.Requires<ArgumentException>(getList != null, "getList");

			this.fGetList = getList;
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="getListAsync">Передает метод (типа SrtData()) для использования при готовности данных</param>
		/// <param name="selectedChanged">Подписчик соответствующего события</param>
		public IEnumerableProperty(Action<Action<IEnumerable<T>>> getListAsync, Action<T> selectedChanged = null)
			: this(selectedChanged)
		{
			Contract.Requires<ArgumentException>(getListAsync != null, "getList");

			this.fGetListAsync = getListAsync;
		}

		/// <summary>
		/// Сброс списка в указанное значение
		/// </summary>
		/// <param name="data">Данные для списка</param>
		public virtual void Reset(IEnumerable<T> data)
		{
			try
			{
				this.SelectedItem = default(T);
				this._list = data;
				var l = this.List;	//	принудительное заполнение списка
				this.OnListChanged(this._list);
			}
			finally
			{
				this.OnIsWorkingChanged(false);
			}
		}

		/// <summary>
		/// Сброс списка, и принудительное заполнение из источника (из конструктора)
		/// </summary>
		public virtual void Reset()
		{
			this.Reset(null);
		}

		/// <summary>
		/// Сброс списка, и принудительное заполнение из источника (из конструктора) в другом потоке
		/// </summary>
		public virtual void ResetAsync()
		{
			this.OnIsWorkingChanged(true);
			System.Threading.ThreadPool.QueueUserWorkItem(o => this.Reset());
		}

		/// <summary>
		/// Поиск по списку
		/// </summary>
		/// <param name="pred">Услоаие поиска</param>
		/// <param name="changeCurrent">Следует ли заполнять результатом свойство Current</param>
		/// <returns></returns>
		public virtual T Select(Predicate<T> pred, bool changeCurrent = false)
		{
			return this.Select(pred, changeCurrent, true);
		}


		/// <summary>
		/// Создает список для поля List посредством вызова метод из конструктора
		/// </summary>
		/// <returns>Результат перегрузки с параметром от метода из конструктора</returns>
		protected virtual IEnumerable<T> GetListInternal()
		{
			if (this.fGetList != null)
			{
				var l = this.fGetList();
				return l;
			}

			if (this.fGetListAsync != null)
			{
				this.OnIsWorkingChanged(true);
				System.Threading.ThreadPool.QueueUserWorkItem(o => fGetListAsync((Action<IEnumerable<T>>)o), (object)(Action<IEnumerable<T>>)this.Reset);
			}

			return null;
		}

		/// <summary>
		/// Вызов OnPropertyChanged("List") и соответствующего события
		/// </summary>
		/// <param name="list">Список для передачи в событие</param>
		protected virtual void OnListChanged(IEnumerable<T> list)
		{
			this.OnPropertyChanged("List");
			if (this.ListChanged != null)
				this.ListChanged(this, new EventArgs<IEnumerable<T>>(list));
		}

		/// <summary>
		/// Установка значения IsWorking и вызов сопутствующих методов
		/// </summary>
		/// <param name="value"></param>
		protected virtual void OnIsWorkingChanged(bool value)
		{
			this.IsWorking = value;
			this.OnPropertyChanged("IsWorking");
			if (this.IsWorkingChanged != null)
				this.IsWorkingChanged(this, EventArgs.Empty);
		}

		/// <summary>
		/// Вызов OnPropertyChanged("Current") и соответствующего события
		/// </summary>
		/// <param name="value">Выбор для передачи в событие</param>
		protected override void OnSelectedChanged(T value)
		{
			base.OnSelectedChanged(value);
			this.OnPropertyChanged("Current");
		}

		/// <summary>
		/// Запускается исключительно из свойства List + запуск событий
		/// </summary>
		/// <returns></returns>
		private IEnumerable<T> GetList()
		{
			IEnumerable<T> l = null;
#if !SILVERLIGHT
			this.Debug("()");
			try
			{
#endif
				l = this.GetListInternal();
				if (!object.Equals(this._list, l))
				{
					this._list = l;
					this.OnListChanged(this._list);
				}
#if !SILVERLIGHT
			}
			catch (Exception ex)
			{
				this.Warn(ex, "()");
			}
#endif

			return this._list;
		}

	}

	/// <summary>
	/// Класс для удобной работы со списками в WPF + Caption
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class IEnumerablePropertyEx<T> : IEnumerableProperty<T>
	{

		/// <summary>
		/// Заголовок свойства
		/// </summary>
		public virtual string Caption { get; protected set; }

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="caption">Заголовок</param>
		protected IEnumerablePropertyEx(string caption)
		{
			this.Caption = caption;
		}


		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="caption">Заголовок</param>
		/// <param name="getList">Метод для получения записей</param>
		/// <param name="selectedChanged"></param>
		public IEnumerablePropertyEx(string caption, Func<IEnumerable<T>> getList, Action<T> selectedChanged = null)
			: base(getList, selectedChanged)
		{
			this.Caption = caption;
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="caption">Заголовок</param>
		/// <param name="getListAsync">Передает метод (типа SrtData()) для использования при готовности данных</param>
		/// <param name="selectedChanged">Подписчик соответствующего события</param>
		public IEnumerablePropertyEx(string caption, Action<Action<IEnumerable<T>>> getListAsync, Action<T> selectedChanged = null)
			: base(getListAsync, selectedChanged)
		{
			this.Caption = caption;
		}

	}
}
