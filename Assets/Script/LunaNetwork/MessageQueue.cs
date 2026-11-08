using System.Collections.Generic;

// Socket callbacks enqueue while Unity's main thread consumes the queue.
public class MessageQueue<T>
{
    private readonly Queue<T> messageQueue = new Queue<T>();
    private readonly object gate = new object();
    public void PushData(T data) { lock (gate) messageQueue.Enqueue(data); }
    public T GetData() { lock (gate) return messageQueue.Count > 0 ? messageQueue.Dequeue() : default(T); }
}
