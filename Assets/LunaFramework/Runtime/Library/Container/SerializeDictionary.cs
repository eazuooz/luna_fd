using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Dictionary는 Unity에서 Serialize를 제공하지 않기 때문에 Serialize가 가능한 class를 만듬
/// </summary>
/// <typeparam name="K"> dictinary에서 Key로 사용할 값의 타입</typeparam>
/// <typeparam name="V"> dictinary에서 Value로 사용할 값의 타입</typeparam>
[Serializable]
public class SerializeDictionary<K, V> : Dictionary<K,V>, ISerializationCallbackReceiver
{
	[SerializeField]
	List<K> keys = new List<K>();

	[SerializeField]
	List<V> values = new List<V>();

	/// <summary>
	/// Serialize가 되기전에 불리는 함수, 데이터를 저장하는 용도로 사용
	/// </summary>
	public void OnBeforeSerialize()
	{
		keys.Clear();
		values.Clear();

		foreach( KeyValuePair<K, V> pair in this )
		{
			keys.Add( pair.Key );
			values.Add( pair.Value );
		}
	}

	/// <summary>
	/// Serialize가 된 후 불리는 함수, 데이터를 불러오는 용도로 사용
	/// </summary>
	public void OnAfterDeserialize()
	{
		this.Clear();

		for( int i = 0, icount = keys.Count; i < icount; ++i )
		{
			this.Add( keys[i], values[i] );
		}
	}
}
