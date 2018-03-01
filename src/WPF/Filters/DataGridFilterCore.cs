using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace IT.WPF.Filters
{
	/// <summary> Управляет фильтрацией и визуализацией фильтров всех колонок </summary>
	public interface IFilterManager
	{
		/// <summary> Признак включенной фильтрации </summary>
		DataGridFilters FilterEnabled { get; }

		/// <summary> Предоставляет фабрику фильтров содержимого. </summary>
		IContentFilterFactory FilterFactory { get; }

		/// <summary> Данные расширяемого контрола </summary>
		IEnumerable ItemsSource { get; }

		/// <summary> Данные расширяемого контрола </summary>
		ItemCollection Items { get; }


		/// <summary> Добавляет новый столбец. </summary>
		/// <param name="filterColumn"></param>
		void AddColumn(IFilterHeaderControl filterColumn);

		/// <summary> Удаляет разгруженный столбец. </summary>
		/// <requires csharp="filterColumn != null" vb="filterColumn &lt;&gt; Nothing">filterColumn != null</requires>
		void RemoveColumn(IFilterHeaderControl filterColumn);

		/// <summary> Уведомляет об изменении какого-либо фильтра </summary>
		void OnFilterChanged();
	}

	/// <summary> Управляет фильтрацией и визуализацией фильтров всех колонок </summary>
	public abstract class FilterCore<T> : NotifyPropertyChangedBase, IFilterManager, ILog where T : ItemsControl
	{
		/// <summary> Предоставляет фабрику фильтров содержимого по умолчанию (SimpleContentFilterFactory). </summary>
		public static IContentFilterFactory DefaultFilterFactory { get; set; } = new SimpleContentFilterFactory( StringComparison.CurrentCultureIgnoreCase);

		/// <summary> Признак процесса фильтрации </summary>
		public bool IsWorking { get; private set; }

		#region IFilterCore

		/// <summary> Признак включенной фильтрации </summary>
		public DataGridFilters FilterEnabled { get; private set; }

		/// <summary> Предоставляет фабрику фильтров содержимого. </summary>
		public virtual IContentFilterFactory FilterFactory => DefaultFilterFactory;

		IEnumerable IFilterManager.ItemsSource => ItemsControl.ItemsSource;

		ItemCollection IFilterManager.Items => ItemsControl.Items;


		void IFilterManager.AddColumn(IFilterHeaderControl filterColumn)
		{
			filterColumn.Visibility = FilterVisibility;
			_filterHeaderControls.Add(filterColumn);
		}

		void IFilterManager.RemoveColumn(IFilterHeaderControl filterColumn)
		{
			_filterHeaderControls.Remove(filterColumn);
			this.OnFilterChanged();
		}

		/// <summary> Уведомляет об изменении какого-либо фильтра </summary>
		public virtual void OnFilterChanged()
		{
			EvaluateFilter();
		}

		#endregion

		/// <summary>
		/// представление фильтров
		/// </summary>
		private Visibility FilterVisibility => FilterEnabled == DataGridFilters.Disabled ? Visibility.Hidden : Visibility.Visible;


		/// <summary> Списочный элемкнт управления, в котором применяется фмльтр (DataGrid, List, ListView etc ...) </summary>
		protected T ItemsControl { get; private set; }

		/// <summary> Заголовки фильтра, появляются при инициализации DataGridFilterHeader</summary>
		private readonly List<IFilterHeaderControl> _filterHeaderControls = new List<IFilterHeaderControl>();


		/// <summary> .ctor </summary>
		/// <param name="itemsControl"></param>
		public FilterCore(T itemsControl)
		{
			ItemsControl = itemsControl;
		}


		/// <summary> Включение/отключение фильтрации + изменение видимости фильтров </summary>
		/// <param name="value"></param>
		public virtual void Enable(DataGridFilters value)
		{
			FilterEnabled = value;
			foreach (UIElement filterColumnControl in _filterHeaderControls)
				filterColumnControl.Visibility = FilterVisibility;
			EvaluateFilter();
		}

		/// <summary> получение колонок, участвующих в фильтрации </summary>
		/// <param name="excluded"></param>
		/// <returns></returns>
		protected virtual IList<IFilterHeaderControl> GetColumnFilters(params IFilterHeaderControl[] excluded)
		{
			var res = _filterHeaderControls
				//.Where(column => column != excluded)
				.Except(excluded)
				.Where(column => column.IsVisible && column.IsFiltered)
				.ToArray();
			return res;
		}

		/// <summary> Создание условия фильтрации по заданным фильтрам </summary>
		/// <param name="columnFilters"></param>
		/// <returns></returns>
		protected internal virtual Predicate<object> CreatePredicate(IEnumerable<IFilterHeaderControl> columnFilters)
		{
			if (columnFilters?.Any() == true)
				return (i => columnFilters.All(filter => filter.Matches(i)));

			return item => true;
		}

		/// <summary> Вычисляет текущие фильтры и применяет фильтрацию к представлению коллекции элемента управления элемента. </summary>
		protected virtual void EvaluateFilter()
		{
			var columnFilters = GetColumnFilters();

			IsWorking = true;
			OnPropertyChanged(nameof(IsWorking));
			var oldCursor = ItemsControl.Cursor;
			ItemsControl.Cursor = Cursors.Wait;
			try
			{
				ItemsControl.Items.Filter = this.CreatePredicate(columnFilters);

				foreach (IFilterHeaderControl filterColumnControl in _filterHeaderControls)
					filterColumnControl.ValuesUpdated();
			}
			catch (InvalidOperationException ex)
			{
				this.Warn(ex);
			}
			catch (Exception ex)
			{
				this.Error(ex);
			}
			finally
			{
				IsWorking = false;
				ItemsControl.Cursor = oldCursor;
				OnPropertyChanged(nameof(IsWorking));
			}
		}

	}

	/// <summary>
	/// Управляет фильтрацией и визуализацией фильтров всех колонок
	/// </summary>
	/// <invariant>_dataGrid != null</invariant>
	/// <invariant>_filterColumnControls != null</invariant>
	public class DataGridFilterCore : FilterCore<DataGrid>
	{
		///// <summary>Occurs before new columns are filtered.</summary>
		//public event EventHandler<DataGridFilteringEventArgs> Filtering;

		///// <summary>Occurs when any filter has changed.</summary>
		//public event EventHandler FilterChanged;


		/// <summary> Таймер задежки применения фильтра до тех пор, пока пользователь не перестанет печатать. </summary>
		private DispatcherTimer _deferFilterEvaluationTimer;

		/// <summary> Глобальный фильтр, который применяется в дополнение к фильтрам столбцов. </summary>
		private Predicate<object> _globalFilter;


		/// <summary> .ctor </summary>
		/// <param name="dg"></param>
		public DataGridFilterCore(DataGrid dg) : base(dg)
		{
			dg.Columns.CollectionChanged += Columns_CollectionChanged;

			//dataGrid.Loaded += new RoutedEventHandler(this.DataGrid_Loaded);

			//	растянуть заголовок по ширине
			if (dg.ColumnHeaderStyle == null)
			{
				Style style = new Style(typeof(DataGridColumnHeader), (Style)dg.FindResource(typeof(DataGridColumnHeader)));
				style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
				dg.ColumnHeaderStyle = style;
			}
		}


		/// <summary> Включение/отключение фильтрации </summary>
		/// <param name="value"></param>
		public override void Enable(DataGridFilters value)
		{
			base.Enable(value);
			SetHeaderTemplate(ItemsControl.Columns);
		}

		/// <summary>
		/// Когда какое-либо условие фильтра изменилось, перезапустите таймер оценки, чтобы отложить оценку до тех пор, пока пользователь не перестанет печатать.
		/// </summary>
		public override void OnFilterChanged()
		{
			if (this.FilterEnabled != DataGridFilters.Disabled)
			{
				this.ItemsControl.CommitEdit();
				this.ItemsControl.CommitEdit();
				if (this._deferFilterEvaluationTimer == null)
					this._deferFilterEvaluationTimer = new DispatcherTimer(ItemsControl.GetFilterEvaluationDelay(), DispatcherPriority.Input, (_, __) => EvaluateFilter(), Dispatcher.CurrentDispatcher);
				this._deferFilterEvaluationTimer.Stop();
				this._deferFilterEvaluationTimer.Start();
			}
		}

		internal void SetGlobalFilter(Predicate<object> globalFilter)
		{
			_globalFilter = globalFilter;
			OnFilterChanged();
		}

		/// <summary> Создание условия фильтрации по заданным фильтрам </summary>
		/// <param name="columnFilters"></param>
		/// <returns></returns>
		protected internal override Predicate<object> CreatePredicate(IEnumerable<IFilterHeaderControl> columnFilters)
		{
			if (columnFilters?.Any() != true)
				return this._globalFilter;

			var basePredicate = base.CreatePredicate(columnFilters);
			if (_globalFilter == null)
				return basePredicate;
			else
				return item => _globalFilter(item) && basePredicate(item);
		}

		private void DataGrid_Loaded(object sender, RoutedEventArgs e)
		{
			object obj1 = ItemsControl.Template?.FindName("DG_ScrollViewer", (FrameworkElement)ItemsControl);

			if (obj1 is ScrollViewer scrollViewer1)
			{
				object obj2 = scrollViewer1.Template?.FindName("PART_ColumnHeadersPresenter", (FrameworkElement)scrollViewer1);

				if (obj2 is FrameworkElement frameworkElement)
				{
					// ISSUE: variable of a boxed type
					frameworkElement.SetValue(KeyboardNavigation.TabNavigationProperty, KeyboardNavigationMode.None);
				}
			}
		}

		//	поиск и установка HeaderTemplate для новых колонок
		private void Columns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			try
			{
				if (e.NewItems != null)
				{
					var columns = e.NewItems
						.OfType<DataGridColumn>()
						.Where(column => /*column.GetIsFilterVisible() && */column.HeaderTemplate == null)
						.ToArray();
					SetHeaderTemplate(columns);
				}
			}
			catch (Exception ex)
			{
				this.Error(ex);
			}
		}

		/// <summary> Evaluates the current filters and applies the filtering to the collection view of the items control. </summary>
		protected override void EvaluateFilter()
		{
			this._deferFilterEvaluationTimer?.Stop();

			// ISSUE: reference to a compiler-generated field
			//if (this.Filtering != null)
			//{
			//	DataGridColumn[] array1 = columnFilters
			//		.Select(filter => filter.Column)
			//		.Where(column => column != null)
			//		.ToArray<DataGridColumn>();
			//	DataGridColumn[] array2 = array1
			//		.Except<DataGridColumn>(this._filteredColumns)
			//		.ToArray<DataGridColumn>();
			//	if (array2.Length != 0)
			//	{
			//		DataGridFilteringEventArgs e = new DataGridFilteringEventArgs((ICollection<DataGridColumn>)array2);
			//		// ISSUE: reference to a compiler-generated field
			//		this.Filtering((object)this._dataGrid, e);
			//		if (e.Cancel)
			//			return;
			//	}
			//	this._filteredColumns = array1;
			//}

			// ISSUE: reference to a compiler-generated field
			//EventHandler eventHandler = this.FilterChanged;
			//if (eventHandler != null)
			//{
			//	EventArgs e = EventArgs.Empty;
			//	eventHandler((object)this, e);
			//}

			base.EvaluateFilter();
		}

		private void SetHeaderTemplate(IEnumerable<DataGridColumn> columns)
		{
			if (columns?.Any() == true)
			{
				object key = null;
				switch (FilterEnabled)
				{
					case DataGridFilters.ComboBox:
						key = DataGridFilter.HeadegFilter_DockButtom_TemplateKey;
						break;
					case DataGridFilters.TextBoxContains:
					//case DataGridFilters.TextBoxContains:
						key = DataGridFilter.HeadegFilter_DockRight_TemplateKey;
						break;
				}

				DataTemplate dataTemplate = (DataTemplate)ItemsControl.FindResource(key, ItemsControl);

				foreach (DataGridColumn dataGridColumn in columns)
					dataGridColumn.HeaderTemplate = dataTemplate;
			}
		}
	}
}
