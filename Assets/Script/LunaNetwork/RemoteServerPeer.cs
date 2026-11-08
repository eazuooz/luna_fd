using System;
using Luna_Network;
using LunaChatServer;

// Network callbacks must never read Unity objects or scene singletons.
public class RemoteServerPeer : IPeer
{
    public UserToken token { get; private set; }
    private readonly Action<string> receive;
    private volatile bool removed;
    public bool IsConnected => !removed && token.socket != null && token.socket.Connected;

    public RemoteServerPeer(UserToken token, Action<string> receive)
    {
        this.token = token;
        this.receive = receive;
        token.set_peer(this);
    }

    void IPeer.on_message(Const<byte[]> buffer)
    {
        var message = new LunaPacket(buffer.Value, this);
        if ((PROTOCOL)message.pop_protocol_id() == PROTOCOL.CHAT_MSG_ACK)
            receive?.Invoke(message.pop_string());
    }
    void IPeer.on_removed() { removed = true; }
    void IPeer.send(LunaPacket message) { if (IsConnected) token.send(message); }
    void IPeer.disconnect() { removed = true; token.disconnect(); }
    void IPeer.process_user_operation(LunaPacket message) { }
}
