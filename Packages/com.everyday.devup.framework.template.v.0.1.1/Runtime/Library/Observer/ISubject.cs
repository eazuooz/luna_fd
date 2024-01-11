using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EverydayDevup.Framework
{
	/// <summary>
	/// 관찰 대상 : 관찰자가 관찰하는 대상 
	/// </summary>
	public interface ISubject
	{
		/// <summary>
		/// 자신을 관찰할 관찰자를 등록
		/// </summary>
		/// <param name="observer">관찰자</param>
		void AddObserver(IObserver observer);
		/// <summary>
		/// 자신을 관찰하는 관찰자를 제거
		/// </summary>
		/// <param name="observer"></param>
		void RemoveObserver(IObserver observer);

		/// <summary>
		/// 관찰 대상의 변화가 발생했을 때 관찰자들에게 알려줌
		/// </summary>
		void OnNotify();

		/// <summary>
		/// 관찰 대상이 삭제될 때 등록된 관찰자를 없앰
		/// </summary>
		void OnClear();
	}
}
