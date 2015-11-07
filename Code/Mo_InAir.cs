using UnityEngine;
using System.Collections;

public class Mo_InAir : Motion {

	float _targetWeight; 

	void HeavyGravity(){ //increses gravity
		if(_rigid.velocity.normalized.y < -.1f || _rigid.velocity.normalized.y > .1f ){
			_rigid.AddForce (Physics.gravity*1f, ForceMode.Acceleration); 
		}
	}
	void Forward(){ //lets them move forward while they are going forward
		Vector3 xyVelocity = new Vector3 (_rigid.velocity.x, 0, _rigid.velocity.z); 
		if (_verticalInput > 0 && _forwardD.IsGrounded() == false && _player.LastState == "ground") {
			Debug.Log("forward in air"); 
			_rigid.AddRelativeForce(new Vector3(0,0,1)*10,ForceMode.Acceleration); 	
		}
	}
	public override void ControlsInput (){
		base.ControlsInput ();
		HeavyGravity (); 
		StraightenSelf (); 
	}
	void AmbientMotion(){ //gives the player ambient motion when in the air. 
		_targetWeight = Mathf.Clamp (_rigid.velocity.magnitude - 12f, 0f, 100f) / 100f;
		BlendAnimation (_targetWeight, 2); 
	}
	void BlendAnimation(float _target, int _layer){ //smoothly lets you blend a layer
		if (_anim.GetLayerWeight (_layer) < _target) { //we are lowering the number
			_anim.SetLayerWeight(_layer , Mathf.Clamp( _anim.GetLayerWeight(_layer)-.01f,_target,1));	
		}
		if (_anim.GetLayerWeight (_layer) > _target){ //we are going up
			_anim.SetLayerWeight(_layer , Mathf.Clamp( _anim.GetLayerWeight(_layer)+.01f,0,_target));	
		}
	}

	public override void ControlsEffect ()
	{
		base.ControlsEffect ();
		Forward (); 
	}
	public override void EnterState ()
	{
		_anim.SetBool ("Grounded", false); 
		_rigid.drag = .5f;
		_camera.Normal (); 
		_camMover.Unattach (); 
	}
	public override void MotionState ()
	{
		if (_groundD.IsGrounded()) {
			_player.NowGrounded(); 
		}
		if( _player.HasNode()){
			_player.NowAttached ();
		}
	}
	protected override void SetAnim ()
	{
		AmbientMotion (); 
		base.SetAnim ();
	}
	public override void Startup (Player_Controller _thePlayer)
	{
		base.Startup (_thePlayer);
		_state = "air"; 
		_lastState = "air"; 
	}
	public override void ExitState ()
	{
		_anim.SetLayerWeight (2, 0); 
		base.ExitState ();
	}
}
