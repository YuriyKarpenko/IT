using System;
using System.Diagnostics;

namespace IT
{
	/// <summary>
	/// Для передачи параметра типа T
	/// </summary>
	[DebuggerDisplay("{Value}")]
	public class EventArgs<T> : EventArgs
	{
		/// <summary>
		/// Собственно параметр
		/// </summary>
		public T Value { get; private set; }

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="value">Передаваемый параметр</param>
		public EventArgs(T value)
		{
			this.Value = value;
		}
	}

	/// <summary>
	/// Для передачи параметра типа T1, T2
	/// </summary>
	[DebuggerDisplay("{Value1}  {Value2}")]
	public class EventArgs<T1, T2> : EventArgs
	{
		/// <summary>
		/// Собственно параметр 1
		/// </summary>
		public T1 Value1 { get; protected set; }

		/// <summary>
		/// Собственно параметр 2
		/// </summary>
		public T2 Value2 { get; protected set; }

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="value1">Передаваемый параметр 1</param>
		/// <param name="value2">Передаваемый параметр 2</param>
		public EventArgs(T1 value1, T2 value2)
		{
			this.Value1 = value1;
			this.Value2 = value2;
		}
	}

	/// <summary>
	/// Для передачи параметра типа T1, T2
	/// </summary>
	[DebuggerDisplay("{Value1}  {Value2}  {Value3}")]
	public class EventArgs<T1, T2, T3> : EventArgs<T1, T2>
	{
		/// <summary>
		/// Собственно параметр 2
		/// </summary>
		public T3 Value3 { get; protected set; }

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="value1">Передаваемый параметр 1</param>
		/// <param name="value2">Передаваемый параметр 2</param>
		/// <param name="value3">Передаваемый параметр 2</param>
		public EventArgs(T1 value1, T2 value2, T3 value3):base(value1, value2)
		{
			this.Value3 = value3;
		}
	}
}
