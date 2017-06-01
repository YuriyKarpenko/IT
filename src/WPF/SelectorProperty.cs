using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// The base class for easy work with lists in WPF
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SelectorPropertyBase<T> : NotifyPropertyChangedBase
    {
        private T selectedItem;


        /// <summary>
        /// Возникает при изменении свойчтва
        /// </summary>
        public event EventHandler<EventArgs<T>> SelectedChanged;

        #region Properties

        /// <summary>
        /// Список
        /// </summary>
        protected abstract IEnumerable<T> Inner_List { get; }

        /// <summary>
        /// Текущее значение
        /// </summary>
        public virtual T SelectedItem
        {
            get { return this.selectedItem; }
            set { this.SelectedItem_Set(value); }
        }

        /// <summary>
        /// Выбран ли какой-либо элемент в данный момент
        /// </summary>
        public bool HasSelected => this.SelectedItem != null;

        #endregion

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
                this.SelectedChanged += (s, e) => selectedChanged(e.Value);
        }


        /// <summary>
        /// Поиск по списку
        /// </summary>
        /// <param name="pred">Услоаие поиска</param>
        /// <param name="changeCurrent">Следует ли заполнять результатом свойство Current</param>
        /// <param name="isRaiseEvent">Следует ли вызывать SelectedChanged</param>
        /// <returns></returns>
        public virtual T Select(Predicate<T> pred, bool changeCurrent = false, bool isRaiseEvent = true)
        {
            T ret = this.Inner_List == null ? default(T) : this.Inner_List.FirstOrDefault(i => pred(i));
            if (changeCurrent)
            {
                this.SelectedItem_Set(ret, isRaiseEvent);
            }
            return ret;
        }

        /// <summary>
        /// Устанавливает значение this._Current
        /// </summary>
        /// <param name="value">Значение</param>
        /// <param name="isRaiseEvent">Следует ли вызывать SelectedChanged</param>
        protected virtual void SelectedItem_Set(T value, bool isRaiseEvent = true)
        {
            if (!object.Equals(this.selectedItem, value))
            {
                this.selectedItem = value;
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
        /// Вызов OnPropertyChanged("Current") и соответствующего события
        /// </summary>
        /// <param name="value">Выбор для передачи в событие</param>
        protected virtual void OnSelectedChanged(T value)
        {
            this.OnPropertyChanged("SelectedItem");
            this.SelectedChanged?.Invoke(this, new EventArgs<T>(value));
        }
    }


    /// <summary>
    /// Класс для удобной работы с выделением элементов списками
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SelectorPropertyReadOnly<T> : SelectorPropertyBase<T>
    {
        /// <summary>
        /// Inner_List for SelectorPropertyBase
        /// </summary>
        protected override IEnumerable<T> Inner_List => List;

        /// <summary>
        /// List
        /// </summary>
        public virtual IEnumerable<T> List { get; private set; }


        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="list"></param>
        /// <param name="onSelectedChanged"></param>
        public SelectorPropertyReadOnly(IEnumerable<T> list, Action<T> onSelectedChanged = null) : base(onSelectedChanged)
        {
            Contract.NotNull(list, "list");
            this.List = list;
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
        /// Inner_List for SelectorPropertyBase
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
        public virtual TList List { get { return this.list ?? this.GetList(); } }

        /// <summary>
        /// Признак работы в асинхронном режиме
        /// </summary>
        public bool IsWorking { get; private set; }

        #endregion


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
        public SelectorProperty(TList data, Action<T> selectedChanged = null)
            : base(selectedChanged)
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
        public SelectorProperty(Action<Action<TList>> getListAsync, Action<T> selectedChanged = null)
            : base(selectedChanged)
        {
            Contract.NotNull(getListAsync, "getListAsync");

            this.fGetListAsync = getListAsync;
        }


        #region reset

        /// <summary>
        /// Сброс списка в указанное значение
        /// </summary>
        /// <param name="data">Данные для списка</param>
        public virtual void Reset(TList data = default(TList))
        {
            try
            {
                this.SelectedItem = default(T);
                this.list = data;
                var l = this.List;  //	принудительное заполнение списка
                this.OnListChanged(this.list);
            }
            finally
            {
                this.OnIsWorkingChanged(false);
            }
        }

        /// <summary>
        /// Сброс списка, и принудительное заполнение из источника (из конструктора) в другом потоке
        /// </summary>
        public virtual void ResetAsync(TList data = default(TList))
        {
            this.OnIsWorkingChanged(true);
            ThreadPool.QueueUserWorkItem(o => this.Reset(data));
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
                this.OnIsWorkingChanged(true);
                System.Threading.ThreadPool.QueueUserWorkItem(o => fGetListAsync((Action<TList>)o), (object)(Action<TList>)this.Reset);
            }

            return null;
        }

        /// <summary>
        /// Вызов OnPropertyChanged("List") и соответствующего события
        /// </summary>
        /// <param name="list">Список для передачи в событие</param>
        protected virtual void OnListChanged(TList list)
        {
            this.OnPropertyChanged("List");
            this.ListChanged?.Invoke(this, new EventArgs<TList>(list));
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
        /// Запускается исключительно из свойства List + запуск событий
        /// </summary>
        /// <returns></returns>
        private TList GetList()
        {
            TList l = null;
#if !SILVERLIGHT
            this.Debug("()");
            try
            {
#endif
                l = this.GetList_Internal();
                if (!object.Equals(this.list, l))
                {
                    this.list = l;
                    this.OnListChanged(this.list);
                }
#if !SILVERLIGHT
            }
            catch (Exception ex)
            {
                this.Warn(ex, "()");
            }
#endif

            return this.list;
        }
    }


    /// <summary>
    /// Класс для удобной работы с выделением элементов списками
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
    /// Класс для удобной работы с выделением элементов списками с использованием ObservableCollection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SelectorPropertyWPF<T> : SelectorProperty<ObservableCollection<T>, T>
    {
        //private bool? isValid = null;
        //public string Color => isValid.HasValue ? (isValid.Value ? Color_Verified : Color_Bad) : Color_Normal;

        /// <summary>
        /// ctor
        /// </summary>
        public SelectorPropertyWPF(Action<T> onSelect = null) : base(new ObservableCollection<T>(), onSelect)
        {
        }
        /// <summary>
        /// ctor
        /// </summary>
        public SelectorPropertyWPF(IEnumerable<T> data, Action<T> onSelect = null) : base(new ObservableCollection<T>(data), onSelect)
        {
        }
        /// <summary>
        /// ctor
        /// </summary>
        public SelectorPropertyWPF(Func<ObservableCollection<T>> getList, Action<T> onSelect = null) : base(getList, onSelect)
        {
        }
        /// <summary>
        /// ctor
        /// </summary>
        public SelectorPropertyWPF(Action<Action<ObservableCollection<T>>> getListAsync, Action<T> onSelect = null) : base(getListAsync, onSelect)
        {
        }
    }

}
