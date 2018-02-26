// Decompiled with JetBrains decompiler
// Type: DataGridExtensions.DataGridFilterHost
// Assembly: DataGridExtensions, Version=1.0.37.0, Culture=neutral, PublicKeyToken=43de855f87de903a
// MVID: 5A6EF75F-94BB-4773-88B0-C65701D66FE0
// Assembly location: D:\SVN\NPF\release\packages\DataGridExtensions.1.0.37\lib\net40-Client\DataGridExtensions.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace IT.WPF.JetBrains
{
	/// <summary>
	/// This class hosts all filter columns and handles the filter changes on the data grid level.
	/// This class will be attached to the DataGrid.
	/// </summary>
	/// <invariant>_dataGrid != null</invariant>
	/// <invariant>_filterColumnControls != null</invariant>
	public sealed class DataGridFilterHost
	{
		/// <summary>Filter information about each column.</summary>
		private readonly List<DataGridFilterColumnControl> _filterColumnControls = new List<DataGridFilterColumnControl>();
		/// <summary>The columns that we are currently filtering.</summary>
		private DataGridColumn[] _filteredColumns = new DataGridColumn[0];
		/// <summary>The data grid this filter is attached to.</summary>
		private readonly DataGrid _dataGrid;
		/// <summary> Timer to defer evaluation of the filter until user has stopped typing. </summary>
		private DispatcherTimer _deferFilterEvaluationTimer;
		/// <summary>Flag indicating if filtering is currently enabled.</summary>
		private bool _isFilteringEnabled;
		/// <summary> A global filter that is applied in addition to the column filters. </summary>
		private Predicate<object> _globalFilter;

		/// <summary>
		/// Gets a the active filter column controls for this data grid.
		/// </summary>
		/// <getter>
		///   <ensures csharp="Contract.Result&lt;System.Collections.Generic.IList&lt;DataGridExtensions.DataGridFilterColumnControl&gt;&gt;() != null" vb="Contract.Result(Of System.Collections.Generic.IList(Of DataGridExtensions.DataGridFilterColumnControl))() &lt;&gt; Nothing">result != null</ensures>
		/// </getter>
		public IList<DataGridFilterColumnControl> FilterColumnControls => new ReadOnlyCollection<DataGridFilterColumnControl>(this._filterColumnControls);

		/// <summary>The data grid this filter is attached to.</summary>
		/// <getter>
		///   <ensures csharp="Contract.Result&lt;System.Windows.Controls.DataGrid&gt;() != null" vb="Contract.Result(Of System.Windows.Controls.DataGrid)() &lt;&gt; Nothing">result != null</ensures>
		/// </getter>
		public DataGrid DataGrid => this._dataGrid;

		/// <summary>Occurs before new columns are filtered.</summary>
		public event EventHandler<DataGridFilteringEventArgs> Filtering;

		/// <summary>Occurs when any filter has changed.</summary>
		public event EventHandler FilterChanged;

		/// <summary>Create a new filter host for the given data grid.</summary>
		/// <param name="dataGrid">The data grid to filter.</param>
		/// <requires csharp="dataGrid != null" vb="dataGrid &lt;&gt; Nothing">dataGrid != null</requires>
		internal DataGridFilterHost(DataGrid dataGrid)
		{
			this._dataGrid = dataGrid;
			dataGrid.Columns.CollectionChanged += new NotifyCollectionChangedEventHandler(this.Columns_CollectionChanged);
			dataGrid.Loaded += new RoutedEventHandler(this.DataGrid_Loaded);
			if (dataGrid.ColumnHeaderStyle != null)
				return;
			Style style = new Style(typeof(DataGridColumnHeader), (Style)dataGrid.FindResource((object)typeof(DataGridColumnHeader)));
			style.Setters.Add((SetterBase)new Setter(Control.HorizontalContentAlignmentProperty, (object)HorizontalAlignment.Stretch));
			dataGrid.ColumnHeaderStyle = style;
		}

		/// <summary>Clear all existing filter conditions.</summary>
		public void Clear()
		{
			foreach (DataGridFilterColumnControl filterColumnControl in this._filterColumnControls)
				filterColumnControl.Filter = (object)null;
			this.EvaluateFilter();
		}

		/// <summary>
		/// Enables filtering by showing or hiding the filter controls.
		/// </summary>
		/// <param name="value">if set to <c>true</c>, filters controls are visible and filtering is enabled.</param>
		internal void Enable(bool value)
		{
			this._isFilteringEnabled = value;
			foreach (UIElement filterColumnControl in this._filterColumnControls)
				filterColumnControl.Visibility = value ? Visibility.Visible : Visibility.Hidden;
			this.EvaluateFilter();
		}

		/// <summary>
		/// When any filter condition has changed restart the evaluation timer to defer
		/// the evaluation until the user has stopped typing.
		/// </summary>
		internal void OnFilterChanged()
		{
			if (!this._isFilteringEnabled)
				return;
			this._dataGrid.CommitEdit();
			this._dataGrid.CommitEdit();
			if (this._deferFilterEvaluationTimer == null)
				this._deferFilterEvaluationTimer = new DispatcherTimer(this._dataGrid.GetFilterEvaluationDelay(), DispatcherPriority.Input, (EventHandler)((_, __) => this.EvaluateFilter()), Dispatcher.CurrentDispatcher);
			this._deferFilterEvaluationTimer.Restart();
		}

		/// <summary>Adds a new column.</summary>
		/// <param name="filterColumn" />
		/// <requires csharp="filterColumn != null" vb="filterColumn &lt;&gt; Nothing">filterColumn != null</requires>
		internal void AddColumn(DataGridFilterColumnControl filterColumn)
		{
			filterColumn.Visibility = this._isFilteringEnabled ? Visibility.Visible : Visibility.Hidden;
			this._filterColumnControls.Add(filterColumn);
		}

		/// <summary>Removes an unloaded column.</summary>
		/// <requires csharp="filterColumn != null" vb="filterColumn &lt;&gt; Nothing">filterColumn != null</requires>
		internal void RemoveColumn(DataGridFilterColumnControl filterColumn)
		{
			this._filterColumnControls.Remove(filterColumn);
			this.OnFilterChanged();
		}

		/// <summary>Creates a new content filter.</summary>
		/// <ensures csharp="Contract.Result&lt;DataGridExtensions.IContentFilter&gt;() != null" vb="Contract.Result(Of DataGridExtensions.IContentFilter)() &lt;&gt; Nothing">result != null</ensures>
		internal IContentFilter CreateContentFilter(object content) => this._dataGrid.GetContentFilterFactory().Create(content);

		private void DataGrid_Loaded(object sender, RoutedEventArgs e)
		{
			ControlTemplate template1 = this.DataGrid.Template;
			object obj1;
			if (template1 == null)
			{
				obj1 = (object)null;
			}
			else
			{
				string name = "DG_ScrollViewer";
				DataGrid dataGrid = this.DataGrid;
				obj1 = template1.FindName(name, (FrameworkElement)dataGrid);
			}
			ScrollViewer scrollViewer1 = obj1 as ScrollViewer;
			object obj2;
			if (scrollViewer1 == null)
			{
				obj2 = (object)null;
			}
			else
			{
				ControlTemplate template2 = scrollViewer1.Template;
				if (template2 == null)
				{
					obj2 = (object)null;
				}
				else
				{
					string name = "PART_ColumnHeadersPresenter";
					ScrollViewer scrollViewer2 = scrollViewer1;
					obj2 = template2.FindName(name, (FrameworkElement)scrollViewer2);
				}
			}
			FrameworkElement frameworkElement = (FrameworkElement)obj2;
			if (frameworkElement == null)
				return;
			DependencyProperty dp = KeyboardNavigation.TabNavigationProperty;
			// ISSUE: variable of a boxed type
			KeyboardNavigationMode local = KeyboardNavigationMode.None;
			frameworkElement.SetValue(dp, (object)local);
		}

		private void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems == null)
				return;
			DataGridColumn[] array = e.NewItems
				.OfType<DataGridColumn>()
				.Where(column => column.GetIsFilterVisible() && column.HeaderTemplate == null)
				.ToArray();

			if (array.Any())
			{
				IResourceLocator resourceLocator = this._dataGrid.GetResourceLocator();
				object obj = resourceLocator?.FindResource(DataGrid, DataGridFilter.ColumnHeaderTemplateKey)
					?? this._dataGrid.TryFindResource((object)DataGridFilter.ColumnHeaderTemplateKey);
				DataTemplate dataTemplate = (DataTemplate)obj;
				foreach (DataGridColumn dataGridColumn in array)
					dataGridColumn.HeaderTemplate = dataTemplate;
			}
		}

		internal void SetGlobalFilter(Predicate<object> globalFilter)
		{
			this._globalFilter = globalFilter;
			this.OnFilterChanged();
		}

		/// <summary> Evaluates the current filters and applies the filtering to the collection view of the items control. </summary>
		private void EvaluateFilter()
		{
			DispatcherTimer dispatcherTimer = this._deferFilterEvaluationTimer;
			if (dispatcherTimer != null)
				dispatcherTimer.Stop();
			ItemCollection items = this._dataGrid.Items;
			IList<DataGridFilterColumnControl> columnFilters = this.GetColumnFilters((DataGridFilterColumnControl)null);
			// ISSUE: reference to a compiler-generated field
			if (this.Filtering != null)
			{
				DataGridColumn[] array1 = columnFilters
					.Select(filter => filter.Column)
					.Where(column => column != null)
					.ToArray<DataGridColumn>();
				DataGridColumn[] array2 = array1
					.Except<DataGridColumn>(this._filteredColumns)
					.ToArray<DataGridColumn>();
				if (array2.Length != 0)
				{
					DataGridFilteringEventArgs e = new DataGridFilteringEventArgs((ICollection<DataGridColumn>)array2);
					// ISSUE: reference to a compiler-generated field
					this.Filtering((object)this._dataGrid, e);
					if (e.Cancel)
						return;
				}
				this._filteredColumns = array1;
			}
			// ISSUE: reference to a compiler-generated field
			EventHandler eventHandler = this.FilterChanged;
			if (eventHandler != null)
			{
				EventArgs e = EventArgs.Empty;
				eventHandler((object)this, e);
			}
			try
			{
				items.Filter = this.CreatePredicate(columnFilters);
				foreach (DataGridFilterColumnControl filterColumnControl in this._filterColumnControls)
					filterColumnControl.ValuesUpdated();
			}
			catch (InvalidOperationException ex)
			{
			}
		}

		internal Predicate<object> CreatePredicate(IList<DataGridFilterColumnControl> columnFilters)
		{
			IList<DataGridFilterColumnControl> source = columnFilters;
			//if ((source != null ? (!source.Any<DataGridFilterColumnControl>() ? 1 : 0) : 1) != 0)
			if (source?.Any() != true)
					return this._globalFilter;

			if (this._globalFilter == null)
				return (i => columnFilters.All(filter => filter.Matches(i)));
			return (Predicate<object>)(item =>
			{
				if (this._globalFilter(item))
					return columnFilters.All<DataGridFilterColumnControl>((Func<DataGridFilterColumnControl, bool>)(filter => filter.Matches(item)));
				return false;
			});
		}

		internal IList<DataGridFilterColumnControl> GetColumnFilters(DataGridFilterColumnControl excluded = null)
		{
			return this._filterColumnControls
				.Where(column => column != excluded)
				.Where(column => column.IsVisible && column.IsFiltered)
				.ToArray();
		}


	}
}
