using UnityEditor;
using UnityEngine;

namespace EverydayDevup.Framework
{
	/// <summary>
	/// 2D 카메라의 설정값을 Inspector에서 보여주기 위한 Editor
	/// </summary>
	[CustomEditor( typeof( Camera2D ) )]
	public class Camera2DEditor : Editor
	{
		Camera2D _cam2D;
		Vector2 _camSize = Vector2.zero;

		private void OnEnable()
		{
			_cam2D = (Camera2D)target;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			/// 카메라의 최소 가로 크기
			_cam2D.minWidth = EditorGUILayout.IntField( "min width", _cam2D.minWidth );
			/// 카메라의 최소 세로 크기
			_cam2D.minHeight = EditorGUILayout.IntField( "min height", _cam2D.minHeight );
			/// 가로 크기 고정인 경우 true, 세로 크기 고정인 경우 false
			_cam2D.matchWidth = EditorGUILayout.Toggle( "match width", _cam2D.matchWidth );

			/// 어플리케이션 플레이 시, 해상도에 따라 설정된 스크린의 크기를 보여줌 
			if( Application.isPlaying )
			{
				EditorGUILayout.IntField( "screen width", _cam2D.screenWidth );
				EditorGUILayout.IntField( "screen height", _cam2D.screenHeight );
			}
		}

	}
}
