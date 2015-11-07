using System.Threading; 
using UnityEngine;
using System.Collections;

public class Node : MonoBehaviour {

	//the nodes look foward and back, the player travels along the lines between these nodes
	BoxCollider _collider;  
	public Node forwardNode;  //next node in the line
	public Node backwardNode; //previous node in the line
	public float speed = 5; 
	public float maxSpeed = 80; 
	public float minSpeed = 15; 
	public UnityEngine.Object renderBox;

	public Color offColor;
	public Color onColor; 
	Color _targetColor; 
	Renderer _rednerBoxRenderer; 
	public bool startOn; 


	Vector3 forwardVec; //the director towards the next node
	public Vector3 ForwardVec {get{ return forwardVec;}}
	Vector3 backwardVec; //the direction towards the previous node
	public Vector3 BackwardVec { get { return backwardVec; } }
	float forwardDistance; //the distance between this node and hte next one

	public bool canJumpUp; //bools to determine what directionality the player can use to jump from each node
	public bool canJumpLeft; 
	public bool canJumpRight; 
	public bool canJumpDown; 

	public bool canAttachUp;
	public bool canAttachLeft;
	public bool canAttachRight;
	public bool canAttachDown; 

	bool _isOn = false; 
	public bool IsOn{ get { return _isOn; } }
	


	public void SetBackNode(Node _backNode){ //since it's a line, previous nodes assign themselves
		backwardNode = _backNode; 
	}
	void SetForwardNode(){ //if you have a forward node, sets forward and back, looks at it, then makes the collision box
		//go all the way to it
		if (forwardNode != null) {
			forwardVec = forwardNode.transform.position - transform.position; 
			backwardVec = forwardVec*-1; 
			forwardDistance = Vector3.Distance (this.transform.position, forwardNode.transform.position); 
			transform.LookAt (forwardNode.transform.position); 
			_collider.center = new Vector3 (0, 0 , forwardDistance * .5f -.5f);
			_collider.size = new Vector3 (1,1,forwardDistance); 
			forwardNode.SetBackNode(this); //if there is a forward node, this node is it's back node. 
		}
		else {
			_collider.enabled = false; 
		}
	}
	public bool EndofLine(bool _forward){ //tells you if this is the last node in the direction you are going. 
		if (_forward) {
			if(forwardNode.HasForwardNode() == false){
				return true;
			}
			else return false; 
		}
		else{
			if(backwardNode == null){
				return true; 
			}
			return false;
		}
	}
	public Node PreviousNode(bool _forward){ //returns the previous node, based on directionality
		if (_forward) {
			return backwardNode;		
		}
		return forwardNode; 
	}
	public Node NextNode(bool _forward){ //returns the next node, based on directionality
		if (_forward) {
			return forwardNode;		
		}
		return backwardNode; 
	}
	public bool HasForwardNode(){
		if (forwardNode == null) {
			return false;	
		}
		else return true;
	}
	public bool HasBackwardNode(){
		if (backwardNode == null) {
			return false;		
		}
		return true;
	}
	void MakeRenderBox(){ //draws the lines beteen the nodes
		GameObject renderBoxObj = Instantiate (renderBox) as GameObject;
		renderBoxObj.transform.parent = transform; 
		renderBoxObj.transform.position = transform.position + transform.forward.normalized * forwardDistance * .5f; 
		renderBoxObj.transform.localScale = new Vector3 (.3f, .3f, forwardDistance); 
		renderBoxObj.transform.LookAt (forwardNode.transform.position); 
		_rednerBoxRenderer =  renderBoxObj.GetComponent<Renderer>(); 
	}



	//Code for limiting the way a player can leave each node
	public bool CanJumpUp(bool forward){
		return canJumpUp; 
	}
	public bool CanJumpLeft(bool forward){
		if (forward) {
			return canJumpLeft;		
		}
		return canJumpRight; 
	}
	public bool CanJumpRight(bool forward){
		if (forward) {
			return canJumpRight;		
		}
		return canJumpLeft; 
	}
	public bool CanJumpDown(bool forward){
		return canJumpDown; 
	}


	public bool CanAttach(string _key){ //restricts the ways you can attack
		if (_key == "up" && canAttachUp)
						return true;
		if (_key == "down" && canAttachDown)
						return true;
		if (_key == "left" && canAttachLeft)
						return true;
		if (_key == "right" && canAttachRight)
						return true;
		return false; 
	}


	//vector trigger related code
	public void TurnOn(bool _state){
		if(HasForwardNode()){
			_isOn = _state; 
			if (_isOn) {
				_targetColor = onColor; 
				_collider.enabled = true; 
			}
			if (!_isOn) {
				_targetColor = offColor; 
				_collider.enabled = false; 
			}
			if(HasForwardNode()){
				if(forwardNode.IsOn != _isOn){ //if the next node is not the same as this one
					forwardNode.TurnOn(_isOn); 
				}
			}
		}
	}
	void ColorLerp(){
		if(HasForwardNode()){
			if (_rednerBoxRenderer.material.color != _targetColor) {
				_rednerBoxRenderer.material.color = Color.Lerp (_rednerBoxRenderer.material.color, _targetColor,.01f); 
				_rednerBoxRenderer.material.SetColor("_OutlineColor",_rednerBoxRenderer.material.color); 
			}
		}
	}



	void Awake(){
		_collider = GetComponent<BoxCollider> (); 
		SetForwardNode ();
		if(HasForwardNode()){
			MakeRenderBox (); 
			if(!startOn){ //if you are not testing hte node
				_rednerBoxRenderer.material.color = offColor; //it starts black 
				_rednerBoxRenderer.material.SetColor("_OutlineColor",_rednerBoxRenderer.material.color); 
				_targetColor = offColor;  
				_collider.enabled = false;//and it starts unable to use 
			}
			else{
				_targetColor = onColor; 
			}
		}


		GetComponent<MeshRenderer> ().enabled = false; //turns off nodes
	}
	void Update(){
		ColorLerp (); 
	}

}
