// Decompiled with JetBrains decompiler
// Type: DataGridExtensions.DataGridFilter
// Assembly: DataGridExtensions, Version=1.0.37.0, Culture=neutral, PublicKeyToken=43de855f87de903a
// MVID: 5A6EF75F-94BB-4773-88B0-C65701D66FE0
// Assembly location: D:\SVN\NPF\release\packages\DataGridExtensions.1.0.37\lib\net40-Client\DataGridExtensions.dll

using System;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Controls;

namespace IT.WPF.JetBrains
{
	/// <summary>
	/// Defines the attached properties that can be set on the data grid level.
	/// </summary>
	public static class DataGridFilter
	{
		/// <summary>Identifies the IsAutoFilterEnabled dependency property</summary>
		public static readonly DependencyProperty IsAutoFilterEnabledProperty = DependencyProperty.RegisterAttached("IsAutoFilterEnabled", typeof(bool), typeof(DataGridFilter), (PropertyMetadata)new FrameworkPropertyMetadata((object)false, new PropertyChangedCallback(DataGridFilter.IsAutoFilterEnabled_Changed)));
		/// <summary>
		/// Identifies the Filters dependency property.
		/// This property definition is private, so it's only accessible by code and can't be messed up by invalid bindings.
		/// </summary>
		private static readonly DependencyProperty FilterProperty = DependencyProperty.RegisterAttached("Filter", typeof(DataGridFilterHost), typeof(DataGridFilter));
		private static readonly IContentFilterFactory DefaultContentFilterFactory = (IContentFilterFactory)new SimpleContentFilterFactory(StringComparison.CurrentCultureIgnoreCase);
		/// <summary>Identifies the ContentFilterFactory dependency property</summary>
		public static readonly DependencyProperty ContentFilterFactoryProperty = DependencyProperty.RegisterAttached("ContentFilterFactory", typeof(IContentFilterFactory), typeof(DataGridFilter), (PropertyMetadata)new FrameworkPropertyMetadata((object)DataGridFilter.DefaultContentFilterFactory, (PropertyChangedCallback)null, new CoerceValueCallback(DataGridFilter.ContentFilterFactory_CoerceValue)));
		/// <summary>
		/// Identifies the FilterEvaluationDelay dependency property
		/// </summary>
		public static readonly DependencyProperty FilterEvaluationDelayProperty = DependencyProperty.RegisterAttached("FilterEvaluationDelay", typeof(TimeSpan), typeof(DataGridFilter), (PropertyMetadata)new FrameworkPropertyMetadata((object)TimeSpan.FromSeconds(0.5)));
		/// <summary>
		/// Identifies the <see cref="P:DataGridExtensions.DataGridFilter.ResourceLocator" /> attached property
		/// </summary>
		/// <AttachedPropertyComments>
		///   <summary>
		///       Set an resource locator to locate resource if the component resource keys can not be found, e.g. because dgx is used in a plugin and multiple assemblies with resources might exist.
		///       </summary>
		/// </AttachedPropertyComments>
		public static readonly DependencyProperty ResourceLocatorProperty = DependencyProperty.RegisterAttached("ResourceLocator", typeof(IResourceLocator), typeof(DataGridFilter), (PropertyMetadata)new FrameworkPropertyMetadata((PropertyChangedCallback)null));
		/// <summary>
		/// Identifies the <see cref="P:DataGridExtensions.GlobalFilter" /> dependency property.
		/// </summary>
		/// <AttachedPropertyComments>
		///   <summary>
		///       Allows to specify a global filter that is applied to the items in addition to the column filters.
		///       </summary>
		/// </AttachedPropertyComments>
		public static readonly DependencyProperty GlobalFilterProperty = DependencyProperty.RegisterAttached("GlobalFilter", typeof(Predicate<object>), typeof(DataGridFilter), (PropertyMetadata)new FrameworkPropertyMetadata(new PropertyChangedCallback(DataGridFilter.GlobalFilter_Changed)));
		
		/// <summary> Template for the filter on a column represented by a DataGridTextColumn. </summary>
		public static readonly ResourceKey TextColumnFilterTemplateKey = (ResourceKey)new ComponentResourceKey(typeof(DataGridFilter), (object)typeof(DataGridTextColumn));
		
		/// <summary> Template for the filter on a column represented by a DataGridCheckBoxColumn. </summary>
		public static readonly ResourceKey CheckBoxColumnFilterTemplateKey = (ResourceKey)new ComponentResourceKey(typeof(DataGridFilter), (object)typeof(DataGridCheckBoxColumn));
		
		/// <summary> Template for the filter on a column represented by a DataGridCheckBoxColumn. </summary>
		public static readonly ResourceKey TemplateColumnFilterTemplateKey = (ResourceKey)new ComponentResourceKey(typeof(DataGridFilter), (object)typeof(DataGridTemplateColumn));
		
		/// <summary>Template for the whole column header.</summary>
		public static readonly ResourceKey ColumnHeaderTemplateKey = (ResourceKey)new ComponentResourceKey(typeof(DataGridFilter), (object)"ColumnHeaderTemplate");
		
