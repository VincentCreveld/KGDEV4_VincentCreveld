using UnityEngine;
using UnityEngine.Networking;

public class Combat : NetworkBehaviour {
	public const int maxHealth = 100;
	public bool destroyOnDeath;

	[SyncVar]
	public int health = maxHealth;

	//Werkt niet?
	[SyncVar]
	public Color meshColor;
	private MeshRenderer mR;

	private void Awake() {
		mR = GetComponent<MeshRenderer>();
		//CmdSetColor();
	}

	private void GetGrade() {
		meshColor =  new Color(0,1f - (((float)maxHealth - (float)health) / (float)maxHealth), 0);
	}

	public void TakeDamage(int amount) {
		Debug.Log("hit");
		//health -= amount;
		//if(health <= 0) {
		//	if(destroyOnDeath) {
		//		Destroy(gameObject);
		//	}
		//	else {
		//		health = maxHealth;
		//		// called on the server, will be invoked on the clients
		//		RpcRespawn();
		//	}
		//}
	}

	[ClientRpc]
	void RpcRespawn() {
		if(isLocalPlayer) {
			// move back to zero location
			transform.position = Vector3.zero;
		}
	}
}