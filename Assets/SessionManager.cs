using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SessionManager : MonoBehaviour {

	public InputField usernameField;
	public InputField passwordField;

	private UserData currentUser;

	private void Awake() {
		currentUser = new UserData();
		DontDestroyOnLoad(this);
	}

	public void CatchUsername() {
		currentUser.username = usernameField.text;
	}
	public void CatchPassword() {
		currentUser.password = passwordField.text;
	}

	public void NextField() {
		passwordField.Select();
	}

	public void Login() {
		// Login here
	}

	public void PushHighscore() {
		// Push highscore here
	}
}

public class UserData {
	public string username;
	public string password;
	public string phpSessionID;
	public int score;
}
