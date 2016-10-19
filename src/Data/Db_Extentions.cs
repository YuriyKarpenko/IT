using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using IT.Log;

namespace IT.Data
{
	/// <summary>
	/// Расширения для некоторых классов пространства имен System.Data.Common
	/// </summary>
	public static class Db_Extentions
	{
		#region Connection

		/// <summary>
		/// Показывает состояние соединения
		/// </summary>
		/// <returns></returns>
		public static bool IsActive(this IDbConnection conn)
		{
			return conn.State == ConnectionState.Open;
		}

		/// <summary>
		/// Проверяет состояние соединения и, если не закрыто, то закрывает его
		/// </summary>
		public static void CloseIfNotClosed(this IDbConnection conn)
		{
			//Logger.ToLogFmt(null, TraceLevel.Verbose, null, "()");
			Contract.Requires<ArgumentException>(conn != null, "Connection");

			try
			{
				if (conn.State != ConnectionState.Closed)
				{
					conn.Close();
				}
			}
			catch //(Exception ex)
			{
				//Logger.Error(ex, "DbConnection.CloseIsNotClose()");
				throw;
			}
		}

		/// <summary>
		/// Проверяет состояние соединения и, если не открыто, то открывает его
		/// </summary>
		public static void OpenIfClosed(this IDbConnection conn)
		{
			//Logger.ToLogFmt(null, TraceLevel.Verbose, null, "()");
			Contract.Requires<ArgumentException>(conn != null, "Connection");

			try
			{
				if (conn.State == ConnectionState.Closed || conn.State == ConnectionState.Broken)
				{
					conn.Open();
				}
			}
			catch //(Exception ex)
			{
				//Logger.Error(ex, "DbConnection.OpenIsClose()");
				throw;
			}
		}

		/// <summary>
		/// Создает объект DbCommand из данного соединения
		/// </summary>
		/// <param name="Connection">Расширяемый экземпляр</param>
		/// <param name="sql">Текст запорса</param>
		/// <param name="type">тип запроса</param>
		/// <param name="parameters">Набор имен нетипизированых параметров</param>
		/// <returns>Заполненая соответствующими параметрами DbCommand</returns>
		[SuppressMessage("Microsoft.Security", "CA2100:Проверка запросов SQL на уязвимости безопасности")]
		public static IDbCommand CreateCommand(this IDbConnection Connection, string sql, CommandType type = CommandType.Text, params string[] parameters)
		{
			Logger.ToLogFmt(null, TraceLevel.Verbose, null, "({0})", sql);
			Contract.Requires<ArgumentException>(Connection != null, "Connection");

			try
			{
				var cmd = Connection.CreateCommand();
				cmd.CommandText = sql;
				cmd.CommandType = type;

				foreach (string p in parameters)
				{
					cmd.AppendParam(p);
				}

				return cmd;
			}
			catch (Exception ex)
			{
				Logger.ToLogFmt(null, TraceLevel.Warning, ex, "DbConnection.CreateCommand({0})", sql);
				throw;
			}
		}

		///// <summary>
		///// Создает объект DbCommand из данного соединения
		///// </summary>
		///// <param name="Connection">Расширяемый экземпляр</param>
		///// <param name="sql">Текст запорса</param>
		///// <param name="type">тип запроса</param>
		///// <param name="parameters">Набор параметров</param>
		///// <returns>Заполненая соответствующими параметрами DbCommand</returns>
		//[SuppressMessage("Microsoft.Security", "CA2100:Проверка запросов SQL на уязвимости безопасности")]
		//public static DbCommand CreateCommand(this DbConnection Connection, string sql, CommandType type = CommandType.Text, params DbParameter[] parameters)
		//{
		//	Logger.ToLogFmt(null, TraceLevel.Verbose, null, "({0})", sql);

		//	Contract.Requires<ArgumentException>(Connection != null, "Connection");

		//	try
		//	{
		//		var cmd = Connection.CreateCommand();
		//		cmd.CommandText = sql;
		//		cmd.CommandType = type;

