using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace IT.WPF.Filters
{
	/// <summary>
	/// Управляет фильтрацией и визуализацией фильтров всех колонок
	/// </summary>
	/// <invariant>_dataGrid != null</invariant>
	/// <invariant>_filterColumnControls != null</invariant>
	public class DataGridFilterCore : ILog
    {
		private readonly DataGrid _dg;
		
		/// <summary>Заголовки фильтра, появляются при инициализации DataGridFilterHeader</summary>
		private readonly List<DataGridFilterHeader> _filterColumnControls = new List<DataGridFilterHeader>();

		/// <summary> Признак включенной фильтрации </summary>
		private bool _isFilteringEnabled;

		/// <summary> Timer to defer evaluation of the filter until user has stopped typing. </summary>
		private DispatcherTimer _deferFilterEvaluationTimer;

		/// <summary> .ctor </summary>
		/// <param name="dg"></param>
		public DataGridFilterCore(DataGrid dg)
		{
			_dg = dg;
			dg.Columns.CollectionChanged += Columns_CollectionChanged;
		}

		/// <summary> Включение/отключение фильтрации </summary>
		/// <param name="val"></param>
		public void Enable(bool val)
		{

		}

		/// <summary>Adds a new column.</summary>
		/// <param name="filterColumn"></param>
		internal void AddColumn(DataGridFilterHeader filterColumn)
		{
			filterColumn.Visibility = this._isFilteringEnabled ? Visibility.Visible : Visibility.Hidden;
			this._filterColumnControls.Add(filterColumn);
		}

		/// <summary>Removes an unloaded column.</summary>
		/// <requires csharp="filterColumn != null" vb="filterColumn &lt;&gt; Nothing">filterColumn != null</requires>
		internal void RemoveColumn(DataGridFilterHeader filterColumn)
		{
			this._filterColumnControls.Remove(filterColumn);
			this.OnFilterChanged();
		}

		/// <summary>
		/// When any filter condition has changed restart the evaluation timer to defer
		/// the evaluation until the user has stopped typing.
		/// </summary>
		internal void OnFilterChanged()
		{
			if (this._isFilteringEnabled)
			{
				this._dg.CommitEdit();
				this._dg.CommitEdit();
				if (this._deferFilterEvaluationTimer == null)
					this._deferFilterEvaluationTimer = new DispatcherTimer(TimeSpan.FromSeconds(0.5), DispatcherPriority.Input, ((_, __) => this.EvaluateFilter()), Dispatcher.CurrentDispatcher);
				this._deferFilterEvaluationTimer.Stop();
				this._deferFilterEvaluationTimer.Start();
			}
		}

		internal IList<DataGridFilterHeader> GetColumnFilters(DataGridFilterHeader excluded = null)
		{
			return this._filterColumnControls
				.Where(column => column != excluded)
				.Where(column => column.IsVisible && column.IsFiltered)
				.ToArray();
		}

		internal Predicate<object> CreatePredicate(IList<DataGridFilterHeader> columnFilters)
		{
			//if (columnFilters?.Any() != true)
			//	return this._globalFilter;

			//if (this._globalFilter == null)
			//	return (i => columnFilters.All(filter => filter.Matches(i)));

			//return (Predicate<object>)(item => this._globalFilter(item)) && columnFilters.All(filter => filter.Matches(item)));

			return (i => columnFilters.All(filter => filter.Matches(i)));
		}

		/// <summary>Creates a new content filter.</summary>
		internal IContentFilter CreateContentFilter(object content) => this._dg.GetContentFilterFactory().Create(content);


		private void Columns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			try
			{
				if(e.NewItems != null)
				{
					var array = e.NewItems
						.OfType<DataGridColumn>()
						.Where(column => /*column.GetIsFilterVisible() && */column.HeaderTemplate == null)
						.ToArray();

					if (array.Any())
					{
						object obj = this._dg.FindResource(DataGridFilter.ColumnHeaderTemplateKey);
						DataTemplate dataTemplate = (DataTemplate)obj;
						foreach (DataGridColumn dataGridColumn in array)
							dataGridColumn.HeaderTemplate = dataTemplate;
					}
				}
			}
			catch (Exception ex)
			{
				this.Error(ex);
			}
		}

		/// <summary> Evaluates the current filters and applies the filtering to the collection view of the items control. </summary>
		private void EvaluateFilter()
		{
			this._deferFilterEvaluationTimer?.Stop();
			ItemCollection items = this._dg.Items;
			IList<DataGridFilterHeader> columnFilters = this.GetColumnFilters(null);

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

			try
			{
				items.Filter = this.CreatePredicate(columnFilters);
				foreach (DataGridFilterHeader filterColumnControl in this._filterColumnControls)
					filterColumnControl.ValuesUpdated();
			}
			catch (InvalidOperationException ex)
			{
			}
		}

	}
}
