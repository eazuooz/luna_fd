using System.Collections.Generic;
using UnityEngine;

namespace EverydayDevup.Framework
{
	/// <summary>
	/// 관찰 대상의 기본 로직
	/// </summary>
	public class Subject : ISubject
	{
		/// <summary>
		/// 관찰자를 관리하는 리스트
		/// </summary>
		List<IObserver> observers;

		/// <summary>
		/// 관찰자가 추가될 때 부여되는 아이디
		/// </summary>
		int observerId = 0;

		/// <summary>
		/// 관찰자를 추가
		/// </summary>
		/// <param name="addObserver">추가할 관찰자</param>
		public void AddObserver(IObserver addObserver)
		{
			if( observers == null )
			{
				observers = new List<IObserver>();
				observerId = 0;
			}

			if (observers.Contains(addObserver))
			{
				return;
			}

			addObserver.ID = observerId++;
			observers.Add( addObserver );
			observers.Sort( ComparePriority );
		}

		/// <summary>
		/// 설정된 priority에 따라 관찰자의 순서를 정함
		/// </summary>
		int ComparePriority(IObserver a, IObserver b)
		{
			return a.Priority.CompareTo( b.Priority );
		}

		/// <summary>
		/// 관찰자를 제거
		/// </summary>
		/// <param name="removeObserver">제거할 관찰자</param>
		public void RemoveObserver(IObserver removeObserver)
		{
			IObserver observer;
			for( int i = observers.Count - 1; i >= 0; i-- )
			{
				observer = observers[i];
				if( observer.ID.Equals( removeObserver.ID ) )
				{
					observers.RemoveAt( i );
				}
			}
		}

		/// <summary>
		/// 관찰 대상에게 변화가 있을 경우 호출되는 함수
		/// </summary>
		public void OnNotify()
		{
			if( observers != null )
			{
				IObserver obserber;
				for( int i = 0, icount = observers.Count; i < icount; i++ )
				{
					obserber = observers[i];
					if( obserber != null )
					{
						obserber.OnResponse( this );
					}
				}
			}
		}

		/// <summary>
		/// 관찰 대상이 삭제될 때 호출되는 함수
		/// </summary>
		public void OnClear()
		{
			if( observers != null )
			{
				observers.Clear();
			}
			observers = null;
			observerId = 0;
		}
	}
}