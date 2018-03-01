namespace IT.WPF.Filters
{
	public interface IContentFilterFactory
	{
		IContentFilter Create(DataGridFilters filterType, object content);
	}
}
