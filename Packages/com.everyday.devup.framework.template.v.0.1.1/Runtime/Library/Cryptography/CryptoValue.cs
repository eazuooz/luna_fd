using System;

namespace EverydayDevup.Framework
{
	/// <summary>
	/// 게임에서 사용할 데이터를 암복호화하는 클래스 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class CryptoValue<T>
	{
		/// <summary>
		/// 암호화된 문자열을 보관하기 위한 변수 
		/// </summary>
		string encryptData = string.Empty;

		/// <summary>
		/// 원본 데이터 
		/// </summary>
		T data;

		/// <summary>
		/// 암호화 변수에 값을 넣는 함수
		/// </summary>
		/// <param name="value"> 값 </param>
		public void Set(T value)
		{
			encryptData = Game.Instance.cryptoManager.EncryptAESbyBase64Key( value.ToString());
			data = value;
		}

		/// <summary>
		/// 암호화가 필요하지 않은 곳에서 사용하는 Get 함수로, UI에 단순 표시 용도로 사용할 수 있음 
		/// </summary>
		/// <returns></returns>
		public T GetUnSafeData()
		{
			return (T)Convert.ChangeType( data, typeof( T ) );
		}
		
		/// <summary>
		/// 중요하게 처리되는 로직에서 사용하는 Get 함수로, 전투 수식 계산과 같은 곳에서 사용할 수 있음 
		/// </summary>
		/// <returns></returns>
		public T Get()
		{
			return (T)Convert.ChangeType( Game.Instance.cryptoManager.DecryptAESByBase64Key( encryptData ), typeof( T ) );
		}
	}
}
