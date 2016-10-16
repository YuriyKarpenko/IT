using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Data;

namespace IT.WPF
{
	/// <summary>
	/// Методы расширения
	/// </summary>
	public static class WPF_Extentions
	{
		/// <summary>
		/// При отображении окна : 
		/// <para>1 задает ShowInTaskbar = false</para>
		/// <para>2 назначает Owner (если null, то Application.Current.MainWindow)</para>
		/// </summary>
		/// <param name="dialog">Расширяемый экземпляр</param>
		/// <param name="dataContext">Если не null, то присваивается dialog.DataContext</param>
		/// <param name="owner">Окно - владелец модального окна</param>
		/// <param name="showInTaskbar">Показывать ли окно в панели задач</param>
		/// <returns></returns>
		public static bool ShowDialog(this Window dialog, object dataContext, Window owner, bool showInTaskbar = false)
		{
			if (dataContext != null)
				dialog.DataContext = dataContext;
			dialog.ShowInTaskbar = showInTaskbar;
			dialog.Owner = owner != null ? owner : (Application.Current == null ? null : Application.Current.MainWindow);
			var b = dialog.ShowDialog();
			return b.HasValue && b.Value;
		}


		/// <summary>
		/// Добавление CommandBinding по его параметрам.
		/// Возвращает созданный CommandBinding
		/// </summary>
		/// <param name="cbs">Расширяемый объект</param>
		/// <param name="cmd">Комманда</param>
		/// <param name="exec">Метод спабатывания</param>
		/// <param name="canExec">Метод проверки доступности</param>
		/// <returns>Созданный CommandBinding</returns>
		public static CommandBinding Add(this CommandBindingCollection cbs, ICommand cmd, ExecutedRoutedEventHandler exec, CanExecuteRoutedEventHandler canExec = null)
		{
			Contract.Requires<ArgumentException>(null != cbs, "cbs");
			Contract.Requires<ArgumentException>(null != cmd, "cmd");
			Contract.Requires<ArgumentException>(null != exec, "exec");

			var cb = canExec == null ? new CommandBinding(cmd, exec) : new CommandBinding(cmd, exec, canExec);
			cbs.Add(cb);
			return cb;
		}

		/// <summary>
		/// Добавление CommandBinding по его параметрам.
		/// Возвращает созданный CommandBinding
		/// </summary>
		/// <param name="cbs">Расширяемый объект</param>
		/// <param name="cmd">Комманда</param>
		/// <param name="exec">Метод спабатывания</param>
		/// <param name="canExec">Метод проверки доступности</param>
		/// <returns>Созданный CommandBinding</returns>
		public static CommandBinding Add(this CommandBindingCollection cbs, ICommand cmd, Action<ExecutedRoutedEventArgs> exec, Action<CanExecuteRoutedEventArgs> canExec = null)
		{
			return canExec == null ? cbs.Add(cmd, (s, e) => exec(e)) : cbs.Add(cmd, (s, e) => exec(e), (s, e) => canExec(e));
		}

		/// <summary>
		/// Добавление CommandBinding по его параметрам.
		/// Возвращает созданный CommandBinding
		/// </summary>
		/// <param name="cbs">Расширяемый объект</param>
		/// <param name="cmd">Комманда</param>
		/// <param name="exec">Метод спабатывания</param>
		/// <param name="canExec">Метод проверки доступности</param>
		/// <returns>Созданный CommandBinding</returns>
		public static CommandBinding Add(this CommandBindingCollection cbs, ICommand cmd, Action exec, Func<bool> canExec = null)
		{
			return canExec == null ? cbs.Add(cmd, e => exec()) : cbs.Add(cmd, e => exec(), e => e.CanExecute = canExec());
		}

		/// <summary>
		/// Добавление CommandBinding по его параметрам.
		/// Возвращает созданный CommandBinding
		/// </summary>
		/// <param name="cbs">Расширяемый объект</param>
		/// <param name="cmd">Комманда</param>
		/// <param name="exec">Метод спабатывания</param>
		/// <param name="canExec">Метод проверки доступности</param>
		/// <returns>Созданный CommandBinding</returns>
		public static CommandBinding Append(this CommandBindingCollection cbs, ICommand cmd, ExecutedRoutedEventHandler exec, CanExecuteRoutedEventHandler canExec = null)
		{
			Contract.Requires<ArgumentException>(null != cbs, "cbs");
			Contract.Requires<ArgumentException>(null != cmd, "cmd");
			Contract.Requires<ArgumentException>(null != exec, "exec");

			var cb = cbs.Cast<CommandBinding>().FirstOrDefault(i => i.Command == cmd);
			if (cb != null)
			{
				cb.Executed += exec;
				if (canExec != null)
					cb.CanExecute += canExec;
			}
			else
			{
				cb = canExec == null ? new CommandBinding(cmd, exec) : new CommandBinding(cmd, exec, canExec);
				cbs.Add(cb);
			}
			return cb;
		}

