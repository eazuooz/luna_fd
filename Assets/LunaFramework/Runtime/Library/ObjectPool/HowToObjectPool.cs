using System;
using System.Collections.Generic;
using EverydayDevup.Framework;
using UnityEngine;
using System.Collections;


#region EVENT BUS
//public class EventBus : MonoBehaviour
//{
//	public float delay = 1;

//	private ObjectPool<DelayedEvent> eventPool;
//	private List<DelayedEvent> activeEvents;

//	void Awake()
//	{
//		eventPool = new ObjectPool<DelayedEvent>(() => new DelayedEvent(), 5);
//		activeEvents = new List<DelayedEvent>();
//	}

//	void Update()
//	{
//		if (Input.GetMouseButtonDown(0))
//		{
//			SpawnEvent();
//		}

//		for (int i = 0; i < activeEvents.Count; i++)
//		{
//			activeEvents[i].Update(Time.time);
//		}
//	}

//	private void SpawnEvent()
//	{
//		var evt = eventPool.GetItem();
//		evt.Start(Time.time, delay);
//		evt.Triggered += OnEvtComplete;
//		activeEvents.Add(evt);

//	}

//	private void OnEvtComplete(DelayedEvent evt)
//	{
//		evt.Triggered -= OnEvtComplete;
//		activeEvents.Remove(evt);
//		eventPool.ReleaseItem(evt);
//		Debug.Log("Delayed event started at " + evt.startTime + " completed at " + evt.endTime);
//	}

//	private void OnGUI()
//	{
//		GUILayout.BeginArea(new Rect(20, 20, 400, 200));

//		GUILayout.Label("Click Mouse to create Delayed Event");
//		GUILayout.Label("Total used objects: " + eventPool.CountUsedItems);
//		GUILayout.Label("Total objects in pool: " + eventPool.Count);

//		GUILayout.EndArea();
//	}
//}

//public class DelayedEvent
//{
//	public event Action<DelayedEvent> Triggered;

//	public float startTime;
//	public float endTime;
//	private float delay;

//	public void Start(float time, float delay)
//	{
//		this.delay = delay;
//		this.startTime = time;
//	}

//	public void Update(float time)
//	{
//		if (time - startTime > delay)
//		{
//			endTime = time;
//			Trigger();
//		}
//	}

//	public void Trigger()
//	{
//		if (Triggered != null)
//		{
//			Triggered(this);
//		}
//	}
//}
#endregion

#region OBJECT POOL
//public class Bullet : MonoBehaviour
//{
//	public float accel;
//	private float velocity;

//	void OnEnable()
//	{
//		velocity = 0;
//	}

//	void Update()
//	{
//		velocity += accel;

//		transform.Translate(0, velocity, 0);

//		if (transform.position.y > 10)
//		{
//			Finish();
//		}
//	}

//	void Finish()
//	{
//		PoolManager.ReleaseObject(this.gameObject);

//		//Note: 
//		// This takes the gameObject instance, and NOT the prefab instance.
//		// Without this call the object will never be available for re-use!
//		// gameObject.SetActive(false) is automatically called
//	}
//}

//public class Gun : MonoBehaviour
//{
//	public GameObject bulletPrefab;

//	//Optional: Warm the pool and preallocate memory
//	void Start()
//	{
//		PoolManager.WarmPool(bulletPrefab, 20);

//		//Notes
//		// Make sure the prefab is inactive, or else it will run update before first use
//	}

//	void Update()
//	{
//		if (Input.GetButton("Fire1"))
//		{
//			Vector3 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);
//			FireBullet(Camera.main.ScreenToWorldPoint(pos), Quaternion.identity);
//		}
//	}

//	//Spawn pooled objects
//	void FireBullet(Vector3 position, Quaternion rotation)
//	{
//		var bullet = PoolManager.SpawnObject(bulletPrefab, position, rotation).GetComponent<Bullet>();

//		//Notes:
//		// bullet.gameObject.SetActive(true) is automatically called on spawn 
//		// When done with the instance, you MUST release it!
//		// If the number of objects in use exceeds the pool size, new objects will be created
//	}

//}
#endregion


