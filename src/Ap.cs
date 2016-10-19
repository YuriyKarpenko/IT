using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Permissions;

using IT.Log;

namespace IT
{
	/// <summary>
	/// Основные методы и свойства из класса System.Windows.Forms.Application + некоторые свои
	/// </summary>
	public static class Ap
	{
		//private const string this_ = "Ap";

		/// <summary>
		/// Папка с выполняемым файлом
		/// </summary>
		public static string AppFolder => AppDomain.CurrentDomain.BaseDirectory;

		/// <summary>
		/// Заголовок программы
		/// </summary>
		public static string AppCaption => Ap.GetCache("AppCaption", () => Ap.GetCaption(null, null));

		/// <summary>
		///	Возвращает процесс, исполняемый в домене приложения по умолчанию.В других
		///	доменах приложений это первый исполняемый процесс, который был выполнен методом
		/// System.AppDomain.ExecuteAssembly(System.String).
		/// </summary>
		public static Assembly CurAssembly => Ap.GetCache("CurAssembly", Ap.GetAssembly);

		/// <summary>
		/// Название этого компьютера
		/// </summary>
		public static string HostName => Ap.GetCache("HostName", Dns.GetHostName);

		/// <summary>
		/// IP-адрес этого компьютера
		/// </summary>
		public static System.Net.IPAddress IPAddress => Ap.GetCache<IPAddress>("IPAddress", () =>
			Dns.GetHostEntry(HostName).AddressList.FirstOrDefault(i => i.AddressFamily == AddressFamily.InterNetwork) ?? IPAddress.None);

		/// <summary>
		/// IP-адрес v6 этого компьютера
		/// </summary>
		public static System.Net.IPAddress IPAddressV6 => Ap.GetCache<IPAddress>("IPAddress", () =>
			Dns.GetHostEntry(HostName).AddressList.FirstOrDefault(i => i.AddressFamily == AddressFamily.InterNetworkV6) ?? IPAddress.None);


		#region properties from extentions

		/// <summary>
		/// Название программы
		/// </summary>
		public static string ProductName => Ap.GetCacheA("ProductName", Assembly_Extention.GetProductName);

		/// <summary>
		/// Название программы альтернативное
		/// </summary>
		public static string TitleName => Ap.GetCacheA("TitleName", Assembly_Extention.GetTitleAttribute);

		/// <summary>
		/// 
		/// </summary>
		public static FileVersionInfo AppFileVersionInfo => Ap.GetCacheA("AppFileVersionInfo", Assembly_Extention.GetAppFileVersionInfo);

		/// <summary>
		/// Версия программы
		/// </summary>
		public static string StrProductVersion => Ap.GetCacheA("StrProductVersion", Assembly_Extention.GetStrProductVersion);

		/// <summary>
		/// 
		/// </summary>
		public static string ExecutablePath => Ap.GetCacheA("ExecutablePath", a =>
				{
					string path = a.GetExecutablePath();

					if (new Uri(path).Scheme == "file")
						new FileIOPermission(FileIOPermissionAccess.PathDiscovery, path).Demand();

					return path;
				});

		#endregion



		/// <summary>
		/// Версия программы
		/// </summary>
		public static Version ProductVersion
		{
			get
			{
				try
				{
					return Version.Parse(Ap.StrProductVersion);
				}
				catch (Exception ex)
				{
					Trace.TraceError("IT.Ap.ProductVersion_get()\n{0}", ex);
				}

				return null;
			}
		}


		private static Assembly GetAssembly()
		{
			//return Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
			var a1 = Assembly.GetAssembly(typeof(Ap));
			var a2 = Assembly.GetCallingAssembly();
			var a3 = Assembly.GetEntryAssembly();
			var a4 = Assembly.GetExecutingAssembly();

			return a3 ?? a2 ?? a4 ?? a1;
		}

		private static string GetCaption(string captopn, string version)
		{
			try
			{
				if (string.IsNullOrEmpty(captopn))
				{
					captopn = Ap.TitleName;
				}

				if (string.IsNullOrEmpty(captopn))
				{
					captopn = Ap.ProductName;
				}

				if (string.IsNullOrEmpty(version))
				{
					version = Ap.StrProductVersion;
				}
			}
			catch (Exception ex)
			{
				Logger.ToLogFmt(null, TraceLevel.Error, ex, "()");
				throw;
			}

			return string.Format("{0} i{1}", captopn, version);
		}


		#region cache

		private static object lockCache = new object();
		private static System.Collections.Hashtable _cache = new System.Collections.Hashtable();

		private static T GetCache<T>(string key, Func<T> getValue) where T : class
		{
			var k = Ap.CreateKey(key);

			if (Ap._cache.ContainsKey(k))
			{
				return (T)Ap._cache[k];
			}

			lock (Ap.lockCache)
			{
				T v = getValue();
				Ap._cache[k] = v;
				return v;
			}
		}
		private static T GetCacheA<T>(string key, Func<Assembly, T> getValue) where T : class
		{
			return GetCache<T>(key, () => Assembly_Extention.UsingAssembly(getValue));
		}

		private static string CreateKey(string key)
		{
			return string.Format("{0} {1}", key, System.Threading.Thread.CurrentThread.ManagedThreadId);
		}

		#endregion

	}
}
