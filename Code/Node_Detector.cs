using UnityEngine;
using System.Collections;

public class Node_Detector : MonoBehaviour {
	
	public Player_Controller _player; 
	public Transform alignPos; 
	bool _shouldAddNodes = true;
	bool _firstNode = true; 
	public string key; 

	//consider this, instead of a bunch of different detectors you have on detector and a bunch of attach transforms.
	//then whenenever you attach you go looking for the closest viable attach transform to attach to.  Then we can
	//have a ridiculously beig attach transform. 

	//also consider putting the push E back in, it might be the best method. 


	void AddNode(Node _theNode){  //if you can attach to a node, and this detector isn't turned off
		if(_shouldAddNodes && _theNode.CanAttach(key)){
			_player.AddNode (_theNode); // attach to it
			if(_firstNode){ //if this is the first node, turn other detectors off
			//	IncreaseScale(); 
				_firstNode = false; 
				_player.OnlyOneNodeD(this); 
			}
		}
	}
	void AddStayNode(Node _theNode){ //this just ensures that all nodes get added, things get buggy
		if(_player.HasNode (_theNode) != true ){
			AddNode (_theNode); 
		}
	}
	void RemoveNode(Node _theNode){
		_player.RemoveNode (_theNode); 
	}
	public void DisableNodeSending(){ //will not send out nodes. Also ensures next time a node is sent, it's fresh.
		_shouldAddNodes = false; 
		_firstNode = true; 
	}
	public void EnableNodeSending(){
		_shouldAddNodes = true; 
		//RevertScale (); 
	}
	void IncreaseScale(){
		transform.localScale = new Vector3 (3, 3, 3); 
	}
	void RevertScale(){
		transform.localScale = new Vector3(1,1,1); 
	}
	void OnTriggerEnter(Collider _collider){
		if (_collider.gameObject.layer == 9) {
			if(_collider.GetComponent<Node>() != null){
				AddNode(_collider.GetComponent<Node>()); 
			}
		}
	}
	void OnTriggerStay(Collider _collider){ //this could cause bugs
		if (_collider.gameObject.layer == 9) {
			if(_collider.GetComponent<Node>() != null){
				AddStayNode(_collider.GetComponent<Node>()); 
			}
		}
	}
	void OnTriggerExit(Collider _collider){
		if (_collider.gameObject.layer == 9) {
			if(_collider.GetComponent<Node>() != null){
				RemoveNode(_collider.GetComponent<Node>()); 
			}		
		}
	}

}
