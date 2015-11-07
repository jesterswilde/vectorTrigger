using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

public class Camera_Controller : MonoBehaviour {

	public Transform player; 
	public Transform theCam; 
	public Cam_Mover camMover; 
	public float stationaryDistance;
	public float speedFactor; 
	public float _currentCamDistance; 
	Vector3 _targetDir; 
	Vector3 _targetPos; 
	public float orbitSpeed; 
	public LayerMask cameraCollision; 
	
	OrbitMethod _orbit; 
	NormalOrbit _normalOrbit;
	FreefallOrbit _freeFallingOrbit;
	AttachedOrbit _attachedOrbit; 

	Rigidbody _rigid; 


	public void Normal(){
		_orbit = _normalOrbit; 
	}
	public void Freefall(){
		_orbit = _freeFallingOrbit; 
	}
	public void Attached(){
		_orbit = _attachedOrbit; 
	}

	void FollowPlayer(){
		transform.position = player.position; 
	}

	void CameraDistance(){ //figures out where walls are between the player and camera
		Ray _ray = new Ray (this.transform.position, theCam.position - this.transform.position); 
		RaycastHit _hit; 
		if (Physics.Raycast (_ray, out _hit, stationaryDistance, cameraCollision)) {
			_currentCamDistance = Mathf.Clamp(Vector3.Distance (_hit.point,this.transform.position),3f, stationaryDistance) -.5f;  
		} 
		else{
			_currentCamDistance = (Mathf.Clamp(stationaryDistance-10,0,100) +_rigid.velocity.magnitude) *speedFactor + stationaryDistance;
			//_currentCamDistance = stationaryDistance;
		}
	}
	void PositionCamera(){ // moves the camera into place with a lerp
		_targetDir =  theCam.position - transform.position; 
		theCam.position = Vector3.Lerp(theCam.position, transform.position + _targetDir.normalized * _currentCamDistance,.1f); 
	}
	void Awake(){
		_targetPos = theCam.position; 


		_attachedOrbit = new AttachedOrbit ();
		_freeFallingOrbit = new FreefallOrbit (); 
		_normalOrbit = new NormalOrbit (); 
		_attachedOrbit.Startup (this);
		_freeFallingOrbit.Startup (this);
		_normalOrbit.Startup (this); 
		_orbit = _normalOrbit; 

		_rigid = player.GetComponent<Rigidbody> (); 

	}

	// Update is called once per frame
	void Update () {
		_orbit.Orbit (); 
		FollowPlayer (); 
		CameraDistance (); 
		PositionCamera (); 
	}
}
/*
	void Orbit(){
		if(!_locked){
			float _yRot = transform.rotation.eulerAngles.y + Input.GetAxis ("Mouse X") * orbitSpeed;
			float _xRot = transform.rotation.eulerAngles.x + Input.GetAxis ("Mouse Y") * orbitSpeed*-1;
			transform.rotation = Quaternion.Euler (ClampValue(_xRot,60,50), _yRot, 0); 
		}
		else{
			Quaternion _rotateTarget = Quaternion.Euler (new Vector3(60,transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z));
			transform.rotation = Quaternion.Lerp(transform.rotation,_rotateTarget, .02f);
		}
		float ClampValue(float _value, float _max, float _min){
		if (_value >= _max+50 && _value <= 360-_min) {
			return 360-_min;		
		}
		if (_value >= _max && _value <= _max+50) {
			return _max; 	
		}
		return _value; 
	}
	}*/






public class OrbitMethod{
	protected Transform _camTrans; 
	protected float _orbitSpeed; 
	protected Player_Controller _player; 
	protected Rigidbody _rigid; 

