using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using IT.WPF.Filters;

namespace IT.WPF
{
	public static class ColumnHeaderFilter
	{
		/// <summary>
		/// 
		/// </summary>
		public static readonly DependencyProperty FilterProperty = DependencyProperty.RegisterAttached(
			"Filter", typeof(IContentFilter), typeof(ColumnHeaderFilter));

		//[AttachedPropertyBrowsableForType(DataGridColumn)]
		public static IContentFilter GetFilter(DependencyObject o) => o.GetValue<IContentFilter>(FilterProperty);

		public static void SetFilter(DependencyObject o, IContentFilter value) => o.SetValue(FilterProperty, value);


	}
}
