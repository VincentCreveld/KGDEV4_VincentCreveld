using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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
