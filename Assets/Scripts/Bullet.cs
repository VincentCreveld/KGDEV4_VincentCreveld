using UnityEngine.Networking;
using UnityEngine;

public class Bullet : NetworkBehaviour {
	public void OnTriggerEnter2D(Collider2D collision) {
		Debug.Log("Collided with " + collision.transform.name);
		var hit = collision.gameObject;
		var hitCombat = hit.GetComponent<Combat>();
		if(hitCombat != null) {
			hitCombat.TakeDamage(10);
			Debug.Log("HitPlayer");
		}
		else
			Debug.Log("Hit something else");

		Destroy(gameObject);
	}

	public void OnCollisionStay2D(Collision2D collision) {
		Destroy(gameObject);
	}
}