		/// <summary>
		/// Добавление CommandBinding по его параметрам.
		/// Возвращает созданный CommandBinding
		/// </summary>
		/// <param name="cbs">Расширяемый объект</param>
		/// <param name="cmd">Комманда</param>
		/// <param name="exec">Метод спабатывания</param>
		/// <param name="canExec">Метод проверки доступности</param>
		/// <returns>Созданный CommandBinding</returns>
		public static CommandBinding Append(this CommandBindingCollection cbs, ICommand cmd, Action<ExecutedRoutedEventArgs> exec, Action<CanExecuteRoutedEventArgs> canExec = null)
		{
			return canExec == null ? cbs.Append(cmd, (s, e) => exec(e)) : cbs.Append(cmd, (s, e) => exec(e), (s, e) => canExec(e));
		}

		/// <summary>
		/// Добавление CommandBinding по его параметрам при этом удаляет существующие привязки к командам.
		/// Возвращает созданный CommandBinding
		/// </summary>
		/// <param name="cbs">Расширяемый объект</param>
		/// <param name="cmd">Комманда</param>
		/// <param name="exec">Метод спабатывания</param>
		/// <param name="canExec">Метод проверки доступности</param>
		/// <returns>Созданный CommandBinding</returns>
		public static CommandBinding ReAdd(this CommandBindingCollection cbs, ICommand cmd, ExecutedRoutedEventHandler exec, CanExecuteRoutedEventHandler canExec = null)
		{
			Contract.Requires<ArgumentException>(null != cbs, "cbs");
			Contract.Requires<ArgumentException>(null != cmd, "cmd");
			Contract.Requires<ArgumentException>(null != exec, "exec");

			var cb = cbs.Cast<CommandBinding>().FirstOrDefault(i => i.Command == cmd);
			if (cb != null)
				cbs.Remove(cb);

			cbs.Add(cmd, exec, canExec);

			return cb;
		}

		/// <summary>
		/// Добавление CommandBinding по его параметрам.
		/// Возвращает созданный CommandBinding
		/// </summary>
		/// <param name="cbs">Расширяемый объект</param>
		/// <param name="cmd">Комманда</param>
		/// <param name="exec">Метод спабатывания</param>
		/// <param name="canExec">Метод проверки доступности</param>
		/// <returns>Созданный CommandBinding</returns>
		public static CommandBinding ReAdd(this CommandBindingCollection cbs, ICommand cmd, Action<ExecutedRoutedEventArgs> exec, Action<CanExecuteRoutedEventArgs> canExec = null)
		{
			return canExec == null ? cbs.ReAdd(cmd, (s, e) => exec(e)) : cbs.ReAdd(cmd, (s, e) => exec(e), (s, e) => canExec(e));
		}


		/// <summary>
		/// Получение ячейки
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dg"></param>
		/// <param name="item"></param>
		/// <param name="colIndex"></param>
		/// <returns></returns>
		public static DataGridCell GetCall(this DataGrid dg, object item, int colIndex)
		{
			var row = dg.ItemContainerGenerator.ContainerFromItem(item);// as DataGridRow;
			if (row != null)
			{
				var cellPresenters = row.GetVisualChildren<DataGridCellsPresenter>();
				if (cellPresenters != null)
				{
					var cg = cellPresenters.First().ItemContainerGenerator;
					var cell = cg.ContainerFromIndex(colIndex);
					return cell as DataGridCell;
				}
			}
			return null;
		}

		/// <summary>
		/// Переводит ячейку в режим редвктирования, предоставляет контрол режима редактирования. Возвращает успех редактирования
		/// </summary>
		/// <param name="cell"></param>
		/// <param name="editControl">Контрол режима редактирования (из GetCellContent())</param>
		public static bool Edit(this DataGridCell cell, Predicate<FrameworkElement> editControl)
		{
			if (!cell.IsReadOnly)
				try
				{
					cell.IsEditing = true;
					if (cell.IsEditing)
					{
						var control = cell.Column.GetCellContent(cell.DataContext);
						cell.UpdateLayout();	//	обновление значений control
						return editControl(control);
					}
				}
				finally
				{
					cell.IsEditing = false;
				}
			return false;
		}

