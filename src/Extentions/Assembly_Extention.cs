using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;

using IT.Log;

namespace IT
{
	/// <summary>
	/// Extention methods for Assembly
	/// </summary>
	public static class Assembly_Extention
	{
		/// <summary>
		/// Получение всех TAttribute из атрибутов сборки
		/// </summary>
		/// <param name="asm"></param>
		/// <returns></returns>
		public static TAttribute[] GetCustomAttributes<TAttribute>(this Assembly asm) where TAttribute : class
		{
			// <param name="inherit">This argument is ignored for objects of type System.Reflection.Assembly</param>
			return CheckAssembly(asm, a => (TAttribute[])a.GetCustomAttributes(typeof(TAttribute), false), "GetCustomAttributes");
		}

		/// <summary>
		/// Получение TAttribute из атрибутов сборки
		/// </summary>
		/// <param name="asm"></param>
		/// <returns></returns>
		public static TAttribute GetCustomAttribute<TAttribute>(this Assembly asm) where TAttribute : class
		{
			return CheckAssembly(asm, a => a.GetCustomAttributes<TAttribute>()?.FirstOrDefault(), "GetCustomAttribute");
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="asm"></param>
		/// <returns></returns>
		public static FileVersionInfo GetAppFileVersionInfo(this Assembly asm)
		{
			return CheckAssembly(asm, a =>
			{
				Type typ = a.EntryPoint?.ReflectedType;
				Logger.ToLogFmt(null, TraceLevel.Verbose, null, "() {0}, type = {1}", a, typ);
				if (typ != null)
				{
					new FileIOPermission(PermissionState.None)
					{
						AllFiles = (FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery)
					}.Assert();

					try
					{
						return FileVersionInfo.GetVersionInfo(typ.Module.FullyQualifiedName);
					}
					finally
					{
						System.Security.CodeAccessPermission.RevertAssert();
					}
				}
				else
				{
					return FileVersionInfo.GetVersionInfo(a.GetExecutablePath());
				}
			}, "GetAppFileVersionInfo");
		}

		/// <summary>
		/// Возвращает значение атрибута из AssemblyInfo.cs
		/// </summary>
		/// <param name="asm"></param>
		/// <returns></returns>
		public static string GetInformationalVersionAttribute(this Assembly asm)
		{
			return CheckAssembly(asm, 
				a => a.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "", 
				"GetInformationalVersionAttribute");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="asm"></param>
		/// <returns></returns>
		public static string GetExecutablePath(this Assembly asm)
		{
			return CheckAssembly(asm, a =>
			{
				Uri uri = new Uri(a.CodeBase);
				return uri.IsFile ? uri.LocalPath + Uri.UnescapeDataString(uri.Fragment) : uri.ToString();
			}, "GetExecutablePath");

			//return null;
		}

		/// <summary>
		/// Возвращает значение атрибута из AssemblyInfo.cs
		/// </summary>
		/// <param name="asm"></param>
		/// <returns></returns>
		public static string GetProductAttribute(this Assembly asm)
		{
			return CheckAssembly(asm, a => a.GetCustomAttribute<AssemblyProductAttribute>()?.Product, "GetProductAttribute");
		}

		/// <summary>
		/// Возвращает значение атрибута из AssemblyInfo.cs
		/// </summary>
		/// <param name="asm"></param>
		/// <returns></returns>
		public static string GetTitleAttribute(this Assembly asm)
		{
			return CheckAssembly(asm, a => a.GetCustomAttribute<AssemblyTitleAttribute>().Title, "GetTitleAttribute");
		}

		/// <summary>
		/// Вычисление ProductName 3-мя способами
		/// </summary>
		/// <param name="asm"></param>
		/// <returns></returns>
		public static string GetProductName(this Assembly asm)
		{
			return CheckAssembly(asm, a =>
			{
				var res = a.GetProductAttribute();

				if (string.IsNullOrEmpty(res))
				{
					res = a.GetAppFileVersionInfo().ProductName?.Trim();
				}

				if (string.IsNullOrEmpty(res))
				{
					Type typ = a.EntryPoint?.ReflectedType;
					if (typ != null)
					{
						string ns = typ.Namespace;
						if (!string.IsNullOrEmpty(ns))
						{
							int i = ns.LastIndexOf(".");
							res = i == -1 || i >= ns.Length - 1 ? ns : ns.Substring(i + 1);

							//var ss = ns.Split('.');
							//productName = ss[ss.Length - 1];
						}
						else
						{
							res = typ.Name;
						}
					}
				}

				return res;
			}, "GetProductName");
		}

		/// <summary>
		/// Вычисление Productversion 2-мя способами
		/// </summary>
		/// <param name="asm"></param>
		/// <returns></returns>
		public static string GetStrProductVersion(this Assembly asm)
		{
			return CheckAssembly(asm, a =>
			{
				var res = a.GetInformationalVersionAttribute();

				if (string.IsNullOrEmpty(res))
				{
					res = a.GetAppFileVersionInfo().ProductVersion?.Trim();
				}

				if (string.IsNullOrEmpty(res))
				{
					res = "1.0.0.0";
				}

				return res;

			}, "GetStrProductVersion");
		}

		/// <summary>
		/// It provides for the use of checked object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="act"></param>
		/// <param name="methodName"></param>
		/// <returns></returns>
		public static T UsingAssembly<T>(Func<Assembly, T> act, string methodName = "UsingAssembly")
		{
			//Logger.ToLogFmt(null, LogLevel.Debug, null, "({0}, ,{1})", a, methodName);	
			return CheckAssembly(Ap.CurAssembly, act, methodName);
		}

		#region cache

		private static object lockCache = new object();
		private static MemCache<string, object> _cache = new MemCache<string, object>();

		private static T GetCache<T>(string key, Func<object, T> getValue, object param = null) where T : class
		{
			var k = CreateKey(key);

			lock (lockCache)
			{
				return (T)_cache[key, () => getValue(param)];
			}
		}
		private static T GetCacheA<T>(string key, Func<Assembly, T> getValue) where T : class
		{
			return GetCache<T>(key, o => UsingAssembly(getValue));
		}

		private static string CreateKey(string key)
		{
			return string.Format("{0} {1}", key, System.Threading.Thread.CurrentThread.ManagedThreadId);
		}

		private static T CheckAssembly<T>(Assembly a, Func<Assembly, T> act, string methodName = "CheckAssembly")
		{
			//Logger.ToLogFmt(null, LogLevel.Debug, null, "({0}, ,{1})", a, methodName);	
			try
			{
				if (a != null)
				{
					return act(a);
				}
			}
			catch (Exception ex)
			{
				Logger.ToLogFmt(null, TraceLevel.Error, ex, "({0})", methodName);
			}

			return default(T);
		}

		#endregion
	}
}
