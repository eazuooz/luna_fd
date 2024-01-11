using UnityEngine;
using System;
using TMPro;
using System.Collections.Generic;

namespace EverydayDevup.Framework
{
	/// <summary>
	/// 참조하는 UI를 관리하기 위한 데이터 
	/// </summary>
	[Serializable]
	public class UIReferenceData
	{
		public string name;	// 오브젝트의 이름
		public string path; // 오브젝트의 프리팹에서의 경로
		public GameObject obj; // 오브젝트
	}

	/// <summary>
	/// ScriptableObject에서 Dictionary는 Serialize가 되지 않아 별도의 Serialize가 되는 별도의 Dictionary를 구현
	/// </summary>
	[Serializable]
	public class SerializeDicGameObject : SerializeDictionary<string, UIReferenceData> { }

	[Serializable]
	public class SerializeSetGameObject : SerializeSet<string, UIReferenceData> { }
	/// <summary>
	/// UI의 참조 데이터를 관리하는 SciptableObject
	/// </summary>
	[CreateAssetMenu( fileName = "UIReference", menuName = "Scriptable Object Asset/UIReference" )]
	public class UIReference : ScriptableObject
	{
		/// <summary>
		/// 특정 UI 요소를 참조할 수 있는지 확인 하기 위한 type 정의 값
		/// </summary>
		public Type[] typeArray = { typeof( TMP_Text ), typeof( Canvas ) };

		/// <summary>
		/// 하나의 ScriptableObject 데이터에서 관리하는 UI Prefab 
		/// </summary>
		[HideInInspector]
		public GameObject uiTarget;

		/// <summary>
		/// 관리되는 UI Prefab의 GameObject 정보를 관리하는 Dictionary
		/// </summary>
		[HideInInspector]
		public SerializeDicGameObject dic = new SerializeDicGameObject();

		/// <summary>
		/// key 값을 가지고 GameObject를 찾은 후 가져올 타입 T로 GetComponent를 함
		/// </summary>
		/// <typeparam name="T">GameObject에서 가져올 타입</typeparam>
		/// <param name="key">UI를 가져올 수 있는 키 값 (오브젝트의 이름)</param>
		public T GetData<T>(string key) where T : class
		{
			T obj = null;
			GameObject go = null;
			UIReferenceData data = null;
			dic.TryGetValue( key, out data );
			if( data != null )
			{
				go = data.obj as GameObject;

				if( typeof( T ) != typeof( GameObject ) )
				{
					obj = go.GetComponent<T>();
				}
				else
				{
					obj = go as T;
				}
			}

			if( obj == null )
			{
				Debug.LogError( "해당 타입의 UI가 존재하지 않습니다." + typeof( T ).ToString() );
			}

			return obj;
		}
	}
}
