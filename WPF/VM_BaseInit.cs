using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Camo.WPF
{
	interface IDataContext
	{
		void OnDataContext_Set(FrameworkElement e);
	}

	public class VM_BaseInit : NotifyPropertyChangedDisposeble, IDataContext, ILog, IInvokable
	{
		#region static

		static VM_BaseInit()
		{
			var pm = new System.Windows.FrameworkPropertyMetadata(CallbackDataContext);
			FrameworkElement.DataContextProperty.AddOwner(typeof(ContentControl), pm);
		}
		static void CallbackDataContext(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue != null)
			{
				IDataContext vm = e.NewValue as IDataContext;
				if (vm != null)
					vm.OnDataContext_Set(d as FrameworkElement);
			}
		}

		#endregion

		public Visibility IsDedug { get; private set; }

		protected Window CurrentWindow = null;
		protected UserControl CurrentUC = null;


		public VM_BaseInit()
		{
			this.Init();
		}

		#region Init

		protected virtual void Init_Command_Core(Window w) { }
		protected virtual void Init_Command_Core(UserControl uc) { }
		protected virtual void Init_Command_Core(FrameworkElement fe) { }

		/// <summary>
		/// Для создания контролов. Так же запускает Init_Core_Async
		/// </summary>
		protected virtual void Init_Core()
		{
#if DEBUG
			this.IsDedug = Visibility.Visible;
#else
			this.IsDedug = Visibility.Collapsed;
#endif
			this.GoAsync(this.Init_Core_Async, ex => this.Error(ex, "() Async"));
		}
		/// <summary>
		/// Для фоновой загрузки данных
		/// </summary>
		protected virtual void Init_Core_Async() { }


		void IDataContext.OnDataContext_Set(FrameworkElement element)
		{
			try
			{
				if (element.DataContext == this)
				{
					this.Init_Command_Core(element);

					var uc = element as UserControl;
					if (uc != null && uc != this.CurrentUC)
					{
						this.CurrentUC = uc;
						this.Init_Command_Core(uc);
					}

					var w = element as Window;
					if (w != null && w != this.CurrentWindow)
					{
						this.CurrentWindow = w;
						this.Init_Command_Core(w);
					}
				}
			}
			catch (Exception ex)
			{
				//Debug.Fail("IDataContext.OnDataContext_Set()", ex.ToString());
				this.Error(ex, "({0})", element);
			}
		}

		void Init()
		{
			try
			{
				this.Init_Core();
			}
			catch (Exception ex)
			{
				this.Error(ex, "()");
				//Debug.Fail("Init()", ex.ToString());
				throw;
			}
		}

		#endregion


		/// <summary>
		/// Облегченное использование MessageBox.Show()
		/// </summary>
		/// <param name="img">Иконка</param>
		/// <param name="formatStr">Строка форматирования</param>
		/// <param name="args">Параметры строки форматирования</param>
		protected virtual void MessageBoxShow(MessageBoxImage img, string formatStr, params object[] args)
		{
			MessageBox.Show(string.Format(formatStr, args), App_Helper.AppCaption, MessageBoxButton.OK, img);
		}
		/// <summary>
		/// Облегченное использование MessageBox.Show()
		/// </summary>
		/// <param name="btn">Кнопки</param>
		/// <param name="formatStr">Строка форматирования</param>
		/// <param name="args">Параметры строки форматирования</param>
		protected virtual MessageBoxResult MessageBoxShow_Question(MessageBoxButton btn, string formatStr, params object[] args)
		{
			return MessageBox.Show(string.Format(formatStr, args), App_Helper.AppCaption, btn, MessageBoxImage.Question);
		}
	}
}
