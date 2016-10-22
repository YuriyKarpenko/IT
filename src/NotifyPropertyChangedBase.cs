using System;
using System.ComponentModel;
using System.Diagnostics;


namespace IT
{
	/// <summary>
	/// Простая реализация INotifyPropertyChanged
	/// </summary>
#if !SILVERLIGHT
	[Serializable]
#endif
	public class NotifyPropertyChangedOnly : INotifyPropertyChanged, INotifyPropertyChanging
	{
		#region Члены INotifyPropertyChanged

		/// <summary>
		/// INotifyPropertyChanged
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;// = (sender, args) => { };	ибо херня при десериализации

		/// <summary>
		/// Упрощение вызова для наследников
		/// </summary>
		/// <param name="propertyName">Имя изменяемого свойства</param>
		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		#region Члены INotifyPropertyChanging

		/// <summary>
		/// INotifyPropertyChanging
		/// </summary>
		public event PropertyChangingEventHandler PropertyChanging;// = (sender, args) => { };	ибо херня при десериализации

		/// <summary>
		/// Упрощение вызова для наследников
		/// </summary>
		/// <param name="propertyName">Имя изменяемого свойства</param>
		protected virtual void OnPropertyChanging(string propertyName)
		{
			if (this.PropertyChanging != null)
				this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
		}

		#endregion
	}

	/// <summary>
	/// Простая реализация INotifyPropertyChanged, наследуется от Disposable
	/// </summary>
	public class NotifyPropertyChangedBase : Disposable, INotifyPropertyChanged, INotifyPropertyChanging
	{
		#region Члены INotifyPropertyChanged

		/// <summary>
		/// INotifyPropertyChanged
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;// = (sender, args) => { };	ибо херня при десериализации

		/// <summary>
		/// Упрощение вызова для наследников
		/// </summary>
		/// <param name="propertyName">Имя изменяемого свойства</param>
		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		#region Члены INotifyPropertyChanging

		/// <summary>
		/// INotifyPropertyChanging
		/// </summary>
		public event PropertyChangingEventHandler PropertyChanging;// = (sender, args) => { };	ибо херня при десериализации

		/// <summary>
		/// Упрощение вызова для наследников
		/// </summary>
		/// <param name="propertyName">Имя изменяемого свойства</param>
		protected virtual void OnPropertyChanging(string propertyName)
		{
			if (this.PropertyChanging != null)
				this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
		}

		#endregion
	}

#if !SILVERLIGHT
	/// <summary>
	/// NotifyPropertyChangedBase + проверку наличия изменяемого свойства
	/// </summary>
	[Obsolete("Не имеет смысла при использовании вне DEBUG")]
	public class NotifyPropertyChangedBaseEx : NotifyPropertyChangedBase, INotifyPropertyChanged, INotifyPropertyChanging
	{
		#region Properties

		[NonSerialized]
		private PropertyDescriptorCollection _props = null;
		/// <summary>
		/// Все свойства данного класса
		/// </summary>
		protected PropertyDescriptorCollection SelfProperties => this._props = this._props ?? TypeDescriptor.GetProperties(this);


		/// <summary>
		/// Выполнять ли проверку наличия изменяемого свойства
		/// </summary>
		[Browsable(false)]
		public bool ThrowOnInvalidPropertyName { get; protected set; }

		#endregion


		/// <summary>
		/// Конструктор
		/// </summary>
		protected NotifyPropertyChangedBaseEx()
		{
			this.ThrowOnInvalidPropertyName = true;
		}


		/// <summary>
		/// Проверка наличия свойства
		/// </summary>
		/// <param name="propertyName">Имя проверяемого свойства</param>
		//[DebuggerStepThrough]
		[Conditional("DEBUG")]  //	вне DEBUG вызов метода игнорируется
		public void VerifyPropertyName(string propertyName)
		{
			// Убедимся, что имя свойства совпадает с реальным,
			// существующим в этом классе свойством.
			if (this.SelfProperties[propertyName] == null)
			{
				string msg = "Проблемное название свойства: " + propertyName;

				if (this.ThrowOnInvalidPropertyName)
				{
					throw new Exception(msg);
				}
				else
				{
					this.Error(null, "() {0}", msg);
				}
			}
		}

#if DEBUG
		/// <summary>
		/// Destructor
		/// </summary>
		~NotifyPropertyChangedBaseEx()
		{
			string msg = string.Format("{0} ({1}) Finalized", this.GetType().Name, this.GetHashCode());
			this.Debug(msg);
			this.Dispose(false);
			return;
		}
#endif

		/// <summary>
		/// Возвращает короткое имя типа
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.GetType().Name;
		}

		/// <summary>
		/// Упрощение вызова для наследников
		/// </summary>
		/// <param name="propertyName">Имя изменяемого свойства</param>
		protected override void OnPropertyChanged(string propertyName)
		{
			this.VerifyPropertyName(propertyName);
			base.OnPropertyChanged(propertyName);
		}

		/// <summary>
		/// Упрощение вызова для наследников
		/// </summary>
		/// <param name="propertyName">Имя изменяемого свойства</param>
		protected override void OnPropertyChanging(string propertyName)
		{
			this.VerifyPropertyName(propertyName);
			base.OnPropertyChanging(propertyName);
		}
	}
#endif
}
