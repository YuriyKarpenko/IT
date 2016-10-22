using System;
using System.Diagnostics;
using System.Threading;

namespace IT
{
	/// <summary>
	/// Интерфейс для расширения возможностей класса за счет IInvoke_Extention
	/// (AppInvoke(), AppBeginInvoke(), GoAsynk(), ActionDelay()
	/// </summary>
	public interface IInvoke : ILog { }

	/// <summary>
	/// Extentions class to IInvoke
	/// </summary>
	public static class IInvoke_Extention
	{
		/// <summary>
		/// Presumably context for UI
		/// </summary>
		public static SynchronizationContext Context { get; set; }

		static IInvoke_Extention()
		{
			Context = SynchronizationContext.Current;
		}

		#region методы, расчитанные на то, что первое исползование класса будет в UI-потоке Context будет назначен отдельно самомтоятельно

		///// <summary>
		///// Выполнение act в потоке sender.StaDispatcher асинхронно в конструкции try..catch
		///// </summary>
		///// <param name="sender">Расширяемый интерфейс</param>
		///// <param name="act">Действие</param>
		///// <param name="onError">Метод обработки исключений</param>
		///// <param name="priotity">Приоритет задания :
		/////<para/>Inactive			-	Элементы работы поставлены в очередь, но не выполняются.  
		/////<para/>SystemIdle		-	Элементы работы направлены в поток интерфейса пользователя во время бездействия системы. Это низший приоритет элементов, выполняемых в данный момент. 
		/////<para/>ApplicationIdle	-	Элементы работы доставлены в поток интерфейса пользователя во время бездействия приложения. 
		/////<para/>ContextIdle		-	Элементы работы доставлены в поток интерфейса пользователя после того, как выполнены элементы с более высоким приоритетом. 
		/////<para/>Background		-	Элементы работы доставлены после обработки макета, прорисовки и обработки входных данных. 
		/////<para/>Input				-	Элементы работы доставлены в поток интерфейса пользователя с тем же приоритетом, что и входные данные пользователя.  
		/////<para/>Loaded			-	Элементы работы доставлены после завершения обработки макета и прорисовки.  
		/////<para/>Render			-	Элементы работы доставлены в поток интерфейса пользователя с тем же приоритетом, что и механизм визуализации. 
		/////<para/>DataBind			-	Элементы работы доставлены в поток интерфейса пользователя с тем же приоритетом, что и привязка данных. 
		/////<para/>Normal			-	Элементы работы доставлены в поток интерфейса пользователя с обычным приоритетом. Это приоритет, с которым должно доставляться большинство рабочих элементов приложения.  
		/////<para/>Send				-	Элементы работы доставлены в поток интерфейса пользователя с высшим  
		///// </param>
		//public static DispatcherOperation AppBeginInvoke(this IInvoke sender, Action act, Action<Exception> onError = null, DispatcherPriority priotity = DispatcherPriority.Background)
		//{
		//	try
		//	{
		//		//if (IVM_Invoke_Extention.CheckAccess(null))act();
		//		return UIDispatcher.BeginInvoke((Action)(() =>
		//			{
		//				try
		//				{
		//					act();
		//				}
		//				catch (Exception ex)
		//				{
		//					if (onError != null)
		//						onError(ex);
		//					else
		//					{
		//						sender.Error(ex, "() inner");
		//					}
		//				}
		//			}), priotity);
		//	}
		//	catch (Exception ex)
		//	{
		//		if (onError != null)
		//			onError(ex);
		//		else
		//		{
		//			sender.Error(ex, "()");
		//		}
		//	}
		//	return null;
		//}

		///// <summary>
		///// Выполнение act в потоке sender.StaDispatcher
		///// </summary>
		///// <param name="sender">Расширяемый интерфейс</param>
		///// <param name="act">Действие</param>
		///// <param name="onError">Метод обработки исключений</param>
		//public static R AppInvoke<R>(this IInvoke sender, Func<R> act, Func<Exception, R> onError = null)
		//{
		//	return sender.AppInvoke<R>(act, TimeSpan.FromMilliseconds(-1.0), onError);
		//}

		///// <summary>
		///// Выполнение act в потоке sender.StaDispatcher
		///// </summary>
		///// <param name="sender">Расширяемый интерфейс</param>
		///// <param name="act">Действие</param>
		///// <param name="timeout">Время ожидания действия</param>
		///// <param name="onError">Метод обработки исключений</param>
		//public static R AppInvoke<R>(this IInvoke sender, Func<R> act, TimeSpan timeout, Func<Exception, R> onError = null)
		//{
		//	try
		//	{
		//		if (IVM_Invoke_Extention.CheckAccess(null))
		//			return act();
		//		else
		//			return (R)UIDispatcher.Invoke(act, timeout);
		//	}
		//	catch (Exception ex)
		//	{
		//		if (onError != null)
		//		{
		//			return onError(ex);
		//		}
		//		else
		//		{
		//			sender.Error(ex, "()");
		//			throw;
		//		}
		//	}
		//}

