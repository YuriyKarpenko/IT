using System;

namespace IT.WPF.JetBrains
{
	/// <summary>
	/// Factory to create a <see cref="T:DataGridExtensions.SimpleContentFilter" /></summary>
	public class SimpleContentFilterFactory : IContentFilterFactory
	{
		/// <summary>Gets or sets the string comparison.</summary>
		public StringComparison StringComparison { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:DataGridExtensions.SimpleContentFilterFactory" /> class.
		/// </summary>
		public SimpleContentFilterFactory()
		  : this(StringComparison.CurrentCultureIgnoreCase)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:DataGridExtensions.SimpleContentFilterFactory" /> class.
		/// </summary>
		/// <param name="stringComparison">The string comparison to use.</param>
		public SimpleContentFilterFactory(StringComparison stringComparison)
		{
			this.StringComparison = stringComparison;
		}

		/// <summary>Creates the content filter for the specified content.</summary>
		/// <param name="content">The content to create the filter for.</param>
		/// <returns>The new filter.</returns>
		/// <ensures inheritedFrom="M:DataGridExtensions.IContentFilterFactory.Create(System.Object)" inheritedFromTypeName="IContentFilterFactory" csharp="Contract.Result&lt;DataGridExtensions.IContentFilter&gt;() != null" vb="Contract.Result(Of DataGridExtensions.IContentFilter)() &lt;&gt; Nothing">result != null</ensures>
		public IContentFilter Create(object content)
		{
			if (content == null)
				throw new ArgumentNullException("content");
			return (IContentFilter)new SimpleContentFilter(content.ToString(), this.StringComparison);
		}
	}
}
