using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DestroyOnLoad : MonoBehaviour 
{
	private void Awake() 
	{
		Destroy( gameObject );
	}
}
