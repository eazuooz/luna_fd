using UnityEngine;
using System.IO;
using System.Text;

namespace EverydayDevup.Framework
{
	/// <summary>
	/// 게임에서 사용하는 데이터의 파일 입출력을 담당하는 클래스
	/// Unity에 PlayerPref를 바로 사용할 경우 보안의 문제가 있을 수 있어 래핑해서 사용
	/// </summary>
	public class FileUtil
	{
		/// <summary>
		/// 데이터를 저장할 때 사용할 playerPref 키를 만드는 함수 
		/// </summary>
		/// <param name="key"> 키값으로 사용할 문자열 </param>
		/// <returns></returns>
		static string GetPlayerPrefKey(string key)
		{
			key = string.Format( "{0}_{1}", Application.productName, key );
			return Crypto.EncodingBase64( key );
		}

		/// <summary>
		/// playerPref에 키값이 있는지 알려주는 함수 
		/// </summary>
		/// <param name="key"> playerPref 저장 시에 사용한 키값 </param>
		/// <returns></returns>
		public static bool HasPlayerPrefsKey(string key)
		{
			string playerPrefsKey = GetPlayerPrefKey( key );
			return PlayerPrefs.HasKey( playerPrefsKey );
		}

		/// <summary>
		/// playerPref에 키 값을 삭제하는 함수 
		/// </summary>
		/// <param name="key"></param>
		public static void DeletePlayerPrefs(string key)
		{
			if( HasPlayerPrefsKey( key ) )
			{
				string playerPrefsKey = GetPlayerPrefKey( key );
				PlayerPrefs.DeleteKey( playerPrefsKey );
				PlayerPrefs.Save();
			}
		}

		/// <summary>
		/// playerPref에 데이터를 저장하는 함수
		/// </summary>
		/// <param name="key"> 저장 시에 사용할 키값</param>
		/// <param name="data"> 저장할 문자열 데이터 </param>
		public static void SavePlayerPrefs(string key, string data)
		{
			string playerPrefsKey = GetPlayerPrefKey( key );

			PlayerPrefs.SetString( playerPrefsKey, data );
			PlayerPrefs.Save();
		}

		/// <summary>
		/// playerPref에 데이터를 저장하는 함수
		/// </summary>
		/// <param name="key"> 저장 시에 사용할 키값</param>
		/// <param name="data"> 저장할 정수 데이터 </param>
		public static void SavePlayerPrefs(string key, int data)
		{
			string playerPrefsKey = GetPlayerPrefKey( key );

			PlayerPrefs.SetInt( playerPrefsKey, data );
			PlayerPrefs.Save();
		}

		/// <summary>
		/// playerPref에 데이터를 저장하는 함수
		/// </summary>
		/// <param name="key"> 저장 시에 사용할 키값</param>
		/// <param name="data"> 저장할 소수 데이터 </param>
		public static void SavePlayerPrefs(string key, float data)
		{
			string playerPrefsKey = GetPlayerPrefKey( key );

			PlayerPrefs.SetFloat( playerPrefsKey, data );
			PlayerPrefs.Save();
		}

		/// <summary>
		/// playerPref에 데이터를  암호화하여 저장하는 함수
		/// </summary>
		/// <param name="key"> 저장 시에 사용할 키값</param>
		/// <param name="data"> 저장할 문자열 데이터 </param>
		/// <param name="base64AesKey"> 암호화에 사용할 base64 AES 키 값 </param>
		/// <param name="base64AesIV"> 암호화에 사용할 base64 AES 초기벡터 값 </param>
		public static void SavePlayerPrefs(string key, string data, string base64AesKey, string base64AesIV)
		{
			data = Crypto.EncryptAESbyBase64Key( data, base64AesKey, base64AesIV );
			SavePlayerPrefs( key, data );
		}

		/// <summary>
		/// playerPref에 데이터를 불러오는 함수
		/// </summary>
		/// <param name="key"> 저장 시에 사용한 키값</param>
		/// <param name="data"> 불러올 데이터가 없을 경우 사용할 기본 문자열 </param>
		public static string GetPlayerPrefs(string key, string defaultData)
		{
			string playerPrefsKey = GetPlayerPrefKey( key );
			return PlayerPrefs.GetString( playerPrefsKey, defaultData );
		}