		/// <summary>
		/// Переводит ячейку в режим редвктирования, предоставляет контрол режима редактирования. По окончании делает BindingGroup.CommitEdit() и выходит из режима редактирования; Возвращает успех редактирования
		/// </summary>
		/// <param name="cell"></param>
		/// <param name="editControl">Контрол режима редактирования (из GetCellContent())</param>
		public static bool EditCommit(this DataGridCell cell, Predicate<FrameworkElement> editControl)
		{
			var res = false;
			IInvoke_Extention.Context.Exec(() => res = EditCommit_Core(cell, editControl), false);
			return res;
		}
		static bool EditCommit_Core(DataGridCell cell, Predicate<FrameworkElement> editControl)
		{ 
			if (!cell.IsReadOnly)
				try
				{
					cell.BindingGroup.BeginEdit();
					var b = cell.Edit(editControl);
					return b;
				}
				finally
				{
					cell.BindingGroup.CommitEdit();
				}
			return false;
		}

		/// <summary>
		/// Пытается вернуть название поля привязки
		/// </summary>
		/// <param name="header"></param>
		/// <returns></returns>
		public static string GetFieldName(this DataGridBoundColumn col)
		{
			if (col != null)
			{
				var b = col.Binding as Binding;
				if (b != null)
				{
					return b.Path?.Path;
				}
				else
				{
					return col.HeaderStringFormat;	//	костыть для DataGridTemplateColumn
				}
			}

			return null;
		}
		/// <summary>
		/// Пытается вернуть название поля привязки
		/// </summary>
		/// <param name="header"></param>
		/// <returns></returns>
		public static string GetFieldName(this DataGridColumnHeader header)
		{
			var c = header == null ? null : header.Column as DataGridBoundColumn;
			return c.GetFieldName();
		}
		/// <summary>
		/// Пытается вернуть название поля привязки
		/// </summary>
		/// <param name="header"></param>
		/// <returns></returns>
		public static string GetFieldName(this GridViewColumnHeader header)
		{
			if (header != null && header.Column != null)
			{
				var c = header.Column;

				var b = c.DisplayMemberBinding as Binding;
				if (b != null)
					return b.Path?.Path;
			}

			return null;
		}



		/// <summary>
		/// Получение дочерних визуальных объектов указанного типа
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="parent">Расширяемый объект</param>
		/// <returns></returns>
		public static IEnumerable<T> GetVisualChildren<T>(this DependencyObject parent) where T : DependencyObject
		{
			Contract.Requires<ArgumentException>(null != parent, "parent");

			if (parent is FrameworkElement)
				(parent as FrameworkElement).ApplyTemplate();

			var count = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < count; i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);

				if (child is T)
					yield return (T)child;

