using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TurnManager : NetworkBehaviour {

	public static TurnManager instance;

	public List<PlayerMove> playerList;

	[SyncVar]
	public int currentTurn = 0;

	public void Awake() {
		playerList = new List<PlayerMove>();
		if(isServer)
			instance = this;
	}

	public void SubscribePlayer(PlayerMove pm) {
		foreach(var p in playerList) {
			if(p == pm)
				break;
		}
		playerList.Add(pm);
		pm.id = playerList.Count;
		RpcUpdateList(playerList);
	}

	public void UnsubscribePlayer(PlayerMove pm, NetworkDisconnection info) {
		for(int i = 0; i < playerList.Count; i++) {
			if(playerList[i] == pm) {
				playerList.Remove(pm);
				playerList[i] = new DummyPlayer();
			}
		}
		Debug.Log($"Player with id: {pm.id} disconnected from the server. {info} 19 = manual, 20 = forced");
	}

	public void NextTurn() {
		if(currentTurn + 1 < playerList.Count) {
			currentTurn++;
		}
		else {
			currentTurn = 0;
		}
	}

	public bool IsTurn(PlayerMove pm) {
		if(pm == playerList[currentTurn]) {
			return true;
		}
		else
			return false;
	}

	[ClientRpc]
	public void RpcUpdateList(List<PlayerMove> pm) {
		playerList = pm;
	}
}
