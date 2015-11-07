using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

public class Mo_Attached : Motion {

	Node _node; 
	float _magnitude; 
	bool _firstNode = true; 
	bool _enableJump = false; 
	float _totalTime; 
	float _waitTime = .5f; 
	bool _jumping; 

	public void GoingForward(){//cehcks to see if they are traveling forward or backwards along the line
		if (Vector3.Dot (_rigid.velocity.normalized, _node.ForwardVec.normalized) >= 0) {
			_forward = true; 
		}
		else _forward = false;
	}
	public override void EnterState ()
	{
		_rigid.useGravity = false;
		_rigid.drag = 0; 
		_enableJump = false;
		_totalTime = 0; 
		_anim.SetBool ("Grounded", false); 
		_jumping = false; 
		_camera.Attached (); 
		_camMover.Attach (); 
	}
	void JumpingTimer(){ //makes it so they can't jump off a node instantly upon getting on
		if (_enableJump == false) { //only checks when you can't jump
			_totalTime += Time.deltaTime; //when time is allotted, you can jump
			if(_totalTime > _waitTime){
				_enableJump = true; 
			}
		}
	}
	public void SetFirst(){ //resets state 
		_firstNode = true; 
	}
	public override void ExitState ()
	{
		_rigid.useGravity = true; 
		_rigid.drag = 1; //this should probably be modified to be a player variable
		_node = null; 
	}
	void EnterNode(){
		if(_forward){ //player is going forward
			if(_node.HasForwardNode()){ //can they go forward (are there more nodes...this might not be needed to be checked
				_magnitude = _rigid.velocity.magnitude; 
				_rigid.velocity = _node.ForwardVec.normalized * _magnitude; 
			}
		}
		else{ //player is going backwarsd down the node line
			_magnitude = _rigid.velocity.magnitude; 
			_rigid.velocity = _node.BackwardVec.normalized * _magnitude; 
		}
	}
	void AlignPlayerPosition(Vector3 _dir){ //sets the player on an exact spot
		float _dist = Vector3.Distance (_node.transform.position, _player.attachPos.position); 
		Vector3 _targetPos = _node.transform.position + _dir.normalized * _dist; 
		Vector3 _targetDir = _targetPos - _player.attachPos.position;   //the direction from player to target position
		Vector3 _offset = _player.transform.position -  _player.attachPos.position; //get the vector between player and attached point
		if (_forward ||  _forward != true && _targetDir.magnitude > .2f && _dist > 2) {
			_player.transform.position = Vector3.Lerp (_player.transform.position, _targetPos +_offset,.1f); 
			//_player.transform.position = Vector3.Lerp(_player.transform.position ,_targetPos, .1f) ;
		}
	}
	void AlignPlayerRotation(Vector3 _dir){
		float _yRot = _node.transform.rotation.eulerAngles.y;
		if(_forward == false) {
			_yRot = (_yRot +180) %360;
		}
		//Quaternion _target = Quaternion.Euler (_player.transform.rotation.eulerAngles.x,    //use if needed. Old rotation
		 //                                     _yRot, _player.transform.rotation.eulerAngles.z);
		if(_forward){
			_player.transform.rotation = Quaternion.Slerp (_player.transform.rotation, _node.transform.rotation, .1f);
		}
		else{
			_player.transform.rotation = Quaternion.Slerp (_player.transform.rotation, _node.transform.rotation*new Quaternion(0,1,0,0), .2f);
		}
	}
	public override void SetNode(Node _theNode){ //sets the current node
		if (_theNode == null ) { //no longer touching nodes
			if(_node.EndofLine(_forward)){ //you are at the end of hte line
				Debug.Log("end of line"); 
				JumpOffNodes(); 
				return; 
			}
			if(!_node.EndofLine(_forward) && !_jumping){ //the game un attached you cuz physics suck
				if(_forward){
					Debug.Log("Game physics suck"); 
					_player.transform.position = _node.forwardNode.transform.position; 
				}
				else{
					_player.transform.position = _node.backwardNode.transform.position; 
				}
			}
			return; 
		}//the node is not null
		if (_firstNode) {
			_firstNode  = false; 	
			_node = _theNode; 
			GoingForward(); 
			EnterNode(); 
			return; 
		}
		if(_theNode != _node && _theNode != _node.PreviousNode(_forward)){ 
			_node = _theNode; 
			EnterNode (); 
		}
	}
	void Acceleration(){ //player is pushing the button to go faster
		if (_verticalInput > 0 && _rigid.velocity.magnitude < _node.maxSpeed) {
			if(_forward){
				_rigid.AddForce(_node.ForwardVec*_node.speed*Time.deltaTime,ForceMode.Acceleration);
			}
			else {
				_rigid.AddForce(_node.BackwardVec*_node.speed*Time.deltaTime,ForceMode.Acceleration);
			}
		}
		if (_verticalInput < 0 && _rigid.velocity.magnitude > _node.minSpeed) {
			if(_forward){
				_rigid.AddForce(_node.BackwardVec*_node.speed*Time.deltaTime,ForceMode.Acceleration);
			}
			else {
				_rigid.AddForce(_node.ForwardVec*_node.speed*Time.deltaTime,ForceMode.Acceleration);
			}
		}
	}




