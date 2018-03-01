// Decompiled with JetBrains decompiler
// Type: DataGridExtensions.DataGridFilterColumnControl
// Assembly: DataGridExtensions, Version=1.0.37.0, Culture=neutral, PublicKeyToken=43de855f87de903a
// MVID: 5A6EF75F-94BB-4773-88B0-C65701D66FE0
// Assembly location: D:\SVN\NPF\release\packages\DataGridExtensions.1.0.37\lib\net40-Client\DataGridExtensions.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;

namespace IT.WPF.JetBrains
{
	/// <summary>
	/// This class is the control hosting all information needed for filtering of one column.
	/// Filtering is enabled by simply adding this control to the header template of the DataGridColumn.
	/// </summary>
	/// <seealso cref="T:System.Windows.Controls.Control" />
	/// <seealso cref="T:System.ComponentModel.INotifyPropertyChanged" />
	/// <invariant>(FilterHost == null) || (DataGrid != null)</invariant>
	public class DataGridFilterColumnControl : Control, INotifyPropertyChanged
	{
		private static readonly BooleanToVisibilityConverter _booleanToVisibilityConverter = new BooleanToVisibilityConverter();
		private static readonly ControlTemplate _emptyControlTemplate = new ControlTemplate();

		#region Filter

		/// <summary>Identifies the Filter dependency property</summary>
		public static readonly DependencyProperty FilterProperty = DependencyProperty.Register("Filter", typeof(object), typeof(DataGridFilterColumnControl)
			//, (PropertyMetadata)new FrameworkPropertyMetadata((object)null, new PropertyChangedCallback(DataGridFilterColumnControl.<>c.<>9.<.cctor>b__3_0))
			, (PropertyMetadata)new FrameworkPropertyMetadata((object)null, new PropertyChangedCallback((o, e) => ((DataGridFilterColumnControl)o).Filter_Changed(e.NewValue)))
			);

		/// <summary>
		/// The user provided filter (IFilter) or content (usually a string) used to filter this column.
		/// If the filter object implements IFilter, it will be used directly as the filter,
		/// else the filter object will be passed to the content filter.
		/// </summary>
		public object Filter
		{
			get => this.GetValue(DataGridFilterColumnControl.FilterProperty);
			set => this.SetValue(DataGridFilterColumnControl.FilterProperty, value);
		}

		#endregion

		#region INotifyPropertyChanged

		/// <summary>Occurs when a property value changes.</summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>Raises the PropertyChanged event.</summary>
		/// <param name="propertyName">Name of the property.</param>
		protected virtual void OnPropertyChanged(string propertyName)
		{
			// ISSUE: reference to a compiler-generated field
			PropertyChangedEventHandler changedEventHandler = this.PropertyChanged;
			if (changedEventHandler == null)
				return;
			PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
			changedEventHandler((object)this, e);
		}

		#endregion

		/// <summary>
		/// Identifies the CellValue dependency property, a private helper property used to evaluate the property path for the list items.
		/// </summary>
		private static readonly DependencyProperty _cellValueProperty = DependencyProperty.Register("_cellValue", typeof(object), typeof(DataGridFilterColumnControl));

		/// <summary>The active filter for this column.</summary>
		private IContentFilter _activeFilter;

		/// <summary>
		/// The column header of the column we are filtering. This control must be a child element of the column header.
		/// </summary>
		protected DataGridColumnHeader ColumnHeader { get; private set; }

		/// <summary>The DataGrid we belong to.</summary>
		/// <getter>
		///   <ensures csharp="this.FilterHost == default(decimal) || Contract.Result&lt;System.Windows.Controls.DataGrid&gt;() != null" vb="Me.FilterHost = Nothing OrElse Contract.Result(Of System.Windows.Controls.DataGrid)() &lt;&gt; Nothing">this.FilterHost == default(decimal) || result != null</ensures>
		/// </getter>
		/// <setter>
		///   <requires csharp="this.FilterHost == default(decimal) || value != null" vb="Me.FilterHost = Nothing OrElse value &lt;&gt; Nothing">this.FilterHost == default(decimal) || value != null</requires>
		/// </setter>
		protected DataGrid DataGrid { get; private set; }

