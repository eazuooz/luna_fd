using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageQueue<T>
{
    public Queue<T> messageQueue = new Queue<T>();
    public MessageQueue()
    {

    }

    public void PushData(T data)
    {
        messageQueue.Enqueue(data);
    }

    //큐에있는 데이타 꺼내서 반환
    public T GetData()
    {
        //데이타가 1개라도 있을 경우 꺼내서 반환
        if (messageQueue.Count > 0)
            return messageQueue.Dequeue();
        else
            return default(T);    //없으면 빈값을 반환
    }
    
}
