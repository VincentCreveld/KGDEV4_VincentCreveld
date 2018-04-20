using UnityEngine;
using UnityEngine.Networking;

public class PlayerMove : NetworkBehaviour {

	public GameObject bullet;
	[SyncVar]
	public int id;

	public override void OnStartLocalPlayer() {
		transform.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.blue;
		TurnManager.instance.SubscribePlayer(this);
	}

	public void OnDisconnectedFromServer(NetworkDisconnection info) {
		TurnManager.instance.UnsubscribePlayer(this, info);
	}

	private void Update() {
		if(!isLocalPlayer)
			return;

		var x = Input.GetAxis("Horizontal") * 0.1f;
		var z = Input.GetAxis("Vertical") * 0.1f;

		transform.Rotate(0, x * 25, 0);
		transform.Translate(0, 0, -z);

		if(Input.GetKeyDown(KeyCode.Space)) {
			//Debug.Log("Fire!");
			CmdFire();
		}

	}

	[Command]
	private void CmdFire() {
		if(TurnManager.instance.IsTurn(this)) {
			GameObject b = Instantiate(bullet, transform.position - transform.forward, transform.rotation);
			b.GetComponent<Rigidbody>().velocity = -transform.forward * 4;

			NetworkServer.Spawn(b);

			Destroy(b, 2f);
			TurnManager.instance.NextTurn();
		}
	}

}