				foreach (T c in GetVisualChildren<T>(child))
					yield return c;
			}
		}

		/// <summary>
		/// Помск визуального родителя
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static T GetVisualParent<T>(this DependencyObject obj) where T : DependencyObject
		{
			Contract.NotNull(obj, "obj");
			do
			{
				if (obj is T)
					return (T)obj;
				obj = VisualTreeHelper.GetParent(obj);
			}
			while (obj != null);

			return null;
		}

		/// <summary>
		/// Прокрутка скрола
		/// </summary>
		/// <param name="itemsControl">Что прокрутить</param>
		/// <param name="item">Куда прокрутить</param>
		public static void ScrollToCenterOfView(this ItemsControl itemsControl, object item)
		{
			// Scroll immediately if possible
			if (!itemsControl.TryScrollToCenterOfView(item))
			{
				// Otherwise wait until everything is loaded, then scroll
				if (itemsControl is ListBox)
					((ListBox)itemsControl).ScrollIntoView(item);

				itemsControl.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
				{
					itemsControl.TryScrollToCenterOfView(item);
				}));
			}
		}



		private static bool TryScrollToCenterOfView(this ItemsControl itemsControl, object item)
		{
			// Find the container
			var container = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as UIElement;
			if (container == null) return false;

			// Find the ScrollContentPresenter
			//var presenter = container.GetVisualParent<ScrollContentPresenter>();
			ScrollContentPresenter presenter = null;
			for (Visual vis = container; vis != null && vis != itemsControl; vis = VisualTreeHelper.GetParent(vis) as Visual)
				if ((presenter = vis as ScrollContentPresenter) != null)
					break;
			if (presenter == null) return false;

			// Find the IScrollInfo
			var scrollInfo =
				!presenter.CanContentScroll ? presenter :
				presenter.Content as IScrollInfo ??
				FirstVisualChild(presenter.Content as ItemsPresenter) as IScrollInfo ??
				presenter;

			// Compute the center point of the container relative to the scrollInfo
			Size size = container.RenderSize;
			Point center = container.TransformToAncestor((Visual)scrollInfo).Transform(new Point(size.Width / 2, size.Height / 2));
			center.Y += scrollInfo.VerticalOffset;
			center.X += scrollInfo.HorizontalOffset;

			// Adjust for logical scrolling
			if (scrollInfo is StackPanel || scrollInfo is VirtualizingStackPanel)
			{
				double logicalCenter = itemsControl.ItemContainerGenerator.IndexFromContainer(container) + 0.5;
				Orientation orientation = scrollInfo is StackPanel ? ((StackPanel)scrollInfo).Orientation : ((VirtualizingStackPanel)scrollInfo).Orientation;
				if (orientation == Orientation.Horizontal)
					center.X = logicalCenter;
				else
					center.Y = logicalCenter;
			}

			// Scroll the center of the container to the center of the viewport
			if (scrollInfo.CanVerticallyScroll) scrollInfo.SetVerticalOffset(CenteringOffset(center.Y, scrollInfo.ViewportHeight, scrollInfo.ExtentHeight));
			if (scrollInfo.CanHorizontallyScroll) scrollInfo.SetHorizontalOffset(CenteringOffset(center.X, scrollInfo.ViewportWidth, scrollInfo.ExtentWidth));
			return true;
		}

		private static double CenteringOffset(double center, double viewport, double extent)
		{
			return Math.Min(extent - viewport, Math.Max(0, center - viewport / 2));
		}

		private static DependencyObject FirstVisualChild(Visual visual)
		{
			if (visual == null) return null;
			if (VisualTreeHelper.GetChildrenCount(visual) == 0) return null;
			return VisualTreeHelper.GetChild(visual, 0);
		}
	}
}

namespace System.Collections.ObjectModel
{
	/// <summary>
	/// Исправленная ObservableCollection
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ObservableCollectionEx<T> : ObservableCollection<T>
	{
		public ObservableCollectionEx() { }
		public ObservableCollectionEx(IEnumerable<T> collection) : base(collection) { }

		/// <summary>
		/// Перекрытое Событие
		/// </summary>
		public override event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <summary>
		/// Запист события
		/// </summary>
		/// <param name="e"></param>
		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			NotifyCollectionChangedEventHandler collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
				foreach (NotifyCollectionChangedEventHandler nh in collectionChanged.GetInvocationList())
				{
					DispatcherObject dispObj = nh.Target as DispatcherObject;
					if (dispObj != null)
					{
						Dispatcher dispatcher = dispObj.Dispatcher;
						if (dispatcher != null && !dispatcher.CheckAccess())
						{
							dispatcher.BeginInvoke(
								(Action)(() => nh.Invoke(this,
									new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset))),
								DispatcherPriority.DataBind);
							continue;
						}
					}
					nh.Invoke(this, e);
				}
		}

		/// <summary>
		/// Добавляет несколько объектов в конец System.Collections.ObjectModel.Collection&lt;T>
		/// </summary>
		/// <param name="args"></param>
		public void AddRange(IEnumerable<T> args)
		{
			foreach (T a in args)
				this.Add(a);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="collection"></param>
		public void InsertRange(int index, IEnumerable<T> collection)
		{
			this.CheckReentrancy();
			var items = this.Items as List<T>;
			items.InsertRange(index, collection);
			this.OnReset();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="count"></param>
		public void RemoveRange(int index, int count)
		{
			this.CheckReentrancy();
			var items = this.Items as List<T>;
			items.RemoveRange(index, count);
			this.OnReset();
		}


		private void OnReset()
		{
			this.OnPropertyChanged("Count");
			this.OnPropertyChanged("Items");
			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		private void OnPropertyChanged(string propertyName)
		{
			this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}
	}

}