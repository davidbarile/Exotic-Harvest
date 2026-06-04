using UnityEngine;

public class DestroyOnLoad : MonoBehaviour 
{
	private void Awake() 
	{
		Destroy( gameObject );
	}
}