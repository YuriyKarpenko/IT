using System.Data;

namespace IT.Data
{
	/// <summary>
	/// Сущность имеет стстояние записи
	/// </summary>
	public interface IDto_RowState
	{
		/// <summary>
		/// Состояние записи
		/// </summary>
		DataRowState RowState { get; set; }
	}
}
