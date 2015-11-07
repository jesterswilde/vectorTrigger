using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour {

	public int id; 
	public Transform spawnPoint;

	void OnTriggerEnter(Collider _collider ){
		if (_collider.gameObject.layer == 10) {
			if(_collider.gameObject.GetComponent<Player_Controller>() != null){
				_collider.gameObject.GetComponent<Player_Controller>().SetCheckpoint(this); 
			}
		}
	}

}
