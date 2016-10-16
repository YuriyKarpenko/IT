using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading;

using IT.Log;

namespace IT
{
	/// <summary>
	/// Скриализатор в/из вайлов фрмата json
	/// </summary>
	public static class Serializer_Json
	{
		const string JsonExtension = ".json";

		/// <summary>
		/// изначально известные сложные типы
		/// </summary>
		public static readonly List<Type> KnownTypes = new List<Type>
        {
            typeof (Type),
            //typeof (Dictionary<string, string>),
			//typeof (SolidColorBrush),
			//typeof (MatrixTransform),
        };

		/// <summary>
		/// Метод сериализации в поток
		/// </summary>
		/// <param name="item"></param>
		/// <param name="stream"></param>
		/// <param name="knownTypes"></param>
		public static void SerializeDataContract(object item, Stream stream, params Type[] knownTypes)
		{
			Logger.ToLogFmt(null, TraceLevel.Verbose, null, "({0}, {1})", item, knownTypes);
			try
			{
				var type = item.GetType();
				UsingSerializer(type, serializer => serializer.WriteObject(stream, item), knownTypes);
			}
			catch (Exception ex)
			{
				Logger.ToLogFmt(null, TraceLevel.Error, ex, "({0}, {1})", item, knownTypes);
			}
		}

		/// <summary>
		/// Сериализация посредством DataContractJsonSerializer
		/// </summary>
		/// <param name="item"></param>
		/// <param name="file"></param>
		/// <param name="knownTypes"></param>
		public static void Serialize_ToFile(this object item, string file = null, params Type[] knownTypes)
		{
			Logger.ToLogFmt(null, TraceLevel.Verbose, null, "({0}, {1})", item, file);
			try
			{
				if (string.IsNullOrEmpty(file))
					file = item.GetType().Name + JsonExtension;
				using (var stream = File.Create(file))
				{
					SerializeDataContract(item, stream, knownTypes);
				}
			}
			catch (Exception ex)
			{
				Logger.ToLogFmt(null, TraceLevel.Error, ex, "({0}, {1})", item, file);
			}
		}
		/// <summary>
		/// Сериализация посредством DataContractJsonSerializer
		/// </summary>
		/// <param name="item"></param>
		/// <param name="knownTypes"></param>
		/// <returns></returns>
		public static Stream Serialize_ToStream(this object item, params Type[] knownTypes)
		{
			Logger.ToLogFmt(null, TraceLevel.Verbose, null, "({0}, {1})", item, knownTypes);
			try
			{
				var stream = new MemoryStream();
				SerializeDataContract(item, stream, knownTypes);
				stream.Position = 0;
				return stream;
			}
			catch (Exception ex)
			{
				Logger.ToLogFmt(null, TraceLevel.Error, ex, "({0}, {1})", item, knownTypes);
			}

			return null;
		}

		/// <summary>
		/// Метод десериализации из файла
		/// </summary>
		/// <typeparam name="R"></typeparam>
		/// <param name="stream"></param>
		/// <param name="knownTypes"></param>
		/// <returns></returns>
		public static R Deserialize<R>(Stream stream, params Type[] knownTypes)
		{
			Logger.ToLogFmt(null, TraceLevel.Verbose, null, "(, {0})", typeof(R));
			try
			{
				object o = null;
				UsingSerializer(typeof(R), serializer => o = serializer.ReadObject(stream), knownTypes);
				return (R)o;
			}
			catch (Exception ex)
			{
				Logger.ToLogFmt(null, TraceLevel.Error, ex, "(, {0})", typeof(R));
			}
			return default(R);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="R"></typeparam>
		/// <param name="file"></param>
		/// <param name="knownTypes"></param>
		/// <returns></returns>
		public static R Deserialize<R>(string file = null, params Type[] knownTypes)
		{
			Logger.ToLogFmt(null, TraceLevel.Verbose, null, "({0}, {1})", file, typeof(R));
			try
			{
				if (string.IsNullOrEmpty(file))
					file = typeof(R).Name + JsonExtension;
				using (var stream = File.OpenRead(file))
				{
					var item = Deserialize<R>(stream, knownTypes);
					return item;
				}
			}
			catch (Exception ex)
			{
				Logger.ToLogFmt(null, TraceLevel.Error, ex, "({0}, {1})", file, typeof(R));
			}
			return default(R);
		}


		private static void UsingSerializer(Type type, Action<DataContractJsonSerializer> act, params Type[] knownTypes)
		{
			var currentCulture = Thread.CurrentThread.CurrentCulture;
			try
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
				var serializer = GetSerializer(type, knownTypes);
				act(serializer);
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = currentCulture;
			}
		}

		private static DataContractJsonSerializer GetSerializer(Type type, params Type[] knownTypes)
		{
			var types = KnownTypes
				.Concat(knownTypes)
				.ToArray();
			var serializer = new DataContractJsonSerializer(type, types);
			return serializer;
		}
	}
}