		/// <summary>The filter we belong to.</summary>
		/// <getter>
		///   <ensures csharp="Contract.Result&lt;DataGridExtensions.DataGridFilterHost&gt;() == default(decimal) || this.DataGrid != null" vb="Contract.Result(Of DataGridExtensions.DataGridFilterHost)() = Nothing OrElse Me.DataGrid &lt;&gt; Nothing">result == default(decimal) || this.DataGrid != null</ensures>
		/// </getter>
		/// <setter>
		///   <requires csharp="value == default(decimal) || this.DataGrid != null" vb="value = Nothing OrElse Me.DataGrid &lt;&gt; Nothing">value == default(decimal) || this.DataGrid != null</requires>
		/// </setter>
		protected DataGridFilterHost FilterHost { get; private set; }

		/// <summary>
		/// Returns a flag indicating whether this column has some filter condition to evaluate or not.
		/// If there is no filter condition we don't need to invoke this filter.
		/// </summary>
		public bool IsFiltered
		{
			get
			{
				if (string.IsNullOrWhiteSpace(Filter != null ? Filter.ToString() : null))
					return false;
				DataGridColumnHeader columnHeader = this.ColumnHeader;
				return (columnHeader != null ? columnHeader.Column : (DataGridColumn)null) != null;
			}
		}


		/// <summary>
		/// Returns all distinct visible (filtered) values of this column as string.
		/// This can be used to e.g. feed the ItemsSource of an AutoCompleteBox to give a hint to the user what to enter.
		/// </summary>
		/// <remarks>
		/// You may need to include "NotifyOnTargetUpdated=true" in the binding of the DataGrid.ItemsSource to get up-to-date
		/// values when the source object changes.
		/// </remarks>
		/// <getter>
		///   <ensures csharp="Contract.Result&lt;System.Collections.Generic.IEnumerable&lt;string&gt;&gt;() != null" vb="Contract.Result(Of System.Collections.Generic.IEnumerable(Of String))() &lt;&gt; Nothing">result != null</ensures>
		/// </getter>
		public IEnumerable<string> Values => this.InternalValues().Distinct().ToArray();

		private Predicate<object> _predicate(IList<DataGridFilterColumnControl> list) => this.FilterHost?.CreatePredicate(list) ?? (i => true);

		/// <summary>
		/// Returns all distinct source values of this column as string.
		/// This can be used to e.g. feed the ItemsSource of an Excel-like auto-filter that always shows all source values that can be selected.
		/// </summary>
		/// <remarks>
		/// You may need to include "NotifyOnTargetUpdated=true" in the binding of the DataGrid.ItemsSource to get up-to-date
		/// values when the source object changes.
		/// </remarks>
		/// <getter>
		///   <ensures csharp="Contract.Result&lt;System.Collections.Generic.IEnumerable&lt;string&gt;&gt;() != null" vb="Contract.Result(Of System.Collections.Generic.IEnumerable(Of String))() &lt;&gt; Nothing">result != null</ensures>
		/// </getter>
		public IEnumerable<string> SourceValues
		{
			get
			{
				//DataGridFilterHost filterHost = this.FilterHost;
				//Predicate<object> predicate;
				//if (filterHost == null)
				//{
				//	predicate = (Predicate<object>)null;
				//}
				//else
				//{
				//	predicate = filterHost.CreatePredicate((IList<DataGridFilterColumnControl>)null);
				//}
				//if (predicate == null)
				//	predicate = (Predicate<object>)(_ => true);

				//Predicate<object> predicate = this.FilterHost?.CreatePredicate((IList<DataGridFilterColumnControl>)null);
				//if (predicate == null)
				//	predicate = (Predicate<object>)(_ => true);


				return (IEnumerable<string>)this.InternalSourceValues(_predicate(null)).Distinct<string>().ToArray<string>();
			}
		}

