using System.Collections.Generic;
using System;
using System.Text;
using System.Security.Cryptography;

namespace EverydayDevup.Framework
{
	/// <summary>
	/// 게임에서 암복호화 기능을 관리하는 클래스 
	/// </summary>
	public class Crypto
	{
		/// <summary>
		/// base64로 문자열을 만드는 함수
		/// </summary>
		/// <param name="plainText"> base64로 변경할 문자열</param>
		/// <returns></returns>
		public static string EncodingBase64(string plainText)
		{
			Byte[] strByte = Encoding.UTF8.GetBytes( plainText );
			return Convert.ToBase64String( strByte );
		}

		/// <summary>
		/// base64 문자열을 원래 문자열로 만드는 함수 
		/// </summary>
		/// <param name="base64PlainText"> base64로 변환된 문자열</param>
		/// <returns></returns>
		public static string DecodingBase64(string base64PlainText)
		{
			Byte[] strByte = Convert.FromBase64String( base64PlainText );
			return Encoding.UTF8.GetString( strByte );
		}

		/// <summary>
		/// 해시 함수에 사용하는 SHA256을 재사용하기 위해 static 변수로 관리 
		/// </summary>
		static SHA256 _sha256 = null;

		/// <summary>
		/// 문자열을 sha256 해쉬 값으로 만드는 함수
		/// </summary>
		/// <param name="plainText"> sha256 해쉬 값으로 변경할 문자열 </param>
		/// <returns></returns>
		public static string SHA256Base64(string plainText)
		{
			if( _sha256 == null )
			{
				_sha256 = new SHA256Managed();
			}

			Byte[] hash = _sha256.ComputeHash( Encoding.UTF8.GetBytes( plainText ) );
			return Convert.ToBase64String( hash );
		}
		
		/// <summary>
		/// 암호화에 사용한 AES클래스를 재사용하기 위해 Dictionary로 관리 
		/// </summary>
		static Dictionary<string, CryptoAES> _aesManages = new Dictionary<string, CryptoAES>();

		/// <summary>
		/// AES 암호화에 사용할 클래스 생성
		/// </summary>
		/// <param name="base64Key"> aes에 사용할 base64 key </param>
		/// <param name="base64IV"> aes에 사용할 base64 iv </param>
		static void CreateAESManage(string base64Key, string base64IV)
		{
			CryptoAES aesManage = new CryptoAES();
			aesManage.Create( base64Key, base64IV );
			_aesManages.Add( base64Key, aesManage );
		}

		/// <summary>
		/// 문자열을 AES로 암호화하는 함수 
		/// </summary>
		/// <param name="plainText"> 암호화할 문자열 </param>
		/// <param name="base64Key"> 암호화에 사용할 base64 key </param>
		/// <param name="base64IV"> 암호화에 사용할 base64 iv</param>
		/// <returns></returns>
		public static string EncryptAESbyBase64Key(string plainText, string base64Key, string base64IV)
		{
			if( _aesManages.ContainsKey( base64Key ) == false )
			{
				CreateAESManage( base64Key, base64IV );
			}

			return _aesManages[base64Key].Encrypt( plainText );
		}

		/// <summary>
		/// AES로 암호화된 문자열을 복호화하여 원래 문자열을 반환하는 함수 
		/// </summary>
		/// <param name="encryptData"> AES로 암호화된 문자열</param>
		/// <param name="base64Key"> 암호화 할때 사용했던 base64 key </param>
		/// <param name="base64IV"> 암호화 할 떄 사용했던 base64 iv </param>
		/// <returns></returns>
		public static string DecryptAESByBase64Key(string encryptData, string base64Key, string base64IV)
		{
			if( _aesManages.ContainsKey( base64Key ) == false )
			{
				CreateAESManage( base64Key, base64IV );
			}

			return _aesManages[base64Key].Decrypt( encryptData );
		}

		/// <summary>
		/// RSA 암호화에 사용한 클래스를 재사용하기 위해 Dictionary로 관리 
		/// </summary>
		static Dictionary<string, CryptoRSA> _rsaManages = new Dictionary<string, CryptoRSA>();

		/// <summary>
		/// RSA 암복호화에 사용할 Class를 생성하는 함수 
		/// </summary>
		/// <param name="base64PublicKey"></param>
		/// <param name="base64PrivateKey"></param>
		static void CreateRSAManage(string base64PublicKey, string base64PrivateKey)
		{
			CryptoRSA rsaManage = new CryptoRSA();
			rsaManage.Create( base64PublicKey, base64PrivateKey );
			_rsaManages.Add( base64PublicKey, rsaManage );
		}

		/// <summary>
		/// 문자열을 RSA로 암호화하는 함수
		/// </summary>
		/// <param name="plainText"> 암호화할 문자열 </param>
		/// <param name="base64PublicKey"> 암호화에 사용할 base64 공개 키 </param>
		/// <param name="base64PrivateKey"> 암호화에 사용할 basse64 개인 키</param>
		/// <returns></returns>
		public static string EncryptRSAbyBase64PublicKey(string plainText, string base64PublicKey, string base64PrivateKey)
		{
			if( _rsaManages.ContainsKey( base64PublicKey ) == false )
			{
				CreateRSAManage( base64PublicKey, base64PrivateKey );
			}

			return _rsaManages[base64PublicKey].Encrypt( plainText );
		}

		/// <summary>
		/// RSA로 암호화된 문자열을 복호화하는 함수 
		/// </summary>
		/// <param name="encryptData"> RSA로 암호화된 문자열 </param>
		/// <param name="base64PublicKey"> RSA로 암호화할때 사용한 공개 키 </param>
		/// <param name="base64PrivateKey"> RSA로 암호호활 때 사용한 개인 키 </param>
		/// <returns></returns>
		public static string DecryptRSAByBase64Key(string encryptData, string base64PublicKey, string base64PrivateKey)
		{
			if( _rsaManages.ContainsKey( base64PublicKey ) == false )
			{
				CreateRSAManage( base64PublicKey, base64PrivateKey );
			}

			return _rsaManages[base64PublicKey].Decrypt( encryptData );
		}
	}
}