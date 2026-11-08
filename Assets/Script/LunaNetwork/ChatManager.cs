using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using EverydayDevup.Framework;
using Luna_Network;
using LunaChatServer;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour, IObserver
{
    public List<string> chatList = new List<string>();
    public Button sendBtn;
    public GameObject chatLog;
    public Text chattingList;
    public GameObject input;
    public GameObject scrollbar;
    [Tooltip("Enable only when a compatible Luna chat server is available.")]
    public bool connectOnStart;
    public string serverAddress = "127.0.0.1";
    public int serverPort = 7979;
    [NonSerialized] public MessageQueue<string> messageQueue = new MessageQueue<string>();
    private readonly object connectionGate = new object();
    private RemoteServerPeer server;
    private volatile bool disposed;
    private bool connectionStarted;
    public int Priority { get; set; }
    public int ID { get; set; }

    public void OnResponse(object value)
    {
        if (value is string text) messageQueue.PushData(text);
    }

    private void Start()
    {
        if (sendBtn != null) sendBtn.interactable = false;
        if (connectOnStart) Connect();
    }

    public void Connect()
    {
        if (disposed || connectionStarted) return;
        if (!IPAddress.TryParse(serverAddress, out var address) || serverPort < 1 || serverPort > 65535)
        {
            Debug.LogWarning("Chat server address or port is invalid.");
            return;
        }
        connectionStarted = true;
        try
        {
            PacketBufferManager.initialize(2000);
            var connector = new Connector(new NetworkService());
            connector.connected_callback += OnConnected;
            connector.connect(new IPEndPoint(address, serverPort));
        }
        catch (SocketException exception)
        {
            connectionStarted = false;
            Debug.LogWarning("Chat is unavailable: " + exception.Message);
        }
    }

    private void Update()
    {
        lock (connectionGate)
            if (sendBtn != null) sendBtn.interactable = server != null && server.IsConnected;
        // Limit work per frame if a server sends a burst.
        for (int i = 0; i < 32; i++)
        {
            var message = messageQueue.GetData();
            if (message == null) break;
            ReceiveMsg(message);
        }
    }

    public void SendButtonOnClicked()
    {
        var field = input != null ? input.GetComponent<InputField>() : null;
        if (field == null || string.IsNullOrWhiteSpace(field.text)) return;
        var text = field.text.Trim();
        // The legacy packet has a fixed buffer; keep UTF-8 messages comfortably within it.
        if (System.Text.Encoding.UTF8.GetByteCount(text) > 512) return;
        lock (connectionGate)
        {
            if (disposed || server == null || !server.IsConnected) return;
            var packet = LunaPacket.create((short)PROTOCOL.CHAT_MSG_REQ);
            try
            {
                packet.push("User : " + text);
                ((IPeer)server).send(packet);
                field.text = string.Empty;
            }
            catch (SocketException exception) { Debug.LogWarning("Chat send failed: " + exception.Message); }
            catch (ObjectDisposedException) { }
            finally { LunaPacket.destroy(packet); }
        }
    }

    private void OnConnected(UserToken token)
    {
        lock (connectionGate)
        {
            if (disposed) { token.disconnect(); return; }
            server = new RemoteServerPeer(token, messageQueue.PushData);
        }
    }

    private void OnApplicationQuit() { Disconnect(); }
    private void OnDestroy() { Disconnect(); }
    private void Disconnect()
    {
        lock (connectionGate)
        {
            if (disposed) return;
            disposed = true;
            if (server != null) ((IPeer)server).disconnect();
            server = null;
        }
    }

    public void ReceiveMsg(string message)
    {
        chatList.Add(message);
        if (chatList.Count > 200) chatList.RemoveAt(0);
        var text = chatLog != null ? chatLog.GetComponent<Text>() : chattingList;
        if (text != null) text.text = string.Join("\n", chatList);
        var scroll = scrollbar != null ? scrollbar.GetComponent<Scrollbar>() : null;
        if (scroll != null) scroll.value = 0f;
    }
}
