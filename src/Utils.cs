using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;

using IT.Log;

namespace IT
{
	/// <summary>
	/// Полезные утилиты
	/// </summary>
	public static class Utils
	{
		/// <summary>
		/// Сравнивает версию (диапазон версий) с верией клиента
		/// </summary>
		/// <param name="workingVersion"></param>
		/// <param name="clientVersion"></param>
		/// <returns></returns>
		public static bool CheckVersion(string workingVersion, Version clientVersion)
		{
			try
			{
				var vers = workingVersion.Split(new char[] { '-', ' ', ':' }, StringSplitOptions.RemoveEmptyEntries);

				if (vers.Length > 0)
				{
					var ver0 = new Version(vers[0]);

					switch (vers.Length)
					{
						case 1:
							if (clientVersion == ver0)
							{
								return true;
							}
							break;

						case 2:
							var ver1 = new Version(vers[1]);
							if (clientVersion >= ver0 && clientVersion <= ver1)
							{
								return true;
							}
							break;
					}

					return false;
				}

				return true;
			}
			catch (Exception ex)
			{
				Logger.ToLogFmt(null, TraceLevel.Error, ex, "()");
				throw;
			}
		}

#if !SILVERLIGHT
		/// <summary>
		/// Выполняет filePath из workDir с параметрами args
		/// </summary>
		/// <param name="filePath">Запускаемый файл</param>
		/// <param name="workDir">Рабочая папка (если надо)</param>
		/// <param name="args">Параметры командной строки (если надо)</param>
		/// <returns></returns>
		public static Process Execute(string filePath, string workDir = null, string args = null)
		{
			Logger.ToLogFmt(null, TraceLevel.Verbose, null, "()");
			try
			{
				string curDir = string.Empty;

				if (!string.IsNullOrEmpty(workDir))
				{
					curDir = Environment.CurrentDirectory;
					Environment.CurrentDirectory = workDir;
				}

				var p = Process.Start(filePath, args);

				if(!string.IsNullOrEmpty(curDir))
					Environment.CurrentDirectory = curDir;

				return p;
			}
			catch (Exception ex)
			{
				Logger.ToLogFmt(null, TraceLevel.Error, ex, "()");
				throw;
			}
		}

		/// <summary>
		/// Пытается создать ActiveX объект по его appId
		/// </summary>
		/// <param name="appId">Зарегистрированное имя ActiveX</param>
		/// <param name="registerPath">Путь к файлу (.bat) регристрации ActiveX</param>
		/// <returns></returns>
		public static dynamic CreateComObject(string appId, string registerPath)
		{
			Logger.ToLogFmt(null, TraceLevel.Verbose, null, "()");
			try
			{
				var type = Type.GetTypeFromProgID("IT.Address", false);

				if (type == null && File.Exists(registerPath))
				{
					var p = Execute(registerPath, Path.GetDirectoryName(registerPath));
					p.WaitForExit();
					type = Type.GetTypeFromProgID("IT.Address", false);
				}

				if (type != null)
				{
					dynamic app = Activator.CreateInstance(type);
					return app;
				}
			}
			catch (Exception ex)
			{
				Logger.ToLogFmt(null, TraceLevel.Error, ex, "()");
			}

			return null;
		}
#endif

		/// <summary>
		/// Если файл присутствует, создает файл вида : [name]_bak[i].[ext] посредством увеличения [i]
		/// </summary>
		/// <param name="filePath">Проверяемый путь</param>
		public static void CreateBakFile(string filePath)
		{
			if (File.Exists(filePath))
			{
				var dir = Path.GetDirectoryName(filePath);
				var ext = Path.GetExtension(filePath);
				var name = Path.GetFileNameWithoutExtension(filePath);
				int i = 0;
				string newPath;

				do
					newPath = Path.Combine(dir, string.Format("{0}_bak{1}.{2}", name, i++, ext));
				while (File.Exists(newPath));

				File.Move(filePath, newPath);
			}
		}

