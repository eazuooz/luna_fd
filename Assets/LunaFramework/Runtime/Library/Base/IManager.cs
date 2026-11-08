namespace EverydayDevup.Framework
{
	/// <summary>
	/// 데이터를 관리하는 클래스는 Manager라는 네이밍과 함께 IManager를 상속 받아서 특정 함수를 구현한다.
	/// </summary>
	public interface IManager
	{
		/// <summary>
		/// Manager 클래스의 정보를 초기화 할때 사용되는 함수
		/// </summary>
		void ManagerInitialize();
		/// <summary>
		/// Game.cs -> Update() 매 프레임 마다 호출 되는 함수
		/// </summary>
		void ManagerUpdate();
		/// <summary>
		/// Game.cs -> Loop() 1초마다 호출되는 함수 
		/// </summary>
		void ManagerLoop();

		/// <summary>
		/// Manager 클래스의 정보를 없앨때 사용하는 함수
		/// </summary>
		void ManagerClear();
	}
}
