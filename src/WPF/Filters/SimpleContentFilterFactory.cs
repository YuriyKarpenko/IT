using System;

namespace IT.WPF.Filters
{
	/// <summary> Factory to create a <see cref="T:IT.WPF.Filter.IContentFilter" /></summary>
	public class SimpleContentFilterFactory : IContentFilterFactory
	{
		/// <summary>Gets or sets the string comparison.</summary>
		public StringComparison StringComparison { get; set; }

		/// <summary> .ctor </summary>
		/// <param name="stringComparison">The string comparison to use.</param>
		public SimpleContentFilterFactory(StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
		{
			this.StringComparison = stringComparison;
		}

		/// <summary> Создает фильтр содержимого для указанного содержимого. </summary>
		/// <param name="content">Содержимое для создания фильтра.</param>
		/// <param name="filterType">Тип фильтра</param>
		/// <returns>The new filter.</returns>
		public IContentFilter Create(DataGridFilters filterType, object content)
		{
			if (content == null)
				throw new ArgumentNullException("content");

			switch (filterType)
			{
				case DataGridFilters.ComboBox:
					return new ContentFilterEquals(content.ToString(), StringComparison);
				case DataGridFilters.TextBoxContains:
					return new ContentFilterContains(content.ToString(), StringComparison);
			}

			return null;
		}
	}
}