		#region Crypt

		private static class CriptHelper
		{
			const int cStart = 456, cMult = 123, cAdd = 789; // добавочные константы для ключей шифрования (увеличивают варинтность шифра)
			public static long StartKeyDD, MultKeyHH, AddKeyMM; // вычисляемы ключи шифрования

			public static void Reset()
			{
				// берём как ключи: дату часы минуты
				StartKeyDD = DateTime.Now.Day + cStart;
				MultKeyHH = DateTime.Now.Hour + cMult;
				AddKeyMM = DateTime.Now.Minute + cAdd;
			}

			public static string Str2Asc(string s) // конвертация текста в ASCII последовательность кодов
			{
				string res = "";

				if (s.Length > 0)
				{
					foreach (var c in s)
					{
						var cc = ((byte)c).ToString();
						res += cc.PadLeft(3, '0');
					}
				}
				return res;
			}

			public static string Asc2Str(string s) // обратная конвертация ASCII последовательности кодов в текст
			{
				string res = "";
				for (int i = 0; i < s.Length; i += 3)
				{
					var sb = s.Substring(i, 3);
					res += (char)byte.Parse(sb);
				}
				return res;
			}
		}

		/// <summary>
		/// Шифрует параметры запуска для программ "Центра запуска"
		/// </summary>
		/// <param name="InString"></param>
		/// <returns></returns>
		public static string Encrypt(string InString)// шифрование
		{
			CriptHelper.Reset();//SetCryptKeys;
			string res = "";
			uint sum = 0;
			foreach (var c in InString)
			{
				var b1 = (byte)c;
				var b2 = (byte)(CriptHelper.StartKeyDD >> 8);
				var c1 = (char)(b1 ^ b2);
				res += c1;
				CriptHelper.StartKeyDD = ((byte)c1 + CriptHelper.StartKeyDD) * CriptHelper.MultKeyHH + CriptHelper.AddKeyMM;
				sum += (byte)c1;   // подсчёт КС
			}

			return CriptHelper.Str2Asc((char)((byte)sum) + res); // добавляем в начало младший байт КС
		}

		/// <summary>
		/// Дешифрует параметры запуска, полученные из "Центра запуска"
		/// </summary>
		/// <param name="InString"></param>
		/// <returns></returns>
		public static bool Decrypt(ref string InString) // дешифрование
		{
			CriptHelper.Reset();
			var MM = CriptHelper.AddKeyMM;
			do
			{ // цикл для текущей и предыдущей минуты
				string OutS = "";
				ushort sum = 0;
				var InS = InString;
				try
				{
					var sb = InS.Substring(0, 3);
					byte LoSum = byte.Parse(sb);
					InS = InS.Remove(0, 3); // вырезаем младший байт КС результата
					InS = CriptHelper.Asc2Str(InS);
					foreach (var c in InS)
					{
						var b1 = (byte)c;
						var b2 = (byte)(CriptHelper.StartKeyDD >> 8);
						var cc = (char)(b1 ^ b2);
						OutS += cc;
						CriptHelper.StartKeyDD = ((byte)c + CriptHelper.StartKeyDD) * CriptHelper.MultKeyHH + CriptHelper.AddKeyMM;
						sum += (byte)c;   // подсчёт КС
					}
					InString = OutS;

					if (((byte)sum) == (byte)LoSum)  // проверка конторльной суммы!
					{
						return true;	// выход при успешном дешифровании
					}
				}
				catch
				{
				}

				CriptHelper.AddKeyMM--; // попробуем предыдущую минуту
			}
			while ((MM - CriptHelper.AddKeyMM) < 2);
			InString = "";
			return false; // выход при неудаче
		}

#if !SILVERLIGHT
		/// <summary>
		/// Дешифрует параметры запуска, полученные из "Центра запуска" в виде XML
		/// </summary>
		/// <param name="xmlContent"></param>
		/// <returns></returns>
		public static XElement DecryptXML(string xmlContent) // дешифрование
		{
			Contract.NotIsNullOrEmpty(xmlContent, "xmlContent");

			var s = xmlContent;

			var m = Regex.Match(s, "\\d+");
			if (s.Length == m.ToString().Length)
			{
				Decrypt(ref s);
			}

			var ss = new StringReader(s);
			var res = XElement.Load(ss);
			return res;
		}

