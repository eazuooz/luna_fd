

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EverydayDevup.Framework
{
	//
	/// <summary>
	/// Framework의 Main Class
	/// </summary>
	public class Game : Singleton<Game>
	{
		/// <summary>
		/// 단말기의 해상도를 관리하는 매니저
		/// </summary>
		public ScreenManager screenManager;
		/// <summary>
		/// Asset을 관리하는 매니저
		/// </summary>
		public ResourcesManager resourcesManager;
		/// <summary>
		/// 암호화를 관리하는 매니저
		/// </summary>
		public CryptoManager cryptoManager;
		/// <summary>
		/// UI를 관리하는 매니저
		/// </summary>
		public UIManager uiManager;
		/// <summary>
		/// 씬간 데이터 이동을 하기 위한 매니저
		/// </summary>
		public DataManager dataManager;
		/// <summary>
		/// 씬 전환 매니저
		/// </summary>
		public LunaSceneManager lunaSceneManager;
		/// <summary>
		/// 라이팅 효과 매니저
		/// </summary>
		public LightManager lightManager;
		/// <summary>
		/// 튜토리얼 매니저
		/// </summary>
		public TutorialManager tutorialManager;
		/// <summary>
		/// Language를 관리하는 매니저
		/// </summary>
		public LanguageManager languageManager;
		/// <summary>
		/// Input system 을 관리하는 매니저
		/// </summary>
		public InputManager inputManager;

		/// <summary>
		/// Sound System 을 관리하는 매니저
		/// </summary>
		public SoundManager soundManager;

		List<IManager> managerList = new List<IManager>();

		//private GameObject gamePlayer;
		//public GameObject GamePlayer
		//      {
		//	get { return gamePlayer; }
		//	set { gamePlayer = value; }
		//      }

		protected virtual void Start()
		{
			managerList.Add( screenManager );
			managerList.Add( resourcesManager );
			managerList.Add( cryptoManager );
			managerList.Add( uiManager );
			managerList.Add( dataManager );
			managerList.Add( lunaSceneManager );
			managerList.Add( lightManager );
			managerList.Add( tutorialManager );
			managerList.Add( languageManager );
			managerList.Add( inputManager );
			managerList.Add( soundManager );

			foreach ( var manager in managerList )
            {
				manager.ManagerInitialize();
            }
		}

		protected virtual void Update()
		{
			IManager manager;
			for( int i = 0, icount = managerList.Count; i < icount; ++i )
			{
				manager = managerList[i];
				if( manager != null )
				{
					manager.ManagerUpdate();
				}
			}


		}

		protected virtual void OnDestroy()
        {
			IManager manager;
			for (int i = 0, icount = managerList.Count; i < icount; ++i)
			{
				manager = managerList[i];
				if (manager != null)
				{
					manager.ManagerClear();
				}
			}
		}

		protected virtual void GameLoop()
		{
			IManager manager;
			for( int i = 0, icount = managerList.Count; i < icount; ++i )
			{
				manager = managerList[i];
				if( manager != null )
				{
					manager.ManagerLoop();
				}
			}
		}

		protected virtual void Clear()
		{
			IManager manager;
			for( int i = 0, icount = managerList.Count; i < icount; ++i )
			{
				manager = managerList[i];
				if( manager != null )
				{
					manager.ManagerClear();
				}
			}
		}

		private IEnumerator Loop()
		{
			WaitForSeconds wfs = new WaitForSeconds( 1f );
			while( true )
			{
				yield return wfs;
				GameLoop();
			}
		}

	}
}
