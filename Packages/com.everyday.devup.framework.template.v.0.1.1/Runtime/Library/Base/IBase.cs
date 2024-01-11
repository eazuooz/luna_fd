/// <summary>
/// Game Framework에서 기본 인터페이스 클래스
/// </summary>
namespace EverydayDevup.Framework
{
	/// <summary>
	/// Framework에서 기본적으로 사용하는 함수 API
	/// </summary>
	public interface IBase
	{
		/// <summary>
		/// 초기화 시 사용되는 함수 
		/// </summary>
		void OnInit();
		/// <summary>
		/// 매 프레임 마다 호출되는 함수
		/// </summary>
		void OnUpdate();
		/// <summary>
		/// 초 마다 호출되는 함수
		/// </summary>
		void OnLoop();
		/// <summary>
		/// 활성화 될 때 호출되는 함수
		/// </summary>
		void OnActive();
		/// <summary>
		/// 비활성화 될 때 호출되는 함수
		/// </summary>
		void OnInActive();
		/// <summary>
		/// 메모리에서 삭제될 때 호출되는 함수
		/// </summary>
		void OnClear();
	}
}