		//public class 

#endif

		/// <summary>
		/// Проверка на шифрование и (если нодо) дешифрует
		/// </summary>
		/// <param name="login"></param>
		/// <param name="pwd"></param>
		/// <returns></returns>
		public static bool CheckLoginForCrypt(ref string login, ref string pwd)
		{
			var m = Regex.Match(login, "\\d+");

			if (login.Length == m.ToString().Length)
			{
				return Utils.Decrypt(ref login) && Utils.Decrypt(ref pwd);
			}

			return true;
		}

		/// <summary>
		/// Проверяет число параметров (если мало, то throw new Exception("Программа должна запускаться из центра запуска!"))
		/// возврашает результат CheckLoginForCrypt()
		/// </summary>
		/// <param name="args">Входные параметры программы</param>
		/// <param name="login">Готовый к применению</param>
		/// <param name="pwd">Готовый к применению</param>
		/// <returns>Результат CheckLoginForCrypt()</returns>
		public static bool CheckArgsForProgramManager(string[] args, out string login, out string pwd)
		{
			login = pwd = string.Empty;

			if (args.Length > 1)
			{
				login = args[0].ToUpper();
				pwd = args[1];
				return CheckLoginForCrypt(ref login, ref pwd);
			}
			else
				throw new Exception("Программа должна запускаться из центра запуска!");
		}

		///// <summary>
		///// Полный анализ командной строки (следует вынести в отдельный класс)
		///// </summary>
		///// <param name="args"></param>
		///// <param name="CreateService_cfg"></param>
		///// <param name="CreateService_xml"></param>
		///// <returns></returns>
		//public static bool AppArgsAnalize(string[] args,
		//	Action<string/* username*/, string /*password*/> CreateService_cfg,
		//	Action<string/* username*/, string /*password*/, Dictionary<string, Binding> /*dic*/> CreateService_xml)
		//{
		//	XElement xmlArgs = null;
		//	Logger.ToLogFmt(null, LogLevel.Debug, null, "()");
		//	try
		//	{
		//		switch (args.Length)
		//		{
		//			case 1:
		//				try
		//				{
		//					var s = args[0];
		//					xmlArgs = Utils.DecryptXML(s);
		//					if (xmlArgs != null && CreateService_xml != null)
		//					{
		//						var dic = IT.Server.ItClientBase.EndPointsFromXml(xmlArgs).ToDictionary(i => i.Key.Address.AbsoluteUri, i => i.Value);
		//						CreateService_xml(xmlArgs.Element("username").Value, xmlArgs.Element("password").Value, dic);
		//					}
		//					return true;
		//				}
		//				catch { }
		//				break;

		//			case 2:
		//				try
		//				{
		//					if (CreateService_cfg != null)
		//					{
		//						string userName, password;
		//						Utils.CheckArgsForProgramManager(args, out userName, out password);
		//						CreateService_cfg(userName, password);
		//					}
		//					return true;
		//				}
		//				catch { }
		//				break;
		//		}
		//		throw new ArgumentException("Программа должна запускаться из 'центра запуска'!");
		//	}
		//	catch (Exception ex)
		//	{
		//		Logger.ToLogFmt(null, LogLevel.Error, ex, "()");
		//		throw;
		//	}
		//}

		#endregion

