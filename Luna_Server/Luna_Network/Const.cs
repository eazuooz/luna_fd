using System;
using System.Collections.Generic;
using System.Text;

namespace Luna_Network
{
	public struct Const<T>
	{
		public T Value { get; private set; }

		public Const(T value)
			: this()
		{
			this.Value = value;
		}
	}
}
