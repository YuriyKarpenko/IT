using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace IT
{
	/// <summary>
	/// Расширения для сжатия
	/// </summary>
	public static class Zip
	{
		#region GZip

		/// <summary>
		/// Распаковка потока посредством GZip в новый MemoryStream
		/// </summary>
		/// <param name="inStr"></param>
		/// <returns></returns>
		public static Stream FromGZip(this Stream inStr)
		{
			var ms = new MemoryStream();
			GZip(inStr, ms, CompressionMode.Decompress);
			return ms;
		}

		/// <summary>
		/// Упаковка потока посредством GZip в новый MemoryStream
		/// </summary>
		/// <param name="inStr"></param>
		/// <returns></returns>
		public static Stream ToGZip(this Stream inStr)
		{
			var ms = new MemoryStream();
			GZip(inStr, ms, CompressionMode.Compress);
			return ms;
		}

		/// <summary>
		/// Работа с потоками посредством GZip + outStr.Position = 0
		/// </summary>
		/// <param name="inStr"></param>
		/// <param name="outStr"></param>
		/// <param name="mode"></param>
		public static void GZip(Stream inStr, Stream outStr, CompressionMode mode)
		{
			Contract.NotNull(inStr, "inStr");
			Contract.NotNull(outStr, "outStr");

			switch (mode)
			{
				case CompressionMode.Compress:
#if TEST_STREEM
					inStr.Position = 0;
					using (var fs1 = new FileStream(@"d:\Work\orig1.csv", FileMode.Create))
						inStr.CopyTo(fs1);
#endif
					inStr.Position = 0;
					using (var z1 = new GZipStream(outStr, mode, true))
						inStr.CopyTo(z1);
					break;

				case CompressionMode.Decompress:
					using (var z2 = new GZipStream(inStr, mode))
						z2.CopyTo(outStr);
#if TEST_STREEM
					outStr.Position = 0;
					using (var fs2 = new FileStream(@"d:\Work\orig2.csv", FileMode.Create))
						outStr.CopyTo(fs2);
#endif
					break;
			}

			outStr.Position = 0;
		}

		#endregion


		#region Deflate

		//public static void Deflate(Stream inStr, Stream outStr, CompressionMode mode)
		//{
		//	Contract.NotNull(inStr, "inStr");
		//	Contract.NotNull(outStr, "outStr");

		//	switch (mode)
		//	{
		//		case CompressionMode.Compress:
		//			inStr.Position = 0;
		//			using (var z1 = new DeflateStream(outStr, mode, true))
		//				inStr.CopyTo(z1);
		//			break;

		//		case CompressionMode.Decompress:
		//			using (var z2 = new DeflateStream(inStr, mode))
		//				z2.CopyTo(outStr);
		//			break;
		//	}

		//	outStr.Position = 0;
		//}

		#endregion
	}
}
