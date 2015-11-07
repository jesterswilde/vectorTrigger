using UnityEngine;
using System.Collections;

public class CleaunUp : MonoBehaviour {

	//this script just makes sure we dont' have a bunch of spawned game objects cluttering up the scene

	public float eventDuration = 5; 

	void KillSelf(){
		Destroy (this.gameObject); 
	}

	// after 5 seconds, kill self. 
	void Start () {
		Invoke ("KillSelf", eventDuration); 
	}
}
