using UnityEngine;
using System.Collections;

public class Mo_Anim : Motion {

	public override void EnterState ()
	{
		base.EnterState ();
		_rigid.isKinematic = true; 
	}
	
	public override void ExitState ()
	{
		_rigid.isKinematic = false; 
		base.ExitState ();
	}
	public override void ControlsInput ()
	{
		base.ControlsInput ();
		if (_anim.GetCurrentAnimatorStateInfo (0).IsName ("VT_End") && _anim.IsInTransition (0)) {
			_player.NowGrounded(); 		
		}
		if (_anim.GetNextAnimatorStateInfo (0).IsName ("VT_Loop")) {
			_player.TurnOnVT();	
		}
	}
	public override void Startup (Player_Controller _thePlayer)
	{
		base.Startup (_thePlayer);
		_state = "anim"; 
		_lastState = "anim"; 
	}
}
