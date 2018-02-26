using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using IT;
using IT.WPF.Filters;

namespace IT.WPF
{
	/// <summary>
	/// 
	/// </summary>
	public class DataGridFilterHeader : Control, INotifyPropertyChanged
	{
		/// <summary> Идентифицирует свойство зависимостей Filter (для привязки значений из UI) </summary>
		public static readonly DependencyProperty FilterProperty = DependencyProperty.Register("Filter", typeof(object), typeof(DataGridFilterHeader)
			//, (PropertyMetadata)new FrameworkPropertyMetadata((object)null, new PropertyChangedCallback(DataGridFilterColumnControl.<>c.<>9.<.cctor>b__3_0))
			, (PropertyMetadata)new FrameworkPropertyMetadata((object)null, new PropertyChangedCallback((o, e) => ((DataGridFilterHeader)o).Filter_Changed(e.NewValue)))	);

		/// <summary> Вспомогательное private свойство, используемое для вычисления пути свойства для элементов списка </summary>
		private static readonly DependencyProperty _cellValueProperty = DependencyProperty.Register("_cellValue", typeof(object), typeof(DataGridFilterHeader));
		
		/// <summary> Активный фильтр для этого столбца.</summary>
		private IContentFilter _activeFilter;


		/// <summary> реализация INotifyPropertyChanged </summary>
		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		/// <summary>
		/// Предоставляемое пользователем значение фильтра (IFilter) или содержимое (обычно строка), используется для фильтрации этого столбца.
		/// Если объект фильтра реализует IFilter, он будет использоваться непосредственно как фильтр,
		/// иначе, значение будет использовано фильтром содержимого (IContentFilter).
		/// </summary>
		public object Filter
		{
			get => this.GetValue(FilterProperty);
			set => this.SetValue(FilterProperty, value);
		}


		/// <summary> Заголовок столбца, который мы фильтруем. Данный контрол внедряется в этот заголовок </summary>
		protected DataGridColumnHeader ColumnHeader { get; private set; }

		/// <summary> Грид, который сожержит данный контрол </summary>
		protected DataGrid DataGrid { get; private set; }

		/// <summary> Менеджер фильтрации </summary>
		protected DataGridFilterCore FilterHost { get; private set; }

		public IEnumerable<string> ItemsSource { get; } = new string[] { "qwe", "asdf" };
		public string SelectedItem { get; set; }

		/// <summary>
		/// Returns a flag indicating whether this column has some filter condition to evaluate or not.
		/// If there is no filter condition we don't need to invoke this filter.
		/// </summary>
		public bool IsFiltered => !string.IsNullOrWhiteSpace(Filter?.ToString()) && ColumnHeader?.Column != null;

		static DataGridFilterHeader()
		{
			DependencyPropertyDescriptor propertyDescriptor = DependencyPropertyDescriptor.FromProperty(Control.TemplateProperty, typeof(Control));
			if (propertyDescriptor != null)
				propertyDescriptor.DesignerCoerceValueCallback = new CoerceValueCallback(Template_CoerceValue);
		}

		/// <summary> .ctor </summary>
		public DataGridFilterHeader()
		{
			this.Loaded += Self_Loaded;
			this.Unloaded += Self_Unloaded;
			this.Focusable = false;
			this.DataContext = (object)this;
		}


