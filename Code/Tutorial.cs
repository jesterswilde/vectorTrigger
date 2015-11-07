using UnityEngine;
using System.Collections;
using UnityEngine.UI; 

public class Tutorial : MonoBehaviour {


	public Canvas theCanvas; 
	public bool attachToPlayer; 
	public UnityEngine.UI.Text _text; 
	GameObject tutScreen; 
	Transform _player; 
	public float visibilitySpeed; 
	bool _visible; 

	void OnTriggerEnter(Collider _collider){
		if (_collider.gameObject.tag == "Player") {
			_player = _collider.transform;
			EnableTutScreen();
		}
	}
	void OnTriggerExit(Collider _collider){
		if (_collider.gameObject.tag == "Player") {
			DisableTutScreen(); 
		}
	}

	void EnableTutScreen(){
		_visible = true; 
		if (attachToPlayer) { //attach it to the player if we need to
			//tutScreen.transform.localScale = new Vector3(.015f,.015f,.015f); 
		}
	}

	void DisableTutScreen(){ 
		_visible = false; 
		if (attachToPlayer) {
			tutScreen.transform.position = transform.position;
			tutScreen.transform.parent = transform; 
		}
	}
	void LookAtCamera(){
		if(attachToPlayer && _visible){
			tutScreen.transform.LookAt(Camera.main.transform.position);
			tutScreen.transform.Rotate (new Vector3(0,180,0)); 
			tutScreen.transform.position = _player.position;
		}
	}
	void TurnUpVisiblity(){
		if (_visible && _text.color.a < 1) {
			_text.color = new Color(_text.color.r,_text.color.g,_text.color.b, Mathf.Clamp(_text.color.a + visibilitySpeed,0,1));	
		}
	}
	void TurnDownVisibilty(){
		if (_visible == false && _text.color.a > 0) {
			_text.color = new Color(_text.color.r,_text.color.g,_text.color.b, Mathf.Clamp(_text.color.a - visibilitySpeed,0,1)); 	
		}
	}
	void Start(){
		_text.color = new Color (1, 1, 1, 0); 
		tutScreen = theCanvas.transform.parent.gameObject; 
	}
	void Update(){
		TurnUpVisiblity ();
		TurnDownVisibilty (); 
		LookAtCamera (); 
	}

}
