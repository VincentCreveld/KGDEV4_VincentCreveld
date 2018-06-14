using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScorePush : MonoBehaviour {

	// Use this for initialization
	void Start () {
		SessionManager.sessionManager.PushHighscore();
	}
}