		//		foreach (var p in parameters)
		//		{
		//			cmd.Parameters.Add(p);
		//		}

		//		return cmd;
		//	}
		//	catch //(Exception ex)
		//	{
		//		//Logger.Error(ex, "DbConnection.CreateCommand({0})", sql);

		//		throw;
		//	}
		//}

		/// <summary>
		/// Предоставляет открытое соединение, если до этого соединение было закрыто - закрывает его после работы
		/// Исключения НЕ обрабатываются
		/// </summary>
		/// <param name="conn">Расширяемый экземпляр</param>
		/// <param name="OpenedConnection"></param>
		public static void DoWork(this IDbConnection conn, Action<IDbConnection> OpenedConnection)
		{
			//Logger.ToLogFmt(null, TraceLevel.Verbose, null, "()");
			Contract.Requires<ArgumentException>(OpenedConnection != null, "OpenedConnection");

			lock (conn)
			{
				bool isClose = !conn.IsActive();

				try
				{
					conn.OpenIfClosed();

					OpenedConnection(conn);
				}
				//catch (Exception ex)
				//{
				//    Logger.Error(ex, "DbConnection.DoWork({0})", Connection.ConnectionString);
				//    throw;
				//}
				finally
				{
					if (isClose)
					{
						conn.Close();
					}
				}
			}
		}

		/// <summary>
		/// Предоставляет открытое соединение, если до этого соединение было закрыто - закрывает его после работы
		/// Исключения НЕ обрабатываются
		/// </summary>
		/// <param name="conn">Расширяемый экземпляр</param>
		/// <param name="OpenedConnection"></param>
		public static R DoWork<R>(this IDbConnection conn, Func<IDbConnection, R> OpenedConnection)
		{
			//Logger.ToLogFmt(null, TraceLevel.Verbose, null, "()");
			Contract.Requires<ArgumentException>(OpenedConnection != null, "OpenedConnection");

			lock (conn)
			{
				bool isClose = !conn.IsActive();

				try
				{
					conn.OpenIfClosed();

					R res = OpenedConnection(conn);

					return res;
				}
				finally
				{
					if (isClose)
					{
						conn.Close();
					}
				}
			}
		}

		/// <summary>
		/// Предоставляет работу внутри транзакции (и открытое соединение). Возвращает успех транзакции.
		/// с externalTran никаких действий не производится, передается как есть.
		/// </summary>
		/// <param name="conn">Расширяемый экземпляр</param>
		/// <param name="usingTran"></param>
		/// <param name="externalTran">Внешняя транзакция</param>
		/// <param name="isNeedTransaction">Следует ли создавать отдельную транзакцию (актуально при transaction == null)</param>
		/// <returns></returns>
		public static void DoWorkInTran(this IDbConnection conn, Action<IDbTransaction> usingTran, IDbTransaction externalTran = null, bool isNeedTransaction = false)
		{
			//Logger.ToLogFmt(null, TraceLevel.Verbose, null, "()");
			Contract.Requires<ArgumentException>(usingTran != null, "usingTran");

			conn.DoWork(con =>
			{
				if (isNeedTransaction && externalTran == null)
				{
					using (var tran = con.BeginTransaction())
					{
						try
						{
							usingTran(tran);
							tran.Commit();
						}
						catch //(Exception ex)
						{
							tran.Rollback();
							//Logger.Def.Error(ex, "DbConnection.DoWorkInTran({0})", Connection.ConnectionString);
							throw;
						}
					}
				}
				else
					usingTran(externalTran);
			});

		}

