using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Luna_Network;


namespace Chat_Console_Client
{
    using LunaChatServer;
    class Program
    {
		static List<IPeer> game_servers = new List<IPeer>();

		static void Main(string[] args)
		{
			PacketBufferManager.initialize(2000);
			// CNetworkService객체는 메시지의 비동기 송,수신 처리를 수행한다.
			// 메시지 송,수신은 서버, 클라이언트 모두 동일한 로직으로 처리될 수 있으므로
			// CNetworkService객체를 생성하여 Connector객체에 넘겨준다.
			NetworkService service = new NetworkService();

			// endpoint정보를 갖고있는 Connector생성. 만들어둔 NetworkService객체를 넣어준다.
			Connector connector = new Connector(service);
			// 접속 성공시 호출될 콜백 매소드 지정.
			connector.connected_callback += on_connected_gameserver;
			IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7979);
			connector.connect(endpoint);
			//System.Threading.Thread.Sleep(10);

			while (true)
			{
				Console.Write("> ");
				string line = Console.ReadLine();
				if (line == "q")
				{
					break;
				}

				LunaPacket msg = LunaPacket.create((short)PROTOCOL.CHAT_MSG_REQ);
				msg.push(line);
				game_servers[0].send(msg);
			}

			((RemoteServerPeer)game_servers[0]).token.disconnect();

			//System.Threading.Thread.Sleep(1000 * 20);
			Console.ReadKey();
		}

		/// <summary>
		/// 접속 성공시 호출될 콜백 매소드.
		/// </summary>
		/// <param name="server_token"></param>
		static void on_connected_gameserver(UserToken server_token)
		{
			lock (game_servers)
			{
				IPeer server = new RemoteServerPeer(server_token);
				game_servers.Add(server);
				Console.WriteLine("Connected!");
			}
		}
	}
}
