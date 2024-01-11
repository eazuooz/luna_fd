using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luna_Network;

namespace Chat_Server
{
    using LunaChatServer;

    /// <summary>
    /// 하나의 session 객체를 나타낸다.
    /// </summary>
    class GameUser : IPeer
    {
        UserToken token;

		private static List<GameUser> users = new List<GameUser>();

        public GameUser(UserToken token)
        {
            this.token = token;
            this.token.set_peer(this);
        }

		public static void AddGameUser(GameUser user)
        {
			users.Add(user);
        }
		public static void RemoveGameUser(GameUser user)
		{
			users.Remove(user);
		}
		void IPeer.on_message(Const<byte[]> buffer)
		{
			// ex)
			LunaPacket msg = new LunaPacket(buffer.Value, this);
			PROTOCOL protocol = (PROTOCOL)msg.pop_protocol_id();
			Console.WriteLine("--------123123----------------------------------------------");
			Console.WriteLine("protocol id " + protocol);
			switch (protocol)
			{
				case PROTOCOL.CHAT_MSG_REQ:
					{
						string text = msg.pop_string();
						Console.WriteLine(string.Format("text {0}", text));

						LunaPacket response = LunaPacket.create((short)PROTOCOL.CHAT_MSG_ACK);
						response.push(text);
						//send(response);
						sendUsers(response);
					}
					break;
			}
		}

		void IPeer.on_removed()
		{
			Console.WriteLine("The client disconnected.");

			Program.remove_user(this);
		}

		public void send(LunaPacket msg)
		{
			this.token.send(msg);
		}

		void sendUsers(LunaPacket msg)
        {
            foreach (GameUser user in users)
            {
				user.send(msg);
            }
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
