using UnityEngine;

namespace EverydayDevup.Framework
{
	/// <summary>
	/// 매프레임마다 ManagerUpdate가 호출되면서 해상도의 변동이 있는지 체크하는 클래스
	/// 듀얼 스크린을 가지고 있는 단말기에서 해상도의 변화가 생김
	/// </summary>
	public class ScreenManager : MonoBehaviour, IManager
	{
		/// <summary>
		/// ScreenManager를 관찰 대상으로 관리하는 변수 
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
		/// <summary>
		/// 기존에 가지고 있던 화면의 가로 크기
		/// </summary>
		public int ScreenWidth { get; private set; } = 0;

		/// <summary>
		/// 기존에 가지고 있던 화면의 세로 크기
		/// </summary>
		public int ScreenHeight { get; private set; } = 0;

		/// <summary>
		/// ScreenManager가 처음 호출되면 화면의 크기를 체크
		/// </summary>
		public ScreenManager()
		{
			ManagerUpdate();
		}

		public void ManagerInitialize()
		{

		}
		/// <summary>
		/// 화면의 크기에 변동이 생긴 경우 관찰자들에게 화면의 변동이 생김을 알려줌
		/// </summary>
		public void ManagerUpdate()
		{
			if( ScreenWidth != Screen.width || ScreenHeight != Screen.height )
			{
				ScreenWidth = Screen.width;
				ScreenHeight = Screen.height;

				subject.OnNotify();
			}
		}

		public void ManagerLoop() {}

		public void ManagerClear()
		{
			if( _subject != null )
			{
				_subject.OnClear();
			}
		}
	}
}
