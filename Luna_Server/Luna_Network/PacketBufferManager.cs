using System;
using System.Collections.Generic;
using System.Text;

namespace Luna_Network
{
    public class PacketBufferManager
    {
        static object cs_buffer = new object();
        static Stack<LunaPacket> pool;
        static int pool_capacity;

        public static void initialize(int capacity)
        {
            pool = new Stack<LunaPacket>();
            pool_capacity = capacity;
            allocate();
        }

        static void allocate()
        {
            for (int i = 0; i < pool_capacity; i++)
            {
                pool.Push(new LunaPacket());
            }
        }
        public static LunaPacket pop()
        {
            lock (cs_buffer)
            {
                if (pool.Count <= 0)
                {
                    Console.WriteLine("reallocate.");
                    allocate();
                }

                return pool.Pop();
            }
        }

        public static void push(LunaPacket packet)
        {
            lock (cs_buffer)
            {
                pool.Push(packet);
            }
        }
    }
}
