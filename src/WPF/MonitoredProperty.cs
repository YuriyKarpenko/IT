using System;
using System.Diagnostics;

namespace IT.WPF
{
	interface IMonitoredProperty<T>
	{
		event EventHandler<IT.EventArgs<T>> ValueChanged;
		T Value { get; set; }
	}

	/// <summary>
	/// Класс-обертка для свойств, которай отслеживает изменение свойства
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[DebuggerDisplay("Value = {Value}")]
	public class MonitoredProperty<T> : NotifyPropertyChangedBase, IMonitoredProperty<T>
	{
		/// <summary>
		/// Срабатывает при изменении значения
		/// </summary>
		public event EventHandler<EventArgs<T>> ValueChanged;
		/// <summary>
		/// Вызывается после изменения свойства
		/// </summary>
		/// <param name="v">Новое значение</param>
		protected virtual void OnValueChanged(T v) => ValueChanged?.Invoke(this, new EventArgs<T>(v));


		/// <summary>
		/// Значение свойства
		/// </summary>
		public virtual T Value
		{
			get { return this._value; }
			set
			{
				if (!object.Equals(value, this._value))
				{
					this.OnPropertyChanging("Value");
					this._value = value;
					this.OnPropertyChanged("Value");
					this.OnValueChanged(value);
				}
			}
		}
		private T _value;


		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="valueChanged">Вызывается после изменения свойства</param>
		public MonitoredProperty(EventHandler<EventArgs<T>> valueChanged = null)
		{
			if (valueChanged != null)
				this.ValueChanged += valueChanged;
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="valueChanged">Вызывается после изменения свойства</param>
		public MonitoredProperty(Action<T> valueChanged)
		{
			if (valueChanged != null)
				this.ValueChanged += (s, e) => valueChanged(e.Value);
		}


		/// <summary>
		/// Вывод значения
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format("MonitoredProperty<{0}> (Value = {1})", typeof(T).Name, this.Value);
		}

		/// <summary>
		/// Неявное преобразование данного класса в тип его значения (T Value)
		/// </summary>
		/// <param name="v"></param>
		public static implicit operator T (MonitoredProperty<T> v) => v.Value;
	}
}
