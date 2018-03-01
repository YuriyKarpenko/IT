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
		public static readonly ResourceKey HeadegFilter_DockButtom_TemplateKey = new ComponentResourceKey(typeof(DataGridFilter), nameof(HeadegFilter_DockButtom_TemplateKey));
		/// <summary>Template для заголовка в общем </summary>
		public static readonly ResourceKey HeadegFilter_DockRight_TemplateKey = new ComponentResourceKey(typeof(DataGridFilter), nameof(HeadegFilter_DockRight_TemplateKey));

		/// <summary> Template для фильтра для столбца, представленного DataGridTextColumn. </summary>
		public static readonly ResourceKey DataGridFilters_ComboBox_TemplateKey = new ComponentResourceKey(typeof(DataGridFilter), DataGridFilters.ComboBox);
		/// <summary> Template для фильтра для столбца, представленного DataGridTextColumn. </summary>
		public static readonly ResourceKey DataGridFilters_TextBoxContains_TemplateKey = new ComponentResourceKey(typeof(DataGridFilter), DataGridFilters.TextBoxContains);

		/// <summary>The filter icon template.</summary>
		public static readonly ResourceKey Icon_TemplateKey = new ComponentResourceKey(typeof(DataGridFilter), "IconTemplate");


		//	styles

		/// <summary>The filter icon style.</summary>
		public static readonly ResourceKey Icon_StyleKey = new ComponentResourceKey(typeof(DataGridFilter), "IconStyle");

		/// <summary> Style for the filter text box in a filtered DataGridTextColumn. </summary>
		public static readonly ResourceKey SearchTextBox_StyleKey = new ComponentResourceKey(typeof(DataGridFilter), nameof(SearchTextBox_StyleKey));

		/// <summary> Style for the filter ComboBox in a filtered DataGridTextColumn. </summary>
		public static readonly ResourceKey ComboBox_StyleKey = new ComponentResourceKey(typeof(DataGridFilter), nameof(ComboBox_StyleKey));

		/// <summary> Style for the clear button in the filter text box in a filtered DataGridTextColumn. </summary>
		public static readonly ResourceKey SearchTextBoxClearButton_StyleKey = new ComponentResourceKey(typeof(DataGridFilter), nameof(SearchTextBoxClearButton_StyleKey));



		#endregion

		#region AutoFilter

		/// <summary> Включение простого автофильтра как в excel </summary>
		public static readonly DependencyProperty AutoFilterProperty = DependencyProperty.RegisterAttached(
			"AutoFilter", typeof(DataGridFilters), typeof(DataGridFilter), new PropertyMetadata(DataGridFilters.Disabled, AutoFilterChangedCallback)
			);

		/// <summary> </summary>
		/// <param name="dg"></param>
		/// <returns></returns>
		[AttachedPropertyBrowsableForType(typeof(DataGrid))]
		public static DataGridFilters GetAutoFilter(this DataGrid dg) => dg.GetValue<DataGridFilters>(AutoFilterProperty);

		/// <summary> </summary>
		/// <param name="dg"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static void SetAutoFilter(this DataGrid dg, DataGridFilters value) => dg.SetValue(AutoFilterProperty, value);

		private static void AutoFilterChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var dg = d as DataGrid;
			if (dg != null)
			{
				dg.GetFilter().Enable((DataGridFilters)e.NewValue);
			}
		}

		#endregion

		#region FilterCore

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

		#region GlobalFilter

		///	<summary> Позволяет указать глобальный фильтр, который применяется к элементам в дополнение к фильтрам столбцов. </summary>
		public static readonly DependencyProperty GlobalFilterProperty = DependencyProperty.RegisterAttached("GlobalFilter", typeof(Predicate<object>), typeof(DataGridFilter)
			, new FrameworkPropertyMetadata(new PropertyChangedCallback(DataGridFilter.GlobalFilter_Changed))
			);

		/// <summary>
		/// Gets the value of the <see cref="P:IT.WPF.GlobalFilter" /> attached property from a given <see cref="T:System.Windows.Controls.DataGrid" />.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Windows.Controls.DataGrid" /> from which to read the property value.</param>
		/// <returns>the value of the <see cref="P:DataGridExtensions.GlobalFilter" /> attached property.</returns>
		/// <requires csharp="obj != null" vb="obj &lt;&gt; Nothing">obj != null</requires>
		[AttachedPropertyBrowsableForType(typeof(DataGrid))]
		public static Predicate<object> GetGlobalFilter(this DataGrid obj)
		{
			// ISSUE: reference to a compiler-generated method
			//__ContractsRuntime.Requires(obj != null, (string)null, "obj != null");
			return (Predicate<object>)obj.GetValue(DataGridFilter.GlobalFilterProperty);
		}

		/// <summary>
		/// Sets the value of the <see cref="P:IT.WPF.GlobalFilter" /> attached property to a given <see cref="T:System.Windows.Controls.DataGrid" />.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Windows.Controls.DataGrid" /> on which to set the property value.</param>
		/// <param name="value">The property value to set.</param>
		/// <requires csharp="obj != null" vb="obj &lt;&gt; Nothing">obj != null</requires>
		public static void SetGlobalFilter(this DataGrid obj, Predicate<object> value)
		{
			// ISSUE: reference to a compiler-generated method
			//__ContractsRuntime.Requires(obj != null, (string)null, "obj != null");
			obj.SetValue(DataGridFilter.GlobalFilterProperty, (object)value);
		}

		/// <requires csharp="d != null" vb="d &lt;&gt; Nothing">d != null</requires>
		private static void GlobalFilter_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((DataGrid)d).GetFilter().SetGlobalFilter((Predicate<object>)e.NewValue);
		}

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

		internal static object FindResource(this FrameworkElement obj, object resourceKey, DependencyObject source)
		{
			if (resourceKey != null)
			{
				//IResourceLocator resourceLocator = dg?.GetResourceLocator();
				object res =
					//resourceLocator?.FindResource(source, resourceKey) ?? 
					obj?.TryFindResource(resourceKey);
				return res;
			}
			return null;
		}

		/// <summary> Возвращает задержку, которая используется для изменения фильтра дроссельной заслонки до применения фильтра. </summary>
		/// <requires csharp="obj != null" vb="obj &lt;&gt; Nothing">obj != null</requires>
		/// <param name="dg"></param>
		/// <returns> Задержка дроссельной заслонки </returns>
		public static TimeSpan GetFilterEvaluationDelay(this DataGrid dg)
		{
			// ISSUE: reference to a compiler-generated method
			//__ContractsRuntime.Requires(obj != null, (string)null, "obj != null");
			//return obj.GetValue<TimeSpan>(DataGridFilter.FilterEvaluationDelayProperty);
			switch (dg.GetAutoFilter())
			{
				case DataGridFilters.TextBoxContains:
					return TimeSpan.FromSeconds(0.5);
			}
			return TimeSpan.FromSeconds(0);
		}

	}
}
