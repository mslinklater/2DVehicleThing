using UnityEngine;

namespace CRSys
{
	#region Message Types
	public enum MessageType
	{
		Unknown,

		ApplicationPaused,
		ApplicationQuit
	}
	#endregion

	#region Base
	public class M_MessageBase
	{
		public MessageType messageType { get{ return _messageType; } }
		protected MessageType _messageType = MessageType.Unknown;

		public virtual string description(){ return ""; }

		protected void Lock()
		{
			if( _locked )
			{
				Debug.LogError("Recursive message calling");
			}
			_locked = true;
		}
		public void Release()
		{
			_locked = false;
		}

		private bool _locked;
	}
	#endregion
}
