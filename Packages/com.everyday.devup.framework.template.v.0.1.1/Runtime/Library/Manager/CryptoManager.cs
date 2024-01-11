using UnityEngine;
using System.Collections;

namespace EverydayDevup.Framework
{
	/// <summary>
	/// Crypto 클래스를 래핑해서 사용하는 암복호화 관리 클래스 
	/// </summary>
	public class CryptoManager : MonoBehaviour, IManager
	{
		/// <summary>
		/// 암복호화에 사용할 키값을 담고 있는 Scriptable Object
		/// </summary>
		public CryptoData cryptoData;

		/// <summary>
		/// 문자열을 base64로 변환하는 함수
		/// </summary>
		/// <param name="plainText"> base64로 변환할 문자열</param>
		/// <returns></returns>
		public string EncodingBase64(string plainText)
		{
			return Crypto.EncodingBase64( plainText );
		}

		/// <summary>
		/// base64로 된 문자열을 원래 문자열로 복원하는 함수 
		/// </summary>
		/// <param name="base64PlainText">base64로 변환된 문자열</param>
		/// <returns></returns>
		public string DecodingBase64(string base64PlainText)
		{
			return Crypto.DecodingBase64( base64PlainText );
		}

		/// <summary>
		/// 문자열을 sha256 해시 문자열로 변환하는 함수 
		/// </summary>
		/// <param name="plainText"></param>
		/// <returns></returns>
		public string SHA256Base64(string plainText)
		{
			return Crypto.SHA256Base64( plainText );
		}

		/// <summary>
		/// 문자열을 AES로 암호화하는 함수
		/// </summary>
		/// <param name="plainText"></param>
		/// <returns></returns>
		public string EncryptAESbyBase64Key(string plainText)
		{
			return Crypto.EncryptAESbyBase64Key( plainText, cryptoData.aesBase64Key, cryptoData.aesBase64IV );
		}

		/// <summary>
		/// AES로 암호화된 문자열을 원래 문자열로 복호화하는 함수 
		/// </summary>
		/// <param name="encryptData"></param>
		/// <returns></returns>
		public string DecryptAESByBase64Key(string encryptData)
		{
			return Crypto.DecryptAESByBase64Key( encryptData, cryptoData.aesBase64Key, cryptoData.aesBase64IV );
		}

		/// <summary>
		/// 문자열을 RSA로 암호화 하는 함수 
		/// </summary>
		/// <param name="plainText">암호화할 문자열</param>
		/// <returns></returns>
		public string EncryptRSAbyBase64PublicKey(string plainText)
		{
			return Crypto.EncryptRSAbyBase64PublicKey( plainText, cryptoData.rsaBase64PublicKey, cryptoData.rsaBase64PrivateKey );
		}

		/// <summary>
		/// RSA로 암호화된 문자열을 복호화하는 함수
		/// </summary>
		/// <param name="encryptData"> RSA로 암호화된 문자열</param>
		/// <returns></returns>
		public string DecryptRSAByBase64Key(string encryptData)
		{
			return Crypto.DecryptRSAByBase64Key( encryptData, cryptoData.rsaBase64PublicKey, cryptoData.rsaBase64PrivateKey );
		}

		/// <summary>
		/// playerPref에 data를 암호화하여 저장하는 함수
		/// </summary>
		/// <param name="key"> 저장 시 사용할 키값</param>
		/// <param name="data">저장 할 데이터</param>
		public void SavePlayerPrefsAESEncrpy(string key, string data)
		{
			FileUtil.SavePlayerPrefs( key, data, cryptoData.aesBase64Key, cryptoData.aesBase64IV );
		}

		/// <summary>
		/// playerPrefs에 저장된 암호화된 데이터를 복호화하는 함수
		/// </summary>
		/// <param name="key"> 저장 시 사용한 키값</param>
		/// <param name="defualtData"> 데이터가 없을 경우 사용할 기본 데이터 값</param>
		/// <returns></returns>
		public string SavePlayerPrefsAESDecrpy(string key, string defualtData)
		{
			return FileUtil.GetPlayerPrefs( key, defualtData, cryptoData.aesBase64Key, cryptoData.aesBase64IV );
		}

		/// <summary>
		/// 파일에 데이터를 AES로 암호화하여 저장하는 함수 
		/// </summary>
		/// <param name="folderPath">저장할 폴더의 경로</param>
		/// <param name="fileName">저장할 파일의 이름</param>
		/// <param name="extention">저장할 파일의 확장자</param>
		/// <param name="data">저장할 데이터</param>
		public void WriteFileAESEncrpy(string folderPath, string fileName, string extention, string data)
		{
			FileUtil.WriteFile( folderPath, fileName, extention, data, cryptoData.aesBase64Key, cryptoData.aesBase64IV );
		}

		/// <summary>
		/// AES 암호화로 저장된 파일을 복호화하는 함수
		/// </summary>
		/// <param name="folderPath">저장된 폴더의 경로</param>
		/// <param name="fileName">저장된 파일의 이름</param>
		/// <param name="extention">저장된 파일의 확장자</param>
		/// <returns></returns>
		public string ReadFileAESDrcpy(string folderPath, string fileName, string extention)
		{
			return FileUtil.ReadFile( folderPath, fileName, extention, cryptoData.aesBase64Key, cryptoData.aesBase64IV );
		}

		public void ManagerInitialize()
		{

		}
		public void ManagerUpdate() { }
		public void ManagerLoop() { }
		public void ManagerClear() { }

        
    }
}
