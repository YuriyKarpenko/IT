// Type: Microsoft.Internal.Requires
// Assembly: System.ComponentModel.Composition, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// Assembly location: C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.ComponentModel.Composition\v4.0_4.0.0.0__b77a5c561934e089\System.ComponentModel.Composition.dll

using System;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace IT
{
	/// <summary>
	/// замена System.Diagnostics.Contracts
	/// </summary>
	public class  Contract
	{
		/// <summary>
		/// замена System.Diagnostics.Contracts.Contract.Requires()
		/// if(!conditions) - создание исключения TException
		/// </summary>
		/// <typeparam name="TException">Тип мсключения, при негативном "conditions" </typeparam>
		/// <param name="conditions">Условие НЕ создания исключения</param>
		/// <param name="args">Параметры конструктора исключения "conditions"</param>
		public static void Requires<TException>(bool conditions, params object[] args) where TException : Exception, new()
		{
			if(!conditions)
			{
				TException ex;
				var ctor = typeof(TException).GetConstructor(args.Select(i => i.GetType()).ToArray());
				if (ctor != null)
					ex = (TException)ctor.Invoke(args);
				else
					ex = new TException();

				throw ex;
			}
		}

		/// <summary>
		/// замена System.Diagnostics.Contracts.Contract.Requires()
		/// if(!conditions) - создание исключения ArgumentException
		/// </summary>
		/// <param name="conditions">Условие НЕ создания исключения</param>
		/// <param name="msg">Параметр конструктора исключения "conditions"</param>
		public static void Requires(bool conditions, string msg)
		{
			Requires<ArgumentException>(conditions, msg);
		}


		/// <summary>
		/// NotNull(value, parameterName) + проверка на нулевую длину, при успехе вызывает ArgumentException("'{0}' не может быть пустой строкой")
		/// </summary>
		/// <param name="value"></param>
		/// <param name="parameterName"></param>
		[DebuggerStepThrough]
		public static void NotIsNullOrEmpty(string value, string parameterName)
		{
			Requires<ArgumentException>(!string.IsNullOrEmpty(value), string.Format("'{0}' не может быть пустой строкой", parameterName));
		}

		/// <summary>
		/// Проверка на null, при успехе вызывает ArgumentNullException
		/// </summary>
		/// <param name="value">Проверяемый параметр</param>
		/// <param name="parameterName">Имя параметра</param>
		[DebuggerStepThrough]
		public static void NotNull(object value, string parameterName) //where T : class
		{
			Contract.Requires<ArgumentNullException>(value != null, parameterName);
		}

		//private static void NotNullElements<T>(IEnumerable<T> values, string parameterName) where T : class
		//{
		//	foreach (T obj in values)
		//	{
		//		if ((object)obj == null)
		//			throw new ArgumentException("Пустые елементы не допустимы", parameterName);
		//	}
		//}

		//private static void NotNullElements<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> values, string parameterName)
		//	where TKey : class
		//	where TValue : class
		//{
		//	foreach (KeyValuePair<TKey, TValue> keyValuePair in values)
		//	{
		//		if ((object)keyValuePair.Key == null || (object)keyValuePair.Value == null)
		//			throw new ArgumentException("Пустые елементы не допустимы", parameterName);
		//	}
		//}

		//[DebuggerStepThrough]
		//public static void IsInMembertypeSet(MemberTypes value, string parameterName, MemberTypes enumFlagSet)
		//{
		//    if ((value & enumFlagSet) == value && (value & value - 1) == (MemberTypes)0)
		//        return;
		//    throw new ArgumentException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, Strings.ArgumentOutOfRange_InvalidEnumInSet, (object)parameterName, (object)value, (object)((object)enumFlagSet).ToString()), parameterName);
		//}

		//internal static void ReportFailure(ContractFailureKind kind, string msg, string conditionTxt, Exception inner)
		//{
		//	string msg1 = System.Runtime.CompilerServices.ContractHelper.RaiseContractFailedEvent(kind, msg, conditionTxt, inner);
		//	if (msg1 == null)
		//		return;
		//	// ISSUE: reference to a compiler-generated method
		//	__ContractsRuntime.TriggerFailure(kind, msg1, msg, conditionTxt, inner);
		//}

	}

}