		/// <summary>
		/// Предоставляет работу внутри транзакции (и открытое соединение). Возвращает успех транзакции.
		/// с externalTran никаких действий не производится, передается как есть.
		/// </summary>
		/// <param name="Connection">Расширяемый экземпляр</param>
		/// <param name="usingTran"></param>
		/// <param name="externalTran">Внешняя транзакция</param>
		/// <param name="isNeedTransaction">Следует ли создавать отдельную транзакцию (актуально при transaction == null)</param>
		/// <returns></returns>
		public static R DoWorkInTran<R>(this IDbConnection Connection, Func<IDbTransaction, R> usingTran, IDbTransaction externalTran = null, bool isNeedTransaction = false)
		{
			//Logger.ToLogFmt(null, TraceLevel.Verbose, null, "()");
			Contract.Requires<ArgumentException>(usingTran != null, "usingTran");

			return Connection.DoWork<R>(con =>
			{
				if (isNeedTransaction && externalTran == null)
				{
					using (var tran = con.BeginTransaction())
					{
						try
						{
							return usingTran(tran);
						}
						catch
						{
							tran.Rollback();
							throw;
						}
						finally
						{
							if (tran.Connection != null)
								tran.Commit();
						}
					}
				}
				else
					return usingTran(externalTran);
			});
		}

		/// <summary>
		/// Выполняет команду в контексте указанной транзакции
		/// При этом открывается соединение, и закрывается (если до этого было закрыто)
		/// </summary>
		/// <param name="con">Расширяемый экземпляр</param>
		/// <param name="sql">Текст команды</param>
		/// <param name="transaction">Транзакция, в контесте которой выполняется команда</param>
		/// <param name="isNeedTransaction">Следует ли создавать отдельную транзакцию (актуально при transaction == null)</param>
		/// <returns></returns>
		public static object ExecuteScalarInTran(this IDbConnection con, string sql, IDbTransaction transaction = null, bool isNeedTransaction = false)
		{
			//Logger.ToLogFmt(null, TraceLevel.Verbose, null, "({0})", sql.Substring(0, 20));
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(sql), "sql");

			using (var cmd = con.CreateCommand(sql, CommandType.Text, new string[] { }))
			{
				return cmd.ExecuteScalarInTran(transaction, isNeedTransaction);
			}
		}

		#endregion

		#region DbCommand

		/// <summary>
		/// Добавляет нетипизированый параметр, и возвращает его для указания прочих свойств
		/// </summary>
		/// <param name="cmd">Расширяемый экземпляр</param>
		/// <param name="paramName">Наименование параметра (допускается впереди указать направление как [ in, Input, inout, InputOutput, out, Output, ret, ReturnValue ] без учета регистра)</param>
		/// <param name="value">value</param>
		/// <returns>Привязаный к команде параметр</returns>
		public static IDbDataParameter SetParameter(this IDbCommand cmd, string paramName, object value)
		{
			//Logger.ToLogFmt(null, TraceLevel.Verbose, null, "({0})", paramName);
			Contract.NotIsNullOrEmpty(paramName, "paramName");

			IDbDataParameter param;
			if (!cmd.Parameters.Contains(paramName))
			{
				param = cmd.CreateParameter();
				param.ParameterName = paramName;
				cmd.Parameters.Add(param);
			}
			else
			{
				param = cmd.Parameters[paramName] as IDbDataParameter;
			}
			param.Value = value;

			return param;
		}

		/// <summary>
		/// Добавляет нетипизированый параметр, и возвращает его для указания прочих свойств
		/// </summary>
		/// <param name="cmd">Расширяемый экземпляр</param>
		/// <param name="paramName">Наименование параметра (допускается впереди указать направление как [ in, Input, inout, InputOutput, out, Output, ret, ReturnValue ] без учета регистра)</param>
		/// <returns>Привязаный к команде параметр</returns>
		public static IDbDataParameter AppendParam(this IDbCommand cmd, string paramName)
		{
			//Logger.ToLogFmt(null, TraceLevel.Verbose, null, "({0})", paramName);
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(paramName), "paramName");

			var param = cmd.CreateParameter();

