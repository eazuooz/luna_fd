using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EverydayDevup.Framework
{
	/// <summary>
	/// 암호화에 사용할 키 값을 Inspector창에서 보여주기 위한 Editor
	/// </summary>
	[CustomEditor( typeof( CryptoData ) )]
	public class CryptoEditor : Editor
	{
		CryptoData _cryptoData;
		int _aesKeySize;
		string _aesKey;

		int _aesIVSize;
		string _aesIV;

		private void OnEnable()
		{
			_cryptoData = (CryptoData)target;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			OnAesUI();

			EditorGUILayout.Space();
			OnRsaUI();
		}

		void OnAesUI()
		{
			OnAesKeyUI();
			OnAesIVUI();
		}

		void OnAesKeyUI()
		{
			if( string.IsNullOrEmpty( _aesKey ) )
			{
				if( string.IsNullOrEmpty( _cryptoData.aesBase64Key ) == false )
				{
					_aesKey = Crypto.DecodingBase64( _cryptoData.aesBase64Key );
				}
				else
				{
					_aesKey = string.Empty;
				}
			}

			_aesKeySize = Encoding.UTF8.GetByteCount( _aesKey ) * 8;
			EditorGUILayout.IntField( "AES Key Size", _aesKeySize );
			_aesKey = EditorGUILayout.TextField( "AES Key", _aesKey );
			EditorGUILayout.TextField( "AES Base64 Key", _cryptoData.aesBase64Key );

			if( GUILayout.Button( "Convert" ) )
			{
				bool isValid = false;
				string validKeySizeText = string.Empty;

				int validKeySize = 0;
				for( int i = 0, icount = CryptoAES.aesKeySize.Length; i < icount; i++ )
				{
					validKeySize = CryptoAES.aesKeySize[i];
					validKeySizeText += validKeySize.ToString() + " ";
					if( _aesKeySize.Equals( validKeySize ) )
					{
						isValid = true;
						break;
					}
				}

				if( isValid )
				{
					_cryptoData.aesBase64Key = Crypto.EncodingBase64( _aesKey );
					EditorUtility.SetDirty( _cryptoData );
				}
				else
				{

					EditorUtility.DisplayDialog( "key size error", string.Format( "key size {0}", validKeySizeText ), "ok" );
				}
			}
		}

		void OnAesIVUI()
		{
			if( string.IsNullOrEmpty( _aesIV ) )
			{
				if( string.IsNullOrEmpty( _cryptoData.aesBase64IV ) == false )
				{
					_aesIV = Crypto.DecodingBase64( _cryptoData.aesBase64IV );
				}
				else
				{
					_aesIV = string.Empty;
				}
			}

			_aesIVSize = Encoding.UTF8.GetByteCount( _aesIV ) * 8;
			EditorGUILayout.IntField( "AES IV Size", _aesIVSize );

			_aesIV = EditorGUILayout.TextField( "AES IV", _aesIV );
			EditorGUILayout.TextField( "AES Base64 Key", _cryptoData.aesBase64IV );

			if( GUILayout.Button( "Convert" ) )
			{
				if( _aesIVSize == CryptoAES.aesIVSize )
				{
					_cryptoData.aesBase64IV = Crypto.EncodingBase64( _aesIV );
					EditorUtility.SetDirty( _cryptoData );
				}
				else
				{
					EditorUtility.DisplayDialog( "iv size error", string.Format( "IV Size {0}", CryptoAES.aesIVSize ), "ok" );
				}
			}
		}

		void OnRsaUI()
		{
			EditorGUILayout.TextField( "RSA Base64 Public Key", _cryptoData.rsaBase64PublicKey );
			EditorGUILayout.TextField( "RSA Base64 Private Key", _cryptoData.rsaBase64PrivateKey );

			if( GUILayout.Button( "Create" ) )
			{
				RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider();

				RSAParameters privateKeyParam = RSA.Create().ExportParameters( true );
				rsaCryptoServiceProvider.ImportParameters( privateKeyParam );
				string privateKey = rsaCryptoServiceProvider.ToXmlString( true );
				_cryptoData.rsaBase64PrivateKey = Crypto.EncodingBase64( privateKey );

				RSAParameters publicKeyParam = new RSAParameters();
				publicKeyParam.Modulus = privateKeyParam.Modulus;
				publicKeyParam.Exponent = privateKeyParam.Exponent;
				rsaCryptoServiceProvider.ImportParameters( publicKeyParam );
				string publicKey = rsaCryptoServiceProvider.ToXmlString( false );
				_cryptoData.rsaBase64PublicKey = Crypto.EncodingBase64( publicKey );

				EditorUtility.SetDirty( _cryptoData );
			}

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.TextField( "RSA Save/Load Path", _cryptoData.folderPath );
			if( GUILayout.Button( "Select" ) )
			{
				_cryptoData.folderPath = EditorUtility.OpenFolderPanel( "Select Folder", _cryptoData.folderPath, "" );
			}
			EditorGUILayout.EndHorizontal();

			_cryptoData.fileName = EditorGUILayout.TextField( "RSA FileName", _cryptoData.fileName );
			if( GUILayout.Button( "Save" ) )
			{
				if( string.IsNullOrEmpty( _cryptoData.folderPath ) )
				{
					EditorUtility.DisplayDialog( "error", "folder path empty", "ok" );
				}
				else if( string.IsNullOrEmpty( _cryptoData.fileName ) )
				{
					EditorUtility.DisplayDialog( "error", "file name empty", "ok" );
				}
				else
				{
					string data = _cryptoData.rsaBase64PublicKey + "\n" + _cryptoData.rsaBase64PrivateKey;
					FileUtil.WriteFile( _cryptoData.folderPath, _cryptoData.fileName, ".txt", data, _cryptoData.aesBase64Key, _cryptoData.aesBase64IV );
				}
			}

			if( GUILayout.Button( "Load" ) )
			{
				if( string.IsNullOrEmpty( _cryptoData.folderPath ) )
				{
					EditorUtility.DisplayDialog( "error", "folder path empty", "ok" );
				}
				else if( string.IsNullOrEmpty( _cryptoData.fileName ) )
				{
					EditorUtility.DisplayDialog( "error", "file name empty", "ok" );
				}
				else
				{
					string data = FileUtil.ReadFile( _cryptoData.folderPath, _cryptoData.fileName, ".txt", _cryptoData.aesBase64Key, _cryptoData.aesBase64IV );
					string[] splitData = data.Split( '\n' );

					_cryptoData.rsaBase64PublicKey = splitData[0];
					_cryptoData.rsaBase64PrivateKey = splitData[1];
				}
			}
		}
	}
}