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
		public static void Serialize_ToFile(object item, string file = null, params Type[] knownTypes)
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
		public static MemoryStream Serialize_ToStream(object item, params Type[] knownTypes)
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
				throw;
			}
			//return null;
		}
		/// <summary>
		/// Сериализация посредством DataContractJsonSerializer
		/// </summary>
		/// <param name="item"></param>
		/// <param name="knownTypes"></param>
		/// <returns></returns>
		public static string Serialize_ToString(object item, params Type[] knownTypes)
		{
			var str = Serialize_ToStream(item, knownTypes);
			if(str?.Length > 0)
			{
				var sr = new StreamReader(str);
				var res = sr.ReadToEnd();
				return res;
			}
			return string.Empty;
		}

		/// <summary>
		/// Метод десериализации из файла
		/// </summary>
		/// <typeparam name="TRes"></typeparam>
		/// <param name="stream"></param>
		/// <param name="knownTypes"></param>
		/// <returns></returns>
		public static TRes Deserialize<TRes>(Stream stream, params Type[] knownTypes)
		{
			Logger.ToLogFmt(null, TraceLevel.Verbose, null, "(, {0})", typeof(TRes));
			try
			{
				object o = null;
				UsingSerializer(typeof(TRes), serializer => o = serializer.ReadObject(stream), knownTypes);
				return (TRes)o;
			}
			catch (Exception ex)
			{
				Logger.ToLogFmt(null, TraceLevel.Error, ex, "(, {0})", typeof(TRes));
				throw;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TRes"></typeparam>
		/// <param name="content"></param>
		/// <param name="knownTypes"></param>
		/// <returns></returns>
		public static TRes Deserialize<TRes>(string content, params Type[] knownTypes)
		{
			if (string.IsNullOrEmpty(content))
			{
				return default(TRes);
			}
			else
			{
				var bb = System.Text.Encoding.UTF8.GetBytes(content);
				var str = new MemoryStream(bb);
				return Deserialize<TRes>(str, knownTypes);
			}
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
