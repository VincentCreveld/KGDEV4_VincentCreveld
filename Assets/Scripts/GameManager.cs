using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {

	public static GameManager instance;

	private List<NetworkConnection> players = new List<NetworkConnection>();
	private List<GameObject> playerObjects = new List<GameObject>();

	public void Awake() {
		if(instance == null)
			instance = this;
		else
			Destroy(this);
	}

	private void Update() {

	}

	[Command]
	public void CmdPlayerJoined(GameObject obj) {
		NetworkConnection networkID = obj.GetComponent<NetworkIdentity>().connectionToClient;
		if(!players.Contains(obj.GetComponent<NetworkIdentity>().connectionToClient)) {
			//add to list else return;
			players.Add(obj.GetComponent<NetworkIdentity>().connectionToClient);
			playerObjects.Add(obj);
			if(playerObjects.Count == 1) {
				playerObjects[0].GetComponent<PlayerMove>().isTurn = true;
				StartCoroutine(playerObjects[0].GetComponent<PlayerMove>().LoadGun());
				playerObjects[0].name = "player1";
				Debug.Log(playerObjects[0].GetComponent<PlayerMove>().isTurn);
			}else if (playerObjects.Count == 2) {
				playerObjects[1].GetComponent<PlayerMove>().canBlock = true;
				playerObjects[1].name = "player2";
			}
		}
		else {
			Debug.Log("Same ID");
			return;
		}
	}

	[Command]
	public void CmdChangeTurn() {
		//Debug.Log("Reached");
		if(playerObjects[0].GetComponent<PlayerMove>().isTurn) {
			playerObjects[0].GetComponent<PlayerMove>().isTurn = false;
			playerObjects[0].GetComponent<PlayerMove>().canBlock = true;
			playerObjects[1].GetComponent<PlayerMove>().isTurn = true;
			StartCoroutine(playerObjects[1].GetComponent<PlayerMove>().LoadGun());
			playerObjects[1].GetComponent<PlayerMove>().canBlock = false;
			//Debug.Log("reached sw1");
		}
		else {
			playerObjects[0].GetComponent<PlayerMove>().isTurn = true;
			StartCoroutine(playerObjects[0].GetComponent<PlayerMove>().LoadGun());
			playerObjects[0].GetComponent<PlayerMove>().canBlock = false;
			playerObjects[1].GetComponent<PlayerMove>().isTurn = false;
			playerObjects[1].GetComponent<PlayerMove>().canBlock = true;
			//Debug.Log("reached sw2");
		}
	}
}
