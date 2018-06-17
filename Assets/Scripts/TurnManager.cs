using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// This script is called by the PlayerMove scripts when they want to take action and 
// get back that they can either spawn a bullet or a shield.
public static class TurnManager {

	public static void CheckTurns(PlayerMove pm) {
		if(pm.isTurn)
			CmdPlayerShoot(pm);
		else
			CmdPlayerBlock(pm);
	}

	public static void CmdPlayerShoot(PlayerMove pm) {
		pm.CmdShoot();
	}


	public static void CmdPlayerBlock(PlayerMove pm) {
		pm.CmdBlock();
	}

}