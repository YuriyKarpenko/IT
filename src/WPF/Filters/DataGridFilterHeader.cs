using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;
using IT.WPF;
using IT.WPF.Filters;

namespace IT.WPF
{

	public interface IFilterHeaderControl
	{
		/// <summary> Признак активного состояния фильтра </summary>
		bool IsFiltered { get; }
		/// <summary> Видимость элемента фильтра </summary>
		bool IsVisible { get; }
		/// <summary> Видимость элемента фильтра </summary>
		Visibility Visibility { get; set; }

		/// <summary> Возвращает true, если данный элемент соответствует условию фильтра для этого столбца. </summary>
		bool Matches(object item);

		/// <summary> Уведомление фильтра о том, что содержимое значений могло быть изменено. </summary>
		void ValuesUpdated();
	}

	/// <summary>
	/// Элемент отображения фмльта
	/// </summary>
	public abstract class FilterHeaderControl : Control, INotifyPropertyChanged, IFilterHeaderControl
	{
		/// <summary> Значение фильтра, которое отключает вильтрацию </summary>
		public static string NoFilterValue { get; set; } = "Все значения";

		#region Filter

		/// <summary> Идентифицирует свойство зависимостей Filter (для привязки значений из UI) </summary>
		public static readonly DependencyProperty FilterProperty = DependencyProperty.Register("Filter", typeof(object), typeof(DataGridFilterHeader)
			, new FrameworkPropertyMetadata(null, new PropertyChangedCallback((o, e) => o.SafeCast<FilterHeaderControl>(true)?.Filter_Changed(e.NewValue)))
			);
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

		#endregion

		#region INotifyPropertyChanged

		/// <summary> реализация INotifyPropertyChanged </summary>
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		#endregion

		#region ItemContent

		/// <summary> Вспомогательное private свойство, используемое для вычисления пути свойства для элементов списка </summary>
		private static readonly DependencyProperty _cellValueProperty = DependencyProperty.Register("_cellValue", typeof(object), typeof(DataGridFilterHeader));

		/// <summary> Путь привязки </summary>
		protected abstract string BindingProperty { get; }

		/// <summary>
		/// Изучает путь свойства и возвращает значение объекта для этого столбца.
		/// </summary>
		private object GetItemContent(object item)
		{
			//	:TODO из-за TemplateColumns (надо бы просто использовать привязку)
			string path = BindingProperty;
			if (!string.IsNullOrEmpty(path))
			{
				BindingOperations.SetBinding(this, DataGridFilterHeader._cellValueProperty, new Binding(path)
				{
					Source = item
				});
				object obj = GetValue(DataGridFilterHeader._cellValueProperty);
				BindingOperations.ClearBinding(this, DataGridFilterHeader._cellValueProperty);
				return obj;
			}
			return null;
		}

		#endregion

		/// <summary>
		/// Возвращает флаг, указывающий, имеет ли этот столбец какое-либо условие фильтра для оценки или нет.
		/// Если нет условия фильтра, нам не нужно вызывать этот фильтр.
		/// </summary>
		public virtual bool IsFiltered => !string.IsNullOrWhiteSpace(Filter?.ToString());

		/// <summary> Менеджер фильтрации </summary>
		protected IFilterManager FilterHost { get; set; }

		/// <summary> Активный фильтр для этого столбца.</summary>
		protected IContentFilter _activeFilter;


		/// <summary> Возвращает true, если данный элемент соответствует условию фильтра для этого столбца. </summary>
		public bool Matches(object item)
		{
			if (this.Filter == null || this.FilterHost == null)
				return true;

			_activeFilter = _activeFilter ?? CreateContentFilter(Filter);

			return _activeFilter.IsMatch(this.GetItemContent(item));
		}

		#region values

