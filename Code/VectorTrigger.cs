using UnityEngine;
using System.Collections;

public class VectorTrigger : MonoBehaviour {

	bool _wasUsed = false; 
	public bool Used { get { return _wasUsed; } set { _wasUsed = value; } }
	public Node[] affectedNodes;
	Renderer _renderer; 
	public Transform VT_Pos; 

	public Color offColor;
	public Color onColor; 
	Color _targetColor; 

	void OnTriggerEnter(Collider _collider){
		if (_collider.gameObject.layer == 10) {
			if(_collider.gameObject.GetComponent<Player_Controller>() != null){
				_collider.GetComponent<Player_Controller>().VTTargeted(this); 
			}
		}
	}

	void OnTriggerExit(Collider _collider){
		if (_collider.gameObject.layer == 10) {
			if(_collider.gameObject.GetComponent<Player_Controller>() != null){
				_collider.GetComponent<Player_Controller>().VTTargeLost(); 
			}
		}
	}

	public void TurnOn(){
		if(!_wasUsed){
			_targetColor = onColor;
			_wasUsed = true; 
			Debug.Log("Vector trigger is now on"); 
			for (int i = 0; i < affectedNodes.Length; i++) {
				affectedNodes[i].TurnOn(true); 
			}
		}
	}
	void ColorLerp(){
		if (_renderer.materials[2].color != _targetColor) {
			_renderer.materials[2].color = Color.Lerp (_renderer.materials[2].color, _targetColor,.01f); 
			_renderer.materials[2].SetColor("_OutlineColor", Color.Lerp(_renderer.materials[2].color,_targetColor,01f)); 
		}
	}

	void Update(){
		ColorLerp (); 
	}

	void Start(){
		_renderer = GetComponent<Renderer> ();

		_renderer.materials [2].color = _targetColor =  offColor; 
	}
}
