using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine.UI; 

public class Player_Controller : MonoBehaviour {

	//bits to modify in the inspector
	public float maxSpeed;
	public float acceleration;
	public float jumpPower; 
	public float nodeJumpPower; 
	public float turnSpeed; 

	//all the inputs from the player (sans mouse...this might go here later
	float _verticalInput;
	float _horizontalInput;
	float _jumpInput; 

	//all the components
	public Transform cameraTrans; 
	public Cam_Mover camMover; 
	Camera_Controller _camera; 
	public Ground_Detection _groundD; 
	public Ground_Detection _forwardD; 
	Node_Detector _currentNodeD; 
	public List<Node_Detector> nodeDList; 
	Rigidbody _rigid;
	public Animator anim; 
	SkinnedMeshRenderer[] _meshes; 
	 

	//all the states, and hte state they go into
	Mo_Ground _moGround = new Mo_Ground(); 
	Mo_InAir _moInAir = new Mo_InAir (); 
	Mo_Attached _moAttached = new Mo_Attached (); 
	Mo_Stasis _moStasis = new Mo_Stasis(); 
	Mo_Anim _moAnim = new Mo_Anim (); 
	Motion _move; 
	string _state; 
	string _lastState;
	public string LastState { get { return _lastState; } set { _lastState = value; } }
	public string CurrentState{ get { return _state; } set { _state = value; } } 

	//node stuffs
	public List<Node> attachedNodes = new List<Node>(); 
	public Transform attachPos; 


	//transporter stuffs
	public Object transporterLeaveFX;
	public Object transporArriveFX;
	public Object transporterPrefab; 
	public Transform thorwOrigin;
	Transporter _port; 
	bool _canTeleport = true; 
	public float throwUp;
	public float throwForward; 
	float _lowerBodyBlendTarget; 
	public UnityEngine.UI.Text speedText; 
	Vector3 _heldSpeed; 

	//Checkpoint stuff
	Checkpoint _lastCheckpoint;


	//this is the housing for all motion controls
	//they are broken into different motion classes that are switched to based on conditions. 

	public void NowInAir(){
		if(_move.CurrentState != _moStasis.CurrentState){
			_move.ExitState (); 
		}
		_move = _moInAir; 
		_move.EnterState ();
	}

	public void NowGrounded(){
		if(_move.CurrentState != _moStasis.CurrentState){
			_move.ExitState (); 
		}
		_move = _moGround; 
		_move.EnterState (); 
	}
	public void NowAttached(){
		if(_move.CurrentState != _moStasis.CurrentState){
			_move.ExitState (); 
		}
		_move = _moAttached; 
		_moAttached.SetFirst (); 
		_moAttached.SetNode (CurrentNode());
		_moAttached.EnterState (); 
	}
	public void NowStasis(){
		_move.ExitState (); 
		_move = _moStasis; 
		_move.EnterState (); 
	}
	public void NowAnim(){
		if(_move.CurrentState != _moStasis.CurrentState){
			_move.ExitState (); 
		}
		_move = _moAnim; 
		_moAnim.EnterState (); 
	}
	void LeaveStasis(){
		_move.ExitState (); 
		_rigid.velocity = _heldSpeed *1.2f; 
		Invoke("TeleportTimer",1f); //don't let them teleport too frequently
	}




	//fucntions relating to nodes
	//--------------------------------------------------------------------------------------------------

