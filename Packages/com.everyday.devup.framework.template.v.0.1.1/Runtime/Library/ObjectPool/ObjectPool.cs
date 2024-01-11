using System;
using System.Collections.Generic;
using UnityEngine;

namespace EverydayDevup.Framework
{
	public class ObjectPool<T>
	{
		private List<ObjectPoolContainer<T>> mList;
		private Dictionary<T, ObjectPoolContainer<T>> mLookup;
		private Func<T> mFactoryFunc;
		private int mLastIndex = 0;

		public ObjectPool(Func<T> factoryFunc, int initialSize)
		{
			this.mFactoryFunc = factoryFunc;

			mList = new List<ObjectPoolContainer<T>>(initialSize);
			mLookup = new Dictionary<T, ObjectPoolContainer<T>>(initialSize);

			Warm(initialSize);
		}

		private void Warm(int capacity)
		{
			for (int i = 0; i < capacity; i++)
			{
				CreateContainer();
			}
		}

		private ObjectPoolContainer<T> CreateContainer()
		{
			var container = new ObjectPoolContainer<T>();
			container.Item = mFactoryFunc();
			mList.Add(container);
			return container;
		}

		public T GetItem()
		{
			ObjectPoolContainer<T> container = null;
			for (int i = 0; i < mList.Count; i++)
			{
				mLastIndex++;
				if (mLastIndex > mList.Count - 1) mLastIndex = 0;
				
				if (mList[mLastIndex].Used)
				{
					continue;
				}
				else
				{
					container = mList[mLastIndex];
					break;
				}
			}

			if (container == null)
			{
				container = CreateContainer();
			}

			container.Consume();
			mLookup.Add(container.Item, container);
			return container.Item;
		}

		public void ReleaseItem(object item)
		{
			ReleaseItem((T) item);
		}

		public void ReleaseItem(T item)
		{
			if (mLookup.ContainsKey(item))
			{
				var container = mLookup[item];
				container.Release();
				mLookup.Remove(item);
			}
			else
			{
				Debug.LogWarning("This object pool does not contain the item provided: " + item);
			}
		}

		public void ReleaseItems()
        {

			foreach(var item in mList)
            {
				if(mLookup.ContainsKey(item.Item))
				{
					var container = mLookup[item.Item];
					container.Release();
					mLookup.Remove(item.Item);
				}
				else
				{
					Debug.LogWarning("This object pool does not contain the item provided: " + item);
				}

			}
		}

		public int Count
		{
			get { return mList.Count; }
		}

		public int CountUsedItems
		{
			get { return mLookup.Count; }
		}
	}
}
