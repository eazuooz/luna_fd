namespace EverydayDevup.Framework
{
	/// <summary>
	/// 관찰자의 기본 로직을 정의하기 위한 인터페이스
	/// </summary>
	public interface IObserver
	{
		/// <summary>
		/// 관찰자의 아이디
		/// </summary>
		int ID { get; set; }

		/// <summary>
		/// 관찰 대상으로부터 응답을 받을 때, 다른 관찰자 보다 우선순위를 높여서 응답을 받을 수 있도록 하기 위한 값
		/// </summary>
		int Priority { get; set; }

		/// <summary>
		/// 관찰 대상으로부터 응답을 받은 경우 호출되는 함수 
		/// </summary>
		void OnResponse(object obj);
	}
}
