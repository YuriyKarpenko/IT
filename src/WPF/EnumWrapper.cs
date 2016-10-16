using System;
using System.Collections.Generic;
using System.Linq;

namespace IT.WPF
{
	/// <summary>
	/// Обертка для удобного манипулирования объектами в списочных элементах управления
	/// </summary>
	/// <typeparam name="T">Тип перечисления</typeparam>
	public class ValueWrapper<T> : NotifyPropertyChangedOnly
	{
		#region Events

		/// <summary>
		/// No Coment
		/// </summary>
		public event EventHandler<EventArgs<bool>> IsCheckedChanged;
		/// <summary>
		/// Метод установки значения + вызов события
		/// </summary>
		/// <param name="value"></param>
		protected virtual void Set_IsChecked(bool value)
		{
			if (this.isChecked != value)
			{
				this.isChecked = value;
				if (this.IsCheckedChanged != null)
					this.IsCheckedChanged(this, new EventArgs<bool>(value));
				this.OnPropertyChanged("IsChecked");
			}
		}

		/// <summary>
		/// No Coment
		/// </summary>
		public event EventHandler<EventArgs<bool>> IsEnabledChanged;
		/// <summary>
		/// Метод установки значения + вызов события
		/// </summary>
		/// <param name="value"></param>
		protected virtual void Set_IsEnabled(bool value)
		{
			if (this.isEnabled != value)
			{
				this.isEnabled = value;
				if (this.IsEnabledChanged != null)
					this.IsEnabledChanged(this, new EventArgs<bool>(value));
				this.OnPropertyChanged("IsEnabled");
			}
		}

		#endregion

		private readonly string caption;
		private readonly T key;
		private bool isChecked;
		private bool isEnabled;

		#region Properties

		/// <summary>
		/// Собственно значение
		/// </summary>
		public virtual T Key => this.key;

		/// <summary>
		/// return this.ToString();
		/// </summary>
		public virtual string Caption => this.caption ?? this.ToString();

		/// <summary>
		/// Для возможности выбирать в списке
		/// </summary>
		public virtual bool IsChecked
		{
			get { return this.isChecked; }
			set { this.Set_IsChecked(value); }
		}

		/// <summary>
		/// Для возможности отключения
		/// </summary>
		public virtual bool IsEnabled
		{
			get { return this.isEnabled; }
			set { this.Set_IsEnabled(value); }
		}


		#endregion

		#region ctor

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="key"></param>
		/// <param name="caption"></param>
		/// <param name="isCheckedChanged"></param>
		public ValueWrapper(T key, string caption = null, EventHandler<EventArgs<bool>> isCheckedChanged = null)
		{
			this.IsChecked = false;
			this.IsEnabled = true;
			this.key = key;
			this.caption = caption;
			if (isCheckedChanged != null)
				this.IsCheckedChanged += isCheckedChanged;
		}

		#endregion

		/// <summary>
		/// No Coment
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format("{0} : [{1}]", this.caption ?? base.ToString(), this.Key);
		}
	}


	/// <summary>
	/// Обертка для удобного использования Enum со списками
	/// </summary>
	/// <typeparam name="T">Тип перечисления</typeparam>
	public class EnumWrapper<T> : ValueWrapper<T> where T : struct
	{
		/// <summary>
		/// return Enum.GetValues(typeof(T)).Cast&lt;T>().Select(i => new EnumWrapper&lt;T>(i));
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<EnumWrapper<T>> CreateFromEnumType()
		{
			var vs = Enum.GetValues(typeof(T));
			return vs.Cast<T>().Select(i => new EnumWrapper<T>(i));
		}
		/// <summary>
		/// return Enum.GetValues(typeof(T)).Cast&lt;T>().Select(i => new EnumWrapper&lt;T>(i, isCheckedChanged));
		/// </summary>
		/// <param name="isCheckedChanged"></param>
		/// <returns></returns>
		public static IEnumerable<EnumWrapper<T>> CreateFromEnumType(EventHandler<EventArgs<bool>> isCheckedChanged)
		{
			var vs = Enum.GetValues(typeof(T));
			return vs.Cast<T>().Select(i => new EnumWrapper<T>(i, isCheckedChanged));
		}



		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="key"></param>
		/// <param name="isCheckedChanged"></param>
		public EnumWrapper(T key, EventHandler<EventArgs<bool>> isCheckedChanged = null) : base(key, null, isCheckedChanged) { }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			var o = obj as EnumWrapper<T>;
			return (o != null) && o.Key.Equals(this.Key);
		}

		/// <summary>
		/// Возвращает хеш-код данного экземпляра
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return this.Key.GetHashCode();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			var k = this.Key as Enum;
			return k.GetDescription() ?? (/*k.Equals((object)0) ? base.ToString() : */k.ToString().Replace('_', ' '));
		}
	}
}