			var pp = paramName.Split(StringSplitOptions.RemoveEmptyEntries, ' ', ',');
			switch (pp.Length)
			{
				case 1:	//	Default
					break;

				case 2:	//	Direction
					switch (pp[0].ToLower())
					{
						case "in":
						case "input":
							param.Direction = ParameterDirection.Input;
							break;

						case "inout":
						case "inputoutput":
							param.Direction = ParameterDirection.InputOutput;
							break;

						case "out":
						case "output":
							param.Direction = ParameterDirection.Output;
							break;

						case "ret":
						case "returnvalue":
							param.Direction = ParameterDirection.ReturnValue;
							break;

						default:
							throw new ArgumentException("Непонятный префикс для параметра", pp[0]);
					}
					paramName = pp[1];
					break;

				default:
					throw new ArgumentException("Непонятный состав параметра", paramName);
			}

			param.ParameterName = paramName;

			cmd.Parameters.Add(param);

			return param;
		}

		/// <summary>
		/// Выполняет команду в контексте указанной транзакции
		/// При этом открывается соединение, и закрывается (если до этого было закрыто)
		/// </summary>
		/// <param name="cmd">Расширяемый экземпляр</param>
		/// <param name="externalTran">Транзакция, в контесте которой выполняется команда</param>
		/// <param name="isNeedTransaction">Следует ли создавать отдельную транзакцию (актуально при transaction == null)</param>
		/// <returns></returns>
		public static int ExecuteNonQueryInTran(this IDbCommand cmd, IDbTransaction externalTran = null, bool isNeedTransaction = false)
		{
			//Logger.ToLogFmt(null, TraceLevel.Verbose, null, "()");

			//try
			//{
			int ret = -1;

			cmd.Connection.DoWorkInTran(tran =>
			{
				cmd.PrepareForExec();
				cmd.Transaction = tran;
				ret = cmd.ExecuteNonQuery();

			}, externalTran, isNeedTransaction);

			return ret;
			//}
			//catch //(Exception ex)
			//{
			//	//Logger.Error(ex, "DbCommand.ExecuteNonQueryInTran({0})", cmd.CommandText);
			//	throw;
			//}
		}

		/// <summary>
		/// Выполняет команду в контексте указанной транзакции
		/// При этом открывается соединение, и закрывается (если до этого было закрыто)
		/// </summary>
		/// <param name="cmd">Расширяемый экземпляр</param>
		/// <param name="ts">Время выполнения команды</param>
		/// <param name="externalTran">Транзакция, в контесте которой выполняется команда</param>
		/// <param name="isNeedTransaction">Следует ли создавать отдельную транзакцию (актуально при transaction == null)</param>
		/// <returns></returns>
		public static int ExecuteNonQueryInTran(this IDbCommand cmd, out TimeSpan ts, IDbTransaction externalTran = null, bool isNeedTransaction = false)
		{
			var t = DateTime.Now;
			var r = ExecuteNonQueryInTran(cmd, externalTran, isNeedTransaction);
			ts = DateTime.Now - t;
			return r;
		}

		/// <summary>
		/// Выполняет команду в контексте указанной транзакции
		/// При этом открывается соединение, и закрывается (если до этого было закрыто)
		/// Возврвщает были ли записи в наборе
		/// </summary>
		/// <param name="cmd">Расширяемый экземпляр</param>
		/// <param name="readRecord">Метод, использующий каждую запись DbDataReader. Выполняется, пока не вернет false или DbDataReader достигнет конца</param>
		/// <param name="externalTran">Транзакция, в контесте которой выполняется команда</param>
		/// <param name="isNeedTransaction">Следует ли создавать отдельную транзакцию (актуально при transaction == null)</param>
		/// <returns>Были ли записи в наборе</returns>
		public static bool ExecuteReaderInTran(this IDbCommand cmd, Predicate<IDataReader> readRecord, IDbTransaction externalTran = null, bool isNeedTransaction = false)
		{
			//Logger.ToLogFmt(null, TraceLevel.Verbose, null, "({0})", cmd.CommandText);
			Contract.Requires<ArgumentException>(readRecord != null, "readRecord");

			var result = false;

			cmd.Connection.DoWorkInTran(tran =>
			{
				cmd.PrepareForExec();
				cmd.Transaction = tran;

				using (var dr = cmd.ExecuteReader())
				{
					result = dr.Read();
					while (result && readRecord(dr) && dr.Read())
						;
					//result = dr.NextResult();
					//if(result)
					//while (result = dr.Read() && readRecord(dr))
					//	;
				}
			}, externalTran, isNeedTransaction);

			return result;
		}

		/// <summary>
		/// Выполняет команду в контексте указанной транзакции
		/// При этом открывается соединение, и закрывается (если до этого было закрыто)
		/// </summary>
		/// <param name="cmd">Расширяемый экземпляр</param>
		/// <param name="externalTran">Транзакция, в контесте которой выполняется команда</param>
		/// <param name="isNeedTransaction">Следует ли создавать отдельную транзакцию (актуально при transaction == null)</param>
		/// <returns></returns>
		public static object ExecuteScalarInTran(this IDbCommand cmd, IDbTransaction externalTran = null, bool isNeedTransaction = false)
		{
			//Logger.ToLogFmt(null, TraceLevel.Verbose, null, "({0})", cmd.CommandText.Substring(0, 20));

			object ret = null;
			cmd.Connection.DoWorkInTran(tran =>
			{
				cmd.PrepareForExec();
				cmd.Transaction = tran;
				ret = cmd.ExecuteScalar();
			}, externalTran, isNeedTransaction);
			return ret;
		}

		///// <summary>
		///// Выполняя ExecuteReaderInTran() вычитывает все записи в результат.
		///// Возвращает заполненный список записей
		///// </summary>
		///// <typeparam name="T">IDtoBase_Reader, new()</typeparam>
		///// <param name="cmd">Расширяемый экземпляр</param>
		///// <param name="externalTran">Транзакция, в контесте которой выполняется команда</param>
		///// <param name="isNeedTransaction">Следует ли создавать отдельную транзакцию (актуально при transaction == null)</param>
		///// <returns>Заполненный список записей</returns>
		//public static List<T> ExecuteListInTran<T>(this DbCommand cmd, DbTransaction externalTran = null, bool isNeedTransaction = false) where T : IDto_Reader, new()
		//{
		//	Logger.ToLogFmt(null, LogLevel.Trace, null, "()");

		//	var ret = new List<T>();

		//	cmd.ExecuteReaderInTran(dr =>
		//	{
		//		var item = new T();
		//		item.Init(dr);
		//		ret.Add(item);
		//		return true;
		//	}, externalTran, isNeedTransaction);

		//	return ret;
		//}
		///// <summary>
		///// Выполняя ExecuteReaderInTran() вычитывает все записи в результат
		///// </summary>
		///// <typeparam name="T">IDtoBase_Reader, new()</typeparam>
		///// <param name="cmd">Расширяемый экземпляр</param>
		///// <param name="ts">Время выполнения команды</param>
		///// <param name="externalTran">Транзакция, в контесте которой выполняется команда</param>
		///// <param name="isNeedTransaction">Следует ли создавать отдельную транзакцию (актуально при transaction == null)</param>
		///// <returns>Заполненный список записей</returns>
		//public static List<T> ExecuteListInTran<T>(this DbCommand cmd, out TimeSpan ts, DbTransaction externalTran = null, bool isNeedTransaction = false) where T : IDto_Reader, new()
		//{
		//	var t = DateTime.Now;
		//	var r = ExecuteListInTran<T>(cmd, externalTran, isNeedTransaction);
		//	ts = DateTime.Now - t;
		//	return r;
		//}

		/// <summary>
		/// Выполняя ExecuteReaderInTran() вычитывает все записи в результат.
		/// Возвращает заполненный список записей
		/// </summary>
		/// <typeparam name="T">IDtoBase_Reader, new()</typeparam>
		/// <param name="cmd">Расширяемый экземпляр</param>
		/// <param name="externalTran">Транзакция, в контесте которой выполняется команда</param>
		/// <param name="isNeedTransaction">Следует ли создавать отдельную транзакцию (актуально при transaction == null)</param>
		/// <returns>Заполненный список записей</returns>
		public static List<T> ExecuteListReflection<T>(this IDbCommand cmd, IDbTransaction externalTran = null, bool isNeedTransaction = false) //where T : class//new()
		{
			//Logger.ToLogFmt(null, TraceLevel.Verbose, null, "()");

			var ps = typeof(T).GetProperties()
				.Where(i => i.CanWrite)
				.ToDictionary(i => i, i => -1);

			var ret = new List<T>();

			cmd.ExecuteReaderInTran(dr =>
			{
				//var item = new T();
				var item = Activator.CreateInstance<T>();
				FillItem(item, dr, ps);
				ret.Add(item);
				return true;
			}, externalTran, isNeedTransaction);

			return ret;
		}

		static void PrepareForExec(this IDbCommand cmd)
		{
			foreach (IDataParameter p in cmd.Parameters)
				if (p != null && p.Value == null)
					p.Value = DBNull.Value;
		}

		static void FillItem(object item, IDataReader dr, Dictionary<PropertyInfo, int> ps)
		{
			var kk = new PropertyInfo[ps.Count];
			ps.Keys.CopyTo(kk, 0);
			foreach (var key in kk)
			{
				try
				{
					if (ps[key] == -1 && key.CanWrite)
						ps[key] = dr.GetOrdinal(key.Name);

					int i = ps[key];
					if (i > -1 && !dr.IsDBNull(i))
					{
						var t = key.PropertyType.FromNullable();
						var o = dr[i];
						object v = o;
						try
						{
							v = Convert.ChangeType(o, t);
						}
						catch { }
						key.SetValue(item, v, null);
					}
				}
				catch (Exception ex)
				{
					Logger.ToLogFmt("Db_Extentions", TraceLevel.Warning, ex, "() {0} [{1}]", key.Name, key.PropertyType);
				}
			}
		}

		#endregion

		#region DbDataReader

		///// <summary>
		///// Вычитывает данные из DbDataReader 
		///// </summary>
		///// <typeparam name="T">Тип материализации данных</typeparam>
		///// <param name="dr">Расширяемый экземпляр</param>
		///// <returns></returns>
		//public static List<T> GetList<T>(this DbDataReader dr) where T : IDto_Reader, new()
		//{
		//	Logger.ToLogFmt(null, LogLevel.Trace, null, "()");

		//	var ret = new List<T>();

		//	while (dr.Read())
		//	{
		//		var item = new T();
		//		item.Init(dr);
		//		ret.Add(item);
		//	};

		//	return ret;
		//}

		public static T Get<T>(this IDataReader dr, int ordinal, T def = default(T))
		{
			try
			{
				if (dr.IsDBNull(ordinal))
					return def;
				return To(dr[ordinal], def);
			}
			catch (Exception ex)
			{
				Logger.ToLogFmt(dr, TraceLevel.Error, ex, "({0}, {1})", ordinal, def);
			}
			return def;
		}

		public static T Get<T>(this IDataReader dr, string paramName, T def = default(T))
		{
			try
			{
				var ordinal = dr.GetOrdinal(paramName);
				if (dr.IsDBNull(ordinal))
					return def;
				return To(dr[paramName], def);
			}
			catch (Exception ex)
			{
				Logger.ToLogFmt(dr, TraceLevel.Error, ex, "({0}, {1})", paramName, def);
			}
			return def;
		}

		public static T To<T>(this object value, T def = default(T))
		{
			try
			{
				var s = (value == null || value == DBNull.Value) ? null : value.ToString();
				if (def is string)
					return (T)(object)s;
				return String_Extentions.To<T>(s, def);
			}
			catch (Exception ex)
			{
				Logger.ToLogFmt(value, TraceLevel.Error, ex, "({0})", def);
			}
			return def;
		}

		#endregion

	}
}
