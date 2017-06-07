using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace IT
{
	/// <summary>
	/// Сериализация объектов
	/// </summary>
	public class Serializer
	{
		private static IFormatter binFmt = new BinaryFormatter();

		/// <summary>
		/// Десериализация обекта из потока
		/// </summary>
		/// <param name="str">Входной поток</param>
		/// <param name="formater"></param>
		/// <returns></returns>
		public static object Deserialize(Stream str, IFormatter formater = null)
		{
			try
			{
				return str == null ? null : (formater ?? binFmt).Deserialize(str);
			}
			finally
			{
				if (str != null)
					str.Close();
			}
		}
		/// <summary>
		/// Десериализация обекта из потока
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="str">Входной поток</param>
		/// <param name="formater"></param>
		/// <returns></returns>
		public static IEnumerable<T> DeserializeCollection<T>(Stream str, IFormatter formater = null)
		{
			return (IEnumerable<T>)Deserialize(str, formater);
		}

		/// <summary>
		/// Сериализация объекта в поток
		/// </summary>
		/// <param name="data"></param>
		/// <param name="str"></param>
		/// <param name="formater"></param>
		/// <returns></returns>
		public static Stream Serialize(object data, Stream str, IFormatter formater = null)
		{
			if (data != null)
			{
				str = str ?? new MemoryStream();
				(formater ?? binFmt).Serialize(str, data);
				str.Position = 0;   //	?
			}
			return str;
		}
		/// <summary>
		/// Сериализация объекта в Base64String
		/// </summary>
		/// <param name="data"></param>
		/// <param name="formater"></param>
		/// <returns></returns>
		public static string Serialize(object data, IFormatter formater = null)
		{
			if (data != null)
			{
				var str = Serialize(data, null, formater);
				if (str.Length > 0)
				{
					str.Position = 0;   //	?
					var bb = new byte[str.Length];
					str.Read(bb, 0, bb.Length);
					var res = Convert.ToBase64String(bb); ;
					return res;
				}
			}
			return null;
		}
	}
}
