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
		CmdSetColor();
	}

	private void GetGrade() {
		meshColor =  new Color(0,1f - (((float)maxHealth - (float)health) / (float)maxHealth), 0);
	}

	[Command]
	private void CmdSetColor() {
		GetGrade();
		RpcUpdateColor(meshColor);
	}

	[ClientRpc]
	public void RpcUpdateColor(Color c) {
		meshColor = c;
		mR.material.color = meshColor;
	}


	public void TakeDamage(int amount) {
		if(!isServer)
			return;
		health -= amount;
		CmdSetColor();
		if(health <= 0) {
			if(destroyOnDeath) {
				Destroy(gameObject);
			}
			else {
				health = maxHealth;
				// called on the server, will be invoked on the clients
				RpcRespawn();
				CmdSetColor();
			}
		}
	}

	[ClientRpc]
	void RpcRespawn() {
		if(isLocalPlayer) {
			// move back to zero location
			CmdSetColor();
			transform.position = Vector3.zero;
		}
	}
}