
namespace EverydayDevup.Framework
{
	public class ObjectPoolContainer<T>
	{
		private T mItem;
		private bool mUsed;

		public bool Used
        {
			get { return mUsed;  }
			private set { mUsed = value; }
        }

		public void Consume()
		{
			Used = true;
		}

		public T Item
		{
			get
			{
				return mItem;
			}
			set
			{
				mItem = value;
			}
		}

		public void Release()
		{
			Used = false;
		}
	}
}
