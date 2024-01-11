using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System;

namespace EverydayDevup.Framework
{
	/// <summary>
	/// RSA 암복호화에 사용하는 클래스 
	/// </summary>
	public class CryptoRSA
	{
		/// <summary>
		/// RSA 암호화에 사용할 클래스
		/// </summary>
		RSACryptoServiceProvider encrypter = null;
		/// <summary>
		/// RSA 복호화에 사용할 클래스 
		/// </summary>
		RSACryptoServiceProvider decrypter = null;

		/// <summary>
		/// RSA 암복호화 클래스 생성 
		/// </summary>
		/// <param name="base64PublicKey"> base64 공개 키</param>
		/// <param name="base64PrivateKey"> base64 개인 키</param>
		public void Create(string base64PublicKey, string base64PrivateKey)
		{
			encrypter = new RSACryptoServiceProvider();
			encrypter.FromXmlString( Crypto.DecodingBase64( base64PublicKey ) );

			decrypter = new RSACryptoServiceProvider();
			decrypter.FromXmlString( Crypto.DecodingBase64( base64PrivateKey ) );
		}

		/// <summary>
		/// RSA로 암호화하는 함수 
		/// </summary>
		/// <param name="plainText"> RSA로 암호화할 문자열</param>
		/// <returns></returns>
		public string Encrypt(string plainText)
		{
			byte[] byteData = Encoding.UTF8.GetBytes( plainText );
			byte[] byteEncrypt = encrypter.Encrypt( byteData, false );

			return Convert.ToBase64String( byteEncrypt );
		}

		/// <summary>
		/// RSA로 암호화된 문자열을 복호화하는 함수
		/// </summary>
		/// <param name="encryptData"> RSA로 암호화된 문자열 </param>
		/// <returns></returns>
		public string Decrypt(string encryptData)
		{
			byte[] byteEncrypt = Convert.FromBase64String( encryptData );
			byte[] byteData = decrypter.Decrypt( byteEncrypt, false );

			return Encoding.UTF8.GetString( byteData );
		}

	}
}
