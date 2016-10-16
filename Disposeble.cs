using System;
using System.Runtime.InteropServices;

namespace IT
{
	/// <summary>
	/// Заготовка для упрщенной реализации IDisposable ( void Dispose(bool disposing) + bool IsDisposed + void VerifyDisposed() + логирование)
	/// </summary>
	[ComVisible(true)]
#if !SILVERLIGHT
	[Serializable]
#endif
	public abstract class Disposable : IDisposable
#if !SILVERLIGHT
		, ILog
#endif
	{
		/// <summary>
		/// Признак уничтожения обЪекта
		/// </summary>
		protected bool IsDisposed { get; private set; }


#if !SILVERLIGHT
		/// <summary>
		/// Конструктор
		/// </summary>
		public Disposable() 
		{
			this.Trace("()");
		}
#endif


		/// <summary>
		/// Метод интерфейса для перекрытия
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
		}

		/// <summary>
		/// Реализзация интерфейса
		/// </summary>
		public void Dispose()
		{
			this.IsDisposed = true;

#if SILVERLIGHT
				this.Dispose(true);
#else
			try
			{
				this.Dispose(true);
			}
			catch (Exception ex)
			{
				this.Error(ex, "()");
			}

			this.Debug("()");
#endif

			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Если объект уже уничтожен - запускает исключение ObjectDisposedException()
		/// </summary>
		protected void VerifyDisposed()
		{
			if (this.IsDisposed)
				throw new ObjectDisposedException(this.ToString());
		}

		//	http://msdn.microsoft.com/ru-ru/magazine/cc163491.aspx
		///// <summary>
		/////	Деструктор
		///// </summary>
		//~Disposable()
		//{
		//	this.Dispose(false);
		//}

	}
}
