using UnityEngine;
using System.Collections;

public class Manager : MonoBehaviour {

	bool _isPaused = false;
	public Object pauseMenuPrefab; 
	public Canvas theCanvas; 
	GameObject _pauseMenuObject;


	public void PauseGame(){
		_isPaused = true;
		Screen.lockCursor = false; 	
		Cursor.visible = true; 
		//_pauseMenuObject = Instantiate(pauseMenuPrefab) as GameObject;
		//_pauseMenuObject.GetComponent<PauseMenu> ().theManager = this; 
		theCanvas.enabled = true; 
		Time.timeScale = 0f; 
	}

	public void UnPauseGame(){
		//Destroy (_pauseMenuObject.gameObject); 
		Screen.lockCursor = true;
		Cursor.visible = false; 
		theCanvas.enabled = false; 
		_isPaused = false; 
		Time.timeScale = 1f; 
	}
	
	void Start(){
		Time.timeScale = 1f; 
		Screen.lockCursor = true;
		Cursor.visible = false;
	}

	void Update(){
		if (Input.GetKeyDown (KeyCode.Escape)) {
			if(_isPaused){
				UnPauseGame();
			}
			else{
				PauseGame(); 
			}
		}
	}
}
