// Decompiled with JetBrains decompiler
// Type: DataGridExtensions.DataGridFilterColumn
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
	/// Defines the attached properties that can be set on the data grid column level.
	/// </summary>
	public static class DataGridFilterColumn
	{
		/// <summary>Identifies the IsFilterVisible dependency property</summary>
		public static readonly DependencyProperty IsFilterVisibleProperty = DependencyProperty.RegisterAttached("IsFilterVisible", typeof(bool), typeof(DataGridFilterColumn), (PropertyMetadata)new FrameworkPropertyMetadata((object)true));
		/// <summary>Identifies the Template dependency property.</summary>
		public static readonly DependencyProperty TemplateProperty = DependencyProperty.RegisterAttached("Template", typeof(ControlTemplate), typeof(DataGridFilterColumn));
		/// <summary>Identifies the Filter dependency property</summary>
		public static readonly DependencyProperty FilterProperty = DependencyProperty.RegisterAttached("Filter", typeof(object), typeof(DataGridFilterColumn));

		/// <summary>Control the visibility of the filter for this column.</summary>
		public static bool GetIsFilterVisible(this DataGridColumn column)
		{
			if (column == null)
				throw new ArgumentNullException("column");
			return column.GetValue<bool>(DataGridFilterColumn.IsFilterVisibleProperty);
		}

		/// <summary>Control the visibility of the filter for this column.</summary>
		public static void SetIsFilterVisible(this DataGridColumn column, bool value)
		{
			if (column == null)
				throw new ArgumentNullException("column");
			column.SetValue(DataGridFilterColumn.IsFilterVisibleProperty, (object)value);
		}

		/// <summary>
		/// Gets the control template for the filter of this column. If the template is null or unset, a default template will be used.
		/// </summary>
		/// <requires csharp="column != null" vb="column &lt;&gt; Nothing">column != null</requires>
		public static ControlTemplate GetTemplate(this DataGridColumn column)
		{
			// ISSUE: reference to a compiler-generated method
			__ContractsRuntime.Requires(column != null, (string)null, "column != null");
			return (ControlTemplate)column.GetValue(DataGridFilterColumn.TemplateProperty);
		}

		/// <summary>
		/// Sets the control template for the filter of this column. If the template is null or unset, a default template will be used.
		/// </summary>
		/// <requires csharp="column != null" vb="column &lt;&gt; Nothing">column != null</requires>
		public static void SetTemplate(this DataGridColumn column, ControlTemplate value)
		{
			// ISSUE: reference to a compiler-generated method
			__ContractsRuntime.Requires(column != null, (string)null, "column != null");
			column.SetValue(DataGridFilterColumn.TemplateProperty, (object)value);
		}

		/// <summary>Gets the filter expression of the column.</summary>
		/// <requires csharp="column != null" vb="column &lt;&gt; Nothing">column != null</requires>
		public static object GetFilter(this DataGridColumn column)
		{
			// ISSUE: reference to a compiler-generated method
			__ContractsRuntime.Requires(column != null, (string)null, "column != null");
			return column.GetValue(DataGridFilterColumn.FilterProperty);
		}

		/// <summary>Sets the filter expression of the column.</summary>
		/// <requires csharp="column != null" vb="column &lt;&gt; Nothing">column != null</requires>
		public static void SetFilter(this DataGridColumn column, object value)
		{
			// ISSUE: reference to a compiler-generated method
			__ContractsRuntime.Requires(column != null, (string)null, "column != null");
			column.SetValue(DataGridFilterColumn.FilterProperty, value);
		}
	}
}
