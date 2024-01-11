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

    //ť���ִ� ����Ÿ ������ ��ȯ
    public T GetData()
    {
        //����Ÿ�� 1���� ���� ��� ������ ��ȯ
        if (messageQueue.Count > 0)
            return messageQueue.Dequeue();
        else
            return default(T);    //������ ���� ��ȯ
    }
    
}
