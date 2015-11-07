using UnityEngine;
using System.Collections;

public class PauseMenu : MonoBehaviour {

	public Manager _manager; 


	public void Restart(){
		_manager.UnPauseGame ();
		Application.LoadLevel (Application.loadedLevelName); 

	}
	public void Quit(){
		Application.Quit (); 
	}
	public void Resume(){
		_manager.UnPauseGame ();
	}

}