		/// <summary> Уведомление фильтра о том, что содержимое значений могло быть изменено. </summary>
		void IFilterHeaderControl.ValuesUpdated()
		{
			OnPropertyChanged(nameof(ItemsSource));
			OnPropertyChanged(nameof(Values));
			OnPropertyChanged(nameof(SourceValues));
		}

		/// <summary>
		/// Возвращает все уникальные исходные значения этого столбца в виде строки.
		/// Это может быть использовано, например, feed the ItemsSource в автоматическом фильтре, подобном Excel, который всегда отображает все исходные значения, которые можно выбрать.
		/// </summary>
		public IEnumerable<string> ItemsSource => PrepareCombo();

		/// <summary>
		/// Возвращает все уникальные видимые (отфильтрованные) значения этого столбца в виде строки.
		/// Это может быть использовано, например, отправьте файл ItemsSource для AutoCompleteBox, чтобы дать подсказку пользователю, что ввести.
		/// </summary>
		/// <remarks>
		/// Возможно, вам потребуется включить «NotifyOnTargetUpdated = true» в привязку DataGrid.ItemsSource для получения актуальных значений при изменении исходного объекта.
		/// </remarks>
		public IEnumerable<string> Values => InternalValues().Distinct().ToArray();

		/// <summary>
		/// Возвращает все уникальные исходные значения этого столбца в виде строки.
		/// Это может быть использовано, например, feed the ItemsSource в автоматическом фильтре, подобном Excel, который всегда отображает все исходные значения, которые можно выбрать.
		/// </summary>
		public IEnumerable<string> SourceValues => InternalSourceValues().Distinct().ToArray();


		/// <summary> Получает содержимое ячейки для всех элементов списка для этого столбца. </summary>
		protected virtual IEnumerable<string> InternalValues()
		{
			var res = FilterHost?.Items.Cast<object>();
				
			return GetColumnValues(res);
		}

		/// <summary> Получает содержимое ячейки для всех элементов списка для этого столбца. </summary>
		protected virtual IEnumerable<string> InternalSourceValues()
		{
			IEnumerable enumerable = FilterHost?.ItemsSource;
			if (enumerable != null)
			{
				ICollectionView collectionView = enumerable as ICollectionView;

				var res = (collectionView?.SourceCollection ?? enumerable).Cast<object>();

				return GetColumnValues(res);
			}
			return Enumerable.Empty<string>();
		}

		/// <summary> подготока значений для ComboBox </summary>
		/// <param name="src"></param>
		/// <returns></returns>
		protected virtual IEnumerable<string> PrepareCombo(IEnumerable<string> src = null)
		{
			var list = (src ?? InternalSourceValues())
				.Distinct()
				.ToList();
			list.Insert(0, NoFilterValue);
			return list;
		}

		#endregion


		private void Filter_Changed(object newValue)
		{
			this._activeFilter = newValue as IContentFilter;
			FilterHost?.OnFilterChanged();
		}

		/// <summary> Создает новый фильтр содержимого. </summary>
		private IContentFilter CreateContentFilter(object content) => FilterHost.FilterFactory.Create(FilterHost.FilterEnabled, content);

		//	получение строк данного столбца из списка объектов
		private IEnumerable<string> GetColumnValues(IEnumerable<object> src) => src?.Select(GetItemContent).Select(content => content?.ToString() ?? string.Empty) ?? Enumerable.Empty<string>();
	}

	/// <summary>
	/// Элемент отображения фмльта
	/// </summary>
	public class DataGridFilterHeader : FilterHeaderControl
	{
		private IFilterHeaderControl thisi => this as IFilterHeaderControl;

		/// <summary>
		/// Фильтрация применяется к SortMemberPath, это путь, используемый для создания привязки.
		/// </summary>
		protected override string BindingProperty => ColumnHeader?.Column.SortMemberPath;

		/// <summary> Заголовок столбца, который мы фильтруем. Данный контрол внедряется в этот заголовок </summary>
		protected DataGridColumnHeader ColumnHeader { get; private set; }

