using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Motion {

	protected Player_Controller _player;
	protected Rigidbody _rigid; 
	protected Animator _anim; 
	protected Camera_Controller _camera; 
	protected Ground_Detection _forwardD; 

	protected float _verticalInput; 
	protected float _horizontalInput; 
	protected float _jumpInput; 
	protected float _nodeJumpPower; 
	protected bool _eDown;
	protected bool _eHeld; 

	protected float _maxSpeed; 
	protected float _acceleration;
	protected float _jumpPower;
	protected float _turnSpeed; 
	protected UnityEngine.UI.Text _speedText; 



	protected Transform _cameraTrans; 
	protected Ground_Detection _groundD; 
	protected Node_Detector _nodeD; 
	protected Cam_Mover _camMover; 

	protected bool _usesNodes = false; 
	protected bool _nodeJumpUp = false; 
	protected bool _nodeJumpLeft = false; 
	protected bool _nodeJumpRight = false; 
	protected bool _nodeJumpDown = false; 

	protected bool _forward = true; 
	public bool Direction {get{return _forward;}} 
	protected string _state; 
	protected string _lastState; 
	public string LastState { get { return _lastState; } }
	public string CurrentState { get { return _state; } } 

	//this bit is just to get all the variables that I need taken from the player controller script

	public virtual void Startup(Player_Controller _thePlayer){ //sets all the variables
		_player = _thePlayer; 
		_rigid = _thePlayer.GetComponent<Rigidbody> (); 
		_maxSpeed = _player.maxSpeed;
		_acceleration = _player.acceleration; 
		_jumpPower = _player.jumpPower; 
		_nodeJumpPower = _player.nodeJumpPower; 
		_turnSpeed = _player.turnSpeed; 
		_cameraTrans = _player.cameraTrans; 
		_camera = _cameraTrans.GetComponent<Camera_Controller> (); 
		_groundD = _player._groundD; 
		_anim = _player.anim; 
		_speedText = _player.speedText; 
		_camMover = _player.camMover; 
		_forwardD = _player._forwardD; 
		//_nodeD = _player._nodeD; 
	}

	public virtual void ControlsEffect(){ //the base bit is just to get input
		 
	}
	public virtual void ControlsInput(){
		SetAxis ();
		SetAnim ();
	}
	protected virtual void SetAnim(){ //gives the animator information every update
		_anim.SetBool ("InFrontOfWall", _player._forwardD.IsGrounded ()); 
		_anim.SetFloat ("Speed", _rigid.velocity.magnitude); 
		int speedInt = (int)_rigid.velocity.magnitude; 
		if(_speedText!= null){
			_speedText.text = "Speed: " + speedInt.ToString (); 
		}
		if (_jumpInput == 1)
						_anim.SetBool ("Jumping", true);
				else
						_anim.SetBool ("Jumping", false); 
		float yVel = Mathf.Clamp (_rigid.velocity.normalized.y, -1, 0) * _rigid.velocity.magnitude*  -1; //get the downard velocity
		_anim.SetFloat ("yVelocity", yVel); 
		if (_anim.GetCurrentAnimatorStateInfo (0).IsName ("JumpLoop")) {
			_anim.SetBool("InAirLoop",true);
		}
		else{
			_anim.SetBool("InAirLoop",false); 
		}

	}
	void SetAxis(){ //gets all inputs
		_verticalInput = Input.GetAxisRaw ("Vertical");
		_horizontalInput = Input.GetAxisRaw ("Horizontal"); 
		_jumpInput = Input.GetAxisRaw ("Jump"); 
		_eDown = Input.GetKeyDown (KeyCode.E); 
		_eHeld = Input.GetKey (KeyCode.E); 
	}
	public virtual void MotionState(){ //the states that can be entered from this place

	}

	//mostly book keeping
	public virtual void EnterState(){
		_player.CurrentState = _state; 
	}
	public virtual void ExitState(){
		_player.LastState = _state; 
	}
	/*public virtual bool LastState(string _name){
		if (_name == _state) {
			return true; 		
		}
		return false; 
	}*/
	//node related stuff
	public virtual bool UsesNodes(){
		return _usesNodes; 
	}
	public virtual void SetNode(Node _node){
		
	}
	public virtual void JumpOffNodes(){
	}
	public virtual void StraightenSelf(){ //sometimes the player is at a wonky angle after being a rail, this fixes that. 
		float xValue;
		if (_player.transform.rotation.eulerAngles.x > 90)
						xValue = 360;
				else
						xValue = 0; 
		_player.transform.rotation = Quaternion.Euler(new Vector3 (xValue, _player.transform.rotation.eulerAngles.y, 0));  
	}
	public virtual void LookTowardsCamera(){ //called when you move forwards or backwards
		Quaternion _targetRotation =  Quaternion.Euler(_player.transform.rotation.eulerAngles.x, _cameraTrans.rotation.eulerAngles.y, _player.transform.rotation.eulerAngles.z);
		_player.transform.rotation = Quaternion.Slerp (_player.transform.rotation, _targetRotation, _turnSpeed); 
	}
	public virtual void LookTowardsCamera(float _turning){
		Quaternion _targetRotation =  Quaternion.Euler(_player.transform.rotation.eulerAngles.x, _cameraTrans.rotation.eulerAngles.y, _player.transform.rotation.eulerAngles.z);
		_player.transform.rotation = Quaternion.Lerp(_player.transform.rotation, _targetRotation, _turning); 
	}
}
