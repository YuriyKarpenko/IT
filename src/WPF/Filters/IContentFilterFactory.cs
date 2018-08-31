namespace IT.WPF.Filters
{
	/// <summary>
	/// Фабрика фильтров контента
	/// </summary>
	public interface IContentFilterFactory
	{
		/// <summary>
		/// получение фильтра
		/// </summary>
		/// <param name="filterType"></param>
		/// <param name="content"></param>
		/// <returns></returns>
		IContentFilter Create(DataGridFilters filterType, object content);
	}
}
