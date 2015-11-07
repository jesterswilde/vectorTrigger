using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Ground_Detection : MonoBehaviour {

	public List<GameObject> _theGround = new List<GameObject>(); 
	
	void AddGround(Collider _collider){
		_theGround.Add (_collider.gameObject); 
	}
	void RemoveGround(Collider _collider){
		_theGround.Remove (_collider.gameObject); 
	}
	void OnTriggerEnter(Collider _collider){
		if (_collider.gameObject.layer == 8) {
			AddGround (_collider); 
		}
	}
	void OnTriggerExit(Collider _collider){
		if (_collider.gameObject.layer == 8) {
			RemoveGround(_collider); 
		}
	}
	public bool IsGrounded(){
		Debug.Log (_theGround.Count); 
		if (_theGround.Count > 0)
						return true;
				else
						return false; 
	}
	/*
	 * bool TestForGround(Collider _collider){
		if (_collider.gameObject.layer == 8) {
			_isGrounded = true; 
			return true; 
		}
		else return false;
	}
	void OnTriggerEnter(Collider _collider){
		TestForGround (_collider); 
	}
	void OnTriggerExit(Collider _collider){
		if (_collider.gameObject.layer == 8) {
			if(TestForGround(_collider)){
				_isGrounded = false;
			}
		}
	}
	public bool IsGrounded(){
		return _isGrounded; 
	}
	*/
}
