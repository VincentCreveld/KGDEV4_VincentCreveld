﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {

	public static GameManager instance;

	public const float GAME_DURATION = 20f;

	public Transform[] spawnPositions;
	private List<NetworkConnection> players = new List<NetworkConnection>();
	private List<GameObject> playerObjects = new List<GameObject>();

	public void Awake() {
		if(instance == null)
			instance = this;
		else
			Destroy(this);
		
		Transform.FindObjectOfType<NetworkManagerHUD>().enabled = true;
	}

	// Called when a player joins the server and adds their data to two lists that hold their gameObject and connectionData.
	// When two players are connected (and no more, limited in network manager) the game initialises.
	[Command]
	public void CmdPlayerJoined(GameObject obj) {
		NetworkConnection networkID = obj.GetComponent<NetworkIdentity>().connectionToClient;
		if(!players.Contains(obj.GetComponent<NetworkIdentity>().connectionToClient)) {
			//add to list else return;
			players.Add(obj.GetComponent<NetworkIdentity>().connectionToClient);
			playerObjects.Add(obj);
			if(playerObjects.Count == 1) {
				Debug.Log("Player1 connected");
			}else if (playerObjects.Count == 2) {
				CmdInitializeGame();
			}
		}
		else {
			Debug.Log("Same ID");
			return;
		}
	}

	// Called when two players are connected. Assigns both players their relevant data.
	// Initialises turns.
	[Command]
	public void CmdInitializeGame() {
		playerObjects[0].GetComponent<PlayerMove>().isTurn = true;
		StartCoroutine(playerObjects[0].GetComponent<PlayerMove>().LoadGun());
		playerObjects[0].name = "player1";
		playerObjects[1].GetComponent<PlayerMove>().canBlock = true;
		playerObjects[1].name = "player2";

		playerObjects[0].transform.position = spawnPositions[0].position;
		playerObjects[1].transform.position = spawnPositions[1].position;

		StartCoroutine(GameDuration());
	}

	// Limits game duration and disconnects players.
	private IEnumerator GameDuration() {
		Debug.Log("Started timer");
		yield return new WaitForSeconds(GAME_DURATION);
		Debug.Log("Timer expired");
		CmdDisconnectPlayers();
	}
	[Command]
	public void CmdDisconnectPlayers() {
		Debug.Log("Reached DC");
		playerObjects[0].GetComponent<PlayerMove>().RpcSubmitScore();
		playerObjects[1].GetComponent<PlayerMove>().RpcSubmitScore();
		players[0].Disconnect();
		players[1].Disconnect();
	}

	// Called when succesfully shot by either player. Sets all relevant bools needed to play.
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
