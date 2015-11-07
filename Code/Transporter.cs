using UnityEngine;
using System.Collections;

public class Transporter : MonoBehaviour {

	public Rigidbody rigid; 
	Vector3 _collisionPoint; 
	public Vector3 Point{ get { return _collisionPoint; } }

	public void Inactive(){
		Destroy (this.gameObject); 
	}
	void Landed(){
		rigid.isKinematic = true; 
		GetComponent<SphereCollider> ().isTrigger = true; 
	}
	// Use this for initialization
	void Awake () {
		rigid = GetComponent<Rigidbody> ();  
	}
	
	void OnCollisionEnter(Collision _collider){
		if(_collider.gameObject.layer != 10){
			_collisionPoint =  _collider.contacts[0].point - this.transform.position;
			Landed(); 
		}
	}
}
