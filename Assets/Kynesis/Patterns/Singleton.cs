using UnityEngine;

namespace Kynesis.Patterns
{
	[DefaultExecutionOrder(-100)]
	public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		public static T Instance { get; private set; }
	
		protected virtual void Awake ()
		{
			if (Instance != null) Destroy(gameObject);
			else Instance = this as T;
		}
	}
	
	[DefaultExecutionOrder(-100)]
	public abstract class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
	{
		protected override void Awake()
		{
			base.Awake();
		
			transform.SetParent(null);
			DontDestroyOnLoad(gameObject);
		}
	}
}