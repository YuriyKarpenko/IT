using System;

namespace IT
{
	/// <summary>
	/// Замена атрибута System.ComponentModel.LookupBindingPropertiesAttribute
	/// Появление данного атрибута обусловлено тем, что в арибуре AttributeUsage класса LookupBindingPropertiesAttribute провтыкали установить AllowMultiple = true, без чего невозможно а одном классе использовать несколько Lookup-полей
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class MyLookupBindingPropertiesAttribute : Attribute
	{
		/// <summary>
		/// Gets the name of the data source property for the component this attribute is bound to.
		/// Возвращает имя свойства источника данных для LookupMember, к которому привязан этот атрибут.
		/// </summary>
		public string DataSource { get; }

		/// <summary>
		/// Gets the name of the display member property for the component this attribute is bound to.
		/// Возвращает имя свойства элемента отображения для компонента, к которому привязан DataSource.
		/// </summary>
		public string DisplayMember { get; }

		/// <summary>
		/// Gets the name of the  member property for the component this attribute is bound to.
		/// Возвращает имя свойства наэначения для компонента, к которому привязан этот атрибут.
		/// </summary>
		public string LookupMember { get; }

		/// <summary>
		/// Gets the name of the value member property for the component this attribute is bound to.
		/// Возвращает имя свойства элемента значения для компонента, к которому привязан этот атрибут.
		/// </summary>
		public string ValueMember { get; }

		/// <summary>
		/// .ctor
		/// </summary>
		/// <param name="dataSource"></param>
		/// <param name="displayMember"></param>
		/// <param name="valueMember"></param>
		/// <param name="lookupMember"></param>
		public MyLookupBindingPropertiesAttribute(string dataSource, string displayMember, string valueMember, string lookupMember)
		{
			DataSource = dataSource;
			DisplayMember = displayMember;
			LookupMember = lookupMember;
			ValueMember = valueMember;
		}
	}
}