		/// <summary>
		/// Notification of the filter that the content of the values might have changed.
		/// </summary>
		internal void ValuesUpdated()
		{
			this.OnPropertyChanged("Values");
			this.OnPropertyChanged("SourceValues");
			this.OnPropertyChanged("SelectableValues");
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
		/// Examines the property path and returns the objects value for this column.
		/// Filtering is applied on the SortMemberPath, this is the path used to create the binding.
		/// </summary>
		protected object GetCellContent(object item)
		{
			//	:TODO из-за TemplateColumns (надо бы просто использовать привязку)
			string path = ColumnHeader?.Column.SortMemberPath;
			if (!string.IsNullOrEmpty(path))
			{
				BindingOperations.SetBinding((DependencyObject)this, DataGridFilterHeader._cellValueProperty, (BindingBase)new Binding(path)
				{
					Source = item
				});
				object obj = this.GetValue(DataGridFilterHeader._cellValueProperty);
				BindingOperations.ClearBinding((DependencyObject)this, DataGridFilterHeader._cellValueProperty);
				return obj;
			}
			return null;
		}


		//	Поиск Template для данного контрола
		private static object Template_CoerceValue(DependencyObject sender, object baseValue)
		{
			if (baseValue == null && sender is DataGridFilterHeader filterHeader)
			{
				Type colType = filterHeader.ColumnHeader?.Column?.GetType();
				if (colType != null)
				{
					ComponentResourceKey componentResourceKey1 = new ComponentResourceKey(typeof(DataGridFilter), colType);
					object res = //filterHeader.DataGrid?.GetResourceLocator()?.FindResource((FrameworkElement)filterHeader, componentResourceKey1) ??
						filterHeader.TryFindResource(componentResourceKey1);
					return res;
				}
			}
			return baseValue;
		}

		private void Self_Loaded(object sender, RoutedEventArgs e)
		{
			if (this.FilterHost == null)
			{
				this.ColumnHeader = this.GetVisualParent<DataGridColumnHeader>();
				this.DataGrid = ColumnHeader?.GetVisualParent<DataGrid>() ??
					throw new InvalidOperationException(nameof(DataGridFilterHeader) + " must be a child element of a DataGridColumnHeader.");
				this.FilterHost = this.DataGrid.GetFilter();
			}

			this.FilterHost.AddColumn(this);
			this.DataGrid.SourceUpdated += DataGrid_SourceOrTargetUpdated;
			this.DataGrid.TargetUpdated += DataGrid_SourceOrTargetUpdated;
			//this.DataGrid.RowEditEnding += new EventHandler<DataGridRowEditEndingEventArgs>(this.DataGrid_RowEditEnding);
			//this.Template = DataGridFilterColumnControl._emptyControlTemplate;

			//PropertyPath propertyPath1 = new PropertyPath("Column.(0)", new object[1] { DataGridFilterColumn.IsFilterVisibleProperty });
			//Binding binding1 = new Binding()
			//{
			//	Mode = BindingMode.OneWay,
			//	Converter = _booleanToVisibilityConverter,
			//	Path = propertyPath1,
			//	Source = (object)ColumnHeader

			//};
			//BindingOperations.SetBinding((DependencyObject)this, VisibilityProperty, (BindingBase)binding1);

			PropertyPath propertyPath2 = new PropertyPath("Column.(0)", new object[1] { DataGridFilterHeader.TemplateProperty });
			Binding binding2 = new Binding()
			{
				Mode = BindingMode.OneWay,
				Path = propertyPath2,
				Source = this.ColumnHeader
			};
			BindingOperations.SetBinding((DependencyObject)this, TemplateProperty, (BindingBase)binding2);

			//PropertyPath propertyPath3 = new PropertyPath("Column.(0)", new object[1] { DataGridFilterColumn.FilterProperty });
			//Binding binding3 = new Binding()
			//{
			//	Mode = BindingMode.TwoWay,
			//	Path = propertyPath3,
			//	Source = (object)this.ColumnHeader
			//};
			//BindingOperations.SetBinding((DependencyObject)this, FilterProperty, (BindingBase)binding3);
		}

		private void Self_Unloaded(object sender, RoutedEventArgs e)
		{
			if (FilterHost != null)
				FilterHost.RemoveColumn(this);

			if (DataGrid != null)
			{
				DataGrid.SourceUpdated -= DataGrid_SourceOrTargetUpdated;
				DataGrid.TargetUpdated -= DataGrid_SourceOrTargetUpdated;
				//dataGrid.RowEditEnding -= new EventHandler<DataGridRowEditEndingEventArgs>(this.DataGrid_RowEditEnding);
			}

			//BindingOperations.ClearBinding((DependencyObject)this, UIElement.VisibilityProperty);
			//BindingOperations.ClearBinding((DependencyObject)this, Control.TemplateProperty);
			//BindingOperations.ClearBinding((DependencyObject)this, DataGridFilterColumnControl.FilterProperty);
		}

		private void DataGrid_SourceOrTargetUpdated(object sender, DataTransferEventArgs e)
		{
			if (e.Property == ItemsControl.ItemsSourceProperty)
			{
				this.ValuesUpdated();
			}
		}

		private void Filter_Changed(object newValue)
		{
			this._activeFilter = newValue as IContentFilter;
			FilterHost?.OnFilterChanged();
		}


	}
}
