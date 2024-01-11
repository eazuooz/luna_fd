using UnityEditor;
using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System;

namespace EverydayDevup.Framework
{
	/// <summary>
	/// UI 프리팹에 있는 GameObject를 확인하여, 데이터화 시킴
	/// </summary>
	[CustomEditor( typeof( UIReference ) )]
	public class UIReferenceEditor : Editor
	{
		public class UIReferences
        {

        }

		

		UIReference uiReference;
		StringBuilder stringBuilder = new StringBuilder();

		Dictionary<int, List<UIReferenceData>> dicUIElement 
			= new Dictionary<int, List<UIReferenceData>>();

		//Tuple<int, List<UIReferenceData>> dicUIElement2 = new Tuple<int, List<UIReferenceData>>();

		List<bool> isFoldList = new List<bool>();
		List<string> errorString = new List<string>();

		private void OnEnable()
		{
			uiReference = (UIReference)target;
			Refresh();
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();


			uiReference.uiTarget = EditorGUILayout.ObjectField( "ui target", uiReference.uiTarget, typeof( GameObject ), false ) as GameObject;
			if( GUILayout.Button( "SET NAME" ) )
			{
				if( uiReference.uiTarget != null )
				{
					stringBuilder.Clear();
					stringBuilder.Append( uiReference.uiTarget.name );
					stringBuilder.Append( "_UIReference" );
					string changeName = stringBuilder.ToString();

					string assetPath = AssetDatabase.GetAssetPath( uiReference.GetInstanceID() );
					AssetDatabase.RenameAsset( assetPath, changeName );

					EditorUtility.SetDirty( uiReference );
				}
			}

			if( GUILayout.Button( "REFRESH" ) )
			{
				Refresh();
			}

			DisplaErrorList();
			DisplayUIElementList();
			DisplayGameObjectList();
		}

		void Refresh()
		{
			if( uiReference.uiTarget == null )
			{
				return;
			}

			uiReference.dic.Clear();
			dicUIElement.Clear();
			isFoldList.Clear();
			errorString.Clear();

			GameObject obj;
			UIReferenceData uiReferenceData;
			Transform[] objList = uiReference.uiTarget.GetComponentsInChildren<Transform>();
			for( int i = 0, icount = objList.Length; i < icount; i++ )
			{
				obj = objList[i].gameObject;
				uiReferenceData = AddGameObject( obj );
				AddUIElement( uiReferenceData );
			}

			EditorUtility.SetDirty( uiReference );
		}

		UIReferenceData AddGameObject(GameObject obj)
		{
			UIReferenceData uiReferenceData = new UIReferenceData();
			uiReferenceData.name = obj.name;
			uiReferenceData.obj = obj;
			uiReferenceData.path = GetPath( obj.transform );

			if( uiReference.dic.ContainsKey( uiReferenceData.name ) )
			{
				UIReferenceData containData = uiReference.dic[uiReferenceData.name];
				string errorText = "Exist : " + containData.path + " Add : " + uiReferenceData.path;
				errorString.Add( errorText );
				Debug.LogError( "<color=red>Exist</color> : " + containData.path + " <color=red>Add</color> : " + uiReferenceData.path );
			}
			else
			{
				uiReference.dic.Add( uiReferenceData.name, uiReferenceData );
			}

			uiReference.dic.Add(uiReferenceData.name, uiReferenceData);

			return uiReferenceData;
		}

		void AddUIElement(UIReferenceData data)
		{
			for( int i = 0, icount = uiReference.typeArray.Length; i < icount; i++ )
			{
				isFoldList.Add( true );
				if( data.obj.GetComponent( uiReference.typeArray[i] ) != null )
				{
					if( dicUIElement.ContainsKey( i ) == false )
					{
						dicUIElement.Add( i, new List<UIReferenceData>() );
					}
					dicUIElement[i].Add( data );
				}
			}
		}

		void DisplaErrorList()
		{
			if( errorString.Count > 0 )
			{
				EditorGUILayout.Space();
				EditorGUILayout.HelpBox( "Error", MessageType.Error );
				for( int i = 0, icount = errorString.Count; i < icount; ++i )
				{
					EditorGUILayout.BeginVertical();
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.TextField( errorString[i] );
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.EndVertical();
				}
			}
		}

		void DisplayUIElementList()
		{
			List<UIReferenceData> dataList = null;
			UIReferenceData data;
			Type type;
			int index = 0;
			foreach( KeyValuePair<int, List<UIReferenceData>> pair in dicUIElement )
			{
				index = pair.Key;
				type = uiReference.typeArray[index];
				isFoldList[index] = EditorGUILayout.Foldout( isFoldList[index], type.ToString() );
				if( isFoldList[index] )
				{
					dataList = pair.Value;
					for( int i = 0, icount = dataList.Count; i < icount; ++i )
					{
						data = dataList[i];
						EditorGUILayout.BeginVertical();
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.TextField( data.name );
						EditorGUILayout.TextField( data.path );
						EditorGUILayout.ObjectField( data.obj, type, false );
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.EndVertical();
					}
				}

				EditorGUILayout.Space();
			}
		}

		void DisplayGameObjectList()
		{
			EditorGUILayout.Space();
			EditorGUILayout.HelpBox( "Managed Data", MessageType.Info );
			foreach( KeyValuePair<string, UIReferenceData> pair in uiReference.dic )
			{
				name = pair.Key;

				EditorGUILayout.BeginVertical();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.TextField( pair.Key );
				EditorGUILayout.TextField( pair.Value.path );
				EditorGUILayout.ObjectField( pair.Value.obj, typeof( GameObject ), false );
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical();
			}
		}

		string GetPath(Transform trans)
		{
			string path = "/" + trans.name;
			while( trans.parent != null )
			{
				trans = trans.parent;
				path = "/" + trans.name + path;
			}
			return path;
		}

	}
}