		/// <summary>
		/// Returns all distinct selectable values of this column as string.
		/// This can be used to e.g. feed the ItemsSource of an Excel-like auto-filter, that only shows the values that are currently selectable, depending on the other filters.
		/// </summary>
		/// <remarks>
		/// You may need to include "NotifyOnTargetUpdated=true" in the binding of the DataGrid.ItemsSource to get up-to-date
		/// values when the source object changes.
		/// </remarks>
		/// <getter>
		///   <ensures csharp="Contract.Result&lt;System.Collections.Generic.IEnumerable&lt;string&gt;&gt;() != null" vb="Contract.Result(Of System.Collections.Generic.IEnumerable(Of String))() &lt;&gt; Nothing">result != null</ensures>
		/// </getter>
		public IEnumerable<string> SelectableValues
		{
			get
			{
				//DataGridFilterHost filterHost = this.FilterHost;
				//Predicate<object> predicate;
				//if (filterHost == null)
				//{
				//	predicate = (Predicate<object>)null;
				//}
				//else
				//{
				//	IList<DataGridFilterColumnControl> columnFilters = this.FilterHost.GetColumnFilters(this);
				//	predicate = filterHost.CreatePredicate(columnFilters);
				//}
				//if (predicate == null)
				//	predicate = (Predicate<object>)(_ => true);
				return (IEnumerable<string>)this.InternalSourceValues(_predicate(this.FilterHost.GetColumnFilters(this))).Distinct<string>().ToArray<string>();
			}
		}

		/// <summary>Gets the column this control is hosting the filter for.</summary>
		public DataGridColumn Column
		{
			get
			{
				DataGridColumnHeader columnHeader = this.ColumnHeader;
				if (columnHeader == null)
					return (DataGridColumn)null;
				return columnHeader.Column;
			}
		}


		static DataGridFilterColumnControl()
		{
			DependencyPropertyDescriptor propertyDescriptor = DependencyPropertyDescriptor.FromProperty(Control.TemplateProperty, typeof(Control));
			if (propertyDescriptor == null)
				return;
			propertyDescriptor.DesignerCoerceValueCallback = new CoerceValueCallback(DataGridFilterColumnControl.Template_CoerceValue);
		}

		/// <inheritdoc />
		/// <summary>
		/// Initializes a new instance of the <see cref="T:DataGridExtensions.DataGridFilterColumnControl" /> class.
		/// </summary>
		public DataGridFilterColumnControl()
		{
			this.Loaded += new RoutedEventHandler(this.Self_Loaded);
			this.Unloaded += new RoutedEventHandler(this.Self_Unloaded);
			this.Focusable = false;
			this.DataContext = (object)this;
		}


		private void Self_Loaded(object sender, RoutedEventArgs e)
		{
			if (this.FilterHost == null)
			{
				this.ColumnHeader = this.FindAncestorOrSelf<DataGridColumnHeader>();
				this.DataGrid = ColumnHeader?.FindAncestorOrSelf<DataGrid>() ?? 
					throw new InvalidOperationException("DataGridFilterColumnControl must be a child element of a DataGridColumnHeader.");
				this.FilterHost = this.DataGrid.GetFilter();
			}
			this.FilterHost.AddColumn(this);
			this.DataGrid.SourceUpdated += new EventHandler<DataTransferEventArgs>(this.DataGrid_SourceOrTargetUpdated);
			this.DataGrid.TargetUpdated += new EventHandler<DataTransferEventArgs>(this.DataGrid_SourceOrTargetUpdated);
			this.DataGrid.RowEditEnding += new EventHandler<DataGridRowEditEndingEventArgs>(this.DataGrid_RowEditEnding);
			this.Template = DataGridFilterColumnControl._emptyControlTemplate;
			PropertyPath propertyPath1 = new PropertyPath("Column.(0)", new object[1] { DataGridFilterColumn.IsFilterVisibleProperty });
			Binding binding1 = new Binding()
			{
				Mode = BindingMode.OneWay,
				Converter = _booleanToVisibilityConverter,
				Path = propertyPath1,
				Source = (object)ColumnHeader

			};
			BindingOperations.SetBinding((DependencyObject)this, VisibilityProperty, (BindingBase)binding1);

			PropertyPath propertyPath2 = new PropertyPath("Column.(0)", new object[1] { DataGridFilterColumn.TemplateProperty });
			Binding binding2 = new Binding()
			{
				Mode = BindingMode.OneWay,
				Path = propertyPath2,
				Source = (object)this.ColumnHeader
			};
			BindingOperations.SetBinding((DependencyObject)this, TemplateProperty, (BindingBase)binding2);

			PropertyPath propertyPath3 = new PropertyPath("Column.(0)", new object[1] { DataGridFilterColumn.FilterProperty });
			Binding binding3 = new Binding()
			{
				Mode = BindingMode.TwoWay,
				Path = propertyPath3,
				Source = (object)this.ColumnHeader
			};
			BindingOperations.SetBinding((DependencyObject)this, FilterProperty, (BindingBase)binding3);
		}

