using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public delegate void PushScore();
public delegate void AddScore(int num);

public class SessionManager : MonoBehaviour {

	public static event AddScore addScoreEvent;
	public static event PushScore pushScoreEvent;

	public static SessionManager sessionManager;

	public InputField emailField;
	public InputField passwordField;

	private string inputEmail;
	private string inputPass;
	public UserData currentUser;

	

	private void Awake() {
		DontDestroyOnLoad(this);
		//Debug.Log(Hash("hallo"));
		addScoreEvent += AddScore;
		//pushScoreEvent += RpcPushHighscore;
		if(sessionManager == null)
			sessionManager = this;
		else
			Debug.LogError("More than one in scene");
	}

	public void Update() {
		if(Input.GetKeyDown(KeyCode.C)) {
			Debug.LogError(currentUser.email);

		}

	}

	//public void EnableNetworkInterface() {
	//	GetComponent<NetworkManagerHUD>().enabled = true;
	//}

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

	public void MoveToOfflineScene() {
		currentUser.score = 0;
		SceneManager.LoadScene("OfflineScene");
		//EnableNetworkInterface();
	}

	public void PushHighscore() {
		StartCoroutine(ScoreWebrequest(currentUser));
	}

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

	private string scoreURL = "http://studenthome.hku.nl/~vincent.creveld/KGDEV4/submitGame.php";

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

	public void AddScore(int num) {
		currentUser.score += num;
	}

	private string loginURL = "http://studenthome.hku.nl/~vincent.creveld/KGDEV4/loginGame.php";

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

	[ContextMenu("LoadFromJson")]
	private UserData LoadJson(string json) {
		return JsonConvert.DeserializeObject<UserData>(json);
	}

	public void _AddScoreEvent(int num) {
		addScoreEvent(num);
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
