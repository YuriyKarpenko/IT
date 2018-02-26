// Decompiled with JetBrains decompiler
// Type: DataGridExtensions.DataGridFilteringEventArgs
// Assembly: DataGridExtensions, Version=1.0.37.0, Culture=neutral, PublicKeyToken=43de855f87de903a
// MVID: 5A6EF75F-94BB-4773-88B0-C65701D66FE0
// Assembly location: D:\SVN\NPF\release\packages\DataGridExtensions.1.0.37\lib\net40-Client\DataGridExtensions.dll

using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace IT.WPF.JetBrains
{
	/// <summary>
	/// Notification about additional columns to be filtered.
	/// Clients can e.g. use this event to cache/preload column data in a different thread and/or display a wait cursor while filtering.
	/// <remarks>
	/// Clients may only cancel the processing when e.g. the data grid is about to be unloaded. Canceling the process of filtering
	/// will cause the UI to be inconsistent.
	/// </remarks></summary>
	public class DataGridFilteringEventArgs : CancelEventArgs
	{
		/// <summary>Gets the additional columns that will be filtered.</summary>
		public ICollection<DataGridColumn> Columns { get; }

		/// <inheritdoc />
		/// <summary>
		/// Initializes a new instance of the <see cref="T:DataGridExtensions.DataGridFilteringEventArgs" /> class.
		/// </summary>
		/// <param name="columns">The additional columns that will be filtered.</param>
		public DataGridFilteringEventArgs(ICollection<DataGridColumn> columns)
		{
			// ISSUE: reference to a compiler-generated field
			this.Columns = columns;
		}
	}
}