		private void Self_Unloaded(object sender, RoutedEventArgs e)
		{
			DataGridFilterHost filterHost = this.FilterHost;
			if (filterHost != null)
				filterHost.RemoveColumn(this);
			DataGrid dataGrid = this.DataGrid;
			if (dataGrid != null)
			{
				dataGrid.SourceUpdated -= new EventHandler<DataTransferEventArgs>(this.DataGrid_SourceOrTargetUpdated);
				dataGrid.TargetUpdated -= new EventHandler<DataTransferEventArgs>(this.DataGrid_SourceOrTargetUpdated);
				dataGrid.RowEditEnding -= new EventHandler<DataGridRowEditEndingEventArgs>(this.DataGrid_RowEditEnding);
			}
			BindingOperations.ClearBinding((DependencyObject)this, UIElement.VisibilityProperty);
			BindingOperations.ClearBinding((DependencyObject)this, Control.TemplateProperty);
			BindingOperations.ClearBinding((DependencyObject)this, DataGridFilterColumnControl.FilterProperty);
		}

		private void DataGrid_SourceOrTargetUpdated(object sender, DataTransferEventArgs e)
		{
			if (e.Property != ItemsControl.ItemsSourceProperty)
				return;
			this.ValuesUpdated();
		}

		private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
		{
			this.BeginInvoke(DispatcherPriority.Background, new Action(this.ValuesUpdated));
		}

		private void Filter_Changed(object newValue)
		{
			this._activeFilter = newValue as IContentFilter;
			DataGridFilterHost filterHost = this.FilterHost;
			if (filterHost == null)
				return;
			filterHost.OnFilterChanged();
		}

		private static object Template_CoerceValue(DependencyObject sender, object baseValue)
		{
			if (baseValue == null && sender is DataGridFilterColumnControl filterColumnControl1)
			{
				Type colType = filterColumnControl1.ColumnHeader?.Column?.GetType();
				if (colType != null)
				{
					ComponentResourceKey componentResourceKey1 = new ComponentResourceKey(typeof(DataGridFilter), (object)colType);
					DataGrid dataGrid = filterColumnControl1.DataGrid;
					object obj;
					if (dataGrid == null)
					{
						obj = (object)null;
					}
					else
					{
						IResourceLocator resourceLocator = dataGrid.GetResourceLocator();
						if (resourceLocator == null)
						{
							obj = (object)null;
						}
						else
						{
							DataGridFilterColumnControl filterColumnControl2 = filterColumnControl1;
							ComponentResourceKey componentResourceKey2 = componentResourceKey1;
							obj = resourceLocator.FindResource((FrameworkElement)filterColumnControl2, (object)componentResourceKey2);
						}
					}
					var res = obj ?? filterColumnControl1.TryFindResource((object)componentResourceKey1);
					return res;
				}
			}
			return baseValue;
		}

		/// <summary>
		/// Returns true if the given item matches the filter condition for this column.
		/// </summary>
		internal bool Matches(object item)
		{
			if (this.Filter == null || this.FilterHost == null)
				return true;
			if (this._activeFilter == null)
				this._activeFilter = this.FilterHost.CreateContentFilter(this.Filter);
			return this._activeFilter.IsMatch(this.GetCellContent(item));
		}

		/// <summary>
		/// Notification of the filter that the content of the values might have changed.
		/// </summary>
		internal void ValuesUpdated()
		{
			this.OnPropertyChanged(nameof(Values));
			this.OnPropertyChanged(nameof(SourceValues));
			this.OnPropertyChanged(nameof(SelectableValues));
		}

