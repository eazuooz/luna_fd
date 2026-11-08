using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace EverydayDevup.Framework
{
	/// <summary>
	/// UI 데이터를 Inspector창에서 관리하기 위한 Editor
	/// </summary>
	[CustomEditor( typeof( UIAddressable ) )]
	public class UIAddressableEditor : Editor
	{
		UIAddressable uiAddressable;
		

		private void OnEnable()
		{
			uiAddressable = (UIAddressable)target;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			/// UI_TYPE이 삭제 된 경우 기존 UI 데이터에서 없어진 UI를 삭제함
			if( GUILayout.Button( "Check" ) )
			{
				bool isRefresh = false;
				bool isValid = false;
				UIAddressableData uiAddressableData;
				for( int i = uiAddressable.uiList.Count - 1; i >= 0; --i )
				{
					isValid = false;
					uiAddressableData = uiAddressable.uiList[i];
					foreach( UI_TYPE type in Enum.GetValues( typeof( UI_TYPE ) ) )
					{
						if( uiAddressableData.type.Equals( type ) )
						{
							isValid = true;
							break;
						}
					}

					if( isValid == false )
					{
						uiAddressable.uiList.RemoveAt( i );
						isRefresh = true;
					}
				}

				if( isRefresh )
				{
					EditorUtility.SetDirty( uiAddressable );
				}
			}
		}
	}
}
