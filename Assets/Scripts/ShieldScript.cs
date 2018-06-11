using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldScript : MonoBehaviour {

	private void OnCollisionEnter2D(Collision2D col) {
		Debug.Log("Hit by " + col.transform.tag);
		if(col.transform.tag == "Bullet")
			Destroy(col.gameObject);
	}
}
