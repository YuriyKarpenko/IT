// Decompiled with JetBrains decompiler
// Type: DataGridExtensions.InternalExtensionMethods
// Assembly: DataGridExtensions, Version=1.0.37.0, Culture=neutral, PublicKeyToken=43de855f87de903a
// MVID: 5A6EF75F-94BB-4773-88B0-C65701D66FE0
// Assembly location: D:\SVN\NPF\release\packages\DataGridExtensions.1.0.37\lib\net40-Client\DataGridExtensions.dll

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace IT.WPF.JetBrains
{
	internal static class InternalExtensionMethods
	{
		/// <summary>Restarts the specified timer.</summary>
		/// <param name="timer">The timer.</param>
		/// <requires csharp="timer != null" vb="timer &lt;&gt; Nothing">timer != null</requires>
		internal static void Restart(this DispatcherTimer timer)
		{
			timer.Stop();
			timer.Start();
		}

		/// <summary>
		/// Walks the elements tree and returns the first element that derives from T.
		/// </summary>
		/// <typeparam name="T">The type to return.</typeparam>
		/// <param name="item">The item to start search with.</param>
		/// <returns>The element if found; otherwise null.</returns>
		internal static T FindAncestorOrSelf<T>(this DependencyObject item) where T : class
		{
			for (; item != null; item = LogicalTreeHelper.GetParent(item) ?? VisualTreeHelper.GetParent(item))
			{
				T obj = item as T;
				if ((object)obj != null)
					return obj;
			}
			return default(T);
		}

		public static IEnumerable<DependencyObject> AncestorsAndSelf(this DependencyObject self)
		{
			for (; self != null; self = LogicalTreeHelper.GetParent(self) ?? VisualTreeHelper.GetParent(self))
				yield return self;
		}

		/// <summary>
		/// Shortcut to <see cref="M:System.Windows.Threading.Dispatcher.BeginInvoke(System.Delegate,System.Object[])" /></summary>
		/// <requires csharp="self != null" vb="self &lt;&gt; Nothing">self != null</requires>
		/// <requires csharp="action != null" vb="action &lt;&gt; Nothing">action != null</requires>
		public static void BeginInvoke(this Visual self, Action action)
		{
			self.Dispatcher.BeginInvoke((Delegate)action);
		}

		/// <summary>
		/// Shortcut to <see cref="M:System.Windows.Threading.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority,System.Delegate)" /></summary>
		/// <requires csharp="self != null" vb="self &lt;&gt; Nothing">self != null</requires>
		/// <requires csharp="action != null" vb="action &lt;&gt; Nothing">action != null</requires>
		public static void BeginInvoke(this Visual self, DispatcherPriority priority, Action action)
		{
			self.Dispatcher.BeginInvoke(priority, (Delegate)action);
		}

		/// <summary>
		/// Performs a cast from object to <typeparamref name="T" />, avoiding possible null violations if <typeparamref name="T" /> is a value type.
		/// </summary>
		/// <typeparam name="T">The target type</typeparam>
		/// <param name="value">The value.</param>
		/// <returns>The value casted to <typeparamref name="T" />, or <c>default(T)</c> if value is <c>null</c>.</returns>
		public static T SafeCast<T>(this object value)
		{
			if (value != null)
				return (T)value;
			return default(T);
		}

		/// <summary>
		/// Gets the value of a dependency property using <see cref="M:DataGridExtensions.InternalExtensionMethods.SafeCast``1(System.Object)" />.
		/// </summary>
		/// <typeparam name="T" />
		/// <param name="self">The dependency object from which to get the value.</param>
		/// <param name="property">The property to get.</param>
		/// <returns>The value safely casted to <typeparamref name="T" /></returns>
		/// <requires csharp="self != null" vb="self &lt;&gt; Nothing">self != null</requires>
		/// <requires csharp="property != null" vb="property &lt;&gt; Nothing">property != null</requires>
		public static T GetValue<T>(this DependencyObject self, DependencyProperty property)
		{
			return self.GetValue(property).SafeCast<T>();
		}
	}
}