	public bool HasNode(){ //is there any node the player could be attached to? 
		if (attachedNodes.Count == 0) {
			return false;	
		}
		return true; 
	}
	public bool HasNode(Node _nd){ //looking for a particular node
		for(int i = 0; i < attachedNodes.Count;i++){
			if(_nd == attachedNodes[i]){
				return true;
			}
		}
		return false; 
	}
	public bool UsesNodes(){ //returns whether we are currently in a state that cares aboutnodes
		return _move.UsesNodes (); 
	}
	public bool Direction{ get { return _move.Direction; } }
	public Node CurrentNode(){ //returns the most recent node
		if(attachedNodes.Count > 0){
			return attachedNodes [attachedNodes.Count - 1]; 
		}
		return null; //if there are no nodes, sends a null
	}
	public Node NextNode(){
		return CurrentNode ().NextNode (Direction); 
	}
	public void AddNode(Node _node){
		attachedNodes.Add (_node); 
		if (_move.UsesNodes()) {
			_move.SetNode(CurrentNode()); 
		}
	}
	public void RemoveNode(Node _node){
		attachedNodes.Remove (_node); 
		if ( _move.UsesNodes()) { //removed this bit of code from this line   HasNode () &&
			_move.SetNode(CurrentNode()); 
		}/*
		if (HasNode () == false && _move.UsesNodes ()) {
			_move.JumpOffNodes(); 
		}*/
	}

	public void TemporarilyDisableNodeD(){ //allows you to actually get off nodes instead of constantly attaching
											//called when you leave a node line
		anim.SetBool (_currentNodeD.key, false);  //sets the animation state to false
		_currentNodeD.DisableNodeSending ();
		_currentNodeD = null; 
		attachedNodes.Clear (); 
		Invoke ("EnableNodeD", .3f);
	}
	public void OnlyOneNodeD(Node_Detector _nd ){ //called  when you first get onto a node line
		_currentNodeD = _nd; 
		anim.SetBool (_currentNodeD.key, true); //sets the animation state to attached
		attachPos = _nd.alignPos; 
		for(int i = 0; i < nodeDList.Count ; i++){
			if(nodeDList[i] != _nd){
				Debug.Log(nodeDList[i].name + " disabled"); 
				nodeDList[i].DisableNodeSending(); 
			}
		}

	}
	public void EnableNodeD(){
		for(int i = 0; i < nodeDList.Count ; i++){
			nodeDList[i].EnableNodeSending (); 
		}
	}





	//transporter code
	//--------------------------------------------------------------------------------------
	void Clicking(){
		if (Input.GetMouseButtonDown (1)) {
			FireTransporter(); 	
		}
		if (Input.GetMouseButtonDown (0)) {
			TransporterStart(); 		
		}
	}
	//this code will need to be changed if I plan on blending any else
	void SpawnTransporter(){ //deals with creating the transporter as well as the animation stuff relating to that
		if(anim.GetCurrentAnimatorStateInfo(0).IsTag("Running") && anim.GetNextAnimatorStateInfo(0).IsName("IdleToShoot")
		   || anim.GetCurrentAnimatorStateInfo(0).IsTag("InAir") && anim.GetNextAnimatorStateInfo(0).IsName("IdleToShoot")){
			//if you are running and about to shoot, turn on the mask
			anim.SetLayerWeight (1,1);  
			//_lowerBodyBlendTarget = 1; 
		}
		if (!anim.GetCurrentAnimatorStateInfo(0).IsTag("Shooting") && anim.GetLayerWeight(1) > 0 && !anim.IsInTransition(0)){
			//anim.GetCurrentAnimatorStateInfo (0).IsName ("ShootLoop") && anim.IsInTransition (0)) { //no longer blend
			anim.SetLayerWeight(1,0); 
			//_lowerBodyBlendTarget = 0; 
		}
		if(anim.GetCurrentAnimatorStateInfo(0).IsTag("Shooting")){ //face the camera when you shot
			FaceCameraWhenShooting(); 
		}
		if(anim.GetCurrentAnimatorStateInfo(0).IsName("ShootLoop") && anim.GetBool("Shooting")){ //now firing the gun
			anim.SetBool("Shooting",false); //can only fire so often
			if (_port != null) { //turn off the old teleporter
				_port.Inactive(); 		
			}
			GameObject _porterObj = Instantiate (transporterPrefab) as GameObject; 
			_port = _porterObj.GetComponent<Transporter> (); 
			_port.transform.position = thorwOrigin.position; 
			Vector3 _fireDirection = new Vector3 (transform.forward.x, cameraTrans.forward.y, transform.forward.z); 
			_port.rigid.velocity = _rigid.velocity + new Vector3 (0, throwUp, 0) + _fireDirection.normalized * throwForward;  
		}
	}

