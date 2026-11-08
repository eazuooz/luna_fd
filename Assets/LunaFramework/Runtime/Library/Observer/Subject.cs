using System.Collections.Generic;
using UnityEngine;

namespace EverydayDevup.Framework
{
    public class Subject : ISubject
    {
        private readonly List<IObserver> observers = new List<IObserver>();
        private int observerId;
        public void AddObserver(IObserver observer)
        {
            if (observer == null || observers.Contains(observer)) return;
            observer.ID = observerId++;
            observers.Add(observer);
            observers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }
        public void RemoveObserver(IObserver observer)
        {
            if (observer != null) observers.Remove(observer);
        }
        public void OnNotify()
        {
            // UI callbacks can open/close forms and change subscriptions while notifying.
            foreach (var observer in observers.ToArray())
            {
                if (!observers.Contains(observer)) continue;
                if (observer is Object unityObject && unityObject == null)
                {
                    observers.Remove(observer);
                    continue;
                }
                observer.OnResponse(this);
            }
        }
        public void OnClear() { observers.Clear(); observerId = 0; }
    }
}
