/* 
 * 
 * Advanced C# messenger by Ilya Suzdalnitski. V1.0
 * 
 * Based on Rod Hyde's "CSharpMessenger" and Magnus Wolffelt's "CSharpMessenger Extended".
 * 
 * Features:
 	* Prevents a MissingReferenceException because of a reference to a destroyed message handler.
 	* Option to log all messages
 	* Extensive error detection, preventing silent bugs
 * 
 * Usage examples:
 	1. Messenger.AddListener<GameObject>("prop collected", PropCollected);
 	   Messenger.Broadcast<GameObject>("prop collected", prop);
 	2. Messenger.AddListener<float>("speed changed", SpeedChanged);
 	   Messenger.Broadcast<float>("speed changed", 0.5f);
 * 
 * Messenger cleans up its evenTable automatically upon loading of a new level.
 * 
 * Don't forget that the messages that should survive the cleanup, should be marked with Messenger.MarkAsPermanent(string)
 * 
 */

#define LOG_ALL_MESSAGES
#define LOG_ADD_LISTENER
#define LOG_BROADCAST_MESSAGE
#define REQUIRE_LISTENER

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CRSys
{
	public static class Messenger 
	{
		#region Internal variables
	
		static public Dictionary<MessageType, Delegate> eventTable = new Dictionary<MessageType, Delegate>();

		//Message handlers that should never be removed, regardless of calling Cleanup
		static public List< MessageType > permanentMessages = new List< MessageType > ();
		#endregion

		#region Helper methods
		//Marks a certain message as permanent.
		static public void MarkAsPermanent(MessageType eventType) 
		{
			#if LOG_ALL_MESSAGES
			Debug.Log("##Messenger MarkAsPermanent \t\"" + eventType + "\"");
			#endif
		
			permanentMessages.Add( eventType );
		}
		
		static public void Cleanup()
		{
			#if LOG_ALL_MESSAGES
			Debug.Log("##Messenger Cleanup. Make sure that none of necessary listeners are removed.");
			#endif
			
			List< MessageType > messagesToRemove = new List< MessageType >();

			foreach (KeyValuePair<MessageType, Delegate> pair in eventTable) 
			{
				bool wasFound = false;
				
				foreach (MessageType message in permanentMessages) 
				{
					if (pair.Key == message) 
					{
						wasFound = true;
						break;
					}
				}
				
				if (!wasFound)
					messagesToRemove.Add( pair.Key );
			}
			
			foreach (MessageType message in messagesToRemove) 
			{
				eventTable.Remove( message );
			}
		}
		
		static public void PrintEventTable()
		{
			Debug.Log("\t\t\t=== MESSENGER PrintEventTable ===");
			
			foreach (KeyValuePair<MessageType, Delegate> pair in eventTable) 
			{
				Debug.Log("\t\t\t" + pair.Key + "\t\t" + pair.Value);
			}
			
			Debug.Log("\n");
		}
		#endregion
		
		#region Message logging and exception throwing
		static public void OnListenerAdding(MessageType eventType, Delegate listenerBeingAdded) 
		{
			#if LOG_ALL_MESSAGES || LOG_ADD_LISTENER
			Debug.Log("##Messenger OnListenerAdding \"" + eventType + "\"\t{" + listenerBeingAdded.Target + " -> " + listenerBeingAdded.Method + "}");
			#endif
			
			if (!eventTable.ContainsKey(eventType)) 
			{
				eventTable.Add(eventType, null );
			}
			
			Delegate d = eventTable[eventType];
			if (d != null && d.GetType() != listenerBeingAdded.GetType()) 
			{
				throw new ListenerException(string.Format("Attempting to add listener with inconsistent signature for event type {0}. Current listeners have type {1} and listener being added has type {2}", eventType, d.GetType().Name, listenerBeingAdded.GetType().Name));
			}
		}
		
		static public bool OnListenerRemoving( MessageType eventType, Delegate listenerBeingRemoved) 
		{
			#if LOG_ALL_MESSAGES
			Debug.Log("##Messenger OnListenerRemoving \"" + eventType + "\"\t{" + listenerBeingRemoved.Target + " -> " + listenerBeingRemoved.Method + "}");
			#endif
			
			if (eventTable.ContainsKey(eventType)) 
			{
				Delegate d = eventTable[eventType];
				
				if (d == null) 
				{
					throw new ListenerException(string.Format("Attempting to remove listener with for event type \"{0}\" but current listener is null.", eventType));
				} 
				else if (d.GetType() != listenerBeingRemoved.GetType()) 
				{
					throw new ListenerException(string.Format("Attempting to remove listener with inconsistent signature for event type {0}. Current listeners have type {1} and listener being removed has type {2}", eventType, d.GetType().Name, listenerBeingRemoved.GetType().Name));
				}
			} 
			else 
			{
				Debug.LogWarning( string.Format("Attempting to remove listener for type \"{0}\" but Messenger doesn't know about this event type.", eventType) );
				return false;
			}
			return true;
		}
		
		static public void OnListenerRemoved(MessageType eventType) 
		{
			if (eventTable[eventType] == null) 
			{
				eventTable.Remove(eventType);
			}
		}
		
		static public void OnBroadcasting( MessageType eventType) 
		{
			#if REQUIRE_LISTENER
			if (!eventTable.ContainsKey(eventType)) 
			{
				throw new BroadcastException(string.Format("Broadcasting message \"{0}\" but no listener found. Try marking the message with Messenger.MarkAsPermanent.", eventType));
			}
			#endif
		}
		
		static public BroadcastException CreateBroadcastSignatureException( MessageType eventType) 
		{
			return new BroadcastException(string.Format("Broadcasting message \"{0}\" but listeners have a different signature than the broadcaster.", eventType));
		}
		
		public class BroadcastException : Exception 
		{
			
			public BroadcastException(string msg)
			: base(msg) 
			{
			}
		}
		
		public class ListenerException : Exception 
		{
			public ListenerException(string msg)
			: base(msg) 
			{
			}
		}
		#endregion
		
		#region AddListener
		static public void AddListener<T>( MessageType eventType, MessengerCallback<T> handler) 
		{
			OnListenerAdding(eventType, handler);

			#if !MASTER
			if(eventTable[eventType] != null)
			{
				Delegate[] delegates = eventTable[eventType].GetInvocationList();

				for(int i = 0; i < delegates.Length; ++i)
				{
					if(delegates[i] == handler)
					{
						Debug.LogError("Adding the same listener multipler times to message " + eventType.ToString() + ", handler = " + handler.ToString());
						break;
					}
				}
			}
			#endif
			eventTable[eventType] = (MessengerCallback<T>)eventTable[eventType] + handler;
		}
		#endregion

		#region RemoveListener
		static public void RemoveListener<T>( MessageType eventType, MessengerCallback<T> handler) 
		{
			if( OnListenerRemoving(eventType, handler) )
			{
				eventTable[eventType] = (MessengerCallback<T>)eventTable[eventType] - handler;
				OnListenerRemoved(eventType);
			}
		}
		#endregion
		
		#region Broadcast
		static public void Broadcast<T>( T arg1) 
		{
			if( !typeof(T).IsSubclassOf( typeof(M_MessageBase) ))
			{
				Debug.DebugBreak();
			}

			M_MessageBase mb = arg1 as M_MessageBase;

			#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
			Debug.Log("##Messenger " + "Broadcast \"" + mb.messageType + "\"" + (arg1 as M_MessageBase).description() );
			#endif

			OnBroadcasting( mb.messageType );

			Delegate d;
			if (eventTable.TryGetValue( mb.messageType, out d)) 
			{
				MessengerCallback<T> callback = d as MessengerCallback<T>;
				
				if (callback != null) 
				{
					callback(arg1);
				} 
				else 
				{
					throw CreateBroadcastSignatureException( mb.messageType );
				}
			}
		}
		#endregion
	}
}	// namespace CRSys
