using System;
using System.Collections.Generic;

namespace IT.WPF
{
	/// <summary>
	/// Класс для управления группой переключателей
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class GroupCheckAdapter<T> : NotifyPropertyChangedBase
	{
		Func<ValueWrapper<T>[]> _getFullArray;

		/// <summary>
		/// Возникает при изменении какого-либо переключателя
		/// </summary>
		public event EventHandler<EventArgs<ValueWrapper<T>>> ValueChanged;
		/// <summary>
		/// Вызывает соответствующее событие
		/// </summary>
		/// <param name="value">Изменившийся переключатель</param>
		protected void OnValueChanged(ValueWrapper<T> value)
		{
			if (this.ValueChanged != null)
				ValueChanged(this, new EventArgs<ValueWrapper<T>>(value));

			this.OnPropertyChanged("Selected");
		}

		#region Properties

		/// <summary>
		/// Весь список переключателей
		/// </summary>
		public ValueWrapper<T>[] List { get; private set; }

		/// <summary>
		/// Выбранные переключатели
		/// </summary>
		public IList<ValueWrapper<T>> Selected { get; private set; }

		#endregion

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="fullArray">Весь список переключателей</param>
		/// <param name="onValueChanged">Вызывает соответствующее событие</param>
		public GroupCheckAdapter(ValueWrapper<T>[] fullArray, EventHandler<EventArgs<ValueWrapper<T>>> onValueChanged = null)
			: this(onValueChanged)
		{
			Contract.Requires<ArgumentException>(fullArray != null, "fullArray");

			this.Reset(fullArray);
		}

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="getFullArray">Метод получения списка переключателей</param>
		/// <param name="onValueChanged">Вызывает соответствующее событие</param>
		public GroupCheckAdapter(Func<ValueWrapper<T>[]> getFullArray, EventHandler<EventArgs<ValueWrapper<T>>> onValueChanged = null)
			: this(onValueChanged)
		{
			this._getFullArray = getFullArray;

			this.Reset();
		}

		private GroupCheckAdapter(EventHandler<EventArgs<ValueWrapper<T>>> onValueChanged)
		{
			this.Selected = new List<ValueWrapper<T>>();

			if (onValueChanged != null)
				this.ValueChanged += onValueChanged;
		}

		/// <summary>
		/// Деструктор, отвязывает события
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
				foreach (var ew in this.List)
					ew.IsCheckedChanged -= this.Flag_Changed;

			base.Dispose(disposing);
		}


		/// <summary>
		/// Очистка значений переклюсателей
		/// </summary>
		public virtual void Clear()
		{
			foreach (ValueWrapper<T> vw in this.List)
				vw.IsChecked = false;
			this.Selected.Clear();
		}

		/// <summary>
		/// Заново заполняет this.List
		/// </summary>
		public void Reset(ValueWrapper<T>[] list)
		{
			this.List = list;
			this.OnPropertyChanged("List");
			this.Init_List();
		}

		/// <summary>
		/// Заново заполняет (this.List = this.getFullArray())
		/// </summary>
		public void Reset()
		{
			Contract.NotNull(this._getFullArray, "getFullArray");
			this.Reset(this._getFullArray());
		}

		/// <summary>
		/// Вызывается при смене состояния переключателя
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void Flag_Changed(object sender, EventArgs<bool> e)
		{
			var vw = sender as ValueWrapper<T>;

			if (vw != null)
			{
				if (e.Value)
					this.Selected.Add(vw);
				else
					this.Selected.Remove(vw);
				this.OnValueChanged(vw);
			}
		}

		/// <summary>
		/// Привязка событий IsCheckedChanged для каждого элемента списка
		/// </summary>
		protected virtual void Init_List()
		{
			foreach (var ew in this.List)
				ew.IsCheckedChanged += this.Flag_Changed;
		}
	}
}
