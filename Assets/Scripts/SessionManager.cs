using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

// The session manager is the only script in the solution that communicates with the website.
// In this script you will find the login and submitting of score functions.
public class SessionManager : MonoBehaviour {

	public static SessionManager sessionManager;

	public InputField emailField;
	public InputField passwordField;

	private string inputEmail;
	private string inputPass;
	public UserData currentUser;

	

	private void Awake() {
		DontDestroyOnLoad(this);
		if(sessionManager == null)
			sessionManager = this;
		else
			Debug.LogError("More than one in scene");
	}

	// Used to debug whether or not both client and server have different data.
	//public void Update() {
	//	
	//	if(Input.GetKeyDown(KeyCode.C)) {
	//		Debug.LogError(currentUser.email);
	//		Debug.LogError(currentUser.score);
	//	}

	//}

	//public void EnableNetworkInterface() {
	//	GetComponent<NetworkManagerHUD>().enabled = true;
	//}

	// These two functions are to catch the data inserted into the input fields on the login screen.
	// Another solution would be to add listeners to the input fields.
	public void CatchUsername() {
		inputEmail = emailField.text;
	}
	public void CatchPassword() {
		inputPass = Hash(passwordField.text);
	}

	public void Login() {
		// Login here
		Debug.Log("Email: " + inputEmail);
		Debug.Log("Password: " + inputPass);
		StartCoroutine(LoginWebrequest(inputEmail, inputPass));
	}

	// A simple function that uses System.Cryptography to SHA256 hash a given string and return the hash as a string. (Written from reference using this page: https://stackoverflow.com/questions/12416249/hashing-a-string-with-sha256)
	private string Hash(string str) {
		byte[] bytes = Encoding.UTF8.GetBytes(str);
		SHA256Managed hashString = new SHA256Managed();
		byte[] hash = hashString.ComputeHash(bytes);
		string hashedString = string.Empty;
		foreach(byte b in hash) {
			hashedString += String.Format("{0:x2}", b);
		}

		return hashedString;
	}

	public void AddScore(int num) {
		currentUser.score += num;
	}

	// This function resets the player's score and boots him back to the offline scene.
	// Called this script is told to push its highscores.
	public void MoveToOfflineScene() {
		currentUser.score = 0;
		SceneManager.LoadScene("OfflineScene");
		//EnableNetworkInterface();
	}

	public void PushHighscore() {
		StartCoroutine(ScoreWebrequest(currentUser));
	}

	private string scoreURL = "http://studenthome.hku.nl/~vincent.creveld/KGDEV4/submitGame.php";

	// Used to push current score of the player to the highscore database.
	private IEnumerator ScoreWebrequest(UserData ud) {
		WWWForm form = new WWWForm();
		form.AddField("id", ud.id);
		form.AddField("gameid", 6);
		form.AddField("score", ud.score);

		WWW www = new WWW(scoreURL, form);
		yield return www;
		if(string.IsNullOrEmpty(www.text))
			Debug.Log("Wrong credentials");
		else {
			Debug.LogError(www.text);
			Debug.Log("Score pushed succesfully");
			Network.Disconnect();
		}

		yield return null;

	}

	

	private string loginURL = "http://studenthome.hku.nl/~vincent.creveld/KGDEV4/loginGame.php";

	// Called when the user has entered their credentials and wants to log in.
	// Input values get sanitized and validated server-side.
	private IEnumerator LoginWebrequest(string email, string pass) {
		WWWForm form = new WWWForm();
		form.AddField("email", email);
		form.AddField("password", pass);

		WWW www = new WWW(loginURL, form);
		yield return www;
		if(string.IsNullOrEmpty(www.text))
			Debug.Log("Wrong credentials");
		else {
			//Debug.Log("WWW: " + www.text);
			currentUser = LoadJson(www.text);
			Debug.Log("Logged in!");
			MoveToOfflineScene();
		}

		yield return null;

	}

	// Creates the storage class that holds a player's data from the json that gets returned from the webrequest.
	[ContextMenu("LoadFromJson")]
	private UserData LoadJson(string json) {
		return JsonConvert.DeserializeObject<UserData>(json);
	}

}

[Serializable]
public class UserData {
	public UserData(string sid, int id, string un, string pw, string email) {
		this.sessionID = sid;
		this.id = id;
		this.username = un;
		this.password = pw;
		this.email = email;
	}
	public string sessionID;
	public int id;
	public string username;
	public string password;
	public string email;
	public int score;
}
