using UnityEngine;
using System.Collections;

public class Cam_Mover : MonoBehaviour {

	public Transform targetPos; 
	public Transform player; 

	Vector3 _prevPos; 
	Vector3 _prevTargetPos; 

	bool _attached = false ;


	public void Attach(){
		_attached = true; 
	}
	public void Unattach(){
		_attached = false; 
	}

	void LerpToTarget(){
		if (_attached) {
			this.transform.position = Vector3.Lerp (this.transform.position, targetPos.position, 1f); 
		}
		else{
			this.transform.position = Vector3.Lerp (this.transform.position, targetPos.position, .2f);
		}
	}
	public void MoveToTarget(){
		transform.position = targetPos.position; 
	}
	void LookAtPlayer(){
		if(_attached){
			this.transform.LookAt (player.position); 
		}
		else{
			Quaternion _startLook = this.transform.rotation;
			transform.LookAt(player.position); 
			Quaternion _endLook = this.transform.rotation; 
			transform.rotation = Quaternion.Lerp(_startLook,_endLook,.3f); 
		}
	}
	void HardAttachRotation(){
		transform.rotation = targetPos.rotation; 
	}
	void SmoothApproachCam(){
		transform.position =  SmoothApproach (_prevPos, _prevTargetPos, targetPos.position, 20f); 
		_prevTargetPos = targetPos.position; 
		_prevPos = this.transform.position;
	}
	Vector3 SmoothApproach( Vector3 pastPosition, Vector3 pastTargetPosition, Vector3 targetPosition, float speed )
	{
		float t = Time.deltaTime * speed;
		Vector3 v = ( targetPosition - pastTargetPosition ) / t;
		Vector3 f = pastPosition - pastTargetPosition + v;
		return targetPosition - v + f * Mathf.Exp( -t );
	}
	void Start(){
		_prevPos = transform.position;
		_prevTargetPos = targetPos.position; 
	}
	public void Update(){
	//	LerpToTarget (); 
	//	LookAtPlayer ();
	//	SmoothApproachCam ();
	//	HardAttachRotation (); 
	}
}