	//code for directionality of leaving the nodes

	public override void JumpOffNodes(){
		ExitState(); 
		_player.NowInAir();
		_player.TemporarilyDisableNodeD (); 
	}
	void CheckForJumping(){ //jumping unattachs, but you have to have a node to do it, and you have to be allowed to leave
		JumpingTimer (); 
		if(_node != null && _enableJump) JumpLeft ();
		if(_node != null && _enableJump) JumpRight ();
		if(_node != null && _enableJump) JumpDown (); 
		if(_node != null && _enableJump) JumpUp ();

	}
	//these jump conditions are all very similar. If the node allows you to go in that direciton (some may
	//resrict directions because there would be a wall there perhaps) and the player pushed jump, then jump in 
	//the pushed direction. 
	void JumpUp(){ 
		if (_node.CanJumpUp (_forward) && _jumpInput > 0) {
			JumpOff(); 
			_rigid.AddForce(new Vector3(0,_nodeJumpPower * 15,0),ForceMode.Impulse); 
		}
	}
	void JumpLeft(){
		if (_node.CanJumpLeft (_forward) && _horizontalInput < 0 && _jumpInput > 0) {
			JumpOff(); 
			_rigid.AddRelativeForce(new Vector3(_nodeJumpPower * -10,_nodeJumpPower *10,0),ForceMode.Impulse); 
		}
	}
	void JumpRight(){
		if (_node.CanJumpRight (_forward) && _horizontalInput > 0 && _jumpInput > 0) {
			JumpOff(); 
			_rigid.AddRelativeForce(new Vector3(_nodeJumpPower * 10,_nodeJumpPower * 10,0),ForceMode.Impulse); 
		}	
	}
	void JumpDown(){
		if (_node.CanJumpDown (_forward) && _verticalInput < 0 && _jumpInput > 0) {
			JumpOff(); 
			_rigid.AddForce(new Vector3(0,_nodeJumpPower *-5 ,0),ForceMode.Impulse); 
		}
	}
	void JumpOff(){ //the meat of a leaving a node
		_jumping = true;
		ExitState();
		_player.NowInAir();
		_player.TemporarilyDisableNodeD(); 
	}

	void SpeedMinMax(){
		if (_rigid.velocity.magnitude < _node.minSpeed) {
			_rigid.velocity = Vector3.Lerp (_rigid.velocity, _rigid.velocity.normalized *_node.minSpeed, Time.deltaTime);		
		}
		if (_rigid.velocity.magnitude > _node.maxSpeed) {
			_rigid.velocity = Vector3.Lerp (_rigid.velocity, _rigid.velocity.normalized *_node.maxSpeed, Time.deltaTime);	
		}
	}

	public override void ControlsEffect ()
	{
		base.ControlsEffect ();
		Acceleration (); 
		AlignPlayerPosition (_node.ForwardVec); 
		AlignPlayerRotation (_node.ForwardVec); 
		SpeedMinMax (); 
	}

	public override void MotionState ()
	{
		CheckForJumping (); 
	}
	public override void Startup (Player_Controller _thePlayer)
	{
		base.Startup (_thePlayer);
		_usesNodes = true; 
		_state = "attached"; 
		_lastState = "attached"; 
	}
}
