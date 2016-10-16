using System;
using System.Collections.Generic;

namespace IT {
	/// <summary>
	/// Удобный справочник, который не ругается на ключи
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Serializable]
	public class MemCache<TKey, TValue> : Dictionary<TKey, TValue> {
		static TKey BAD_KEY = default(TKey);
		static TValue DEFAULT = default(TValue);

		/// <summary>
		/// Не ругается на запрос отсутствующего ключа
		/// </summary>
		/// <param name="key">Ключ</param>
		/// <returns></returns>
		public new TValue this[TKey key] {
			get { return this[key, null]; }
			set { base[key] = value; }
		}

		/// <summary>
		/// Позволяет создать значение указанного ключа из <see cref="getValue"/>
		/// </summary>
		/// <param name="key">Ключ</param>
		/// <param name="getValue">Метод получения значения</param>
		/// <returns></returns>
		public TValue this[TKey key, Func<TValue> getValue] {
			get {
				if (object.Equals(BAD_KEY, key))
					return DEFAULT;

				TValue res;

				if (!base.TryGetValue(key, out res)) {
					if (getValue != null)
						this[key] = res = getValue();
					else
						return DEFAULT;
				}

				return res;
			}
		}
	}
}