		/// <summary> Грид, который сожержит данный контрол </summary>
		protected DataGrid DataGrid { get; private set; }

		/// <summary>
		/// Возвращает флаг, указывающий, имеет ли этот столбец какое-либо условие фильтра для оценки или нет.
		/// Если нет условия фильтра, нам не нужно вызывать этот фильтр.
		/// </summary>
		public override bool IsFiltered => base.IsFiltered && ColumnHeader?.Column != null;


		static DataGridFilterHeader()
		{
			DependencyPropertyDescriptor propertyDescriptor = DependencyPropertyDescriptor.FromProperty(Control.TemplateProperty, typeof(Control));
			if (propertyDescriptor != null)
			{
				var oldCallback = propertyDescriptor.DesignerCoerceValueCallback;
				//propertyDescriptor.DesignerCoerceValueCallback = new CoerceValueCallback(()Template_CoerceValue);
				propertyDescriptor.DesignerCoerceValueCallback = new CoerceValueCallback((d, v) => Template_CoerceValue(d, oldCallback?.Invoke(d, v) ?? v));
			}
		}

		/// <summary> .ctor </summary>
		public DataGridFilterHeader()
		{
			this.Loaded += Self_Loaded;
			this.Unloaded += Self_Unloaded;
			this.Focusable = false;
			this.DataContext = (object)this;
		}




		//	Поиск Template для данного контрола
		private static object Template_CoerceValue(DependencyObject sender, object baseValue)
		{
			if (baseValue == null && sender is DataGridFilterHeader filterHeader)
			{
				ResourceKey resourceKey = null;
				//Type colType = filterHeader.ColumnHeader?.Column?.GetType();
				//if (colType != null)
				{
					switch (filterHeader.FilterHost.FilterEnabled)
					{
						case DataGridFilters.ComboBox:
							resourceKey = DataGridFilter.DataGridFilters_ComboBox_TemplateKey;
							break;
						case DataGridFilters.TextBoxContains:
							resourceKey = DataGridFilter.DataGridFilters_TextBoxContains_TemplateKey;
							break;
					}

					object res = filterHeader.FindResource(resourceKey, filterHeader);
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
			this.DataGrid.RowEditEnding += DataGrid_RowEditEnding;
			//this.Template = DataGridFilterColumnControl._emptyControlTemplate;	TODO: ???

			//PropertyPath propertyPath1 = new PropertyPath("Column.(0)", new object[1] { DataGridFilterColumn.IsFilterVisibleProperty });
			//Binding binding1 = new Binding()
			//{
			//	Mode = BindingMode.OneWay,
			//	Converter = _booleanToVisibilityConverter,
			//	Path = propertyPath1,
			//	Source = (object)ColumnHeader

			//};
			//BindingOperations.SetBinding((DependencyObject)this, VisibilityProperty, (BindingBase)binding1);

			Binding binding2 = new Binding()
			{
				Mode = BindingMode.OneWay,
				Path = new PropertyPath("Column.(0)", DataGridFilterHeader.TemplateProperty),
				Source = this.ColumnHeader
			};
			BindingOperations.SetBinding(this, TemplateProperty, binding2);

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
				DataGrid.RowEditEnding -= DataGrid_RowEditEnding;
			}

			//BindingOperations.ClearBinding((DependencyObject)this, UIElement.VisibilityProperty);
			BindingOperations.ClearBinding(this, TemplateProperty);
			//BindingOperations.ClearBinding((DependencyObject)this, DataGridFilterColumnControl.FilterProperty);
		}

		private void DataGrid_SourceOrTargetUpdated(object sender, DataTransferEventArgs e)
		{
			if (e.Property == ItemsControl.ItemsSourceProperty)
			{
				thisi.ValuesUpdated();
			}
		}

		private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
		{
			this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(thisi.ValuesUpdated));
		}
	}
}