		/// <summary>
		/// playerPref에 데이터를 불러오는 함수
		/// </summary>
		/// <param name="key"> 저장 시에 사용한 키값</param>
		/// <param name="data"> 불러올 데이터가 없을 경우 사용할 기본 정수 값 </param>
		public static int GetPlayerPrefs(string key, int defaultData)
		{
			string playerPrefsKey = GetPlayerPrefKey( key );
			return PlayerPrefs.GetInt( playerPrefsKey, defaultData );
		}

		/// <summary>
		/// playerPref에 데이터를 불러오는 함수
		/// </summary>
		/// <param name="key"> 저장 시에 사용한 키값</param>
		/// <param name="data"> 불러올 데이터가 없을 경우 사용할 기본 소수 값 </param>
		public static float GetPlayerPrefs(string key, float defaultData)
		{
			string playerPrefsKey = GetPlayerPrefKey( key );
			return PlayerPrefs.GetFloat( playerPrefsKey, defaultData );
		}

		/// <summary>
		/// playerPref에 암호화 된 데이터를 불러오는 함수
		/// </summary>
		/// <param name="key"> 저장 시에 사용한 키값</param>
		/// <param name="defaultData"> 데이터가 없을 경우 사용할 기본 문자열 값</param>
		/// <param name="base64AesKey"> 암호화에 사용한 base64 AES 키 값 </param>
		/// <param name="base64AesIV"> 암호화에 사용한 base64 AES 초기벡터 값 </param>
		public static string GetPlayerPrefs(string key, string defaultData, string base64AesKey, string base64AesIV)
		{
			string data = GetPlayerPrefs( key, defaultData );
			if( string.IsNullOrEmpty( data ) == false )
			{
				return Crypto.DecryptAESByBase64Key( data, base64AesKey, base64AesIV );
			}

			return defaultData;
		}

		/// <summary>
		/// 파일에 암호화된 문자열을 저장하는 함수
		/// </summary>
		/// <param name="folderPath">파일의 경로</param>
		/// <param name="fileName">파일의 이름</param>
		/// <param name="extention">파일의 확장자</param>
		/// <param name="data">저장할 데이터</param>
		/// <param name="base64AesKey"> 파일 저장 시에 사용할 base64 AES 키</param>
		/// <param name="base64AesIV"> 파일 저장 시에 사용할 base64 AES 초기 벡터</param>
		public static void WriteFile(string folderPath, string fileName, string extention, string data, string base64AesKey, string base64AesIV)
		{
			data = Crypto.EncryptAESbyBase64Key( data, base64AesKey, base64AesIV );
			WriteFile( folderPath, fileName, extention, data );
		}

		/// <summary>
		/// 파일에 암호화된 문자열을 불러오는 함수
		/// </summary>
		/// <param name="folderPath">파일의 경로</param>
		/// <param name="fileName">파일의 이름</param>
		/// <param name="extention">파일의 확장자</param>
		/// <param name="base64AesKey"> 파일 저장 시에 사용한 base64 AES 키</param>
		/// <param name="base64AesIV"> 파일 저장 시에 사용한 base64 AES 초기 벡터</param>
		public static string ReadFile(string folderPath, string fileName, string extention, string base64AesKey, string base64AesIV)
		{
			string data = ReadFile( folderPath, fileName, extention );
			return Crypto.DecryptAESByBase64Key( data, base64AesKey, base64AesIV );
		}

		/// <summary>
		/// 파일에 문자열을 저장하는 함수
		/// </summary>
		/// <param name="folderPath">파일의 경로</param>
		/// <param name="fileName">파일의 이름</param>
		/// <param name="extention">파일의 확장자</param>
		/// <param name="data">저장할 데이터</param>
		public static void WriteFile(string folderPath, string fileName, string extention, string data)
		{
			File.WriteAllText( Path.Combine( folderPath, string.Format( fileName, extention ) ), data, Encoding.UTF8 );
		}

		/// <summary>
		/// 파일에 문자열을 불러하는 함수
		/// </summary>
		/// <param name="folderPath">파일의 경로</param>
		/// <param name="fileName">파일의 이름</param>
		/// <param name="extention">파일의 확장자</param>
		public static string ReadFile(string folderPath, string fileName, string extention)
		{
			return File.ReadAllText( Path.Combine( folderPath, string.Format( fileName, extention ) ), Encoding.UTF8 );
		}
	}
}