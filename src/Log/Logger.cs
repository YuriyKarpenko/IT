using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace IT.Log
{
	/// <summary>
	/// Вспомогательный класс для логирования
	///	получение методов - дорогое удовольствие : https://rsdn.ru/forum/dotnet/3309060.flat	
	/// </summary>
	public class Logger
	{
		#region Events

		/// <summary>
		/// Сообщение о возникновении записи в логе (не требует ссылки на NLog.dll) ASYNC !!!
		/// </summary>
		public static event EventHandler<EventArgs<TraceLevel, string, Exception>> MessageSmall;

		#endregion

		static bool _Include_Method = true;
		static bool _Include_FileInfo = false;

		#region Properties

		/// <summary>
		/// Минимальный уровень срабатывания Logger
		/// </summary>
		public static TraceLevel MinLevel { get; set; }

		/// <summary>
		/// Следует ли в лог влючать Ip данного компьютера в каждое сообщение лога
		/// </summary>
		[DefaultValue(true)]
		public static bool Include_Ip { get; set; }

		/// <summary>
		/// Включать id потока
		/// </summary>
		[DefaultValue(false)]
		public static bool Include_ThreadId { get; set; }

		/// <summary>
		/// Включать № строки в исходном файле
		/// </summary>
		[DefaultValue(false)]
		public static bool Include_Line
		{
			get { return _Include_FileInfo; }
			set
			{
				_Include_FileInfo = value;
				_Include_Method |= _Include_FileInfo;
			}
		}

		/// <summary>
		/// Включать название метода
		/// </summary>
		[DefaultValue(true)]
		public static bool Include_Method
		{
			get { return _Include_Method; }
			set
			{
				_Include_Method = value;
				_Include_FileInfo &= _Include_Method;
			}
		}

		#endregion

		static Logger()
		{
			Logger.Include_Ip = true;
		}

		/// <summary>
		/// Получает StackFrame и дополняет сообщение на онове информации StackFrame 
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		public static string GetMsg_Ext(string msg)
		{
			//	0	-	этот метод
			//	1	-	LogExtentions или ToLogFmt
			//	2	-	LogExtentions.
			//	3	-	То, что нада
			var fr = new StackFrame(3, Include_Line);
			return GetMsg_Method(fr, msg);
		}

		/// <summary>
		/// Асинхронная отправка сообщений подписчикам + непосредственно запись в NLogger
		/// </summary>
		/// <param name="source">Класс-источник логирования</param>
		/// <param name="level"></param>
		/// <param name="ex"></param>
		/// <param name="formatStr"></param>
		/// <param name="args"></param>
		public static string ToLogFmt(object source, TraceLevel level, Exception ex, string formatStr, params object[] args)
		{
			//	0	-	этот метод
			//	1	-	То, что нада
			var fr = new StackFrame(1, Include_Line);
			var msg = GetMsg_Method(fr, string.Format(formatStr, args));
			return Logger.ToLog(source, level, msg, ex);
		}


		/// <summary>
		/// Асинхронная отправка сообщений подписчикам + запись в System.Diagnostics.Debug
		/// </summary>
		/// <param name="source">Класс-источник логирования</param>
		/// <param name="level"></param>
		/// <param name="msg"></param>
		/// <param name="ex"></param>
		public static string ToLog(object source, TraceLevel level, string msg, Exception ex = null)
		{
			try
			{
				msg = $"{DateTime.Now.ToLongTimeString()} : {level} : {msg} [{source}]";

				if (Logger.MinLevel < level)
					return msg;

				//	отправка сообщения подписчикам
				if (Logger.MessageSmall != null)
				{
					var e = new EventArgs<TraceLevel, string, Exception>(level, msg, ex);
					//Task.Factory.StartNew(() => Logger.MessageSmall(source, e));
					ThreadPool.QueueUserWorkItem(o => Logger.MessageSmall(source, e));
				}

				if (ex != null)
					msg = string.Format("{0}\r\n{1}", msg, ex);

				Debug.WriteLine(msg, Ap.ProductName);

//#if !SILVERLIGHT
//				switch (level)
//				{
//					case TraceLevel.Error:
//						Trace.TraceError(msg);
//						break;
//					case TraceLevel.Warning:
//						Trace.TraceWarning(msg);
//						break;
//					case TraceLevel.Info:
//						Trace.TraceInformation(msg);
//						break;
//					case TraceLevel.Verbose:
//						Trace.WriteLine(msg, Ap.ProductName);
//						//TraceSource.
//						//TraceEventType;
//						//TraceLevel + SourceLevels
//						//TraceOptions
//						//SourceLevels
//						break;
//				}
//#endif
			}
			catch (Exception exc)
			{
				Trace.TraceError("Logger.OnMessage({0})\n{1}", msg, exc);
			}

			return msg;
		}

		/// <summary>
		/// Дополняет сообщение на онове информации StackFrame 
		/// </summary>
		/// <param name="sf"></param>
		/// <param name="msg"></param>
		/// <returns></returns>
		private static string GetMsg_Method(StackFrame sf, string msg)
		{
			if (Logger.Include_Method && sf != null)
			{
				var m = sf.GetMethod();
				if (m != null)
				{
					if (Include_Line)
					{
						var l = sf.GetFileLineNumber();
						msg = string.Format("L:{3,4} {0}.{1}{2}", m.DeclaringType, m.Name, msg, l);
					}
					else
						msg = string.Format("{0}.{1}{2}", m.DeclaringType, m.Name, msg);
				}
			}
			return GetMsgInternal(msg);
		}

		/// <summary>
		/// окончательное формирование сообщения
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		private static string GetMsgInternal(string msg)
		{
#if SILVERLIGHT
			return msg;
#else
			if (Logger.Include_ThreadId)
				msg = string.Format("id:{0} : {1}", Thread.CurrentThread.ManagedThreadId, msg);
			if (Logger.Include_Ip)
				msg = string.Format("IP {0} : {1}", Ap.IPAddress, msg);
			return  msg;
#endif
		}

	}


}