		/// <summary>
		/// Формирование строки размера (файла)
		/// </summary>
		/// <param name="valueBytes"></param>
		/// <param name="criteriaCompare"></param>
		/// <param name="precision"></param>
		/// <returns></returns>
		public static string SizeToString(long valueBytes, int criteriaCompare = 950, int precision = 1)
		{
			decimal res = valueBytes;
			var unit = "B";

			if(res > criteriaCompare)
			{
				res = res / 1024;
				unit = "Kb";
			}

			if(res > criteriaCompare)
			{
				res = res / 1024;
				unit = "Mb";
			}

			if(res > criteriaCompare)
			{
				res = res / 1024;
				unit = "Tb";
			}

			return string.Format("{0:n" + precision + "} {1}", res, unit);
		}
		
		/// <summary>
		/// Преобразование размера в байтах из строки размера (из SizeToString)
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static long SizeFromString(string value)
		{
			var sSize = value
				.ToLowerInvariant()
				//.Replace(" ", "")
				;
			var sUnit = "";

			var m = Regex.Match(value, @"(?<num>[\d\s\,\.]+)\s+(?<unit>\D+)");
			if (m != null && m.Groups.Count > 1)
			{
				sSize = m.Groups["num"].Value;
				sUnit = m.Groups["unit"].Value;
			}

			var res = sSize.To<decimal>(0);
			switch (sUnit[0])
			{
				//case 'b':
				//	this.Size = (long)res;
				//	break;
				case 'k':
					res = res * 1024;
					break;
				case 'm':
					res = res * 1024 * 1024;
					break;
				case 't':
					res = res * 1024 * 1024 * 1024;
					break;
			}

			return (long)res;
		}


		public static void UsingExclusive(Action act)
		{
			if (Monitor.TryEnter(act, 1))
			try
			{
				act();
			}
			finally
			{
				Monitor.Exit(act);
			}
		}

	}

	/// <summary>
	/// Методы для упрощения работы с отражением
	/// </summary>
	public class UtilsReflection
	{ 

