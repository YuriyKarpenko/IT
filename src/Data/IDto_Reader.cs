using System.Data;

namespace IT.Data
{
	/// <summary>
	/// Интерфейс, используемый для расширения  List%3CT%3E ExecuteList%3CT%3E(this DbCommand cmd, DbTransaction transaction = null) where T : IDtoBase_Reader, new()'
	/// </summary>
	public interface IDto_Reader
	{
		/// <summary>
		/// Заполняет себя данными из DbDataReader
		/// </summary>
		/// <param name="dr">Открытый и готовый для считывания одной записи DbDataReader</param>
		void Init(IDataReader dr);
	}
}
