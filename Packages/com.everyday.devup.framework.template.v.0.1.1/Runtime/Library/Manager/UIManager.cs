//#define ASSETBUNDLE
#define ADDRESSIBLE

using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;

namespace EverydayDevup.Framework
{
	/// <summary>
	/// UI를 관리하는 클래스
	/// </summary>
	public class UIManager : MonoBehaviour, IManager
	{
		/// <summary>
		/// 게임에서 사용하는 모든 UI가 정의된 ScriptableObject
		/// </summary>
		public UIAddressable uIAddressable;

		/// <summary>
		/// 게임에서 사용하는 UI 에셋번들
		/// </summary>
		public AssetBundle uiAssetBundle;
		
		/// <summary>
		/// UI의 최상위 Canvas로, 해당 Canvas하위에 UI가 생성되고 삭제됨
		/// </summary>
		public Canvas rootCanvas;

		/// <summary>
		/// 현재 생성되고 있는 UI 정보 
		/// </summary>
		UIAddressableData currentData = null;
		/// <summary>
		/// UI를 순서에 따라 순차적으로 생성하기 위해 Queue로 관리 
		/// </summary>
		Queue<int> requestUIQueue = new Queue<int>();

		/// <summary>
		/// 현재 화면에 보여지는 UI를 Stack으로 관리 
		/// </summary>
		Stack<UIBase> uiBases = new Stack<UIBase>();

		/// <summary>
		/// 제일 최근(마지막)에 보여진 UI
		/// </summary>
		UI_TYPE currentBase = UI_TYPE.NONE;

		/// <summary>
		/// 화면에 새로운 UI를 추가하기 위해 호출되는 함수
		/// </summary>
		/// <param name="type">추가할 UI의 type</param>
		public void Push(UI_TYPE type)
		{
			currentBase = type;
			requestUIQueue.Enqueue( (int)type );
		}

		/// <summary>
		/// 매 프레임마다 현재 UI를 업데이트하며, 추가할 ui가 있는 경우 ResourceManager에서 읽어옴 
		/// </summary>
		public void ManagerUpdate()
		{
			UIBase uiBase;
			UIBase[] uiBaseArray = uiBases.ToArray();
			for( int i = 0, icount = uiBaseArray.Length; i < icount; ++i )
			{
				uiBase = uiBaseArray[i];
				if( uiBase != null )
				{
					uiBase.UIUpdate();
				}
			}

			/// 추가할 UI가 있는 경우
			if( requestUIQueue != null && requestUIQueue.Count > 0 )
			{
				///현재 로드 중인 UI가 없는 경우
				if( currentData == null )
				{
					UI_TYPE requestUI = (UI_TYPE)requestUIQueue.Dequeue();
					OnLoad( requestUI );
				}
			}
		}

		/// <summary>
		/// UI를 불러오는 함수 
		/// </summary>
		/// <param name="type"></param>
		void OnLoad(UI_TYPE type)
		{
			/// 정의된 UI 데이터로 부터 추가할 UI 데이터를 얻은 후 리소스 매니저를 통해 로드 
			currentData = uIAddressable.GetUIAddressableDataByUIType( type );
			
			#if ASSETBUNDLE
			Game.Instance.resourcesManager.Load(uiAssetBundle, currentData.prefab, rootCanvas.transform, OnComplete, OnFail);
			#endif
			#if ADDRESSIBLE
			Game.Instance.resourcesManager.Load( currentData.reference, rootCanvas.transform, OnComplete, OnFail );
			#endif
		}

