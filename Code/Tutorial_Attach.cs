using UnityEngine;
using System.Collections;
using UnityEngine.UI; 

public class Tutorial_Attach : MonoBehaviour {

	public string tutorialText; 
	bool _isActive = false; 
	public UnityEngine.UI.Text uiText; 
	public float visibilitySpeed = .02f; 
	public int fontSize = 28;

	void OnTriggerEnter(Collider _collider){
		if(_collider.gameObject.layer ==10){
			uiText.text = tutorialText; 
			uiText.fontSize = fontSize; 
			_isActive = true; 
		}
	}

	void OnTriggerExit(Collider _collider){
		if(_collider.gameObject.layer ==10){
			_isActive = false; 
		}
	}

	void TurnUpVisiblity(){
		if (_isActive && uiText.color.a < 1 && uiText.text == tutorialText) {
			uiText.color = new Color(uiText.color.r,uiText.color.g,uiText.color.b, Mathf.Clamp(uiText.color.a + visibilitySpeed,0,1));	
		}
	}
	void TurnDownVisibilty(){
		if (_isActive == false && uiText.color.a > 0 && uiText.text == tutorialText) {
			uiText.color = new Color(uiText.color.r,uiText.color.g,uiText.color.b, Mathf.Clamp(uiText.color.a - visibilitySpeed,0,1)); 	
		}
	}

	void Update(){
		TurnUpVisiblity ();
		TurnDownVisibilty (); 
	}

}
