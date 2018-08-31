using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace IT.WPF
{
	interface IDataContext
	{
		void OnDataContext_Set(FrameworkElement e);
	}

	/// <summary>
	/// базовый класс для ViewModel, упрощает привязку команд к целевому View
	/// </summary>
	public class VM_BaseInit : NotifyPropertyChangedBase, IDataContext, ILog//, IInvokable
	{
		#region static

		static VM_BaseInit()
		{
			DependencyPropertyDescriptor propertyDescriptor = DependencyPropertyDescriptor.FromProperty(Control.DataContextProperty, typeof(ContentControl));
			if (propertyDescriptor != null)
				propertyDescriptor.DesignerCoerceValueCallback = new CoerceValueCallback(DataContext_CoerceValue);

			var pm = new FrameworkPropertyMetadata(CallbackDataContext);
			FrameworkElement.DataContextProperty.AddOwner(typeof(ContentControl), pm);
		}
		static void CallbackDataContext(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue != null && e.NewValue is IDataContext vm)
			{
				vm.OnDataContext_Set(d as FrameworkElement);
			}
		}
		static object DataContext_CoerceValue(DependencyObject d, object value)
		{
			if (value != null && value is IDataContext vm)
			{
				vm.OnDataContext_Set(d as FrameworkElement);
			}
			return value;
		}

		#endregion

		/// <summary> Позволяет скрывать, используемые для отладки, визуальные компоненты </summary>
		public Visibility IsDedug { get; private set; }

		/// <summary> Окно, в котором находится привязанный View </summary>
		protected Window CurrentWindow = null;
		/// <summary> привязанный View </summary>
		protected UserControl CurrentUC = null;

		/// <summary> .ctor </summary>
		public VM_BaseInit()
		{
			this.Init();
		}

		#region Init

		/// <summary> Запускается в момент назначения данного ViewModel в качестве View.DataContext </summary>
		/// <param name="w">привязанный View</param>
		protected virtual void Init_Command_Core(Window w) { }
		/// <summary> Запускается в момент назначения данного ViewModel в качестве View.DataContext </summary>
		/// <param name="uc">привязанный View</param>
		protected virtual void Init_Command_Core(UserControl uc) { }
		/// <summary> Запускается в момент назначения данного ViewModel в качестве View.DataContext </summary>
		/// <param name="fe">привязанный View</param>
		protected virtual void Init_Command_Core(FrameworkElement fe) { }

		/// <summary>
		/// Для создания контролов. Так же запускает Init_Core_Async
		/// </summary>
		protected virtual Task Init_Core()
		{
#if DEBUG
			this.IsDedug = Visibility.Visible;
#else
			this.IsDedug = Visibility.Collapsed;
#endif
			//this.GoAsync(this.Init_Core_Async, ex => this.Error(ex, "() Async"));
			return new Task(Init_Core_Async);
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
			MessageBox.Show(string.Format(formatStr, args), Ap.AppCaption, MessageBoxButton.OK, img);
		}
		/// <summary>
		/// Облегченное использование MessageBox.Show()
		/// </summary>
		/// <param name="btn">Кнопки</param>
		/// <param name="formatStr">Строка форматирования</param>
		/// <param name="args">Параметры строки форматирования</param>
		protected virtual MessageBoxResult MessageBoxShow_Question(MessageBoxButton btn, string formatStr, params object[] args)
		{
			return MessageBox.Show(string.Format(formatStr, args), Ap.AppCaption, btn, MessageBoxImage.Question);
		}
	}
}
