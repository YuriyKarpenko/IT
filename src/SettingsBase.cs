using System;
using System.Configuration;

namespace IT
{
	/// <summary>
	/// 
	/// </summary>
	internal interface ISettingsBase
	{
		/// <summary>
		/// Без коментариев, default = null
		/// </summary>
		/// <param name="key">Ключ параметра (можно null)</param>
		/// <returns></returns>
		string GetValue(string key);

		/// <summary>
		/// Получение ConnectionStringSettings с указанным ключем
		/// </summary>
		/// <param name="key">Ключ</param>
		/// <returns></returns>
		ConnectionStringSettings GetConnectionStringSettings(string key);
	}

	/// <summary>
	/// Класс для облегчения управления настройками программы. 
	/// </summary>
	public class SettingsBase : ISettingsBase, ILog
	{
		/// <summary>
		/// Кешированные значения ключей
		/// </summary>
		protected MemCache<string, string> valueCache = new MemCache<string, string>();

		#region ISettingsBase

		/// <summary>
		/// Без коментариев, default = null
		/// </summary>
		/// <param name="key">Ключ параметра (можно null)</param>
		/// <returns></returns>
		public virtual string GetValue(string key)
		{
			try
			{
				return this.valueCache[key, () => ConfigurationManager.AppSettings[key]];
			}
			catch (Exception ex)
			{
				this.Error(ex, "({0})", key);
			}

			return null;
		}

		/// <summary>
		/// Получение ConnectionStringSettings с указанным ключем
		/// </summary>
		/// <param name="key">Ключ</param>
		/// <returns></returns>
		public virtual ConnectionStringSettings GetConnectionStringSettings(string key)
		{
			try
			{
				return ConfigurationManager.ConnectionStrings[key];
			}
			catch (Exception ex)
			{
				this.Error(ex, "()");
				throw;
			}
		}

		#endregion


		/// <summary>
		/// Получение массива значений из строки.
		/// Разделитель = new char[] { ';', '|', ' ', ',' }
		/// </summary>
		/// <param name="key">Ключ параметра (можно null)</param>
		/// <param name="defValue">Дефолтное значение</param>
		/// <returns></returns>
		public virtual string[] GetValueArray(string key, string defValue = "")
		{
			return this.GetValueArray(key, new char[] { ';', '|', ' ', ',' }, defValue);
		}


		/// <summary>
		/// Получение массива значений из строки с указанными разделителями
		/// </summary>
		/// <param name="key">Ключ параметра (можно null)</param>
		/// <param name="separators">Разделитель значений</param>
		/// <param name="defValue">Дефолтное значение</param>
		/// <returns></returns>
		protected virtual string[] GetValueArray(string key, char[] separators, string defValue = "")
		{
			try
			{
				var ret = this.GetValue(key) ?? defValue;

				return ret.Split(separators, StringSplitOptions.RemoveEmptyEntries);
			}
			catch (Exception ex)
			{
				this.Error(ex, "({0})", key);
				throw;
			}
		}

		/// <summary>
		/// Без коментариев, default = 0
		/// </summary>
		protected int GetValueInt(string key, int def = 0)
		{
			try
			{
				var s = this.GetValue(key);
				return s.To<int>(def);
			}
			catch (Exception ex)
			{
				this.Error(ex, "({0})", key);
				throw;
			}
		}

		/// <summary>
		/// Без коментариев, default = false
		/// </summary>
		protected bool GetValueBool(string key, bool def = false)
		{
			try
			{
				var s = this.GetValue(key);
				if (string.IsNullOrEmpty(s))
					return def;
				else
					return s.ToBool();
			}
			catch (Exception ex)
			{
				this.Error(ex, "({0})", key);
				throw;
			}
		}

