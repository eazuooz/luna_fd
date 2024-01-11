using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Luna_Network
{
    class SocketAsyncEventArgsPool
    {
        Stack<SocketAsyncEventArgs> mPool;

        public SocketAsyncEventArgsPool(int capacity)
        {
            mPool = new Stack<SocketAsyncEventArgs>(capacity);
        }

        public void Push(SocketAsyncEventArgs item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("Item added to a SocketAsyncEventArgsPool can't be null");
            }

            lock (mPool)
            {
                mPool.Push(item);
            }
        }

        public SocketAsyncEventArgs Pop()
        {
            lock (mPool)
            {
                return mPool.Pop();
            }
        }

        public int Count
        {
            get { return mPool.Count; }
        }
    }
}
