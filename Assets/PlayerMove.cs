using UnityEngine;
using UnityEngine.Networking;

public class PlayerMove : NetworkBehaviour {

	public GameObject bullet;

	public override void OnStartLocalPlayer() {
		GetComponent<MeshRenderer>().material.color = Color.blue;
	}

	private void Update() {
		if(!isLocalPlayer)
			return;

		var x = Input.GetAxis("Horizontal") * 0.1f;
		var z = Input.GetAxis("Vertical") * 0.1f;

		transform.Translate(x, 0, z);

		if(Input.GetKeyDown(KeyCode.Space)) {
			//Debug.Log("Fire!");
			CmdFire();
		}

	}

	[Command]
	private void CmdFire() {
		GameObject b = Instantiate(bullet, transform.position - transform.forward, Quaternion.identity);
		b.GetComponent<Rigidbody>().velocity = -transform.forward * 4;

		NetworkServer.Spawn(b);

		Destroy(b, 2f);
	}

}
