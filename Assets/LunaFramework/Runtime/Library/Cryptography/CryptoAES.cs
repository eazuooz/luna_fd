using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EverydayDevup.Framework
{
	/// <summary>
	/// AES로 암복호화 시 사용하는 클래스 
	/// </summary>
	public class CryptoAES
	{
		/// <summary>
		/// AES의 키의 길이는 128, 192, 256 으로 설정
		/// </summary>
		public static int[] aesKeySize = { 128, 192, 256 };
		/// <summary>
		/// AES의 초기 벡터의 길이는 128을 사용 
		/// </summary>
		public static int aesIVSize = 128;

		/// <summary>
		/// AES 암호화에 사용하는 클래스
		/// </summary>
		ICryptoTransform encrypter;
		/// <summary>
		/// AES 복호화에 사용하는 클래스 
		/// </summary>
		ICryptoTransform decrypter;

		/// <summary>
		/// AES 암복호화에 사용할 클래스를 생성하는 함수 
		/// </summary>
		/// <param name="base64Key"> AES 클래스에서 사용할 base64 키 값</param>
		/// <param name="base64IV"> AES 클래스에서 사용할 base64 초기 벡터 값</param>
		public void Create(string base64Key, string base64IV)
		{
			byte[] key = Convert.FromBase64String( base64Key );
			byte[] iv = Convert.FromBase64String( base64IV );

			RijndaelManaged rijndaelManaged = new RijndaelManaged();
			rijndaelManaged.KeySize = key.Length * 8;
			rijndaelManaged.BlockSize = aesIVSize;
			rijndaelManaged.Padding = PaddingMode.PKCS7;
			rijndaelManaged.Mode = CipherMode.CBC;

			rijndaelManaged.Key = key;
			rijndaelManaged.IV = iv;

			encrypter = rijndaelManaged.CreateEncryptor();
			decrypter = rijndaelManaged.CreateDecryptor();
		}

		/// <summary>
		/// AES로 암호화하는 함수 
		/// </summary>
		/// <param name="plainText"> AES로 암호화할 문자열 </param>
		/// <returns></returns>
		public string Encrypt(string plainText)
		{
			using( MemoryStream memoryStream = new MemoryStream() )
			{
				using( CryptoStream cryptoStream = new CryptoStream( memoryStream, encrypter, CryptoStreamMode.Write ) )
				{
					byte[] byteData = Encoding.UTF8.GetBytes( plainText );
					cryptoStream.Write( byteData, 0, byteData.Length );
				}

				byte[] byteCrypto = memoryStream.ToArray();
				return Convert.ToBase64String( byteCrypto );
			}
		}

		/// <summary>
		/// AES로 된 문자열을 복호화하는 함수 
		/// </summary>
		/// <param name="encryptData"> AES로 암호화된 문자열 </param>
		/// <returns></returns>
		public string Decrypt(string encryptData)
		{
			using( MemoryStream memoryStream = new MemoryStream() )
			{
				using( CryptoStream cryptoStream = new CryptoStream( memoryStream, decrypter, CryptoStreamMode.Write ) )
				{
					byte[] byteEncrpt = Convert.FromBase64String( encryptData );
					cryptoStream.Write( byteEncrpt, 0, byteEncrpt.Length );
				}

				byte[] byteCrypto = memoryStream.ToArray();
				return Encoding.UTF8.GetString( byteCrypto );
			}
		}
	}
}