	void FaceCameraWhenShooting(){
		_move.LookTowardsCamera (.2f); 
	}
	void FireTransporter(){
		if(!anim.GetBool("Shooting") || !anim.GetCurrentAnimatorStateInfo (0).IsName("ShootLoop")){
				anim.SetBool ("Shooting", true); 
		}
	}
	void TransporterStart(){
		if(_port != null && _canTeleport){
			_move.JumpOffNodes (); 
			Instantiate(transporterLeaveFX, transform.position, new Quaternion()); 
			_heldSpeed = _rigid.velocity; 
			_rigid.velocity = Vector3.zero; 
			NowStasis(); 
			//your point will be the same as the teleporters, but out a bit from the collider 
			//transform.position = _port.transform.position + (_port.Point.normalized*-1)*(Mathf.Clamp(_rigid.velocity.magnitude/10f,0,1)); 
			Invoke("TransporterEnd",.2f); 
			_canTeleport = false;
		}
	}
	void TransporterEnd(){
		Invoke("LeaveStasis",.2f); 
		Vector3 _newDirection = new Vector3 (_heldSpeed.normalized.x, Mathf.Clamp (_heldSpeed.normalized.y, 0, 1), _heldSpeed.z); 
		transform.position = _port.transform.position + new Vector3(0,1.2f,0) +(_newDirection.normalized*_heldSpeed.magnitude/50f) ; 
		Instantiate(transporArriveFX, transform.position, new Quaternion()); 
	}
	void TeleportTimer(){ //just turns on the ability to teleport, invoekd in UseTransporter
		_canTeleport = true; 
	}

	void GetMeshes(){ //collects all the meshes the player has 
		_meshes = GetComponentsInChildren<SkinnedMeshRenderer> (); 
		Debug.Log (_meshes.Length); 
	}
	public void PlayerRenderState(bool _state){ //turns on or off all the states
		foreach (SkinnedMeshRenderer _mesh in _meshes) {
			_mesh.enabled = _state; 	
		}
	}



	//vector trigger code
	//------------------------------------------------------------------------------------------------------

	VectorTrigger _vectorTrigger; 

	public void VTTargeted(VectorTrigger _vt){
		_vectorTrigger = _vt; 
	}
	public void VTTargeLost(){
		_vectorTrigger = null; 
	}
	public bool HasVTTarget(){
		if (_vectorTrigger != null) {
			return true;		
		}
		return false; 
	}
	public void TurnOnVT(){
			_vectorTrigger.TurnOn(); 
	}
	public bool _activatingTrigger = false; 
	public bool IsUsingTrigger{ get { return _activatingTrigger; } set { _activatingTrigger = value; } }
	public void TurnOnTrigger(){
		_activatingTrigger = true; 
	}
	void UseVectorTrigger(){
		if(_vectorTrigger != null && _activatingTrigger){
			if(_state != "anim"){
				Debug.Log("entering anim state"); 
				NowAnim(); 
			}
			if(Vector3.Distance(transform.position, _vectorTrigger.VT_Pos.position) <= .05f){
				Debug.Log("Turning on VT"); 
				PlayVTAnim();  
				_activatingTrigger = false; 
				Invoke("PlayVTToIdleAnim",2f); 
			}
			else{
				Debug.Log(Vector3.Distance(transform.position, _vectorTrigger.VT_Pos.position)); 
				Debug.Log("Playing Vector Trigger"); 
				transform.position = Vector3.Lerp (transform.position, _vectorTrigger.VT_Pos.position, Time.deltaTime * 5); 
				transform.rotation = Quaternion.Lerp (transform.rotation,_vectorTrigger.VT_Pos.rotation, Time.deltaTime * 5); 
			}
		}
	}
	void PlayVTAnim(){
		anim.Play ("VT_Activate"); 
	}
	void PlayVTToIdleAnim(){
		anim.Play ("VT_End"); 
	}
	public bool VTWasUsed(){
		if(_vectorTrigger != null){
			return _vectorTrigger.Used;
		}
		return true; 
	}