		/// <summary>
		/// UI가 정상적으로 로드 되었을 때 불리는 함수 
		/// </summary>
		/// <param name="go"></param>
		void OnComplete(GameObject go)
		{
			/// UI의 기본 좌표 및 앵커 값을 수정
			RectTransform rectTransform = go.GetComponent<RectTransform>();
			rectTransform.SetParent( rootCanvas.transform );
			rectTransform.offsetMax = Vector2.zero;
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.localPosition = Vector3.zero;

			/// 추가된 UI가 전체화면인 경우 기존 UI에서 전체화면인 UI를 비활성화
			UIBase addUI = go.GetComponent<UIBase>();
			/// 추가된 UI 설정
			addUI.UIInit( currentData, uiBases.Count );
			addUI.UIActive();
			addUI.UIUpdate();

			if( addUI.IsFullScreen )
			{
				UIBase[] uiBaseArray = uiBases.ToArray();
				UIBase uiBase;
				for( int i = uiBaseArray.Length - 1; i >= 0; --i )
				{
					uiBase = uiBaseArray[i];
					if( uiBase.IsFullScreen )
					{
						uiBase.UIInActive();
						break;
					}
				}
			}

			/// 추가된 UI를 Stack에 추가
			uiBases.Push( addUI );
			currentData = null;
		}

		/// <summary>
		/// UI 로드에 실패할 경우 호출되는 함수 
		/// </summary>
		void OnFail()
		{
			currentData = null;
		}

		/// <summary>
		/// UI를 삭제할 때 호출되는 함수 
		/// </summary>
		public void Pop(UI_TYPE type)
		{
			if( uiBases.Count <= 0 )
			{
				return;
			}

			/// Pop할 UI_TYPE을 찾기 위해 임시 Stack을 설정 
			Stack<UIBase> tempStack = new Stack<UIBase>();
			UIBase uiBase;
			while( uiBases.Count > 0 )
			{
				uiBase = uiBases.Pop();
				if( uiBase.type == type )
				{
					/// pop하는 UI가 전체 화면이였던 경우, 남은 UI중 전체화면인 가장 상단의 UI를 활성화 
					if( uiBase.IsFullScreen )
					{
						UIBase otherUIBase;
						UIBase[] uiBaseArray = uiBases.ToArray();
						for( int i = uiBaseArray.Length - 1; i >= 0; --i )
						{
							otherUIBase = uiBaseArray[i];
							if( otherUIBase.IsFullScreen )
							{
								otherUIBase.UIActive();
								break;
							}
						}
					}

					#if ADDRESSIBLE
					Game.Instance.resourcesManager.UnLoad( uiBase.AssetReference, uiBase.gameObject, uiBase.IsCaching );
					#endif
					#if ASSETBUNDLE
					Game.Instance.resourcesManager.UnLoad(uiAssetBundle, uiBase.gameObject, uiBase.IsCaching);
					#endif

					uiBase.UIClear();
				}
				else
				{
					tempStack.Push( uiBase );
				}
			}

			// 기존 UI의 순서를 맞추기 위해 tempStack을 다시 옮김 
			while( tempStack.Count > 0 )
			{
				uiBase = tempStack.Pop();
				uiBases.Push( uiBase );
			}
		}

		/// <summary>
		/// 인자로 받은 UI가 STACK의 TOP인지 아닌지 결과를 반환해주는 함수
		/// </summary>
		public bool CompareTop(UI_TYPE type)
        {
			if( uiBases.Peek().type != type )
            {
				return false;
            }

			return true;
        }

		//public UIBase Top()
  //      {
  //          if (uiBases.Count == 0)
  //          {
		//		return null;
  //          }

		//	return uiBases.Peek();
		//}

		public void ManagerInitialize()
        {
			//string aseetBundlePath 
			//	= Path.Combine(Application.dataPath, "AssetBundles/uiassetbundle");
			//uiAssetBundle = AssetBundle.LoadFromFile(aseetBundlePath);


			//최초 Loading ui 초기화
			Push(UI_TYPE.LOADING);
		}
		

		public void ManagerLoop() 
		{

		}

		public void ManagerClear() 
		{
			//uiAssetBundle.Unload(false);
			Resources.UnloadUnusedAssets();
		}
	}
}