		/// <summary>The filter icon template.</summary>
		public static readonly ResourceKey IconTemplateKey = (ResourceKey)new ComponentResourceKey(typeof(DataGridFilter), (object)"IconTemplate");
		
		/// <summary>The filter icon style.</summary>
		public static readonly ResourceKey IconStyleKey = (ResourceKey)new ComponentResourceKey(typeof(DataGridFilter), (object)"IconStyle");
		
		/// <summary> Style for the filter check box in a filtered DataGridCheckBoxColumn. </summary>
		public static readonly ResourceKey ColumnHeaderSearchCheckBoxStyleKey = (ResourceKey)new ComponentResourceKey(typeof(DataGridFilter), (object)"ColumnHeaderSearchCheckBoxStyle");
		
		/// <summary> Style for the filter text box in a filtered DataGridTextColumn. </summary>
		public static readonly ResourceKey ColumnHeaderSearchTextBoxStyleKey = (ResourceKey)new ComponentResourceKey(typeof(DataGridFilter), (object)"ColumnHeaderSearchTextBoxStyle");
		
		/// <summary> Style for the clear button in the filter text box in a filtered DataGridTextColumn. </summary>
		public static readonly ResourceKey ColumnHeaderSearchTextBoxClearButtonStyleKey = (ResourceKey)new ComponentResourceKey(typeof(DataGridFilter), (object)"ColumnHeaderSearchTextBoxClearButtonStyle");

		/// <summary> Gets if the default filters are automatically attached to each column. </summary>
		/// <requires csharp="obj != null" vb="obj &lt;&gt; Nothing">obj != null</requires>
		[AttachedPropertyBrowsableForType(typeof(DataGrid))]
		public static bool GetIsAutoFilterEnabled(this DataGrid obj)
		{
			// ISSUE: reference to a compiler-generated method
			__ContractsRuntime.Requires(obj != null, (string)null, "obj != null");
			return obj.GetValue<bool>(DataGridFilter.IsAutoFilterEnabledProperty);
		}

		/// <summary>
		/// Sets if the default filters are automatically attached to each column. Set to false if you want to control filters by code.
		/// </summary>
		/// <requires csharp="obj != null" vb="obj &lt;&gt; Nothing">obj != null</requires>
		public static void SetIsAutoFilterEnabled(this DataGrid obj, bool value)
		{
			// ISSUE: reference to a compiler-generated method
			__ContractsRuntime.Requires(obj != null, (string)null, "obj != null");
			obj.SetValue(DataGridFilter.IsAutoFilterEnabledProperty, (object)value);
		}

		private static void IsAutoFilterEnabled_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			DataGrid dataGrid = sender as DataGrid;
			if (dataGrid == null)
				return;
			dataGrid.GetFilter().Enable(true.Equals(e.NewValue));
		}

		/// <summary>
		/// Filter attached property to attach the DataGridFilterHost instance to the owning DataGrid.
		/// This property is only used by code and is not accessible from XAML.
		/// </summary>
		/// <requires csharp="dataGrid != null" vb="dataGrid &lt;&gt; Nothing">dataGrid != null</requires>
		/// <ensures csharp="Contract.Result&lt;DataGridExtensions.DataGridFilterHost&gt;() != null" vb="Contract.Result(Of DataGridExtensions.DataGridFilterHost)() &lt;&gt; Nothing">result != null</ensures>
		public static DataGridFilterHost GetFilter(this DataGrid dataGrid)
		{
			// ISSUE: reference to a compiler-generated method
			__ContractsRuntime.Requires(dataGrid != null, (string)null, "dataGrid != null");
			DataGridFilterHost dataGridFilterHost = (DataGridFilterHost)dataGrid.GetValue(DataGridFilter.FilterProperty);
			if (dataGridFilterHost == null)
			{
				dataGridFilterHost = new DataGridFilterHost(dataGrid);
				dataGrid.SetValue(DataGridFilter.FilterProperty, (object)dataGridFilterHost);
			}
			return dataGridFilterHost;
		}

		/// <summary>
		/// Gets the content filter factory for the data grid filter.
		/// </summary>
		/// <requires csharp="dataGrid != null" vb="dataGrid &lt;&gt; Nothing">dataGrid != null</requires>
		/// <ensures csharp="Contract.Result&lt;DataGridExtensions.IContentFilterFactory&gt;() != null" vb="Contract.Result(Of DataGridExtensions.IContentFilterFactory)() &lt;&gt; Nothing">result != null</ensures>
		[AttachedPropertyBrowsableForType(typeof(DataGrid))]
		public static IContentFilterFactory GetContentFilterFactory(this DataGrid dataGrid)
		{
			// ISSUE: reference to a compiler-generated method
			__ContractsRuntime.Requires(dataGrid != null, (string)null, "dataGrid != null");
			return (IContentFilterFactory)dataGrid.GetValue(DataGridFilter.ContentFilterFactoryProperty) ?? DataGridFilter.DefaultContentFilterFactory;
		}

