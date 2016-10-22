using System.Data;
using System.Data.Common;

namespace IT.Data
{

	/// <summary>
	/// Класс, ипользующий одно соединение, который предоставляет метод для кеширования DbCommand : Сmd()/>
	/// </summary>
	public class DataAdapterBase : Disposable
	{
		static object _locker = new object();

		/// <summary>
		/// Кэш команд
		/// </summary>
		protected MemCache<string, IDbCommand> _cmdCache = new MemCache<string, IDbCommand>();


		/// <summary>
		/// Реализация интерфейса IDataAdapterBase
		/// </summary>
		public virtual IDbConnection Connection { get; private set; }

		/// <summary>
		/// Получение/создание кешированного DbCommand (параметры не должны вызывать затруднений ;)
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="type"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		protected IDbCommand Cmd(string sql, CommandType type = CommandType.Text, params string[] parameters)
		{
			return this._cmdCache[sql, () => this.Connection.CreateCommand(sql, type, parameters)];
		}


		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="conn">Соединение для создания комманд</param>
		public DataAdapterBase(IDbConnection conn)
		{
			Contract.NotNull(conn, "conn");
			this.Connection = conn;
		}


		/// <summary>
		/// Реализация интерфейса IDisposable
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			//this.Debug("Dispose({0})", disposing);

			if (disposing)
			{
				if (this.Connection != null)
				{
					this.Connection.CloseIfNotClosed();
					this.Connection.Dispose();
				}

				if (this._cmdCache != null)
				{
					this._cmdCache.Clear();
					this._cmdCache = null;
				}
			}
			this.Connection = null;

			base.Dispose(disposing);
		}
	}

}
