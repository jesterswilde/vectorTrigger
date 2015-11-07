using UnityEngine;
using System.Collections;

public class Mo_Stasis : Motion {



	public override void EnterState ()
	{
		_player.PlayerRenderState (false); 
		base.EnterState ();
		_rigid.isKinematic = true; 
	}

	public override void ExitState ()
	{
		_rigid.isKinematic = false; 
		_player.PlayerRenderState (true); 
		_player.NowInAir (); 
		base.ExitState ();
		_anim.Play ("JumpLoop"); 
	}
	public override void Startup (Player_Controller _thePlayer)
	{
		base.Startup (_thePlayer);
		_state = "stasis"; 
	}

}
