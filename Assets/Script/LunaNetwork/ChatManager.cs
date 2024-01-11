using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
using UnityEngine;
using Luna_Network;
using LunaChatServer;
using EverydayDevup.Framework;
using System.Collections;
//using Chat_Luna_Client;

public class ChatManager : MonoBehaviour, IObserver
{
    public List<string> chatList = new List<string>();
    public Button sendBtn;
    public GameObject chatLog;
    public Text chattingList;
    public GameObject input;
    public GameObject scrollbar;
    string usersName;

    public MessageQueue<string> messageQueue;  

    static List<IPeer> game_servers = new List<IPeer> ();

    public int Priority { get; set; }
    public int ID { get; set; }
    public void OnResponse(object obj)
    {
        string text = Game.Instance.dataManager.chatStr;
        messageQueue.PushData(text);
    }
    // Start is called before the first frame update
    void Start()
    {
        messageQueue = new MessageQueue<string> ();

        PacketBufferManager.initialize(2000);

        // CNetworkService��ü�� �޼����� �񵿱� ��,���� ó���� �����Ѵ�.
        // �޽��� ��,������ ����, Ŭ���̾�Ʈ ��� ������ �������� ó���� �� ���̹Ƿ�
        // CNetworkService��ü�� �����Ͽ� Connector��ü�� �Ѱ��ش�.
        NetworkService service = new NetworkService();

        // endpoint ������ �����ִ� Connector ����. ������ NetworkService��ü�� �־��ش�.
        Connector connector = new Connector(service);
        // ���� ������ ȣ��� �ݹ� �޼ҵ� ����
        connector.connected_callback += on_connected_gameserver;

        IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7979);
        connector.connect(endpoint);

        //Game.Instance.dataManager.chatManager = this.gameObject;

        StartCoroutine(CheckMessageQueue());
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("> ");
        

    }

    private IEnumerator CheckMessageQueue()
    {
        WaitForSeconds waitSec = new WaitForSeconds(0.5f);

        while (true)
        {
            //InfiniteLoopDetector.Run();
            string msg = messageQueue.GetData();

            if (msg != null)
            {
                ReceiveMsg(msg);
            }

            yield return waitSec;
        }
    }

    public void SendButtonOnClicked()
    {
        string nickName = "User : ";
        string line = input.GetComponent<InputField>().text;

        string message = nickName + line;

        LunaPacket msg = LunaPacket.create((short)PROTOCOL.CHAT_MSG_REQ);
        msg.push(message);

        game_servers[0].send(msg);
    }
    private void OnApplicationQuit()
    {
        if(game_servers.Count != 0)
            ((RemoteServerPeer)game_servers[0]).token.disconnect();
    }

    /// <summary>
    /// ���� ������ ȣ��� �ݹ� �żҵ�.
    /// </summary>
    /// <param name="server_token"></param>
    void on_connected_gameserver(UserToken server_token)
    {
        lock (game_servers)
        {
            IPeer server = new RemoteServerPeer(server_token);
            game_servers.Add(server);

            Debug.Log("Connected!");

            //(server as RemoteServerPeer).chatManager = this.gameObject;
            (server as RemoteServerPeer).subject.AddObserver(this);
        }

        
    }

    void chattingBoxUpdate()
    {
        usersName = "Player List\n";

        //foreach (Player item in collection)
        //{

        //}
    }

    public void ReceiveMsg(string msg)
    {
        chatLog.GetComponent<Text>().text += "\n" + msg;

        scrollbar.GetComponent<Scrollbar>().value = 0.0f;

        input.GetComponent<InputField>().ActivateInputField();
        input.GetComponent<InputField>().text = "";
        //scroll_rect
        //scroll_rect
    }
}