	public virtual void Startup(Camera_Controller _controller){
		_camTrans = _controller.transform;
		_orbitSpeed = _controller.orbitSpeed; 
		_player = _controller.player.GetComponent<Player_Controller> ();
		_rigid = _player.gameObject.GetComponent<Rigidbody> (); 
	}
	public virtual void Orbit(){
		
	}
	public virtual float ClampValue(float _value, float _max, float _min){
		if (_value >= _max+50 && _value <= 360-_min) {
			return 360-_min;		
		}
		if (_value >= _max && _value <= _max+50) {
			return _max; 	
		}
		return _value; 
	}
}


public class NormalOrbit : OrbitMethod {
	public override void Orbit ()
	{
		float _yRot = _camTrans.rotation.eulerAngles.y + Input.GetAxis ("Mouse X") * _orbitSpeed;
		float _xRot = _camTrans.rotation.eulerAngles.x + Input.GetAxis ("Mouse Y") * _orbitSpeed*-1;
		_camTrans.rotation = Quaternion.Euler (ClampValue(_xRot,70,70), _yRot, 0); 
	}
}


public class FreefallOrbit : OrbitMethod{
	public override void Orbit ()
	{
		Quaternion _rotateTarget = Quaternion.Euler (new Vector3(60,_camTrans.rotation.eulerAngles.y, _camTrans.rotation.eulerAngles.z));
		_camTrans.rotation = Quaternion.Lerp(_camTrans.rotation,_rotateTarget, .02f);
	}
}


public class AttachedOrbit : OrbitMethod{
	List<Vector3> lookAtTargets = new List<Vector3>(); 
	GameObject _pointer; 
	public override void Startup (Camera_Controller _controller)
	{
		base.Startup (_controller);
		_pointer = GameObject.CreatePrimitive (PrimitiveType.Cube); 
		_pointer.GetComponent<BoxCollider> ().enabled = false; 
	}
	public override void Orbit ()
	{
		if(_player.CurrentNode()!= null){
			LookAtThese();
			Quaternion _prevLookAt = _camTrans.transform.rotation; 
			_camTrans.LookAt (GetVectorAverage()); 
			Quaternion _nextLookAt = _camTrans.transform.rotation; 
			_camTrans.transform.rotation = Quaternion.Slerp(_prevLookAt,_nextLookAt,Time.deltaTime*_rigid.velocity.magnitude/10); 
		}
	}
	void LookAtThese(){ //adds all the relevant points to be looking at. 
		lookAtTargets.Clear (); 
		lookAtTargets.Add (_player.transform.position);
		if (_player.UsesNodes ()) {
			if(_player.NextNode() != null){
				if(_player.NextNode().NextNode(_player.Direction) != null){
					TraceDistanceLines(_player.transform.position, _player.NextNode(), _rigid.velocity.magnitude); 
				}
			}
			else{
				lookAtTargets.Add (_player.transform.position + _rigid.velocity); 
			}
		}
	}
	void TraceDistanceLines(Vector3 currentPosition, Node _nextNode, float remainingSpeed){ //runs down the line adding nearby nodes depending on speed
		if(_nextNode != null){
			float _distance = Vector3.Distance (currentPosition, _nextNode.transform.position); 
			remainingSpeed -= _distance; 
			if (remainingSpeed > 0) { //If you have more speed, find the next node
				lookAtTargets.Add(_nextNode.transform.position);  //add next node
				TraceDistanceLines(_nextNode.transform.position, _nextNode.NextNode(_player.Direction),remainingSpeed); //rinse repeat with remainder
			}
		}
	}

	Vector3 GetVectorAverage(){
		float avgX = 0;
		float avgY = 0;
		float avgZ = 0; 
		for (int i = 0; i < lookAtTargets.Count; i++) {
			avgX += lookAtTargets[i].x;
			avgY += lookAtTargets[i].y;
			avgZ += lookAtTargets[i].z; 
		}
		avgX /= lookAtTargets.Count; 
		avgY /= lookAtTargets.Count; 
		avgZ /= lookAtTargets.Count; 
		return new Vector3(avgX,avgY,avgZ); 
	}
}
