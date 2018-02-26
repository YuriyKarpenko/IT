namespace IT.WPF.JetBrains
{
	/// <summary>
	/// Interface to be implemented by a content filter factory.
	/// </summary>
	public interface IContentFilterFactory
	{
		/// <summary>Creates the content filter for the specified content.</summary>
		/// <param name="content">The content to create the filter for.</param>
		/// <returns>The new filter.</returns>
		/// <ensures csharp="Contract.Result&lt;DataGridExtensions.IContentFilter&gt;() != null" vb="Contract.Result(Of DataGridExtensions.IContentFilter)() &lt;&gt; Nothing">result != null</ensures>
		IContentFilter Create(object content);
	}
}