		/// <summary>
		/// Получает значение static поля (в том числе сложного)
		/// </summary>
		/// <param name="typ">Тип-владелец статического поля</param>
		/// <param name="propertyPath">Название свойства или путь</param>
		/// <returns></returns>
		public static object GetFieldValue(Type typ, string propertyPath)
		{
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(propertyPath), "propertyPath");
			if (typ != null)
			{
				var pName = propertyPath.Split('.')[0];
				var fi = typ.GetField(pName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				if (fi != null)
				{
					var o = fi.GetValue(null);

					if (pName == propertyPath)
					{
						return o;
					}
					else
					{
						return GetFieldValue(o.GetType(), propertyPath.Substring(pName.Length + 1));
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Получает значение поля (в том числе сложного)
		/// </summary>
		/// <param name="obj">Расширяемый объект</param>
		/// <param name="propertyPath">Название свойства или путь</param>
		/// <returns></returns>
		public static object GetFieldValue(object obj, string propertyPath)
		{
			Contract.NotIsNullOrEmpty(propertyPath, "propertyPath");
			if (obj != null)
			{
				var pName = propertyPath.Split('.')[0];
				var fi = obj.GetType().GetField(pName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				if (fi != null)
				{
					var o = fi.GetValue(obj);

					if (pName == propertyPath)
					{
						return o;
					}
					else
					{
						return GetFieldValue(o, propertyPath.Substring(pName.Length + 1));
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Получает значение свойства (в том числе сложного)
		/// </summary>
		/// <param name="obj">Расширяемый объект</param>
		/// <param name="propertyPath">Название свойства или путь</param>
		public static object GetPropertyValue(object obj, string propertyPath)
		{
			Contract.NotIsNullOrEmpty(propertyPath, "propertyPath");
			if (obj != null)
			{
				var pName = propertyPath.Split('.')[0];
				var pi = obj.GetType().GetProperty(pName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				if (pi != null)
				{
					var o = pi.GetValue(obj, null);

					if (pName == propertyPath)
					{
						return o;
					}
					else
					{
						return GetPropertyValue(o, propertyPath.Substring(pName.Length + 1));
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Устанавливаен значение свойства (в том числе сложного)
		/// </summary>
		/// <param name="obj">Расширяемый объект</param>
		/// <param name="propertyPath">Название свойства или путь</param>
		/// <param name="value">Знсчение</param>
		public static void SetPropertyValue(object obj, string propertyPath, object value)
		{
			Contract.NotIsNullOrEmpty(propertyPath, "propertyPath");
			if (obj != null)
			{
				var pName = propertyPath.Split('.')[0];
				var pi = obj.GetType().GetProperty(pName);
				if (pi != null)
				{
					if (pName == propertyPath)
					{
						pi.SetValue(obj, value, null);
					}
					else
					{
						var o = pi.GetValue(obj, null);
						SetPropertyValue(o, propertyPath.Substring(pName.Length + 1), value);
					}
				}
			}
		}


		/// <summary>
		/// Выполняет статический метод из типа typ
		/// </summary>
		/// <typeparam name="T">Возвращаемое методом значение</typeparam>
		/// <param name="typ">Расширяемый тип</param>
		/// <param name="methodName">Название метода</param>
		/// <param name="args">Параметры метода (при использовании параметров out следует передавать реальный массив и потом из него читоать нужные элементы)</param>
		/// <returns></returns>
		public static T ExecStaticMethod<T>(Type typ, string methodName, params object[] args) //where T : struct
		{
			Contract.NotIsNullOrEmpty(methodName, "methodName");
			Type t = typ;
			var mm = t.GetMethods().Where(i => i.Name == methodName && i.GetParameters().Length == args.Length).ToArray();
			//var methodInfo = t.GetMethod(methodName);
			if (mm != null && mm.Length > 0)
			{
				MethodInfo methodInfo = mm[0];

				if (methodInfo?.IsStatic ?? false)
				{
					return (T)methodInfo.Invoke(null, args);
				}
			}
			return default(T);
		}

		/// <summary>
		/// Выполняет метод
		/// </summary>
		/// <param name="obj">Расширяемый объект</param>
		/// <param name="methodName">Название емтода</param>
		/// <param name="args">Параметры метода</param>
		/// <returns></returns>
		public static object ExecMethod(object obj, string methodName, params object[] args)
		{
			Contract.NotIsNullOrEmpty(methodName, "methodName");
			if (obj != null)
			{
				var mi = obj.GetType().GetMethod(methodName);
				if (mi != null)
				{
					return mi.Invoke(obj, args);
				}
			}
			return null;
		}

		/// <summary>
		/// Выполняет метод
		/// </summary>
		/// <typeparam name="T">Тип результата</typeparam>
		/// <param name="obj">Расширяемый объект</param>
		/// <param name="methodName">Название емтода</param>
		/// <param name="args">Параметры метода</param>
		/// <returns></returns>
		public static T ExecMethod<T>(object obj, string methodName, params object[] args)
		{
			Contract.NotIsNullOrEmpty(methodName, "methodName");
			if (obj != null)
			{
				var mi = obj.GetType().GetMethod(methodName);
				if (mi != null)
				{
					return (T)mi.Invoke(obj, args);
				}
			}
			return default(T);
		}



#if !SILVERLIGHT
		/// <summary>
		/// Пытается заполнить свойства переданного объекта собственными значениями соответствующих свойств
		/// используя PropertyDescription (уиитываются атрибуты : ReadOnly(), ...)
		/// </summary>
		/// <param name="source">Расширяемый экземпляр</param>
		/// <param name="dest">Объект для заполнения соответствующих свойств</param>
		/// <param name="isIcgnoreCase">Параметр сравнения наименованй свойств</param>
		/// <returns>Объект с заполнеными соответствующими свойствами</returns>
		public static object ClonePropertyTo_PD(object source, object dest, bool isIcgnoreCase = false)
		{
			try
			{
				var pds = TypeDescriptor.GetProperties(dest);
				var sourPds = TypeDescriptor.GetProperties(source);

				foreach (PropertyDescriptor pd in pds)
				{
					if (!pd.IsReadOnly)
					{
						var sourPd = sourPds.Find(pd.Name, isIcgnoreCase);

						if (sourPd != null)
						{
							var o = sourPd.GetValue(source);
							pd.SetValue(dest, o);
						}
					}
				}

				return dest;
			}
			catch (Exception ex)
			{
				Logger.ToLogFmt(null, TraceLevel.Error, ex, "()");
				throw;
			}
		}
#endif

		/// <summary>
		/// Пытается заполнить свойства переданного объекта значениями source соответствующих свойств используя PropertyInfo
		/// (без рекурсии, т.е. сложные объекты могут иметь одинаковые ссылки на свойства) 
		/// </summary>
		/// <param name="source">Расширяемый экземпляр</param>
		/// <param name="dest">Объект для заполнения соответствующих свойств</param>
		/// <param name="isIcgnoreCase">Параметр сравнения наименованй свойств</param>
		/// <returns>Объект с заполнеными соответствующими свойствами</returns>
		public static object ClonePropertyTo(object source, object dest, bool isIcgnoreCase = false)
		{
			Contract.NotNull(source, "source");
			Contract.NotNull(dest, "dest");

			try
			{
				var t1 = source.GetType();
				var t2 = dest.GetType();

				var param = BindingFlags.Default // = 0
					//| BindingFlags.DeclaredOnly	//	= 2	Наследуемые члены не учитываются.
					| BindingFlags.Instance	//	= 4
					//| BindingFlags.Static	//	= 8
					| BindingFlags.Public	//	= 16
					//| BindingFlags.NonPublic	//	= 32
					//| BindingFlags.SetField	//	= 2048
					//| BindingFlags.SetProperty	//	= 4096
				;

				if (isIcgnoreCase)
					param |= BindingFlags.IgnoreCase;	//	= 1

				var pis2 = t2.GetProperties(param);

				foreach (var pi2 in pis2)
				{
					var pi1 = t1.GetProperty(pi2.Name);

					if (pi1 != null && pi1.CanRead && pi2.CanWrite)
					{
						var o1 = pi1.GetValue(source, null);

						if (o1 is ICloneable)
							pi2.SetValue(dest, (o1 as ICloneable).Clone(), null);
						else
							pi2.SetValue(dest, o1, null);
					}
				}

				return dest;
			}
			catch (Exception ex)
			{
				Log.Logger.ToLog(source, TraceLevel.Warning, "ClonePropertyTo()", ex);
				throw;
			}
		}

		/// <summary>
		/// Клонирует заданный объект используя указанный конструктор, так же учитывает ICloneable
		/// (внетренние объекты должны иметь конструкторы без параметров!)
		/// </summary>
		/// <param name="source"></param>
		/// <param name="constructor"></param>
		/// <returns></returns>
		public static object CloneObject(object source, Func<object> constructor)
		{
			Contract.NotNull(source, "source");
			Contract.NotNull(constructor, "constructor");
			try
			{
				var t = source.GetType();
				if (typeof(ICloneable).IsAssignableFrom(t))
				{
					var c1 = source as ICloneable;
					var c = c1.Clone();
					return c;
				}

				var dest = constructor();
				var pis = t.GetProperties();
				foreach (var pi in pis)
				{
					if (pi != null && pi.CanRead && pi.CanWrite)
					{
						var o1 = pi.GetValue(source, null);

						if (pi.PropertyType.IsClass && pi.PropertyType != typeof(string))
						{
							var o2 = CloneObject(o1, () => Activator.CreateInstance(pi.PropertyType));
							o1 = o2;
						}

						pi.SetValue(dest, o1, null);
					}
				}

				return dest;
			}
			catch (Exception ex)
			{
				Log.Logger.ToLog(source, TraceLevel.Warning, "CloneObject()", ex);
				throw;
			}
		}

		/// <summary>
		/// Клонирует заданный объект используя конструктор без параметров
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <returns></returns>
		public static T CloneObject<T>(T source)
		{
			return (T)CloneObject(source, () => Activator.CreateInstance<T>());
		}
	}

}
