using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[NetworkSettings(channel = 0, sendInterval = 0.05f)]
public class PlayerMove : NetworkBehaviour {

	public GameObject bullet;
	public GameObject gun;
	public GameObject shield;
	public float blockTime = 2f;
	public float bulletSpeed = 7.5f;
	[SyncVar]
	public int id;

	public float loadTime = 0f;

	public GameObject cameraObj;
	public Transform cameraSlot;
	public Transform shootPos;

	[SyncVar]
	public bool isTurn = false;
	[SyncVar]
	public bool canBlock = false;
	[SyncVar]
	public bool canShoot = false;

	private Rigidbody2D rb2D;
	public GameObject GraphicsSlot;
	private float extentsX;

	private void Awake() {
		rb2D = GetComponent<Rigidbody2D>();
		ySize = GetComponent<Collider2D>().bounds.extents.y;
		extentsX = GetComponent<Collider2D>().bounds.extents.x;
	}

	public override void OnStartLocalPlayer() {
		Debug.Log("Connected " + transform.name);
		CmdPlayerJoined();
		GameObject go = Instantiate(cameraObj, cameraSlot);
		go.transform.position = cameraSlot.position;
		go.transform.parent = cameraSlot;
		//TurnManager.instance.SubscribePlayer(this);
	}

	[Command]
	public void CmdPlayerJoined() {
		GameManager.instance.CmdPlayerJoined(gameObject);
	}

	public void OnDisconnectedFromServer(NetworkDisconnection info) {
		Debug.Log("Disconnect " + transform.name);
	}

	private void Update() {
		if(!isLocalPlayer)
			return;

		if(Input.GetMouseButtonDown(0)) {
			Debug.Log("Fire! " + transform.name);
			TurnManager.CheckTurns(this);
		}

		CheckJump();
		CheckMovement();
		CheckDash();
	}

	private void FixedUpdate() {
		if(!isLocalPlayer)
			return;

		ModifyJumpForce();
	}

	public IEnumerator EndShotDelay() {
		CmdToggleGun(false);
		yield return new WaitForSeconds(loadTime);
		CmdChangeTurn();
	}

	public IEnumerator LoadGun() {
		yield return new WaitForSeconds(loadTime);
		CmdToggleGun(true);
		canShoot = true;
	}

	[ClientRpc]
	public void RpcShoot() {
		GameObject b = Instantiate(bullet, shootPos.position, Quaternion.identity);
		float dir = isFacingRight ? bulletSpeed : -bulletSpeed;
		b.GetComponent<Rigidbody2D>().velocity = transform.right * dir;
		NetworkServer.Spawn(b);
		Destroy(b, 2f);
	}

	[Command]
	public void CmdShoot() {
		// fire bullet
		//Debug.Log("Reached fire on" + transform.name);
		if(canShoot) {
			//Debug.Log("Shooting " + transform.name);
			canShoot = false;
			RpcShoot();
			StartCoroutine(EndShotDelay());
		}
		else
			Debug.Log("Already shooting " + transform.name);
	}

	[ClientRpc]
	public void RpcBlock() {
		GameObject b = Instantiate(shield, shootPos.position, Quaternion.identity);
		b.transform.parent = shootPos;
		NetworkServer.Spawn(b);
		Destroy(b, blockTime);
	}

	[Command]
	public void CmdBlock() {
		//Debug.Log("Reached block on" + transform.name);
		if(canBlock) {
			// spawn shield for X time
			//Debug.Log("Blocking " + transform.name);
			RpcBlock();
			canBlock = false;
		}
		else
			Debug.Log("Already blocked " + transform.name);
	}

	[Command]
	public void CmdToggleGun(bool b) {
		RpcToggleGun(b);
	}

	[ClientRpc]
	public void RpcToggleGun(bool b) {
		gun.SetActive(b);
	}

	[Command]
	public void CmdChangeTurn() {
		Debug.Log("Changing turns");
		GameManager.instance.CmdChangeTurn();
	}

	#region Dash related code
	[Header("Dash variables")]

	[SerializeField]
	private AnimationCurve animC;
	[SerializeField]
	private float dashCooldownTime = 5f;
	[SerializeField]
	private float dashUptime = .1f;
	[SerializeField]
	private float dashDistance = 2f;
	private bool canDash = true;

	/// <summary>
	/// This function acts as an input check
	/// </summary>
	private void CheckDash() {
		if(Input.GetButtonDown("Dash") && canDash) {
			StartCoroutine(DashCooldown(dashCooldownTime));
			StartCoroutine(Dash());
		}
	}

	/// <summary>
	/// This function moves the character according to an animation curve defined pubicly
	/// </summary>
	private IEnumerator Dash() {
		Vector2 DashPos = Vector2.zero;
		float t = 0f;
		int moveDir = (isFacingRight) ? 1 : -1;
		float curveValue;

		DashPos.x = transform.position.x + dashDistance * moveDir;

		while(t < dashUptime) {
			t += Time.deltaTime;
			curveValue = animC.Evaluate(ExtensionFunctions.Map(t, 0, dashUptime, 0, 1));
			Debug.Log(curveValue);
			rb2D.MovePosition(Vector3.Lerp(transform.position, new Vector2(DashPos.x, transform.position.y), curveValue / dashUptime));
			yield return null;
		}
	}