		/// <summary>
		/// Получение свойства ConnectionString из ConnectionStringSettings с указанным ключем
		/// </summary>
		/// <param name="key">Ключ</param>
		/// <param name="defValue">Значение по умолчанию</param>
		/// <returns></returns>
		protected virtual string GetConnectionString(string key, string defValue = "")
		{
			var cs = this.GetConnectionStringSettings(key);
			return cs == null ? defValue : cs.ConnectionString;
		}

	}

	/// <summary>
	/// Инкапсуляция свойства Def
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SettingsBase<T> : SettingsBase where T : SettingsBase, new()
	{
		/// <summary>
		/// Гарантированный экземпляр
		/// </summary>
		public static T Def { get { return def ?? (def = new T()); } }
		static T def;
	}

	/// <summary>
	/// Класс для облегчения управления настройками программы. 
	/// <para>public static Settings Def { get { return _defaultInstancef ?? (_defaultInstancef = new Settings()); } }</para>
	/// <para>private static Settings _defaultInstancef = null;</para>
	/// </summary>
	public class SettingsBaseSaved : SettingsBase, ISettingsBase
	{
		/*	For copy-past
		/// <summary>
		/// Создает стандартную конфигурацию
		/// </summary>
		public static Settings Def { get { return _defaultInstancef ?? (_defaultInstancef = new Settings()); } }
		private static Settings _defaultInstancef = null;
		*/

		/// <summary>
		/// Без коментариев
		/// </summary>
		public Configuration Cfg
		{
			get
			{
				if (this._cfg == null)
				{
					this.Trace("()");
					try
					{
						this._cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
					}
					catch (Exception ex)
					{
						this.Error(ex, "()");
						//this._cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
						throw;
					}
				}
				return this._cfg;
			}
		}
		private Configuration _cfg = null;

		/// <summary>
		/// Для задания конфигурации из другого файла
		/// </summary>
		/// <param name="cfg">Новая конфигурация</param>
		[Obsolete]
		public void Init(Configuration cfg)
		{
			this._cfg = cfg;
			this.valueCache.Clear();
		}

		#region ISettingsBase

		/// <summary>
		/// Без коментариев, default = null
		/// </summary>
		/// <param name="key">Ключ параметра (можно null)</param>
		/// <returns></returns>
		public override string GetValue(string key)
		{
			try
			{
				return this.valueCache[key, () => this.Cfg.AppSettings.Settings[key].Value];
			}
			catch (Exception ex)
			{
				this.Error(ex, "({0})", key);
			}

			return null;
		}

		/// <summary>
		/// Получение ConnectionStringSettings с указанным ключем
		/// </summary>
		/// <param name="key">Ключ</param>
		/// <returns></returns>
		public override ConnectionStringSettings GetConnectionStringSettings(string key)
		{
			try
			{
				return this.Cfg.ConnectionStrings.ConnectionStrings[key];
			}
			catch (Exception ex)
			{
				this.Error(ex, "()");
				throw;
			}
		}

		#endregion


		/// <summary>
		/// Без коментариев
		/// </summary>
		/// <param name="key">Ключ параметра</param>
		/// <param name="value">Значение</param>
		/// <returns></returns>
		[Obsolete]
		protected bool SetValue(string key, string value)
		{
			try
			{
				this.Cfg.AppSettings.Settings[key].Value = value;
				this.valueCache.Remove(key);
				return true;
			}
			catch (Exception ex)
			{
				this.Error(ex, "({0}, {1})", key, value);
			}

			return false;
		}

		/// <summary>
		/// Без коментариев
		/// </summary>
		[Obsolete]
		protected void Save(ConfigurationSaveMode mode)
		{
			this.Cfg.Save(mode);
		}
	}


	/// <summary>
	/// Инкапсуляция свойства Def
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SettingsBaseSaved<T> : SettingsBaseSaved where T : SettingsBaseSaved, new()
	{
		/// <summary>
		/// Гарантированный экземпляр
		/// </summary>
		public static T Def { get { return def ?? (def = new T()); } }
		static T def;
	}

}