		/// <summary>
		/// Examines the property path and returns the objects value for this column.
		/// Filtering is applied on the SortMemberPath, this is the path used to create the binding.
		/// </summary>
		protected object GetCellContent(object item)
		{
			DataGridColumnHeader columnHeader = this.ColumnHeader;
			string path = columnHeader != null ? columnHeader.Column.SortMemberPath : (string)null;
			if (string.IsNullOrEmpty(path))
				return (object)null;
			BindingOperations.SetBinding((DependencyObject)this, DataGridFilterColumnControl._cellValueProperty, (BindingBase)new Binding(path)
			{
				Source = item
			});
			object obj = this.GetValue(DataGridFilterColumnControl._cellValueProperty);
			BindingOperations.ClearBinding((DependencyObject)this, DataGridFilterColumnControl._cellValueProperty);
			return obj;
		}

		/// <summary>
		/// Gets the cell content of all list items for this column.
		/// </summary>
		/// <ensures csharp="Contract.Result&lt;System.Collections.Generic.IEnumerable&lt;string&gt;&gt;() != null" vb="Contract.Result(Of System.Collections.Generic.IEnumerable(Of String))() &lt;&gt; Nothing">result != null</ensures>
		protected IEnumerable<string> InternalValues()
		{
			DataGrid dataGrid = this.DataGrid;
			return (dataGrid != null ? dataGrid.Items.Cast<object>().Select<object, object>(new Func<object, object>(this.GetCellContent)).Select<object, string>((Func<object, string>)(content => (content != null ? content.ToString() : (string)null) ?? string.Empty)) : (IEnumerable<string>)null) ?? Enumerable.Empty<string>();
		}

		/// <summary>
		/// Gets the cell content of all list items for this column.
		/// </summary>
		/// <requires csharp="new DataGridExtensions.DataGridFilterColumnControl.&lt;&gt;c__DisplayClass42_0(){&#xD;&#xA;    predicate = predicate, &#xD;&#xA;}.predicate != null" vb="(() =&gt; {&#xD;&#xA;    DataGridExtensions.DataGridFilterColumnControl.&lt;&gt;c__DisplayClass42_0 local_0_prime = New DataGridExtensions.DataGridFilterColumnControl.&lt;&gt;c__DisplayClass42_0();&#xD;&#xA;    (local_0_prime.predicate = predicate)&#xD;&#xA;    return local_0_prime; })().predicate &lt;&gt; Nothing">new DataGridExtensions.DataGridFilterColumnControl.&lt;&gt;c__DisplayClass42_0(){
		///     predicate = predicate,
		/// }.predicate != null</requires>
		/// <ensures csharp="Contract.Result&lt;System.Collections.Generic.IEnumerable&lt;string&gt;&gt;() != null" vb="Contract.Result(Of System.Collections.Generic.IEnumerable(Of String))() &lt;&gt; Nothing">result != null</ensures>
		protected IEnumerable<string> InternalSourceValues(Predicate<object> predicate)
		{
			// ISSUE: object of a compiler-generated type is created
			// ISSUE: reference to a compiler-generated field
			// ISSUE: reference to a compiler-generated method
			//__ContractsRuntime.Requires(new DataGridFilterColumnControl.DataGridFilterColumnControl_<>c__DisplayClass42_0_0()
	  //{
			//	predicate = predicate

	  //}.predicate != null, (string)null, "predicate != null");
			DataGrid dataGrid = this.DataGrid;
			IEnumerable enumerable = dataGrid != null ? dataGrid.ItemsSource : (IEnumerable)null;
			if (enumerable == null)
				return Enumerable.Empty<string>();
			ICollectionView collectionView = enumerable as ICollectionView;
			return (
				(collectionView != null ? collectionView.SourceCollection : (IEnumerable)null) ?? enumerable)
				.Cast<object>()
				.Where<object>((Func<object, bool>)(item => predicate(item)))
				.Select<object, object>(new Func<object, object>(this.GetCellContent))
				.Select<object, string>((Func<object, string>)(content => (content != null ? content.ToString() : (string)null) ?? string.Empty));
		}

	}
}