		/// <summary>
		/// Sets the content filter factory for the data grid filter.
		/// </summary>
		public static void SetContentFilterFactory(this DataGrid dataGrid, IContentFilterFactory value)
		{
			if (dataGrid == null)
				throw new ArgumentNullException("dataGrid");
			dataGrid.SetValue(DataGridFilter.ContentFilterFactoryProperty, (object)value);
		}

		private static object ContentFilterFactory_CoerceValue(DependencyObject sender, object value)
		{
			return value ?? (object)DataGridFilter.DefaultContentFilterFactory;
		}

		/// <summary>
		/// Gets the delay that is used to throttle filter changes before the filter is applied.
		/// </summary>
		/// <param name="obj">The data grid</param>
		/// <returns>The throttle delay.</returns>
		/// <requires csharp="obj != null" vb="obj &lt;&gt; Nothing">obj != null</requires>
		public static TimeSpan GetFilterEvaluationDelay(this DataGrid obj)
		{
			// ISSUE: reference to a compiler-generated method
			__ContractsRuntime.Requires(obj != null, (string)null, "obj != null");
			return obj.GetValue<TimeSpan>(DataGridFilter.FilterEvaluationDelayProperty);
		}

		/// <summary>
		/// Sets the delay that is used to throttle filter changes before the filter is applied.
		/// </summary>
		/// <param name="obj">The data grid</param>
		/// <param name="value">The new throttle delay.</param>
		/// <requires csharp="obj != null" vb="obj &lt;&gt; Nothing">obj != null</requires>
		public static void SetFilterEvaluationDelay(this DataGrid obj, TimeSpan value)
		{
			// ISSUE: reference to a compiler-generated method
			__ContractsRuntime.Requires(obj != null, (string)null, "obj != null");
			obj.SetValue(DataGridFilter.FilterEvaluationDelayProperty, (object)value);
		}

		/// <summary>Gets the resource locator.</summary>
		/// <param name="obj">The object.</param>
		/// <returns>The locator</returns>
		/// <requires csharp="obj != null" vb="obj &lt;&gt; Nothing">obj != null</requires>
		[AttachedPropertyBrowsableForType(typeof(DataGrid))]
		public static IResourceLocator GetResourceLocator(this DataGrid obj)
		{
			// ISSUE: reference to a compiler-generated method
			__ContractsRuntime.Requires(obj != null, (string)null, "obj != null");
			return obj.GetValue<IResourceLocator>(DataGridFilter.ResourceLocatorProperty);
		}

		/// <summary>Sets the resource locator.</summary>
		/// <param name="obj">The object.</param>
		/// <param name="value">The value.</param>
		/// <requires csharp="obj != null" vb="obj &lt;&gt; Nothing">obj != null</requires>
		public static void SetResourceLocator(this DataGrid obj, IResourceLocator value)
		{
			// ISSUE: reference to a compiler-generated method
			__ContractsRuntime.Requires(obj != null, (string)null, "obj != null");
			obj.SetValue(DataGridFilter.ResourceLocatorProperty, (object)value);
		}

		/// <summary>
		/// Gets the value of the <see cref="P:DataGridExtensions.GlobalFilter" /> attached property from a given <see cref="T:System.Windows.Controls.DataGrid" />.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Windows.Controls.DataGrid" /> from which to read the property value.</param>
		/// <returns>the value of the <see cref="P:DataGridExtensions.GlobalFilter" /> attached property.</returns>
		/// <requires csharp="obj != null" vb="obj &lt;&gt; Nothing">obj != null</requires>
		[AttachedPropertyBrowsableForType(typeof(DataGrid))]
		public static Predicate<object> GetGlobalFilter(DataGrid obj)
		{
			// ISSUE: reference to a compiler-generated method
			__ContractsRuntime.Requires(obj != null, (string)null, "obj != null");
			return (Predicate<object>)obj.GetValue(DataGridFilter.GlobalFilterProperty);
		}

		/// <summary>
		/// Sets the value of the <see cref="P:DataGridExtensions.GlobalFilter" /> attached property to a given <see cref="T:System.Windows.Controls.DataGrid" />.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Windows.Controls.DataGrid" /> on which to set the property value.</param>
		/// <param name="value">The property value to set.</param>
		/// <requires csharp="obj != null" vb="obj &lt;&gt; Nothing">obj != null</requires>
		public static void SetGlobalFilter(DataGrid obj, Predicate<object> value)
		{
			// ISSUE: reference to a compiler-generated method
			__ContractsRuntime.Requires(obj != null, (string)null, "obj != null");
			obj.SetValue(DataGridFilter.GlobalFilterProperty, (object)value);
		}

		/// <requires csharp="d != null" vb="d &lt;&gt; Nothing">d != null</requires>
		private static void GlobalFilter_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((DataGrid)d).GetFilter().SetGlobalFilter((Predicate<object>)e.NewValue);
		}
	}
}
