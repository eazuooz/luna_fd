using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luna_Network;

namespace Chat_Console_Client
{

	using LunaChatServer;
	class RemoteServerPeer : IPeer
	{
		public UserToken token { get; private set; }

		public RemoteServerPeer(UserToken token)
		{
			this.token = token;
			this.token.set_peer(this);
		}

		void IPeer.on_message(Const<byte[]> buffer)
		{
			LunaPacket msg = new LunaPacket(buffer.Value, this);
			PROTOCOL protocol_id = (PROTOCOL)msg.pop_protocol_id();
			switch (protocol_id)
			{
				case PROTOCOL.CHAT_MSG_ACK:
					{
						string text = msg.pop_string();
						Console.WriteLine(string.Format("text {0}", text));
					}
					break;
			}
		}

		void IPeer.on_removed()
		{
			Console.WriteLine("Server removed.");
		}

		void IPeer.send(LunaPacket msg)
		{
			this.token.send(msg);
		}

		void IPeer.disconnect()
		{
			this.token.socket.Disconnect(false);
		}

		void IPeer.process_user_operation(LunaPacket msg)
		{
		}
	}
}
