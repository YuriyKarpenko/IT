using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows.Data;

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
	/// Абстрактный Класс для удобной работы с выделением элементов ...
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class SelectorPropertyBase<T> : NotifyPropertyChangedBase
	{
		/// <summary>
		/// Возникает при изменении свойчтва
		/// </summary>
		public event EventHandler<EventArgs<T>> SelectedChanged;

		// Properties
		/// <summary>
		/// Список
		/// </summary>
		protected abstract IEnumerable<T> Inner_List { get; }

		/// <summary>
		/// Текущее значение
		/// </summary>
		public virtual T SelectedItem
		{
			get { return selectedItem; }
			set { SelectedItem_Set(value, true); }
		}
		private T selectedItem;
		/// <summary>
		/// Устанавливает значение this._Current
		/// </summary>
		/// <param name="value">Значение</param>
		/// <param name="isRaiseEvent">Следует ли вызывать SelectedChanged</param>
		protected virtual void SelectedItem_Set(T value, bool isRaiseEvent)
		{
			if (!object.Equals(selectedItem, value))
			{
				selectedItem = value;
				OnPropertyChanged(nameof(SelectedItem));

				if (isRaiseEvent)
					OnSelectedChanged(value);

				OnPropertyChanged(nameof(HasSelected));
			}
		}

		/// <summary>
		/// Выбран ли какой-либо элемент в данный момент
		/// </summary>
		public bool HasSelected => !Equals(SelectedItem, default(T));

		//	ctors
		/// <summary>
		/// Constructor, only for heirs
		/// </summary>
		protected SelectorPropertyBase() { }

		/// <summary>
		/// constructor, only for heirs. Bind a method to an event
		/// </summary>
		/// <param name="selectedChanged">Метод, запускаемый при возникновении события</param>
		protected SelectorPropertyBase(Action<T> selectedChanged)
		{
			if (selectedChanged != null)
				SelectedChanged += (s, e) => selectedChanged(e.Value);
		}

		//	publics
		/// <summary>
		/// Поиск по списку
		/// </summary>
		/// <param name="pred">Услоаие поиска</param>
		/// <param name="changeCurrent">Следует ли заполнять результатом свойство Current</param>
		/// <param name="isRaiseEvent">Следует ли вызывать SelectedChanged</param>
		/// <returns></returns>
		public virtual T Select(Func<T, bool> pred, bool changeCurrent = false, bool isRaiseEvent = true)
		{
			T ret = Inner_List == null ? default(T) : Inner_List.FirstOrDefault(pred);
			if (changeCurrent)
			{
				SelectedItem_Set(ret, isRaiseEvent);
			}
			return ret;
		}

		//	protecteds
		/// <summary>
		/// Вызов OnPropertyChanged("Current") и соответствующего события
		/// </summary>
		/// <param name="value">Выбор для передачи в событие</param>
		protected virtual void OnSelectedChanged(T value)
		{
			SelectedChanged?.Invoke(this, new EventArgs<T>(value));
		}
	}


	/// <summary>
	/// Класс для удобной работы с выделением элементов из константных списков
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SelectorPropertyReadOnly<T> : SelectorPropertyBase<T>
	{
		/// <summary>
		/// Inner_List for SelectorPropertyBase
		/// </summary>
		protected override IEnumerable<T> Inner_List => ItemsSource;

		/// <summary>
		/// List
		/// </summary>
		public virtual IEnumerable<T> ItemsSource { get; private set; }


		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="list"></param>
		/// <param name="onSelectedChanged"></param>
		public SelectorPropertyReadOnly(IEnumerable<T> list, Action<T> onSelectedChanged = null) : base(onSelectedChanged)
		{
			Contract.NotNull(list, "list");
			this.ItemsSource = list;
		}
	}


	/// <summary>
	/// class for easy work with lists in WPF
	/// </summary>
	/// <typeparam name="TList"></typeparam>
	/// <typeparam name="T"></typeparam>
	public class SelectorProperty<TList, T> : SelectorPropertyBase<T> where TList : class, IEnumerable<T>
	{
		private TList list;

		/// <summary>
		/// overrided abstract property Inner_List (for SelectorPropertyBase)
		/// </summary>
		protected override IEnumerable<T> Inner_List => list;


		#region Events

		/// <summary>
		/// Event
		/// </summary>
		public event EventHandler<EventArgs<TList>> ListChanged;

		/// <summary>
		/// Возникает при смене состояния IsWorking (используется только при асинхронных операциях)
		/// </summary>
		public event EventHandler IsWorkingChanged;
		//public event NotifyCollectionChangedEventHandler CollectionChanged;

		#endregion


		/// <summary>
		/// Метод получения списка из конструктора
		/// </summary>
		protected readonly Func<TList> fGetList;

		/// <summary>
		/// Метод заполнения списка из конструктора
		/// </summary>
		protected readonly Action<Action<TList>> fGetListAsync;

		#region Properties

		/// <summary>
		/// Список
		/// </summary>
		public virtual TList List => list ?? List_Set(GetList_Internal());
		/// <summary>
		/// Проверка на отличие + Назначение list + запуск событий
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		protected virtual TList List_Set(TList data)
		{
			this.Debug("()");
			try
			{
				if (!object.Equals(this.list, data))
				{
					SelectedItem = default(T);  //	
					list = data;
					this.OnListChanged(this.list);
				}
			}
			catch (Exception ex)
			{
				this.Warn(ex, $"({data})");
			}
			finally
			{
				OnIsWorkingChanged(false);
			}
			return list;
		}

		/// <summary>
		/// Признак работы в асинхронном режиме
		/// </summary>
		public bool IsWorking { get; private set; }

		#endregion

		//	ctors
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="selectedChanged">Подписчик соответствующего события</param>
		protected SelectorProperty(Action<T> selectedChanged = null)
			: base(selectedChanged)
		{
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="data">Функсия получения списка</param>
		/// <param name="selectedChanged">Подписчик соответствующего события</param>
		public SelectorProperty(TList data, Action<T> selectedChanged = null) : base(selectedChanged)
		{
			this.Reset(data);
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="getList">Функсия получения списка</param>
		/// <param name="selectedChanged">Подписчик соответствующего события</param>
		public SelectorProperty(Func<TList> getList, Action<T> selectedChanged = null)
			: base(selectedChanged)
		{
			Contract.Requires<ArgumentException>(getList != null, "getList");

			this.fGetList = getList;
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="getListAsync">Передает метод (типа SrtData()) для использования при готовности данных</param>
		/// <param name="selectedChanged">Подписчик соответствующего события</param>
		public SelectorProperty(Action<Action<TList>> getListAsync, Action<T> selectedChanged = null) : base(selectedChanged)
		{
			Contract.NotNull(getListAsync, "getListAsync");

			this.fGetListAsync = getListAsync;
		}


		#region reset

		/// <summary>
		/// Сброс списка в указанное значение
		/// </summary>
		/// <param name="data">Данные для списка</param>
		public virtual void Reset(TList data)
		{
			List_Set(data);
		}

		/// <summary>
		/// Сброс списка, и принудительное заполнение из источника (из конструктора)
		/// </summary>
		public virtual void Reset()
		{
			List_Set(null);
			List_Set(GetList_Internal());
		}

		/// <summary>
		/// Сброс списка, и принудительное заполнение из источника (из конструктора) в другом потоке
		/// </summary>
		public virtual void ResetAsync()
		{
			OnIsWorkingChanged(true);
			ThreadPool.QueueUserWorkItem(o => Reset());
		}

		#endregion


		/// <summary>
		/// Создает список для поля List посредством вызова метод из конструктора
		/// </summary>
		/// <returns>Результат перегрузки с параметром от метода из конструктора</returns>
		protected virtual TList GetList_Internal()
		{
			if (this.fGetList != null)
			{
				var l = this.fGetList();
				return l;
			}

			if (this.fGetListAsync != null)
			{
				OnIsWorkingChanged(true);
				ThreadPool.QueueUserWorkItem(o => fGetListAsync((Action<TList>)o), (Action<TList>)Reset);
			}

			return null;
		}

		/// <summary>
		/// Вызов OnPropertyChanged("List") и соответствующего события
		/// </summary>
		/// <param name="list">Список для передачи в событие</param>
		protected virtual void OnListChanged(TList list)
		{
			OnPropertyChanged(nameof(List));
			ListChanged?.Invoke(this, new EventArgs<TList>(list));
		}

		/// <summary>
		/// Установка значения IsWorking и вызов сопутствующих методов
		/// </summary>
		/// <param name="value"></param>
		protected virtual void OnIsWorkingChanged(bool value)
		{
			IsWorking = value;
			OnPropertyChanged(nameof(IsWorking));
			IsWorkingChanged?.Invoke(this, EventArgs.Empty);
		}



	}

	/// <summary>
	/// Класс для удобной работы с выделением элементов списками с использованием IEnumerable<T/>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SelectorProperty<T> : SelectorProperty<IEnumerable<T>, T>
	{
		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="getList"></param>
		/// <param name="selectedChanged"></param>
		public SelectorProperty(Func<IEnumerable<T>> getList, Action<T> selectedChanged = null)
			: base(getList, selectedChanged)
		{
		}
	}

	/// <summary>
	/// Класс для удобной работы с выделением элементов списками с использованием ObservableCollection<T/>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SelectorPropertyWPF<T> : SelectorProperty<ObservableCollection<T>, T> where T : class
	{
		//private Action<ObservableCollection<T>> _getListAsync;
		//private bool? isValid = null;
		//public string Color => isValid.HasValue ? (isValid.Value ? Color_Verified : Color_Bad) : Color_Normal;

		//	constructors
		/// <summary>
		/// ctor
		/// </summary>
		public SelectorPropertyWPF(Action<T> onSelect) : base(new ObservableCollection<T>(), onSelect) { }
		
		/// <summary>
		/// ctor
		/// </summary>
		public SelectorPropertyWPF(IEnumerable<T> data, Action<T> onSelect = null) : base(new ObservableCollection<T>(data), onSelect) { }
		
		/// <summary>
		/// ctor
		/// </summary>
		public SelectorPropertyWPF(Func<ObservableCollection<T>> getList, Action<T> onSelect = null) : base(getList, onSelect) { }
		
		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="fillListAsync"></param>
		/// <param name="onSelect"></param>
		public SelectorPropertyWPF(Action<Action<ObservableCollection<T>>> fillListAsync, Action<T> onSelect = null) : base(fillListAsync, onSelect) { }
		
		///// <summary>
		///// ctor
		///// </summary>
		//public SelectorPropertyWPF(Action<ObservableCollection<T>> fillListAsync, Action<T> onSelect = null) : base(onSelect)
		//{
		//	Contract.NotNull(fillListAsync, nameof(fillListAsync));
		//	_getListAsync = fillListAsync;
		//}

		//protected override ObservableCollection<T> GetList_Internal()
		//{
		//	ThreadPool.QueueUserWorkItem(o =>
		//	{
		//		var l = new ObservableCollection<T>();
		//		_getListAsync(l);
		//		Reset(l);
		//	});
		//	return null;// base.GetList_Internal();
		//}

		//IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();	base

		#endregion
	}
}
