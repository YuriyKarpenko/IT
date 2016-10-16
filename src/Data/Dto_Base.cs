using System;
using System.ComponentModel;
using System.Data;
using System.Runtime.Serialization;

namespace IT.Data
{
	/// <summary>
	/// Стандартный справочник
	/// </summary>
	[System.Diagnostics.DebuggerDisplay("{RowState}  {ID}  {NAME}")]
	[Serializable]
	[DataContract]
	public class Dto_Base : IDto_Reader, IDto_RowState
	{
		/// <summary>
		/// Состояние записи
		/// </summary>
		[DataMember, Browsable(false)]
		public DataRowState RowState { get; set; }

		/// <summary>
		/// Ключ, Browsable(false)
		/// </summary>
		[DataMember]
		public virtual long Id { get; set; }

		/// <summary>
		/// Значение
		/// </summary>
		[DataMember, Description("Наименование")]
		public virtual string Name { get; set; }


		/// <summary>
		/// ctor
		/// </summary>
		public Dto_Base()
		{
			this.RowState = DataRowState.Added;
		}

		/// <summary>
		/// Реализация интерфейса IDtoBase_Reader
		/// </summary>
		/// <param name="dr"></param>
		public virtual void Init(IDataReader dr)
		{
			this.RowState = DataRowState.Unchanged;

			this.Id = dr.Get<long>("ID");
			this.Name = dr.Get<string>("NAME");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.Name;
		}
	}

}
