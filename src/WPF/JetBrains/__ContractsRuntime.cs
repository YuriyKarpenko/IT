// Decompiled with JetBrains decompiler
// Type: System.Diagnostics.Contracts.__ContractsRuntime
// Assembly: DataGridExtensions, Version=1.0.37.0, Culture=neutral, PublicKeyToken=43de855f87de903a
// MVID: 5A6EF75F-94BB-4773-88B0-C65701D66FE0
// Assembly location: D:\SVN\NPF\release\packages\DataGridExtensions.1.0.37\lib\net40-Client\DataGridExtensions.dll

using System.Diagnostics.Contracts.Internal;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Serialization;

namespace System.Diagnostics.Contracts
{
	[CompilerGenerated]
	internal static class __ContractsRuntime
	{
		internal static void ReportFailure(ContractFailureKind kind, string msg, string conditionTxt, Exception inner)
		{
			string msg1 = ContractHelper.RaiseContractFailedEvent(kind, msg, conditionTxt, inner);
			if (msg1 == null)
				return;
			// ISSUE: reference to a compiler-generated method
			__ContractsRuntime.TriggerFailure(kind, msg1, msg, conditionTxt, inner);
		}

		internal static void TriggerFailure(ContractFailureKind kind, string msg, string userMessage, string conditionTxt, Exception inner)
		{
			// ISSUE: object of a compiler-generated type is created
			throw new __ContractsRuntime.ContractException(kind, msg, userMessage, conditionTxt, inner);
		}

		[DebuggerNonUserCode]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal static void Requires(bool condition, string msg, string conditionTxt)
		{
			if (condition)
				return;
			// ISSUE: reference to a compiler-generated method
			__ContractsRuntime.ReportFailure(ContractFailureKind.Precondition, msg, conditionTxt, (Exception)null);
		}

		[DebuggerNonUserCode]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal static void Assume(bool condition, string msg, string conditionTxt)
		{
			if (condition)
				return;
			// ISSUE: reference to a compiler-generated method
			__ContractsRuntime.ReportFailure(ContractFailureKind.Assume, msg, conditionTxt, (Exception)null);
		}

		[Serializable]
		private sealed class ContractException : Exception
		{
			[NonSerialized]
			private __ContractsRuntime.ContractException.ContractExceptionData m_data = new __ContractsRuntime.ContractException.ContractExceptionData();

			public ContractFailureKind Kind => this.m_data._Kind;

			public string Failure => this.Message;

			public string UserMessage => this.m_data._UserMessage;

			public string Condition => this.m_data._Condition;

			public ContractException(ContractFailureKind kind, string failure, string userMessage, string condition, Exception innerException)
			  : base(failure, innerException)
			{
				// ISSUE: reference to a compiler-generated field
				// ISSUE: reference to a compiler-generated field
				this.m_data._Kind = kind;
				// ISSUE: reference to a compiler-generated field
				// ISSUE: reference to a compiler-generated field
				this.m_data._UserMessage = userMessage;
				// ISSUE: reference to a compiler-generated field
				// ISSUE: reference to a compiler-generated field
				this.m_data._Condition = condition;
				// ISSUE: reference to a compiler-generated field
				this.SerializeObjectState += (EventHandler<SafeSerializationEventArgs>)((exception, eventArgs) => eventArgs.AddSerializedState((ISafeSerializationData)this.m_data));
			}

			[Serializable]
			private struct ContractExceptionData : ISafeSerializationData
			{
				public ContractFailureKind _Kind;
				public string _UserMessage;
				public string _Condition;

				void ISafeSerializationData.CompleteDeserialization(object obj)
				{
					// ISSUE: variable of a compiler-generated type
					__ContractsRuntime.ContractException contractException = obj as __ContractsRuntime.ContractException;
					// ISSUE: reference to a compiler-generated field
					contractException.m_data = this;
				}
			}
		}
	}
}
