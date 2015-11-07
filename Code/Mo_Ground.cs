using UnityEngine;
using System.Collections;

public class Mo_Ground : Motion {

	Vector3 _speed; 
	bool _nowJumping; 
	bool _canJump; 

	float _time; 
	float _jumpTime; 

	void MoveForward(){ //controls when they move forward. This whole script should be broken up into different states.
		if (_verticalInput > 0 && _rigid.velocity.magnitude < _maxSpeed){
			_speed.z += 5; 
			//_rigid.AddRelativeForce (new Vector3(0,0,1) * _verticalInput * _acceleration,ForceMode.Acceleration); 
			LookTowardsCamera(); 
		}
	}
	void MoveBackwards(){ //simlar to forward, this is different because I may want to clamp and have different anims and such
		if(_verticalInput < 0 && _rigid.velocity.magnitude < _maxSpeed){
			_speed.z -= 1; 
			//_rigid.AddRelativeForce (new Vector3(0,0,1) * _verticalInput * _acceleration,ForceMode.Acceleration); 
			LookTowardsCamera(); 
		}
	}
	void Strafe(){
		_speed.x += 3 * _horizontalInput; 
		//_rigid.AddRelativeForce (new Vector3 (1, 0, 0) * _horizontalInput * _acceleration, ForceMode.Acceleration); 
	}

	void Jump(){ //can only jump when on the ground. When space is pushed they go up
		if (_jumpInput > 0.5f && _verticalInput > 0 && _anim.GetFloat ("Speed") < 5 && _canJump && !_forwardD.IsGrounded()) {
			//they are moving forward and jumping
			_rigid.AddRelativeForce(new Vector3(0,0,1) * 300,ForceMode.Impulse); 
			_rigid.AddForce(new Vector3(0,1,0)*_jumpPower,ForceMode.Impulse);
			_nowJumping = true; 
			return;
		}
		if (_jumpInput > 0.5f && _canJump) {
			_nowJumping = true; 
			_rigid.AddForce(new Vector3(0,1,0)*_jumpPower,ForceMode.Impulse);
		}
	} 
	void CalculateSpeed(){
		if (_speed == Vector3.zero && _nowJumping == false) { //if there is no input, stop them immediately
			_rigid.velocity = Vector3.zero;  	
		}
		else{
			if(_rigid.velocity.magnitude <= 15) { //speed cap
				_rigid.AddRelativeForce (_speed.normalized * _acceleration, ForceMode.Acceleration); //moves them in their selected direction
			}
		}
	}
	void UseVectorTrigger(){
		if (_eHeld && _player.VTWasUsed() == false) {
			Debug.Log("Pressed E"); 
			_player.TurnOnTrigger(); 
		}
	}
	
	public override void EnterState ()
	{
		_anim.SetBool ("Grounded", true); 
		_rigid.drag = 1;
		_nowJumping = false; 
		_camera.Normal (); 
		_camMover.Unattach ();
		StartJumpDelay ();
	}
	void StartJumpDelay(){
		_canJump = false; 
		_jumpTime = _time + .5f;
		_time = 0; 
	}
	void JumpTimer(){
		if (!_canJump) {
			_time += Time.deltaTime;
			if(_jumpTime >= _time){
				_canJump = true; 
			}
		}
	}
	public override void ControlsEffect ()
	{
		base.ControlsEffect ();
		_speed = Vector3.zero; 
		MoveForward (); 
		MoveBackwards ();
		Strafe (); 
		CalculateSpeed (); 
		Jump ();
		UseVectorTrigger (); 
		StraightenSelf (); 
	}
	public override void ControlsInput ()
	{
		base.ControlsInput ();
		JumpTimer (); 
	}
	public override void MotionState ()
	{
		if (_groundD.IsGrounded() == false) {
			_player.NowInAir(); 
		}
		if( _player.HasNode()){
			_player.NowAttached ();
		}
	}
	public override void Startup (Player_Controller _thePlayer)
	{
		base.Startup (_thePlayer);
		_state = "ground"; 
		_lastState = "ground"; 
	}
}
