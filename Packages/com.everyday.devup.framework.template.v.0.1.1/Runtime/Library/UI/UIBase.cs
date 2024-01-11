
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EverydayDevup.Framework
{
	/// <summary>
	/// UI가 기본적으로 상속받는 클래스
	/// </summary>
	[RequireComponent( typeof( Canvas ))]
	public class UIBase : MonoBehaviour, IBase
	{
		/// <summary>
		/// UI 데이터를 가지고 있는 ScriptableObject
		/// </summary>
		UIAddressableData uiAddressableData;
		/// <summary>
		/// UI의 최상위에 있는 Canvas
		/// </summary>
		Canvas canvas;
		
		/// <summary>
		/// 해당 UI가 전체 화면인지 체크
		/// </summary>
		public bool IsFullScreen
		{
			get
			{
				return uiAddressableData.isFullScreen;
			}
		}

		/// <summary>
		/// 해당 UI가 캐싱되는지 체크
		/// </summary>
		public bool IsCaching
		{
			get
			{
				return uiAddressableData.isCaching;
			}
		}

		/// <summary>
		/// 해당 UI의 Addressable 주소 
		/// </summary>
		public AssetReference AssetReference
		{
			get
			{
				return uiAddressableData.reference;
			}
		}

        /// <summary>
        /// 해당 UI의 UI_TYPE
        /// </summary>
        public UI_TYPE type
		{
			get
			{
				return uiAddressableData.type;
			}
		}

		/// <summary>
		/// UI가 로드 되었을 때 불리는 초기화 함수
		/// </summary>
		/// <param name="uiAddressableData"> UI 정보</param>
		/// <param name="stackCount"> UI의 스택 위치 </param>
		public void UIInit(UIAddressableData uiAddressableData, int stackCount )
		{
			this.uiAddressableData = uiAddressableData;
			canvas = GetComponent<Canvas>();
			canvas.overrideSorting = true;
			canvas.sortingOrder = stackCount;

			OnInit();
		}

		/// <summary>
		/// UI가 활성화 되면 불리는 함수 
		/// </summary>
		public void UIActive()
		{
			canvas.enabled = true;
			OnActive();
		}

		/// <summary>
		/// UI가 비활성화 되면 불리는 함수
		/// </summary>
		public void UIInActive()
		{
			canvas.enabled = false;
			OnInActive();
		}

		/// <summary>
		/// UI가 업데이트 될때마다 호출되는 함수
		/// </summary>
		public void UIUpdate()
		{
			if( canvas.enabled == false )
			{
				return;
			}

			OnUpdate();
		}

		/// <summary>
		/// 매 초마다 호출되는 함수
		/// </summary>
		public void UILoop()
		{
			if( canvas.enabled == false )
			{
				return;
			}

			OnLoop();
		}

		/// <summary>
		/// UI가 사라질 때 호출 되는 함수 
		/// </summary>
		public void UIClear()
		{
			OnClear();
			uiAddressableData = null;
			GameObject.Destroy( gameObject, 0f );
		}

		public virtual void OnInit() 
		{
			gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
		}

		public virtual void OnActive() {}

		public virtual void OnInActive() {}

		public virtual void OnUpdate() {}

		public virtual void OnLoop() {}

		public virtual void OnClear() {}


	}
}