	/// <summary>
	/// This function is a simple cooldown function based on real time.
	/// </summary>
	private IEnumerator DashCooldown(float cd) {
		canDash = false;
		yield return new WaitForSeconds(cd);
		canDash = true;
	}
	#endregion

	#region Horizontal movement related code
	[Header("Horizontal movement variables")]
	[SerializeField]
	private float controllerDeadzone = 0.19f;
	[SerializeField]
	private float moveSpeed = 6f;
	[SerializeField]
	private float sprintSpeed = 10f;
	[SerializeField]
	private float drag = 0.65f;

	private bool isFacingRight = true;

	/// <summary>
	/// This function acts as the input check
	/// </summary>
	private void CheckMovement() {
		//Debug.Log(Input.GetAxisRaw("Horizontal"));
		//Debug.Log(Input.GetButton("Sprint"));
		float speedMod = (Input.GetButton("Sprint")) ? sprintSpeed : moveSpeed;
		if(Input.GetAxis("Horizontal") < -controllerDeadzone || Input.GetAxis("Horizontal") > controllerDeadzone) {
			MoveHorizontal(Input.GetAxis("Horizontal"), speedMod);
			CmdFlipGraphics();
		}
		else {
			//LimitSpeed(speedMod);
			ApplyDrag();
		}
	}

	/// <summary>
	/// This is where the horizontal movement is applied
	/// </summary>
	/// <param name="weight"> The horizontal input axis </param>
	/// <param name="speedMod"> Depends on whether or not the "Sprint" key is pressed </param>
	private void MoveHorizontal(float weight, float speedMod) {
		if(rb2D.velocity.x > -moveSpeed || rb2D.velocity.x < moveSpeed)
			rb2D.velocity = new Vector2(weight * speedMod, rb2D.velocity.y);
	}

	/// <summary>
	/// Prevents constant addition of velocity to the player and limits the speed
	/// </summary>
	/// <param name="speedMod"> Depends on whether or not the "Sprint" key is pressed </param>
	private void LimitSpeed(float speedMod) {
		rb2D.velocity = new Vector2(Mathf.Clamp(rb2D.velocity.x, -speedMod, speedMod), Mathf.Clamp(rb2D.velocity.y, -speedMod, speedMod));
	}

	/// <summary>
	/// Custom drag function to slow the playermovement down faster/slower.
	/// </summary>
	private void ApplyDrag() {
		rb2D.velocity += new Vector2(rb2D.velocity.x * -(drag), 0);
	}

	/// <summary>
	/// Flips the GraphicsSlot variable
	/// </summary>
	[Command]
	public void CmdFlipGraphics() {
		RpcFlipGraphics();
	}
	[ClientRpc]
	private void RpcFlipGraphics() {
		if(isFacingRight && Mathf.Sign(rb2D.velocity.x) < 0) {
			GraphicsSlot.transform.localScale = new Vector3(-1 * GraphicsSlot.transform.localScale.x, GraphicsSlot.transform.localScale.y, GraphicsSlot.transform.localScale.z);
			isFacingRight = !isFacingRight;
		}
		else if(!isFacingRight && Mathf.Sign(rb2D.velocity.x) > 0) {
			GraphicsSlot.transform.localScale = new Vector3(Mathf.Abs(GraphicsSlot.transform.localScale.x), GraphicsSlot.transform.localScale.y, GraphicsSlot.transform.localScale.z);
			isFacingRight = !isFacingRight;
		}
	}

	#endregion

	#region Jump related code
	[Header("Jump variables")]
	[SerializeField]
	private float fallMultiplier = 2.5f;
	[SerializeField]
	private float lowJumpMultiplier = 2f;
	[SerializeField]
	private float jumpForce = 5f;
	private float ySize;
	[SerializeField]
	private bool doubleJumpEnabled;
	private bool canDoubleJump;

	///<summary>
	///This function is called in the FixedUpdate() to improve the way jumping feels.
	///</summary>
	private void ModifyJumpForce() {
		if(rb2D.velocity.y < 0) {
			rb2D.velocity += Vector2.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
		}
		else if(rb2D.velocity.y > 0 && !Input.GetButton("Jump")) {
			rb2D.velocity += Vector2.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
		}
	}

	///<summary>
	///This function is called in the Update() and acts as an input check for jumping.
	///</summary>
	private void CheckJump() {
		if(IsGrounded()) {
			canDoubleJump = true;
		}

		if(Input.GetButtonDown("Jump")) {
			if(IsGrounded()) {
				//canDoubleJump = true;
				Jump();
			}
			if(!IsGrounded() && canDoubleJump && doubleJumpEnabled) {
				Jump();
				canDoubleJump = false;
			}
		}
	}

	///<summary>
	///This function adds vertical velocity to the held rigidbody
	///</summary>
	private void Jump() {
		rb2D.velocity = new Vector2(rb2D.velocity.x, 0);
		rb2D.velocity += new Vector2(0f, jumpForce);
	}

	///<summary>
	///This functions returns true when the player object is on the ground.
	///</summary>
	private bool IsGrounded() {
		return Physics2D.Raycast(transform.position, -Vector3.up, ySize + .25f, LayerMask.GetMask("Level", "InteractableObject"));//, LayerMask.GetMask("Level"));
	}


	#endregion


}
