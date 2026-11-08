using UnityEngine;
using UnityEngine.UI;

namespace EverydayDevup.Framework
{
	[RequireComponent( typeof( Canvas ) )]
	public class CanvasScaleByCamera2D : MonoBehaviour, IObserver
	{
		public int Priority { get; set; }
		public int ID { get; set; }

		public Camera2D camera2D;

		Canvas _canvas;
		CanvasScaler _canvasScaler;

		public void OnEnable()
		{
			/// Canvas가 활성화 될 때 Canvas의 사이즈 설정 및 2D카메라에 관찰자로 등록
			//OnScreenSize();
			camera2D.subject.AddObserver( this );
		}

		public void OnDisable()
		{
			/// Canvas가 비활성화 될 때 2D카메라에 관찰자에서 제거
			camera2D.subject.RemoveObserver( this );
		}

		public void OnResponse(object obj)
		{
			/// 2D 카메라 사이즈 변경 시 Canvas의 사이즈 변경 
			//OnScreenSize();
		}

		void OnScreenSize()
		{
			if( _canvas == null )
			{
				_canvas = GetComponent<Canvas>();
			}

			if( _canvas.renderMode != RenderMode.ScreenSpaceCamera )
			{
				_canvas.renderMode = RenderMode.ScreenSpaceCamera;
				_canvas.worldCamera = camera2D.GetComponent<Camera>();
			}

			if( _canvasScaler == null )
			{
				_canvasScaler = GetComponent<CanvasScaler>();
			}

			if( _canvasScaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize )
			{
				_canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			}

			_canvasScaler.referenceResolution = new Vector2( camera2D.screenWidth, camera2D.screenHeight );
			_canvasScaler.matchWidthOrHeight = camera2D.matchWidth ? 0 : 1;
		}
	}
}