	//checkpoint code
	//-------------------------------------------------------------------------------------------------------

	public void SetCheckpoint(Checkpoint _newCheckpoint){ //sets the new checkpoint
		if ( _lastCheckpoint == null) {
			_lastCheckpoint = _newCheckpoint; 	
			return; 
		}
		if (_newCheckpoint.id > _lastCheckpoint.id) {
			_lastCheckpoint = _newCheckpoint; 
		}
	}
	void ReturnToCheckpoint(){//if the player falls too far, return them to the last checkpoint
		if(transform.position.y < 0){
			_camera.Freefall(); 
		}
		if (transform.position.y < -20) {
			GetComponent<Rigidbody>().velocity = new Vector3 (0,-55,0); 
			cameraTrans.position = transform.position = _lastCheckpoint.transform.position + new Vector3(0,150,0); 
			camMover.MoveToTarget(); 
		}
	}




	void Start () {
		_rigid = GetComponent<Rigidbody> (); 
		_moGround.Startup (this); 
		_moInAir.Startup (this); 
		_moAttached.Startup (this); 
		_moStasis.Startup (this); 
		_moAnim.Startup (this); 
		_camera = cameraTrans.GetComponent<Camera_Controller> (); 

		_move = _moGround; 
		_move.EnterState (); 

		GetMeshes (); 

	}
	void FixedUpdate () {
		_move.ControlsEffect (); //calls has the effects of physics stuff happen
	}
	void Update(){
		_move.ControlsInput ();  //gets input
		_move.MotionState (); //checks to see if it should change states
		Clicking (); 
		SpawnTransporter (); 
		ReturnToCheckpoint (); 
		UseVectorTrigger ();
	}



	/*
void MoveControls(){
		SetAxis ();
		MoveForward (); 
		MoveBackwards ();
		Jump (); 
	}
	void SetAxis(){ //gets all inputs
		_verticalInput = Input.GetAxis ("Vertical");
		_horizontalInput = Input.GetAxis ("Horizontal"); 
		_jumpInput = Input.GetAxis ("Jump"); 
	}
	void MoveForward(){ //controls when they move forward. This whole script should be broken up into different states.
		if (_verticalInput > 0 && _rigid.velocity.magnitude < maxSpeed && _grounded.IsGrounded()){
			_rigid.AddRelativeForce (new Vector3(0,0,1) * _verticalInput * acceleration,ForceMode.Acceleration); 
			LookTowardsCamera(); 
		}
	}
	void MoveBackwards(){ //simlar to forward, this is different because I may want to clamp and have different anims and such
		if(_verticalInput < 0 && _rigid.velocity.magnitude < maxSpeed && _grounded.IsGrounded()){
			_rigid.AddRelativeForce (new Vector3(0,0,1) * _verticalInput * acceleration,ForceMode.Acceleration); 
			LookTowardsCamera(); 
		}
	}
	void LookTowardsCamera(){ //called when you move forwards or backwards
		Quaternion _targetRotation =  Quaternion.Euler(transform.rotation.eulerAngles.x, cameraTrans.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
		transform.rotation = Quaternion.Slerp (transform.rotation, _targetRotation, turnSpeed); 
	}
	void Jump(){ //can only jump when on the ground. When space is pushed they go up
		if (_jumpInput > 0 && _grounded.IsGrounded()) {
			_rigid.AddForce(new Vector3(0,1,0)*jumpPower,ForceMode.Impulse);
		}
	}
	void HeavyGravity(){
		if(_rigid.velocity.normalized.y < -.1f || _rigid.velocity.normalized.y > .1f ){
			_rigid.AddForce (Physics.gravity*.5f, ForceMode.Acceleration); 
		}
	}
	*/
}
