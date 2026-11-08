using UnityEngine;

namespace EverydayDevup.Framework
{
	/// <summary>
	/// 2D 카메라를 통해 UI가 그려질 화면의 크기를 조정함
	/// </summary>
	[RequireComponent( typeof( Camera ) )]
	public class Camera2D : MonoBehaviour, IObserver
	{
		/// <summary>
		/// 가로 크기 최소값, 해당 최소값이 보장됨
		/// </summary>
		[HideInInspector]
		public int minWidth;
		/// <summary>
		/// 세로 크기 최소 값, 해당 최소 값이 보장됨
		/// </summary>
		[HideInInspector]
		public int minHeight;
		/// <summary>
		/// minWidth, minHeight를 기준으로 고정된 크기를 가로로 할지, 세로로 할지 정하는 변수 
		/// </summary>
		[HideInInspector]
		public bool matchWidth;

		/// <summary>
		/// 디바이스의 해상도와 Camera2D의 설정된 minWidth,minHeight를 계산하여 실제 보여지는 카메라의 크기를 결정
		/// </summary>
		[HideInInspector]
		public int screenWidth;
		[HideInInspector]
		public int screenHeight;

		/// <summary>
		/// 카메라가 따라다닐 트랜스폼 컴포넌트
		/// </summary>
		public Transform target;

		/// <summary>
		/// CanvasScaleByCamera2D 관찰자에게 노티를 하기 위함
		/// </summary>
		Subject _subject;
		public Subject subject
		{
			get
			{
				if( _subject == null )
				{
					_subject = new Subject();
				}

				return _subject;
			}
		}

		public int Priority { get; set; }
		public int ID { get; set; }

		private Camera _cam;

		public void OnEnable()
		{
			if( Game.Instance == null )
			{
				return;
			}
			
			/// Camera가 활성화 될 때마다 ScreenManager에서 단말기의 해상도를 가져와서 화면 설정을 함
			ScreenManager screenManager = Game.Instance.screenManager;
			OnScreenSize( screenManager.ScreenWidth, screenManager.ScreenHeight );

			screenManager.subject.AddObserver( this );

		}

		public void OnDisable()
		{
			if( Game.Instance == null )
			{
				return;
			}

			/// Camera가 비활성화 될 때마다 ScreenManager에 등록된 관찰자를 제
			ScreenManager screenManager = Game.Instance.screenManager;
			screenManager.subject.RemoveObserver( this );
		}

		public void OnResponse(object obj)
		{
			/// ScreenManager에서 단말기의 해상도가 변경됬다는 것을 알려주면 카메라 사이즈를 변경함
			ScreenManager screenManager = obj as ScreenManager;
			if( screenManager != null )
			{
				OnScreenSize( screenManager.ScreenWidth, screenManager.ScreenHeight );
			}
		}

		/// <summary>
		/// 단말기의 해상도에 따라 카메라의 크기를 결정함
		/// </summary>
		/// <param name="screenWidth">단말기의 가로 크기</param>
		/// <param name="screenHeight">단말기의 세로 크기</param>
		public void OnScreenSize(int screenWidth, int screenHeight)
		{
			//if( _cam == null )
			//{
			//	_cam = GetComponent<Camera>();
			//}

			//if( _cam.orthographic == false )
			//{
			//	_cam.orthographic = true;
			//}

			//int orthographicSize = GetOrthographicSize( screenWidth, screenHeight );
			//_cam.orthographicSize = orthographicSize;

			//orthographicSize *= 2;

			//this.screenWidth = ( screenWidth * orthographicSize ) / screenHeight;
			//this.screenHeight = orthographicSize;

			subject.OnNotify();
		}

		/// <summary>
		/// 2D 카메라의 크기를 계산하여 반환
		/// </summary>
		/// <param name="screenWidth">단말기의 가로 크기</param>
		/// <param name="screenHeight">단말기의 세로 크기</param>
		/// <returns></returns>
		public int GetOrthographicSize(int screenWidth, int screenHeight)
		{
			int orthographicSize = 0;
			float addRate = 0.0f;

			/// 가로 크기 고정 
			if( matchWidth )
			{
				orthographicSize = Mathf.RoundToInt( ( screenHeight * minWidth ) / screenWidth );
				if( orthographicSize < minHeight )
				{
					addRate = (float)minHeight / orthographicSize;
					orthographicSize = Mathf.RoundToInt( orthographicSize * addRate );
				}
			}
			else
			{
				orthographicSize = minHeight;
				int width = ( screenWidth * minHeight ) / screenHeight;
				if( width < minWidth )
				{
					addRate = (float)minWidth / width;
					orthographicSize = Mathf.RoundToInt( orthographicSize * addRate );
				}
			}

			return Mathf.RoundToInt( orthographicSize * 0.5f );
		}

		public void LookAtTarget(GameObject targetObj)
        {
			gameObject.transform.LookAt(targetObj.transform);
		}
    }
}
