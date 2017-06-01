using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace IT
{
	/// <summary>
	/// Заготовка для использования в деревьях (только базовые свойтва и события)
	/// </summary>
	[DebuggerDisplay("{Value} ({Childs.Count})")]
	public class TreeNode_Base : NotifyPropertyChangedOnly
	{
		#region events

		/// <summary>
		/// Срабатывает при выборе узла
		/// </summary>
		public event EventHandler IsCheckedChanged;
		/// <summary>
		/// Срабатывает при раскрытии узла
		/// </summary>
		public event EventHandler IsExpandedChanged;
		/// <summary>
		/// Срабатывает при выборе узла
		/// </summary>
		public event EventHandler IsSelectedChanged;

		#endregion

		private bool? isChecked = false;
		private bool isExpanded;
		private bool isSelected;

		#region Properties

		/// <summary>
		/// Для возможности выбора
		/// </summary>
		public virtual bool? IsChecked
		{
			get { return this.isChecked; }
			set { this.SetIsChecked(value, true); }
		}

		/// <summary>
		/// Индикатор состояния узла (binding)
		/// </summary>
		public virtual bool IsExpanded
		{
			get { return this.isExpanded; }
			set { this.SetIsExpanded(value); }
		}

		/// <summary>
		/// Индикатор состояния узла  (binding)
		/// </summary>
		public virtual bool IsSelected
		{
			get { return this.isSelected; }
			set { this.SetIsSelected(value); }
		}


		/// <summary>
		/// Позволяет использовать картинку
		/// </summary>
		public object Image { get; protected set; }

		#endregion


		/// <summary>
		/// установка дочерних IsChecked, установка данного IsChecked, вызов IsCheckedChanged
		/// </summary>
		/// <param name="value"></param>
		/// <param name="isRiseEvent"></param>
		protected virtual void SetIsChecked(bool? value, bool isRiseEvent)
		{
			if (this.isChecked != value)
			{
				if (this.isChecked != value)
				{
					this.OnPropertyChanging("IsChecked");
					this.isChecked = value;
					this.OnPropertyChanged("IsChecked");
				}

				if (isRiseEvent && this.IsCheckedChanged != null)
					this.IsCheckedChanged(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Позволяет переназначить действие при паскрытии узла
		/// </summary>
		/// <param name="value"></param>
		protected virtual void SetIsExpanded(bool value)
		{
			if (this.isExpanded != value)
			{
				this.OnPropertyChanging("IsExpanded");
				this.isExpanded = value;
				this.OnPropertyChanged("IsExpanded");

				if (this.IsExpandedChanged != null)
					this.IsExpandedChanged(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Позволяет переназначить действие при выборе узла
		/// </summary>
		/// <param name="value"></param>
		protected virtual void SetIsSelected(bool value)
		{
			if (this.isSelected != value)
			{
				this.OnPropertyChanging("IsSelected");
				this.isSelected = value;
				this.OnPropertyChanged("IsSelected");

				if (this.IsSelectedChanged != null)
					this.IsSelectedChanged(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Позволяет применить картинку
		/// </summary>
		/// <param name="image"></param>
		protected virtual void SetImage(object image)
		{
			this.Image = image;
			this.OnPropertyChanged("Image");
		}

	}

	/// <summary>
	/// Заготовка для использования в деревьях
	/// </summary>
	/// <typeparam name="TNode">Тип класса-наследника</typeparam>
	[DebuggerDisplay("{Level} {Value} ({Childs.Count})")]
	public class TreeNode_Base<TNode> : TreeNode_Base, ILog where TNode : TreeNode_Base<TNode>
	{
		/// <summary>
		/// Вычисляет и устанавливает IsChecked в зависимости от состояния IsChecked дочерних элементов
		/// </summary>
		/// <param name="node"></param>
		public static void SetIsCheckedByChilds(TreeNode_Base<TNode> node)
		{
			var b = node.Childs.All(i => i.IsChecked.HasValue && !i.IsChecked.Value);
			if (b)
				node.IsChecked = false;
			else
			{
				b = node.Childs.All(i => i.IsChecked.HasValue && i.IsChecked.Value);
				if (b)
					node.IsChecked = true;
				else
					node.IsChecked = null;
			}
		}

		#region Properties

		/// <summary>
		/// Родительский узел
		/// </summary>
		public virtual TNode Parent { get; private set; }

		/// <summary>
		/// Список дочерних узлов
		/// </summary>
		public virtual IList<TNode> Childs { get; protected set; }

		/// <summary>
		/// Уровень вложенности
		/// </summary>
		public uint Level { get { return this.Parent == null ? 0 : Parent.Level + 1; } }

		#endregion


		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="parent"></param>
		public TreeNode_Base(TNode parent)
		{
			this.Trace("()");

			this.Parent = parent;

			this.Childs = this.CreateChilds();
		}


		/// <summary>
		/// Получение верхнего узла данной ветки
		/// </summary>
		/// <returns></returns>
		public TNode GetRoot()
		{
			var p = (TNode)this;
			while (p.Parent != null)
				p = p.Parent;
			return p;
		}

		/// <summary>
		/// установка дочерних IsCheckedб установка данного IsChecked, вызов IsCheckedChanged
		/// </summary>
		/// <param name="value"></param>
		/// <param name="isRiseEvent"></param>
		protected override void SetIsChecked(bool? value, bool isRiseEvent)
		{
			this.Trace("()");
			try
			{
				if (this.IsChecked != value)
				{
					if (isRiseEvent && value.HasValue)	//	по-любому устанавливаем выбор дочерних
					{
						foreach (var c in this.Childs)
							c.IsChecked = value;
					}

					base.SetIsChecked(value, isRiseEvent);
				}
			}
			catch (Exception ex)
			{
				this.Error(ex, "()");
				throw;
			}
		}

		/// <summary>
		/// Позволяет переназначить действие при выборе узла
		/// </summary>
		/// <param name="value"></param>
		protected override void SetIsSelected(bool value)
		{
			this.Trace("()");
			try
			{
				if (this.IsSelected != value)
				{
					if (value)// && this.Parent != null)
						//Parent.ChildSelected((TNode)this, (TNode)this);
						this.GetRoot().ClearSelected();

					base.SetIsSelected(value);
				}
			}
			catch (Exception ex)
			{
				this.Error(ex, "()");
				throw;
			}
		}

		///// <summary>
		///// Позволяет снять выделение с остальных дочерних элементов
		///// </summary>
		///// <param name="sender"></param>
		///// <param name="selectedItem"></param>
		//protected virtual void ChildSelected(TNode sender, TNode selectedItem)
		//{
		//	this.Trace("()");

		//	if (this.Parent != null)
		//		Parent.ChildSelected((TNode)this, selectedItem);	//	находим root
		//	else
		//		this.ClearSelected();
		//}

		/// <summary>
		/// Очистка IsSelected данного узла и всех дочерних
		/// </summary>
		protected void ClearSelected()
		{
			this.SetIsSelected(false);
			if (this.Childs != null)
				foreach (var c in this.Childs)
					c.ClearSelected();
		}

		/// <summary>
		/// При переопределении в наследнике позволяет указать другой класс соллекции
		/// </summary>
		/// <returns></returns>
		protected virtual IList<TNode> CreateChilds()
		{
			this.Trace("()");

			return new Collection<TNode>();
		}

	}

	/// <summary>
	/// Заготовка для использования в деревьях (в основном методы для работы с T)
	/// </summary>
	/// <typeparam name="TNode">Тип класса-наследника</typeparam>
	/// <typeparam name="T"></typeparam>
	[DebuggerDisplay("{Level} {Value} ({Childs.Count})")]
	public class TreeNode<TNode, T> : TreeNode_Base<TNode>, ILog where TNode : TreeNode<TNode, T>
	{
		#region Properties

		/// <summary>
		/// Значение узла
		/// </summary>
		public virtual T Value { get; private set; }

		#endregion


		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="parent">Родительский узел</param>
		/// <param name="value">Значение узла</param>
		public TreeNode(TreeNode<TNode, T> parent, T value)
			: base((TNode)parent)
		{
			this.Trace("()");

			this.Value = value;
		}



		/// <summary>
		/// Формирует + добавляет дочерний узел и возвращает его
		/// </summary>
		/// <param name="item">Значение нового узла</param>
		/// <returns></returns>
		public virtual TNode Add(T item)
		{
			this.Trace("()");

			var n = this.CreateChild(item);
			this.Childs.Add(n);
			return n;
		}

		/// <summary>
		/// Позволяет наследникам контрольровать процесс создания экземпляра
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public virtual TNode CreateChild(T item)
		{
			this.Trace("()");

			var n = new TreeNode<TNode, T>(this, item);
			return (TNode)n;
		}


		/// <summary>
		/// Проверяет наличие значения в узле и всех дочерних узлах
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public virtual bool Contains(T item)
		{
			this.Trace("()");

			if (object.Equals(item, this.Value))
				return true;

			foreach (var ti in this.Childs)
				if (ti.Contains(item))
					return true;

			return false;
		}

		/// <summary>
		/// Поиск первого узла по критерию в узле и всех дочерних узлах
		/// </summary>
		/// <param name="criteria"></param>
		/// <returns></returns>
		public virtual TNode Find(Predicate<TNode> criteria)
		{
			this.Trace("()");

			if (criteria((TNode)this))
				return (TNode)this;

			foreach (var ti in this.Childs)
			{
				var x = ti.Find(criteria);
				if (x != null)
					return x;
			}

			return null;
		}

		/// <summary>
		/// Поиск всех узлов по критерию в узле и всех дочерних узлах
		/// </summary>
		/// <param name="criteria"></param>
		/// <returns></returns>
		public virtual IEnumerable<TNode> FindListNode(Func<TNode, bool> criteria)
		{
			this.Trace("()");

			if (criteria((TNode)this))
				yield return (TNode)this;

			foreach (var item in this.Childs)
			{
				var l = item.FindListNode(criteria).ToArray();
				foreach (var x in l)
					yield return x;

			}
		}



		/// <summary>
		/// Позволяет выводить дерево родителей + себя
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return (this.Parent == null ? "" : Parent.ToString() + " -> ") + (this.Value == null ? "" : this.Value.ToString()) + " ";
		}

	}
}
