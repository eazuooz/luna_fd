using UnityEngine;
using System.Linq;

namespace EverydayDevup.Framework
{
	/// <summary>
	/// 해당 Class를 상속받는 것만으로 Singleton Class를 구현할 수 있도록 만든 템플릿 클래스
	/// </summary>
	/// <typeparam name="T">Singleton으로 구현할 Class의 타입</typeparam>
	public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		private static bool m_ShuttingDown = false;
		private static object m_Lock = new object();
		private static T m_Instance;

		public static T Instance
		{
			get
			{
				/// 어플리케이션이 종료된 상태에서 Singleton이 만들어 지는 것을 막기위함 
				if( m_ShuttingDown )
				{
					return null;
				}

				/// 스레드 안정성을 위해 생성 시 Lock을 걸어둠
				lock( m_Lock )
				{
					if( m_Instance == null )
					{
						m_Instance = (T)FindObjectOfType( typeof( T ) );

						GameObject singletonObject = null;
						if( m_Instance == null )
						{
							singletonObject = new GameObject();
							m_Instance = singletonObject.AddComponent<T>();
						}
						else
						{
							singletonObject = m_Instance.gameObject;
						}

						DontDestroyOnLoad( singletonObject );
						singletonObject.name = typeof( T ).ToString() + " (Singleton)";
					}

					return m_Instance;
				}
			}
		}

		private void OnApplicationQuit()
		{
			m_ShuttingDown = true;
		}

		private void OnDestroy()
		{
			m_ShuttingDown = true;
		}
	}

	public class SingletonSO<T> : ScriptableObject where T : ScriptableObject
	{
		private static bool m_ShuttingDown = false;
		private static object m_Lock = new object();
		private static T m_Instance;

		public static T Instance
		{
			get
			{
				/// 어플리케이션이 종료된 상태에서 Singleton이 만들어 지는 것을 막기위함 
				if (m_ShuttingDown)
				{
					return null;
				}

				/// 스레드 안정성을 위해 생성 시 Lock을 걸어둠
				lock (m_Lock)
				{
					if (m_Instance == null)
					{
						m_Instance = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();

						T singletonObject = null;
						if (m_Instance == null)
						{
							singletonObject = CreateInstance<T>();
							m_Instance = singletonObject;
						}
						else
						{
							singletonObject = m_Instance;
						}

						DontDestroyOnLoad(singletonObject);
						singletonObject.name = typeof(T).ToString() + " (Singleton)";
					}

					return m_Instance;
				}
			}
		}

		private void OnApplicationQuit()
		{
			m_ShuttingDown = true;
		}

		private void OnDestroy()
		{
			m_ShuttingDown = true;
		}
	}
}