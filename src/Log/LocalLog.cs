namespace System
{
	/// <summary>
	/// 
	/// </summary>
	public static class LocalLog
	{
		private static string GetFileName()
		{
			return string.Format("{0}\\{1}.log", AppDomain.CurrentDomain.BaseDirectory, "internal");
		}

		private static string GetMessage(string formatString, params object[] args)
		{ 
			string msg = (args == null || args.Length == 0) ? formatString : string.Format(formatString, args);
			return string.Format("\r\n{0}\t{1}", DateTime.Now, msg);
		}

		private static void Write(string s)
		{
			try
			{
				System.Diagnostics.Trace.Write(s);
				System.Diagnostics.Debug.Write(s);
				System.IO.File.AppendAllText(GetFileName(), s);
			}
			catch { }
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="ex"></param>
		/// <param name="formatString"></param>
		/// <param name="args"></param>
		public static void Error(Exception ex, string formatString, params object[] args)
		{
			Write(string.Format("\r\n{0}\t{1}", GetMessage(formatString, args), ex));
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="formatString"></param>
		/// <param name="args"></param>
		public static void Info(string formatString, params object[] args)
		{
			Write(GetMessage(formatString, args));
		}
	}
}
