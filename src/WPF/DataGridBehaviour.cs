using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace IT.WPF
{
	/// <summary>
	/// Behaviour class for DataGrid
	/// </summary>
	public class DataGridBehaviour
	{
		#region ScrollToView

		//private static SelectedCellsChangedEventHandler _scrollToViewEventHandler = new SelectedCellsChangedEventHandler(OnScrollToView);

		/// <summary>
		/// 
		/// </summary>
		public static readonly DependencyProperty ScrollToViewProperty = DependencyProperty.RegisterAttached(
			"ScrollToView", typeof(bool), typeof(DataGridBehaviour), new FrameworkPropertyMetadata(false, (d, e) =>
						{
							//DependencyObject d, DependencyPropertyChangedEventArgs e
							var u = (System.Windows.Controls.DataGrid)d;
							if ((bool)e.OldValue && !(bool)e.NewValue)
							{
								// remove handlers
								u.SelectedCellsChanged -= OnScrollToView;// _scrollToViewEventHandler;
							}

							if (!(bool)e.OldValue && (bool)e.NewValue)
							{
								// add handlers
								u.SelectedCellsChanged += OnScrollToView;// _scrollToViewEventHandler;
							}
						}));

		/// <summary>
		/// Retun current value of a ScrollToView
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public static bool GetScrollToView(DependencyObject element)
		{
			return (bool)element.GetValue(ScrollToViewProperty);
		}

		/// <summary>
		/// Set the value of a ScrollToView
		/// </summary>
		/// <param name="element"></param>
		/// <param name="value"></param>
		public static void SetScrollToView(DependencyObject element, bool value)
		{
			element.SetValue(ScrollToViewProperty, value);
		}

		private static void OnScrollToView(object sender, SelectedCellsChangedEventArgs e)
		{
			if (sender is DataGrid)
			{
				DataGrid d = (DataGrid)sender;
				if (d.SelectedItem != null)
				{
					d.UpdateLayout();
					d.ScrollIntoView(d.SelectedItem);
					if (GetSetKeyboardFocusOnScrollToView(d))
					{
						#region settign keyboard focus to a first cell on the row
						//TODO check if it is a normal solution
						DataGridRow row = (DataGridRow)d.ItemContainerGenerator.ContainerFromIndex(d.SelectedIndex);
						if (row != null)
						{
							DataGridCellsPresenter dgCP = row.GetVisualChildren<DataGridCellsPresenter>().FirstOrDefault();

							if (dgCP == null)
							{
								d.ScrollIntoView(row, d.Columns[0]);
								dgCP = row.GetVisualChildren<DataGridCellsPresenter>().FirstOrDefault();
							}

							if (dgCP != null)
							{
								DataGridCell cell = (DataGridCell)dgCP.ItemContainerGenerator.ContainerFromIndex(0);
								if (cell != null)
								{
									cell.Focus();
								}
							}
						}
						#endregion // settign keyboard focus to a first cell on the row
					}
				}
			}
		}

		#endregion

		#region SetKeyboardFocusOnScrollToView

		/// <summary>
		/// 
		/// </summary>
		public static readonly DependencyProperty SetKeyboardFocusOnScrollToViewProperty = DependencyProperty.RegisterAttached(
			"SetKeyboardFocusOnScrollToView", typeof(bool), typeof(DataGridBehaviour));

		/// <summary>
		/// Retun current value of a SetKeyboardFocusOnScrollToView
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public static bool GetSetKeyboardFocusOnScrollToView(DependencyObject element)
		{
			return (bool)element.GetValue(SetKeyboardFocusOnScrollToViewProperty);
		}

		/// <summary>
		/// Set the value of a SetKeyboardFocusOnScrollToView
		/// </summary>
		/// <param name="element"></param>
		/// <param name="value"></param>
		public static void SetSetKeyboardFocusOnScrollToView(DependencyObject element, bool value)
		{
			element.SetValue(SetKeyboardFocusOnScrollToViewProperty, value);
		}

		#endregion

		#region FilteredColumns (on Autogenerate)

		/// <summary>
		/// Фильтрация столбцов во атрибуту BrowsableAttribute, 
		/// для EnumDataTypeAttribute создает DataGridComboBoxColumn, 
		/// по атрибуту EditableAttribute определяется свойство IsReadOnly
		/// вытаскивание имени столбца из атрибутов DisplayAttribute и DisplayNameAttribute
		/// </summary>
		public static readonly DependencyProperty FilteredColumnsProperty = DependencyProperty.RegisterAttached(
			"FilteredColumns", typeof(bool), typeof(DataGridBehaviour), new PropertyMetadata(false, FilteredColumnsChangedCallback));

		/// <summary>
		/// Retun current value of a FilteredColumns
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public static bool GetFilteredColumns(DependencyObject element)
		{
			return (bool)element.GetValue(FilteredColumnsProperty);
		}

		/// <summary>
		/// Set the value of a FilteredColumns
		/// </summary>
		/// <param name="element"></param>
		/// <param name="value"></param>
		public static void SetFilteredColumns(DependencyObject element, bool value)
		{
			element.SetValue(FilteredColumnsProperty, value);
		}

		private static void FilteredColumnsChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var dg = d as DataGrid;
			if ((bool)e.NewValue)
			{
				dg.AutoGeneratingColumn += dg_AutoGeneratingColumn;
			}
			else
			{
				dg.AutoGeneratingColumn -= dg_AutoGeneratingColumn;
			}
		}

		private static void dg_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
		{
			var pd = (PropertyDescriptor)e.PropertyDescriptor;
			if (pd.Attributes.OfType<BrowsableAttribute>()?.SingleOrDefault()?.Browsable ?? true)
			{
				var txtCol = e.Column as DataGridTextColumn;

				var enumType = pd.Attributes.OfType<EnumDataTypeAttribute>()?.SingleOrDefault()?.EnumType;
				if (enumType != null && txtCol != null)
				{
					var cbCol = e.Column as DataGridComboBoxColumn;
					if (cbCol == null)
					{
						cbCol = new DataGridComboBoxColumn();
						var intType = Enum.GetUnderlyingType(enumType);
						cbCol.ItemsSource = Enum.GetValues(enumType)
							.Cast<Enum>()
							.ToDictionary(i => Convert.ChangeType(i, intType), i => i.ToString());
						cbCol.SelectedValueBinding = txtCol.Binding;
						cbCol.SelectedValuePath = "Key";
						cbCol.DisplayMemberPath = "Value";
					}
					e.Column = cbCol;
				}

				e.Column.IsReadOnly = !(pd.Attributes.OfType<EditableAttribute>()?.SingleOrDefault()?.AllowEdit ?? true);
				var attr = pd.Attributes.OfType<DisplayAttribute>().FirstOrDefault();
				if (attr != null)
				{
					e.Column.Header = attr.ShortName ?? attr.Name ?? attr.Description ?? e.PropertyName;
				}
				else
				{
					e.Column.Header = pd.Attributes.OfType<DisplayNameAttribute>()?.SingleOrDefault()?.DisplayName ?? e.PropertyName;
				}
			}
			else
			{
				e.Cancel = true;
			}
		}

		#endregion

		#region Columns

		/// <summary>
		/// It allows you to bind a collection of columns
		/// </summary>
		public static readonly DependencyProperty ColumnsProperty = DependencyProperty.RegisterAttached(
			"Columns", typeof(IEnumerable<DataGridColumn>), typeof(DataGridBehaviour), new UIPropertyMetadata(null, ColumnsPropertyChanged));

		/// <summary>
		/// Retun current value of a Columns
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public static ObservableCollection<DataGridColumn> GetColumns(DependencyObject element)
		{
			return (ObservableCollection<DataGridColumn>)element.GetValue(ColumnsProperty);
		}

		/// <summary>
		/// Set the value of a Columns
		/// </summary>
		/// <param name="element"></param>
		/// <param name="value"></param>
		public static void SetColumns(DependencyObject element, ObservableCollection<DataGridColumn> value)
		{
			element.SetValue(ColumnsProperty, value);
		}

		private static void ColumnsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
		{
			var dataGrid = (DataGrid)source;
			var columns = (IEnumerable<DataGridColumn>)e.NewValue;

			if (e.OldValue != null)
			{
				foreach (DataGridColumn column in (IEnumerable<DataGridColumn>)e.OldValue)
				{
					dataGrid.Columns.Remove(column);
				}
			}

			if (columns != null)
			{
				dataGrid.AutoGenerateColumns = false;
				foreach (DataGridColumn column in columns)
				{
					dataGrid.Columns.Add(column);
				}

				//columns.CollectionChanged += ColumnsChanged;
				//Localization.Localization.UpdateLocaleAtDataGridColumns(dataGrid);
			}
		}

		//static void ColumnsChanged(object sender, NotifyCollectionChangedEventArgs e2)
		//{
		//		if (e2.Action == NotifyCollectionChangedAction.Reset) {
		//			dataGrid.Columns.Clear();
		//			foreach (DataGridColumn column in e2.OldItems) {
		//				dataGrid.Columns.Remove(column);
		//			}
		//			foreach (DataGridColumn column in e2.NewItems) {
		//				dataGrid.Columns.Add(column);
		//			}
		//		} else if (e2.Action == NotifyCollectionChangedAction.Add) {
		//			foreach (DataGridColumn column in e2.NewItems) {
		//				dataGrid.Columns.Add(column);
		//			}
		//		} else if (e2.Action == NotifyCollectionChangedAction.Move) {
		//			dataGrid.Columns.Move(e2.OldStartingIndex, e2.NewStartingIndex);
		//		} else if (e2.Action == NotifyCollectionChangedAction.Remove) {
		//			foreach (DataGridColumn column in e2.OldItems) {
		//				dataGrid.Columns.Remove(column);
		//			}
		//		} else if (e2.Action == NotifyCollectionChangedAction.Replace) {
		//			dataGrid.Columns[e2.NewStartingIndex] = (DataGridColumn)e2.NewItems[0];
		//		}
		//		//Localization.Localization.UpdateLocaleAtDataGridColumns(dataGrid);

		//}

		#endregion

	}
}
