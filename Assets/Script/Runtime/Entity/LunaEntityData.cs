using System.Threading;
using GameFramework;
using UnityEngine;

namespace Luna.Gameplay
{
    public class LunaEntityData : IReference
    {
        private static int serial;
        public int Id;
        public int TypeId;
        public int RoundId;
        public int OwnerId;
        public Vector3 Position;
        public Quaternion Rotation = Quaternion.identity;
        public Transform Parent;
        internal bool OwnedByEntity;

        public static T Create<T>() where T : LunaEntityData, new()
        {
            T data = ReferencePool.Acquire<T>();
            data.Id = Interlocked.Increment(ref serial);
            return data;
        }

        public virtual void Clear()
        {
            Id = TypeId = RoundId = OwnerId = 0;
            Position = Vector3.zero;
            Rotation = Quaternion.identity;
            Parent = null;
            OwnedByEntity = false;
        }
    }
}
