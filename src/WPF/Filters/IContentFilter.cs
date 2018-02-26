namespace IT.WPF.Filters

{
	/// <summary>Интерфейс для фильтрации содезжимого (Content)</summary>
	public interface IContentFilter
	{
		/// <summary>
		/// Determines whether the specified value matches the condition of this filter.
		/// </summary>
		/// <param name="value">The content.</param>
		/// <returns>
		/// <c>true</c> if the specified value matches the condition; otherwise, <c>false</c>.
		///     </returns>
		bool IsMatch(object value);
	}
}
