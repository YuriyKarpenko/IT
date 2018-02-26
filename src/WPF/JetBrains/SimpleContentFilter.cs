using System;
using System.Diagnostics.Contracts;

namespace IT.WPF.JetBrains
{
	/// <summary>
	/// A content filter using a simple "contains" string comparison to match the content and the value.
	/// </summary>
	/// <invariant>_content != null</invariant>
	public class SimpleContentFilter : IContentFilter
	{
		private readonly string _content;
		private readonly StringComparison _stringComparison;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:DataGridExtensions.SimpleContentFilter" /> class.
		/// </summary>
		/// <param name="content">The content.</param>
		/// <param name="stringComparison">The string comparison.</param>
		/// <requires csharp="content != null" vb="content &lt;&gt; Nothing">content != null</requires>
		public SimpleContentFilter(string content, StringComparison stringComparison)
		{
			// ISSUE: reference to a compiler-generated method
			__ContractsRuntime.Requires(content != null, (string)null, "content != null");
			// ISSUE: explicit constructor call
			this._content = content;
			this._stringComparison = stringComparison;
		}

		/// <summary>
		/// Determines whether the specified value matches the condition of this filter.
		/// </summary>
		/// <param name="value">The content.</param>
		/// <returns>
		/// <c>true</c> if the specified value matches the condition; otherwise, <c>false</c>.
		///     </returns>
		public bool IsMatch(object value)
		{
			if (value == null)
				return false;
			return value.ToString().IndexOf(this._content, this._stringComparison) >= 0;
		}
	}
}
