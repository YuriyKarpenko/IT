using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using IT.WPF.Filters;

namespace IT.WPF
{
	/// <summary>
	/// Присоединенные свойства для использования фильтрации
	/// </summary>
    public static class DataGridFilter
    {
		/*
AttachedPropertyBrowsableAttribute 
AttachedPropertyBrowsableForChildrenAttribute 
AttachedPropertyBrowsableForTypeAttribute 
AttachedPropertyBrowsableWhenAttributePresentAttribute 
		*/

		private static readonly SimpleContentFilterFactory DefaultContentFilterFactory = new SimpleContentFilterFactory(StringComparison.CurrentCultureIgnoreCase);


		#region ResourceKey

		/// <summary>Template для заголовка в общем </summary>
		public static readonly ResourceKey ColumnHeaderTemplateKey = new ComponentResourceKey(typeof(DataGridFilter), "ColumnHeaderTemplate");

		/// <summary> Template для фильтра для столбца, представленного DataGridTextColumn. </summary>
		public static readonly ResourceKey TextColumnFilterTemplateKey = (ResourceKey)new ComponentResourceKey(typeof(DataGridFilter), typeof(DataGridTextColumn));

		/// <summary> Template для фильтра для столбца, представленного DataGridCheckBoxColumn. </summary>
		public static readonly ResourceKey CheckBoxColumnFilterTemplateKey = (ResourceKey)new ComponentResourceKey(typeof(DataGridFilter), typeof(DataGridCheckBoxColumn));

		/// <summary> Template для фильтра для столбца, представленного DataGridCheckBoxColumn. </summary>
		public static readonly ResourceKey TemplateColumnFilterTemplateKey = (ResourceKey)new ComponentResourceKey(typeof(DataGridFilter), typeof(DataGridTemplateColumn));

		#endregion

		#region AutoFilter

		/// <summary> Включение простого автофильтра как в excel </summary>
		public static readonly DependencyProperty AutoFilterProperty = DependencyProperty.RegisterAttached(
			"AutoFilter", typeof(bool), typeof(DataGridBehaviour), new PropertyMetadata(false, AutoFilterChangedCallback));

		/// <summary> </summary>
		/// <param name="dg"></param>
		/// <returns></returns>
		[AttachedPropertyBrowsableForType(typeof(DataGrid))]
		public static bool GetAutoFilter(this DataGrid dg) => dg.GetValue<bool>(AutoFilterProperty);

		/// <summary> </summary>
		/// <param name="dg"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static void SetAutoFilter(this DataGrid dg, bool value) => dg.SetValue(AutoFilterProperty, value);

		private static void AutoFilterChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var dg = d as DataGrid;
			if (dg != null)
			{
				dg.GetFilter().Enable(true.Equals(e.NewValue));
			}
		}

		#endregion

		#region Filter

		private static readonly DependencyProperty FilterProperty = DependencyProperty.RegisterAttached("Filter", typeof(DataGridFilterCore), typeof(DataGridFilter));

		internal static DataGridFilterCore GetFilter(this DataGrid dg)
		{
			var res = dg.GetValue<DataGridFilterCore>(FilterProperty);
			if (res == null)
			{
				res = new DataGridFilterCore(dg);
				dg.SetFilter(res);
			}
			return res;
		}

		private static void SetFilter(this DataGrid dg, DataGridFilterCore val) => dg.SetValue(FilterProperty, val);

		#endregion

		#region ContentFilterFactory

		/// <summary> получение фабрики фильтров </summary>
		/// <param name="dg"></param>
		/// <returns></returns>
		public static SimpleContentFilterFactory GetContentFilterFactory(this DataGrid dg)
		{
			return DefaultContentFilterFactory;
		}

		#endregion

		internal static object FindResource(this DataGrid dg, object resourceKey)
		{
			//IResourceLocator resourceLocator = dg?.GetResourceLocator();
			object res =
				//resourceLocator?.FindResource(DataGrid, DataGridFilter.ColumnHeaderTemplateKey) ?? 
				dg?.TryFindResource((object)DataGridFilter.ColumnHeaderTemplateKey);
			return res;
		}
	}
}
