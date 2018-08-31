using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using IT.Log;

namespace IT
{
	/// <summary>
	/// Интерфейс для возможности расширения других классов методами логирования
	/// </summary>
	public interface ILog { }

	/// <summary>
	/// Класс расширения методов логирования
	/// </summary>
	public static class LogExtentions
	{
		/// <summary>
		/// Формирует сообщение
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="level"></param>
		/// <param name="ex"></param>
		/// <param name="msg"></param>
		/// <param name="args">Аргументы метода</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static string LogFmt(this ILog obj, TraceLevel level, Exception ex, string msg, params object[] args)
		{
			msg = Logger.GetMsg_Ext(msg, args);
			return Logger.ToLog(obj, level, msg, ex);
		}

		/// <summary>
		/// Логирование соответствующего уровня
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="ex"></param>
		/// <param name="msg"></param>
		/// <param name="args">Аргументы метода</param>
		[MethodImpl(MethodImplOptions.NoInlining)]
		[Obsolete]
		public static string Error(this ILog obj, Exception ex, string msg = null, params object[] args)
		{
			var s = LogFmt(obj, TraceLevel.Error, ex, msg, args);
			return s;
		}
		/// <summary>
		/// Логирование соответствующего уровня
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="ex"></param>
		/// <param name="args">Аргументы метода</param>
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static string Error(this ILog obj, Exception ex, params object[] args)
		{
			var s = LogFmt(obj, TraceLevel.Error, ex, null, args);
			return s;
		}

		/// <summary>
		/// Логирование соответствующего уровня
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="ex"></param>
		/// <param name="msg"></param>
		/// <param name="args">Аргументы метода</param>
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static string Warn(this ILog obj, Exception ex, string msg = null, params object[] args)
		{
			var s = LogFmt(obj, TraceLevel.Warning, ex, msg, args);
			return s;
		}

		/// <summary>
		/// Логирование соответствующего уровня
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="msg"></param>
		/// <param name="args">Аргументы метода</param>
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static string Info(this ILog obj, string msg = null, params object[] args)
		{
			var s = LogFmt(obj, TraceLevel.Info, null, msg, args);
			return s;
		}

		/// <summary>
		/// Логирование соответствующего уровня
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="msg"></param>
		/// <param name="args">Аргументы метода</param>
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static string Debug(this ILog obj, string msg = null, params object[] args)
		{
			var s = LogFmt(obj, TraceLevel.Verbose, null, msg, args);
			return s;
		}

		/// <summary>
		/// Логирование соответствующего уровня
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="msg"></param>
		/// <param name="args">Аргументы метода</param>
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static string Trace(this ILog obj, string msg = null, params object[] args)
		{
			var s = LogFmt(obj, TraceLevel.Verbose, null, msg, args);
			return s;
		}
	}
}
