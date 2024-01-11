using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Luna_Network;
using LunaChatServer;
using EverydayDevup.Framework;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


//namespace /Chat_Luna_Client
//{
public class RemoteServerPeer : IPeer
{
    /// <summary>
    /// 강화 또는 레벨업 관찰자들에게 Noti를 하기 위한 subject
    /// </summary>
    Subject _subject;
    public Subject subject
    {
        get
        {
            if (_subject == null)
            {
                _subject = new Subject();
            }

            return _subject;
        }
    }
    public void OnNotify()
    {
        subject.OnNotify();
    }


    public UserToken token { get; private set; }
    public GameObject chatManager { get; set; }
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
                    Debug.Log(string.Format("text {0}", text));

                    WriteMessage(text);
                }
                break;
        }
    }

    void IPeer.on_removed()
    {
        Debug.Log("Server removed.");
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

    void WriteMessage(string text)
    {
        Game.Instance.dataManager.chatStr = text;
        OnNotify();

        //chatManager.GetComponent<ChatManager>().ReceiveMsg(text);
    }
}

//}