		/// <summary>
		/// Выполнение act в главном потоке 
		/// </summary>
		/// <param name="sender">Расширяемый интерфейс</param>
		/// <param name="act">Действие</param>
		/// <param name="onError">Метод обработки исключений</param>
		public static void AppInvoke(this IInvoke sender, Action act, Action<Exception> onError = null)
		{
			try
			{
				Context.Exec(act);
			}
			catch (Exception ex)
			{
				if (onError != null)
				{
					onError(ex);
				}
				else
				{
					sender.Error(ex, "()");
					throw;
				}
			}
		}

		#endregion

		#region надежные методы

		/// <summary>
		/// Выполнение задачи в ThreadPool с подсчетом времени, запускат onFynally/onError в контексте вызывавшего потока
		/// </summary>
		/// <param name="sender">Расширяемый интерфейс</param>
		/// <param name="doWork">Метод, выполняющий полезную работу</param>
		/// <param name="onError">Метод, вызываемый при ошибке</param>
		/// <param name="onFynally"></param>
		public static void GoAsync(this IInvoke sender, Action doWork, Action<TimeSpan> onFynally = null, Action<Exception> onError = null)
		{
			Contract.NotNull(doWork, "act");
			Contract.NotNull(onFynally, "onFynally");

			var res = SynchronizationContext.Current;
			ThreadPool.QueueUserWorkItem(o =>
			{
				var context = o as SynchronizationContext;
				var sw = Stopwatch.StartNew();
				try
				{
					doWork();
				}
				catch (Exception ex)
				{
					if (onError == null)
						throw;

					//onError(ex);
					context.Exec(onError, ex);
				}
				finally
				{
					sw.Stop();
					if (onFynally != null)
						//onFynally(sw.Elapsed);
						context.Exec(onFynally, sw.Elapsed);
				}
			}, res);

			//return res;
		}

		/// <summary>
		/// Выполнение задачи в ThreadPool с подсчетом времени, запускат onFynally/onError в контексте вызывавшего потока
		/// Выполнение задачи в Task с подсчетом времени
		/// </summary>
		/// <param name="sender">Расширяемый интерфейс</param>
		/// <param name="doWork"></param>
		/// <param name="onComplete"></param>
		/// <param name="onError"></param>
		/// <param name="onFynally"></param>
		public static void GoAsync<R>(this IInvoke sender, Func<R> doWork, Action<R> onComplete, Action<TimeSpan> onFynally = null, Action<Exception> onError = null)
		{
			Contract.NotNull(null != doWork, "act");
			Contract.NotNull(null != onComplete, "onComplete");
			//Contract.NotNull(null != onFynally, "onFynally");

			ThreadPool.QueueUserWorkItem(o =>
			{
				var context = o as SynchronizationContext;
				var sw = Stopwatch.StartNew();
				try
				{
					var r = doWork();
					//onComplete(r);
					context.Exec(onComplete, r);
					return;
				}
				catch (Exception ex)
				{
					if (onError == null)
						throw;

					//onError(ex);
					context.Exec(onError, ex);
				}
				finally
				{
					sw.Stop();
					if (onFynally != null)
						//onFynally(sw.Elapsed);
						context.Exec(onFynally, sw.Elapsed);
				}

			}, SynchronizationContext.Current);
		}

		/// <summary>
		/// Выполняет указанное действие по истечению указанного времени
		/// </summary>
		/// <param name="sender">Расширяемый интерфейс</param>
		/// <param name="action"></param>
		/// <param name="delaySecond"></param>
		public static Timer ActionDelay(this IInvoke sender, Action action, int delaySecond)
		{
			return new Timer(o => (o as SynchronizationContext).Exec(action), SynchronizationContext.Current, delaySecond * 1000, -1);
		}

		/// <summary>
		/// Выполняет метод act в указанном контексте, если указан
		/// </summary>
		/// <param name="context"></param>
		/// <param name="act"></param>
		/// <param name="isAsysnc"></param>
		public static void Exec(this SynchronizationContext context, Action act, bool isAsysnc = true)
		{
			if (context == null)
				act();
			else
			{
				if (isAsysnc)
					context.Post(o => act(), null);
				else
					context.Send(o => act(), null);
			}
		}

		/// <summary>
		/// Выполняет метод act в указанном контексте, если указан
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="context"></param>
		/// <param name="act"></param>
		/// <param name="value"></param>
		/// <param name="isAsysnc"></param>
		public static void Exec<T>(this SynchronizationContext context, Action<T> act, T value, bool isAsysnc = true)
		{
			if (context == null)
				act(value);
			else
			{
				if (isAsysnc)
					context.Post(o => act((T)o), value);
				else
					context.Send(o => act((T)o), value);
			}
		}

		#endregion
	}
}
