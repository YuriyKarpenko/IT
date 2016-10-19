using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows;
using System.Windows.Input;

//	http://marlongrech.wordpress.com/2008/12/04/attachedcommandbehavior-aka-acb/
namespace IT.WPF
{
    /// <summary>
    /// Defines the attached properties to create a CommandBehaviorBinding
    /// </summary>
	public class CommandBehavior
	{
		#region Behavior

		/// <summary>
		/// Behavior Attached Dependency Property
		/// </summary>
		private static readonly DependencyProperty BehaviorProperty = DependencyProperty.RegisterAttached("Behavior", typeof(CommandBehaviorBinding), typeof(CommandBehavior),
				new FrameworkPropertyMetadata((CommandBehaviorBinding)null));

		/// <summary>
		/// Gets the Behavior property. 
		/// </summary>
		private static CommandBehaviorBinding GetBehavior(DependencyObject d)
		{
			return (CommandBehaviorBinding)d.GetValue(BehaviorProperty);
		}

		/// <summary>
		/// Sets the Behavior property.  
		/// </summary>
		private static void SetBehavior(DependencyObject d, CommandBehaviorBinding value)
		{
			d.SetValue(BehaviorProperty, value);
		}

		#endregion

		#region Command

		/// <summary>
		/// Command Attached Dependency Property
		/// </summary>
		public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(CommandBehavior),
				new FrameworkPropertyMetadata((ICommand)null, new PropertyChangedCallback(OnCommandChanged)));

		/// <summary>
		/// Gets the Command property.  
		/// </summary>
		public static ICommand GetCommand(DependencyObject d)
		{
			return (ICommand)d.GetValue(CommandProperty);
		}

		/// <summary>
		/// Sets the Command property. 
		/// </summary>
		public static void SetCommand(DependencyObject d, ICommand value)
		{
			d.SetValue(CommandProperty, value);
		}

