using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using System;

namespace EverydayDevup.Framework
{
	/// <summary>
	/// 게임에서 사용하는 UI 를 관리하는 ScriptableObject
	/// </summary>
	[CreateAssetMenu( fileName = "UIAddressable", menuName = "Scriptable Object Asset/UIAddressable" )]
	public class UIAddressable : ScriptableObject
	{
		/// <summary>
		/// 게임에서 사용하는 UI
		/// </summary>
		public List<UIAddressableData> uiList = new List<UIAddressableData>();

		/// <summary>
		/// UI_TYPE에 해당하는 UI 데이터를 반환
		/// </summary>
		/// <param name="type"> 사용할 UI의 Type</param>
		/// <returns></returns>
		public UIAddressableData GetUIAddressableDataByUIType(UI_TYPE type)
		{
			UIAddressableData data;
			for( int i = 0, icount = uiList.Count; i < icount; ++i )
			{
				if( ( data = uiList[i] ) != null && data.type.Equals( type ) )
				{
					return data;
				}
			}

			return null;
		}
	}

	public enum UI_TYPE
	{
		NONE,
		LOADING,
		LOBBY,
		CHARACTERLIST,
		UPGRADE,
		SELECTSTAGE,
		PLAYER,
		RESULT,
		PROGRESSBAR,
		STAGETEXT,
		GAMEOPTION,
		POPUP,
		CHARACTERINFO,
		GOODS,
		GAMESHOP,
		ACCOUNTSHOP,
		ADVENTURE,
		DUNGEON,
		PVP,
		LOTTERY,
		SKILL,
		EQUIPDESIGN,
		SELECTPOPUP,
		STAGEINFO,
		DUNGEONINFO,
		INVENTORY,
		QUITPOPUP,
		ITEMINFO,
		SKILLINFO,
	}

	/// <summary>
	/// Addressable에서 사용할 UI의 데이터
	/// </summary>
	[Serializable]
	public class UIAddressableData
	{
		/// <summary>
		/// UI의 타입
		/// </summary>
		public UI_TYPE type;
		/// <summary>
		/// Addressable로 UI Prefab를 불러올 때, UI가 사라져도 메모리에 가지고 있을지 판단하는 변수
		/// </summary>
		public bool isCaching;
		/// <summary>
		/// UI가 전체 화면을 가리는지 판단하는 변수
		/// </summary>
		public bool isFullScreen;
		/// <summary>
		/// UIPrefab를 가리키는 변수 
		/// </summary>
		public AssetReference reference;
		public GameObject prefab;
	}
}