		/// <summary>
		/// Одна из точек входа
		/// </summary>
		private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CommandBehaviorBinding binding = FetchOrCreateBinding(d);
			binding.Command = (ICommand)e.NewValue;
		}

		#endregion

		#region CommandParameter

		/// <summary>
		/// CommandParameter Attached Dependency Property
		/// </summary>
		public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(CommandBehavior),
				new FrameworkPropertyMetadata((object)null, new PropertyChangedCallback(OnCommandParameterChanged)));

		/// <summary>
		/// Gets the CommandParameter property.  
		/// </summary>
		public static object GetCommandParameter(DependencyObject d)
		{
			return (object)d.GetValue(CommandParameterProperty);
		}

		/// <summary>
		/// Sets the CommandParameter property. 
		/// </summary>
		public static void SetCommandParameter(DependencyObject d, object value)
		{
			d.SetValue(CommandParameterProperty, value);
		}

		/// <summary>
		/// Одна из точек входа
		/// </summary>
		private static void OnCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CommandBehaviorBinding binding = FetchOrCreateBinding(d);
			binding.CommandParameter = e.NewValue;
		}

		#endregion

		#region Event

		/// <summary>
		/// Event Attached Dependency Property
		/// </summary>
		public static readonly DependencyProperty EventProperty = DependencyProperty.RegisterAttached("Event", typeof(string), typeof(CommandBehavior),
				new FrameworkPropertyMetadata((string)String.Empty, new PropertyChangedCallback(OnEventChanged)));

		/// <summary>
		/// Gets the Event property.  This dependency property 
		/// indicates ....
		/// </summary>
		public static string GetEvent(DependencyObject d)
		{
			return (string)d.GetValue(EventProperty);
		}

		/// <summary>
		/// Sets the Event property.  This dependency property 
		/// indicates ....
		/// </summary>
		public static void SetEvent(DependencyObject d, string value)
		{
			d.SetValue(EventProperty, value);
		}

		/// <summary>
		/// Главная точка входа
		/// </summary>
		private static void OnEventChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CommandBehaviorBinding binding = FetchOrCreateBinding(d);
			//	check if the Event is set. If yes we need to rebind the Command to the new event and unregister the old one
			if (binding.Event != null && binding.Owner != null)
				binding.Dispose();
			//bind the new event to the command
			binding.BindEvent(d, e.NewValue.ToString());
		}

		#endregion

		#region Helpers

		//tries to get a CommandBehaviorBinding from the element. Creates a new instance if there is not one attached
		private static CommandBehaviorBinding FetchOrCreateBinding(DependencyObject d)
		{
			CommandBehaviorBinding binding = CommandBehavior.GetBehavior(d);
			if (binding == null)
			{
				binding = new CommandBehaviorBinding();
				CommandBehavior.SetBehavior(d, binding);
			}
			return binding;
		}

		#endregion

		/// <summary>
		/// Defines the command behavior binding
		/// </summary>
		class CommandBehaviorBinding : IDisposable
		{
			#region Properties

			/// <summary>
			/// Get the owner of the CommandBinding ex: a Button
			/// This property can only be set from the BindEvent Method
			/// </summary>
			public DependencyObject Owner { get; private set; }
			/// <summary>
			/// The command to execute when the specified event is raised
			/// </summary>
			public ICommand Command { get; set; }
			/// <summary>
			/// Gets or sets a CommandParameter
			/// </summary>
			public object CommandParameter { get; set; }
			/// <summary>
			/// Имя события для подключения
			/// Это свойство может быть задано только из метода BindEvent
			/// </summary>
			public string EventName { get; private set; }
			/// <summary>
			/// The event info of the event
			/// Для создания обработчика события
			/// </summary>
			public EventInfo Event { get; private set; }
			/// <summary>
			/// Gets the EventHandler for the binding with the event
			/// Обработчик данного события
			/// </summary>
			public Delegate EventHandler { get; private set; }

			#endregion

			//Создает EventHandler во время выполнения и регистрирует обработчик указанного события
			public void BindEvent(DependencyObject owner, string eventName)
			{
				this.EventName = eventName;
				this.Owner = owner;
				this.Event = Owner.GetType().GetEvent(this.EventName, BindingFlags.Public | BindingFlags.Instance);
				if (this.Event == null)
					throw new InvalidOperationException(String.Format("Не удалось разрешить имя события {0}", EventName));

				//	Создается обработчик события для event, который вызовет ExecuteCommand()
				var mi = typeof(CommandBehaviorBinding).GetMethod("ExecuteCommand2", BindingFlags.Public | BindingFlags.Instance);
				this.EventHandler = CommandBehaviorBinding.CreateDelegate(this.Event.EventHandlerType, mi, this);

				//	Регистрация нашего обработчика для Event
				this.Event.AddEventHandler(this.Owner, this.EventHandler);
				this.disposed = false;
			}

			/// <summary>
			/// Выполняет комманду, если CommandParameter не указан, то передает EventArgs
			/// </summary>
			public void ExecuteCommand2(EventArgs e)
			{ 
				if (this.Command.CanExecute(CommandParameter))
					this.Command.Execute(CommandParameter ?? e);
			}

			/// <summary>
			/// Выполняет комманду
			/// </summary>
			public void ExecuteCommand()//object args)
			{
				if (this.Command.CanExecute(CommandParameter))
					this.Command.Execute(CommandParameter);
			}

			#region IDisposable Members

			bool disposed = false;
			/// <summary>
			/// Unregisters the EventHandler from the Event
			/// </summary>
			public void Dispose()
			{
				if (!disposed)
				{
					this.Event.RemoveEventHandler(this.Owner, this.EventHandler);
					this.disposed = true;
				}
			}

			#endregion

			/// <summary>
			/// Generates a delegate with a matching signature of the supplied eventHandlerType
			/// This method only supports Events that have a delegate of type void
			/// </summary>
			/// <param name="eventHandlerType">тип делегата, который надо построить. Note that this must always be a void delegate</param>
			/// <param name="methodToInvoke">The method to invoke</param>
			/// <param name="methodInvoker">The object where the method resides</param>
			/// <returns>Returns a delegate with the same signature as eventHandlerType that calls the methodToInvoke inside</returns>
			public static Delegate CreateDelegate(Type eventHandlerType, MethodInfo methodToInvoke, object methodInvoker)
			{
				//	получение MethodInfo из делегата события
				var eventHandlerInfo = eventHandlerType.GetMethod("Invoke");
				//	проверка возвращаемого значения делегатом
				Type returnType = eventHandlerInfo.ReturnParameter.ParameterType;
				if (returnType != typeof(void))
					throw new ApplicationException("Delegate has a return type. This only supprts event handlers that are void");

				//	Получение параметров делегата события
				ParameterInfo[] delegateParameters = eventHandlerInfo.GetParameters();
				//	Получение типов параметров. В позиции 0 находится тип класса-владельца метода (methodToInvoke)
				Type[] hookupParameters = new Type[delegateParameters.Length + 1];
				hookupParameters[0] = methodInvoker.GetType();
				for (int i = 0; i < delegateParameters.Length; i++)
					hookupParameters[i + 1] = delegateParameters[i].ParameterType;

				//////////////////////////////////////
				//	==	--	Создание метода	--	==	//
				DynamicMethod handler = new DynamicMethod("", null, hookupParameters, typeof(CommandBehaviorBinding));
				//	генератор для данного метода
				ILGenerator eventIL = handler.GetILGenerator();

				////load the parameters or everything will just BAM :)
				////	Переменная object[delegateParameters.Length + 1]
				//LocalBuilder local = eventIL.DeclareLocal(typeof(object[]));
				//eventIL.Emit(OpCodes.Ldc_I4, delegateParameters.Length + 1);
				//eventIL.Emit(OpCodes.Newarr, typeof(object));
				//eventIL.Emit(OpCodes.Stloc, local);

				////start from 1 because the first item is the instance. Load up all the arguments
				////	чтение аргументов в local
				//for (int i = 1; i < delegateParameters.Length + 1; i++)
				//{
				//	eventIL.Emit(OpCodes.Ldloc, local);
				//	eventIL.Emit(OpCodes.Ldc_I4, i);
				//	eventIL.Emit(OpCodes.Ldarg, i);
				//	eventIL.Emit(OpCodes.Stelem_Ref);
				//}

				//eventIL.Emit(OpCodes.Ldloc, local);

				//Загрузка первого аргумента экземпляр объекта для methodToInvoke т.е. methodInvoker
				eventIL.Emit(OpCodes.Ldarg_0);
				eventIL.Emit(OpCodes.Ldarg_2);	//	для вызова ExecuteCommand2

				//Now that we have it all set up call the actual method that we want to call for the binding
				eventIL.EmitCall(OpCodes.Call, methodToInvoke, null);

				//eventIL.Emit(OpCodes.Pop);
				eventIL.Emit(OpCodes.Ret);

				//создать делегат из динамического метода
				return handler.CreateDelegate(eventHandlerType, methodInvoker);
			}
		}

	}


	////	ЛАЖА (( почему-то, надо будет разобраться ...
	//public class CommandBehavior2 : DependencyObject
	//{
	//	#region Behavior

	//	/// <summary>
	//	/// Behavior Attached Dependency Property
	//	/// </summary>
	//	private static readonly DependencyProperty BehaviorProperty = DependencyProperty.RegisterAttached("Behavior", typeof(CommandBehaviorBinding), typeof(CommandBehavior),
	//			new FrameworkPropertyMetadata((CommandBehaviorBinding)null));

	//	private CommandBehaviorBinding Behavior
	//	{
	//		get { return (CommandBehaviorBinding)this.GetValue(BehaviorProperty); }
	//		set { this.SetValue(BehaviorProperty, value); }
	//	}

	//	#endregion

	//	#region Command

	//	/// <summary>
	//	/// Command Attached Dependency Property
	//	/// </summary>
	//	public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(CommandBehavior2),
	//			new FrameworkPropertyMetadata((ICommand)null, new PropertyChangedCallback(OnCommandChanged)));

	//	/// <summary>
	//	/// Gets the Command property.  
	//	/// </summary>
	//	public ICommand Command
	//	{
	//		get { return (ICommand)this.GetValue(CommandProperty); }
	//		set { this.SetValue(CommandProperty, value); }
	//	}

	//	/// <summary>
	//	/// Handles changes to the Command property.
	//	/// </summary>
	//	private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	//	{
	//		CommandBehaviorBinding binding = FetchOrCreateBinding(d as CommandBehavior2);
	//		binding.Command = (ICommand)e.NewValue;
	//	}

	//	#endregion

	//	#region CommandParameter

	//	/// <summary>
	//	/// CommandParameter Attached Dependency Property
	//	/// </summary>
	//	public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(CommandBehavior2),
	//			new FrameworkPropertyMetadata((object)null, new PropertyChangedCallback(OnCommandParameterChanged)));

	//	/// <summary>
	//	/// Gets the CommandParameter property.  
	//	/// </summary>
	//	public object CommandParameter
	//	{
	//		get { return this.GetValue(CommandParameterProperty); }
	//		set { this.SetValue(CommandParameterProperty, value); }
	//	}

	//	/// <summary>
	//	/// Handles changes to the CommandParameter property.
	//	/// </summary>
	//	private static void OnCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	//	{
	//		CommandBehaviorBinding binding = FetchOrCreateBinding(d as CommandBehavior2);
	//		binding.CommandParameter = e.NewValue;
	//	}

	//	#endregion

	//	#region Event

	//	/// <summary>
	//	/// Event Attached Dependency Property
	//	/// </summary>
	//	public static readonly DependencyProperty EventProperty = DependencyProperty.RegisterAttached("Event", typeof(string), typeof(CommandBehavior2),
	//			new FrameworkPropertyMetadata((string)String.Empty, new PropertyChangedCallback(OnEventChanged)));

	//	/// <summary>
	//	/// Gets the Event property.  This dependency property 
	//	/// indicates ....
	//	/// </summary>
	//	public string Event
	//	{
	//		get { return (string)this.GetValue(EventProperty); }
	//		set { this.SetValue(EventProperty, value); }
	//	}

	//	/// <summary>
	//	/// Handles changes to the Event property.
	//	/// </summary>
	//	private static void OnEventChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	//	{
	//		CommandBehaviorBinding binding = FetchOrCreateBinding(d as CommandBehavior2);
	//		//check if the Event is set. If yes we need to rebind the Command to the new event and unregister the old one
	//		if (binding.Event != null && binding.Owner != null)
	//			binding.Dispose();
	//		//bind the new event to the command
	//		binding.BindEvent(d, e.NewValue.ToString());
	//	}

	//	#endregion

	//	#region Helpers

	//	//tries to get a CommandBehaviorBinding from the element. Creates a new instance if there is not one attached
	//	private static CommandBehaviorBinding FetchOrCreateBinding(CommandBehavior2 d)
	//	{
	//		if (d != null)
	//		{
	//			CommandBehaviorBinding binding = d.Behavior;
	//			if (binding == null)
	//			{
	//				binding = new CommandBehaviorBinding();
	//				d.Behavior = binding;
	//			}
	//			return binding;
	//		}
	//		return null;
	//	}

	//	#endregion

	//}
}
